// Copyright ?2004, 2013, Oracle and/or its affiliates. All rights reserved.
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
using System.Diagnostics;
using System.Text;
using MySql.Data.Common;


namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for MySqlStream.
	/// </summary>
	internal class MySqlStream : IDisposable
	{
		private byte sequenceByte;
		private int maxBlockSize;
		private ulong maxPacketSize;
		private byte[] packetHeader = new byte[4];
		private MySqlPacket packet;
		private TimedStream timedStream;
		private Stream inStream;
		private Stream outStream;

		internal Stream BaseStream
		{
			get { return timedStream; }
		}

		public MySqlStream(Encoding encoding)
		{
			// we have no idea what the real value is so we start off with the max value
			// The real value will be set in NativeDriver.Configure()
			maxPacketSize = ulong.MaxValue;

			// we default maxBlockSize to MaxValue since we will get the 'real' value in 
			// the authentication handshake and we know that value will not exceed 
			// true maxBlockSize prior to that.
			maxBlockSize = Int32.MaxValue;

			packet = new MySqlPacket(encoding);
		}

		public MySqlStream(Stream baseStream, Encoding encoding, bool compress)
			: this(encoding)
		{
			timedStream = new TimedStream(baseStream);
			Stream stream;
			if (compress)
				stream = new CompressedStream(timedStream);
			else
				stream = timedStream;

 
			inStream = new System.IO.BufferedStream(stream);
 
			outStream = stream;
		}

		public void Close()
		{

			outStream.Dispose();
			inStream.Dispose();


			timedStream.Dispose();
		}

		#region Properties

		public Encoding Encoding
		{
			get { return packet.Encoding; }
			set { packet.Encoding = value; }
		}

		public void ResetTimeout(int timeout)
		{
			timedStream.ResetTimeout(timeout);
		}

		public byte SequenceByte
		{
			get { return sequenceByte; }
			set { sequenceByte = value; }
		}

		public int MaxBlockSize
		{
			get { return maxBlockSize; }
			set { maxBlockSize = value; }
		}

		public ulong MaxPacketSize
		{
			get { return maxPacketSize; }
			set { maxPacketSize = value; }
		}

		#endregion

		#region Packet methods

		/// <summary>
		/// ReadPacket is called by NativeDriver to start reading the next
		/// packet on the stream.
		/// </summary>
		public MySqlPacket ReadPacket()
		{
			//Debug.Assert(packet.Position == packet.Length);

			// make sure we have read all the data from the previous packet
			//Debug.Assert(HasMoreData == false, "HasMoreData is true in OpenPacket");

			LoadPacket();

			// now we check if this packet is a server error
			if (packet.Buffer[0] == 0xff)
			{
				packet.ReadByte(); // read off the 0xff

				int code = packet.ReadInteger(2);
				string msg = String.Empty;

				if (packet.Version.isAtLeast(5, 5, 0))
					msg = packet.ReadString(Encoding.UTF8);
				else
					msg = packet.ReadString();

				if (msg.StartsWith("#", StringComparison.Ordinal))
				{
					msg.Substring(1, 5); /* state code */
					msg = msg.Substring(6);
				}
				throw new MySqlException(msg, code);
			}
			return packet;
		}

		/// <summary>
		/// Reads the specified number of bytes from the stream and stores them at given 
		/// offset in the buffer.
		/// Throws EndOfStreamException if not all bytes can be read.
		/// </summary>
		/// <param name="stream">Stream to read from</param>
		/// <param name="buffer"> Array to store bytes read from the stream </param>
		/// <param name="offset">The offset in buffer at which to begin storing the data read from the current stream. </param>
		/// <param name="count">Number of bytes to read</param>
		internal static void ReadFully(Stream stream, byte[] buffer, int offset, int count)
		{
			int numRead = 0;
			int numToRead = count;
			while (numToRead > 0)
			{
				int read = stream.Read(buffer, offset + numRead, numToRead);
				if (read == 0)
				{
					throw new EndOfStreamException();
				}
				numRead += read;
				numToRead -= read;
			}
		}

		/// <summary>
		/// LoadPacket loads up and decodes the header of the incoming packet.
		/// </summary>
		public void LoadPacket()
		{
			try
			{
				packet.Length = 0;
				int offset = 0;
				while (true)
				{
					ReadFully(inStream, packetHeader, 0, 4);
					sequenceByte = (byte) (packetHeader[3] + 1);
					int length = (int) (packetHeader[0] + (packetHeader[1] << 8) +
					                    (packetHeader[2] << 16));

					// make roo for the next block
					packet.Length += length;

#if RT
          byte[] tempBuffer = new byte[length];
          ReadFully(inStream, tempBuffer, offset, length);
          packet.Write(tempBuffer);
#else
					ReadFully(inStream, packet.Buffer, offset, length);
#endif
					offset += length;

					// if this block was < maxBlock then it's last one in a multipacket series
					if (length < maxBlockSize) break;
				}
				packet.Position = 0;
			}
			catch (IOException ioex)
			{
				throw new MySqlException("Resources.ReadFromStreamFailed", true, ioex);
			}
		}

		public void SendPacket(MySqlPacket packet)
		{
			byte[] buffer = packet.Buffer;
			int length = packet.Position - 4;

			if ((ulong) length > maxPacketSize)
				throw new MySqlException("Resources.QueryTooLarge", (int) MySqlErrorCode.PacketTooLarge);

			int offset = 0;
			while (length > 0)
			{
				int lenToSend = length > maxBlockSize ? maxBlockSize : length;
				buffer[offset] = (byte) (lenToSend & 0xff);
				buffer[offset + 1] = (byte) ((lenToSend >> 8) & 0xff);
				buffer[offset + 2] = (byte) ((lenToSend >> 16) & 0xff);
				buffer[offset + 3] = sequenceByte++;

				outStream.Write(buffer, offset, lenToSend + 4);
				outStream.Flush();
				length -= lenToSend;
				offset += lenToSend;
			}
		}

		public void SendEntirePacketDirectly(byte[] buffer, int count)
		{
			buffer[0] = (byte) (count & 0xff);
			buffer[1] = (byte) ((count >> 8) & 0xff);
			buffer[2] = (byte) ((count >> 16) & 0xff);
			buffer[3] = sequenceByte++;
			outStream.Write(buffer, 0, count + 4);
			outStream.Flush();
		}

		public void Dispose()
		{
 
		}

		#endregion

	}
}
