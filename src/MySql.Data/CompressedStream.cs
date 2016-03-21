// Copyright ?2004, 2010, Oracle and/or its affiliates. All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.IO;
using zlib;

using MySql.Data.Common;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for CompressedStream.
	/// </summary>
	internal class CompressedStream : Stream
	{
		// writing fields
		private Stream baseStream;
		private MemoryStream cache;

		// reading fields
		private byte[] localByte;
		private byte[] inBuffer;
		private byte[] lengthBytes;
		private WeakReference inBufferRef;
		private int inPos;
		private int maxInPos;
		private ZInputStream zInStream;

		public CompressedStream(Stream baseStream)
		{
			this.baseStream = baseStream;
			localByte = new byte[1];
			lengthBytes = new byte[7];
			cache = new MemoryStream();
			inBufferRef = new WeakReference(inBuffer, false);
		}

		#region Properties


		public override bool CanRead
		{
			get { return baseStream.CanRead; }
		}

		public override bool CanWrite
		{
			get { return baseStream.CanWrite; }
		}

		public override bool CanSeek
		{
			get { return baseStream.CanSeek; }
		}

		public override long Length
		{
			get { return baseStream.Length; }
		}

		public override long Position
		{
			get { return baseStream.Position; }
			set { baseStream.Position = value; }
		}

		#endregion

 
		protected override void Dispose(bool ii)
		{
			baseStream.Dispose();

			cache.Dispose();
			base.Dispose();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException("Resources.CSNoSetLength");
		}

		public override int ReadByte()
		{
			try
			{
				Read(localByte, 0, 1);
				return localByte[0];
			}
			catch (EndOfStreamException)
			{
				return -1;
			}
		}

		public override bool CanTimeout
		{
			get
			{
				return baseStream.CanTimeout;
			}
		}

		public override int ReadTimeout
		{
			get
			{
				return baseStream.ReadTimeout;
			}
			set
			{
				baseStream.ReadTimeout = value;
			}
		}

		public override int WriteTimeout
		{
			get
			{
				return baseStream.WriteTimeout;
			}
			set
			{
				baseStream.WriteTimeout = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer", "Resources.BufferCannotBeNull");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException("offset", "Resources.OffsetMustBeValid");
			if ((offset + count) > buffer.Length)
				throw new ArgumentException("Resources.BufferNotLargeEnough", "buffer");

			if (inPos == maxInPos)
				PrepareNextPacket();

			int countToRead = Math.Min(count, maxInPos - inPos);
			int countRead;
			if (zInStream != null)
				countRead = zInStream.read(buffer, offset, countToRead);
			else
				countRead = baseStream.Read(buffer, offset, countToRead);
			inPos += countRead;

			// release the weak reference
			if (inPos == maxInPos)
			{
				zInStream = null;

				inBufferRef = new WeakReference(inBuffer, false);
				inBuffer = null;
			}

			return countRead;
		}

		private void PrepareNextPacket()
		{
			MySqlStream.ReadFully(baseStream, lengthBytes, 0, 7);
			int compressedLength = lengthBytes[0] + (lengthBytes[1] << 8) + (lengthBytes[2] << 16);
			// lengthBytes[3] is seq
			int unCompressedLength = lengthBytes[4] + (lengthBytes[5] << 8) +
						 (lengthBytes[6] << 16);

			if (unCompressedLength == 0)
			{
				unCompressedLength = compressedLength;
				zInStream = null;
			}
			else
			{
				ReadNextPacket(compressedLength);
				MemoryStream ms = new MemoryStream(inBuffer);
				zInStream = new ZInputStream(ms);
				zInStream.maxInput = compressedLength;
			}

			inPos = 0;
			maxInPos = unCompressedLength;
		}

		private void ReadNextPacket(int len)
		{
			inBuffer = inBufferRef.Target as byte[];

			if (inBuffer == null || inBuffer.Length < len)
				inBuffer = new byte[len];
			MySqlStream.ReadFully(baseStream, inBuffer, 0, len);
		}

		private MemoryStream CompressCache()
		{
			// small arrays almost never yeild a benefit from compressing
			if (cache.Length < 50)
				return null;
			ArraySegment<byte> tmp1;
			if (cache.TryGetBuffer(out tmp1))
			{
				byte[] cacheBytes = tmp1.Array;
				MemoryStream compressedBuffer = new MemoryStream();
				ZOutputStream zos = new ZOutputStream(compressedBuffer, zlibConst.Z_DEFAULT_COMPRESSION);
				zos.Write(cacheBytes, 0, (int)cache.Length);
				zos.finish();

				// if the compression hasn't helped, then just return null
				if (compressedBuffer.Length >= cache.Length)
					return null;
				return compressedBuffer;
			}
			throw new IOException("Can't get buffer.");
		}

		private void CompressAndSendCache()
		{
			long compressedLength, uncompressedLength;

			ArraySegment<byte> tmp1;
			if (cache.TryGetBuffer(out tmp1))
			{
				byte[] cacheBuffer = tmp1.Array;
				byte seq = cacheBuffer[3];
				cacheBuffer[3] = 0;



				// first we compress our current cache
				MemoryStream compressedBuffer = CompressCache();

				// now we set our compressed and uncompressed lengths
				// based on if our compression is going to help or not
				MemoryStream memStream;

				if (compressedBuffer == null)
				{
					compressedLength = cache.Length;
					uncompressedLength = 0;
					memStream = cache;
				}
				else
				{
					compressedLength = compressedBuffer.Length;
					uncompressedLength = cache.Length;
					memStream = compressedBuffer;
				}

				// Make space for length prefix (7 bytes) at the start of output
				long dataLength = memStream.Length;
				int bytesToWrite = (int)dataLength + 7;
				memStream.SetLength(bytesToWrite);


				ArraySegment<byte> tmp;
				if (memStream.TryGetBuffer(out tmp))
				{
					byte[] buffer = tmp.Array;
					Array.Copy(buffer, 0, buffer, 7, (int)dataLength);

					// Write length prefix
					buffer[0] = (byte)(compressedLength & 0xff);
					buffer[1] = (byte)((compressedLength >> 8) & 0xff);
					buffer[2] = (byte)((compressedLength >> 16) & 0xff);
					buffer[3] = seq;
					buffer[4] = (byte)(uncompressedLength & 0xff);
					buffer[5] = (byte)((uncompressedLength >> 8) & 0xff);
					buffer[6] = (byte)((uncompressedLength >> 16) & 0xff);

					baseStream.Write(buffer, 0, bytesToWrite);
					baseStream.Flush();
					cache.SetLength(0);
					if (compressedBuffer != null)
					{
						compressedBuffer.Dispose();
					}
				}
			}
		}

		public override void Flush()
		{
			if (!InputDone()) return;

			CompressAndSendCache();
		}

		private bool InputDone()
		{
			// if we have not done so yet, see if we can calculate how many bytes we are expecting
			if (baseStream is TimedStream && ((TimedStream)baseStream).IsClosed) return false;
			if (cache.Length < 4) return false;
			ArraySegment<byte> tmp;
			if (cache.TryGetBuffer(out tmp))
			{
				byte[] buf = tmp.Array;
				int expectedLen = buf[0] + (buf[1] << 8) + (buf[2] << 16);
				if (cache.Length < (expectedLen + 4)) return false;
				return true;
			}
			return false;
		}

		public override void WriteByte(byte value)
		{
			cache.WriteByte(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			cache.Write(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return baseStream.Seek(offset, origin);
		}
	}
}
