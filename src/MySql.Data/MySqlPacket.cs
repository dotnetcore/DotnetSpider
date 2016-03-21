// Copyright © 2004, 2015, Oracle and/or its affiliates. All rights reserved.
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
using System.Diagnostics;
using System.Text;
using System.IO;
using MySql.Data.Common;


namespace MySql.Data.MySqlClient
{
	internal class MySqlPacket
	{
		private byte[] tempBuffer = new byte[256];
		private Encoding encoding;
		private MemoryStream buffer = new MemoryStream(5);
		private DBVersion version;

		private MySqlPacket()
		{
			Clear();
		}

		public MySqlPacket(Encoding enc)
			: this()
		{
			Encoding = enc;
		}

		public MySqlPacket(MemoryStream stream)
			: this()
		{
			buffer = stream;
		}

		#region Properties

		public Encoding Encoding
		{
			get { return encoding; }
			set
			{
				Debug.Assert(value != null);
				encoding = value;
			}
		}

		public bool HasMoreData
		{
			get { return buffer.Position < buffer.Length; }
		}

		public int Position
		{
			get { return (int)buffer.Position; }
			set { buffer.Position = (long)value; }
		}

		public int Length
		{
			get { return (int)buffer.Length; }
			set { buffer.SetLength(value); }
		}

		public bool IsLastPacket
		{
			get
			{
				ArraySegment<byte> tmp;
				buffer.TryGetBuffer(out tmp);
				var bits = tmp.Array;
				return bits[0] == 0xfe && Length <= 5;
			}
		}

		public byte[] Buffer
		{
			get
			{
				ArraySegment<byte> tmp;
				buffer.TryGetBuffer(out tmp);
				return tmp.Array;
			}
		}

		public DBVersion Version
		{
			get { return version; }
			set { version = value; }
		}

		#endregion

		public void Clear()
		{
			Position = 4;
		}


		#region Byte methods

		public byte ReadByte()
		{
			return (byte)buffer.ReadByte();
		}

		public int Read(byte[] byteBuffer, int offset, int count)
		{
			return buffer.Read(byteBuffer, offset, count);
		}

		public void WriteByte(byte b)
		{
			buffer.WriteByte(b);
		}

		public void Write(byte[] bytesToWrite)
		{
			Write(bytesToWrite, 0, bytesToWrite.Length);
		}

		public void Write(byte[] bytesToWrite, int offset, int countToWrite)
		{
			buffer.Write(bytesToWrite, offset, countToWrite);
		}

		public int ReadNBytes()
		{
			byte c = ReadByte();
			if (c < 1 || c > 4)
				throw new MySqlException("Resources.IncorrectTransmission");
			return ReadInteger(c);
		}

		public void SetByte(long position, byte value)
		{
			long currentPosition = buffer.Position;
			buffer.Position = position;
			buffer.WriteByte(value);
			buffer.Position = currentPosition;
		}

		#endregion

		#region Integer methods

		public long ReadFieldLength()
		{
			byte c = ReadByte();

			switch (c)
			{
				case 251:
					return -1;
				case 252:
					return ReadInteger(2);
				case 253:
					return ReadInteger(3);
				case 254:
					return ReadLong(8);
				default:
					return c;
			}
		}

		public ulong ReadBitValue(int numbytes)
		{
			ulong value = 0;

			int pos = (int)buffer.Position;
			ArraySegment<byte> tmp;
			if (buffer.TryGetBuffer(out tmp))
			{
				byte[] bits = tmp.Array;
				int shift = 0;

				for (int i = 0; i < numbytes; i++)
				{
					value <<= shift;
					value |= bits[pos++];
					shift = 8;
				}
				buffer.Position += numbytes;
				return value;
			}
			throw new IOException("Read bit value failed.");
		}

		public long ReadLong(int numbytes)
		{
			Debug.Assert((buffer.Position + numbytes) <= buffer.Length);
			ArraySegment<byte> tmp;
			if (buffer.TryGetBuffer(out tmp))
			{
				byte[] bytes = tmp.Array;
				int pos = (int)buffer.Position;
				buffer.Position += numbytes;
				switch (numbytes)
				{
					case 2:
						return BitConverter.ToUInt16(bytes, pos);
					case 4:
						return BitConverter.ToUInt32(bytes, pos);
					case 8:
						return BitConverter.ToInt64(bytes, pos);
				}
				throw new NotSupportedException("Only byte lengths of 2, 4, or 8 are supported");
			}
			throw new IOException("Read bit value failed.");
		}



		public ulong ReadULong(int numbytes)
		{Debug.Assert((buffer.Position + numbytes) <= buffer.Length);
			ArraySegment<byte> tmp;
			
			if (buffer.TryGetBuffer(out tmp))
			{
				byte[] bytes = tmp.Array;
				int pos = (int) buffer.Position;
				buffer.Position += numbytes;
				switch (numbytes)
				{
					case 2:
						return BitConverter.ToUInt16(bytes, pos);
					case 4:
						return BitConverter.ToUInt32(bytes, pos);
					case 8:
						return BitConverter.ToUInt64(bytes, pos);
				}
				throw new NotSupportedException("Only byte lengths of 2, 4, or 8 are supported");
			}
			throw new IOException("Read bit value failed.");
		}

		public int Read3ByteInt()
		{
			int value = 0;

			int pos = (int)buffer.Position;
			ArraySegment<byte> tmp;

			if (buffer.TryGetBuffer(out tmp))
			{
				byte[] bits = tmp.Array;
				int shift = 0;

				for (int i = 0; i < 3; i++)
				{
					value |= (int) (bits[pos++] << shift);
					shift += 8;
				}
				buffer.Position += 3;
				return value;
			}
			throw new IOException("Read bit value failed.");
		}

		public int ReadInteger(int numbytes)
		{
			if (numbytes == 3)
				return Read3ByteInt();
			Debug.Assert(numbytes <= 4);
			return (int)ReadLong(numbytes);
		}

		/// <summary>
		/// WriteInteger
		/// </summary>
		/// <param name="v"></param>
		/// <param name="numbytes"></param>
		public void WriteInteger(long v, int numbytes)
		{
			long val = v;

			Debug.Assert(numbytes > 0 && numbytes < 9);

			for (int x = 0; x < numbytes; x++)
			{
				tempBuffer[x] = (byte)(val & 0xff);
				val >>= 8;
			}
			Write(tempBuffer, 0, numbytes);
		}

		public int ReadPackedInteger()
		{
			byte c = ReadByte();

			switch (c)
			{
				case 251: return -1;
				case 252: return ReadInteger(2);
				case 253: return ReadInteger(3);
				case 254: return ReadInteger(4);
				default: return c;
			}
		}

		public void WriteLength(long length)
		{
			if (length < 251)
				WriteByte((byte)length);
			else if (length < 65536L)
			{
				WriteByte(252);
				WriteInteger(length, 2);
			}
			else if (length < 16777216L)
			{
				WriteByte(253);
				WriteInteger(length, 3);
			}
			else
			{
				WriteByte(254);
				WriteInteger(length, 4);
			}
		}

		#endregion

		#region String methods

		public void WriteLenString(string s)
		{
			byte[] bytes = encoding.GetBytes(s);
			WriteLength(bytes.Length);
			Write(bytes, 0, bytes.Length);
		}

		public void WriteStringNoNull(string v)
		{
			byte[] bytes = encoding.GetBytes(v);
			Write(bytes, 0, bytes.Length);
		}

		public void WriteString(string v)
		{
			WriteStringNoNull(v);
			WriteByte(0);
		}

		public string ReadLenString()
		{
			long len = ReadPackedInteger();
			return ReadString(len);
		}

		public string ReadAsciiString(long length)
		{
			if (length == 0)
				return String.Empty;
			//            byte[] buf = new byte[length];
			Read(tempBuffer, 0, (int)length);
			return Encoding.GetEncoding("us-ascii").GetString(tempBuffer, 0, (int)length);
			//return encoding.GetString(tempBuffer, 0, (int)length); //buf.Length);
		}

		public string ReadString(long length)
		{
			if (length == 0)
				return String.Empty;
			if (tempBuffer == null || length > tempBuffer.Length)
				tempBuffer = new byte[length];
			Read(tempBuffer, 0, (int)length);
			return encoding.GetString(tempBuffer, 0, (int)length);
		}

		public string ReadString()
		{
			return ReadString(encoding);
		}

		public string ReadString(Encoding theEncoding)
		{
			byte[] bytes = ReadStringAsBytes();
			string s = theEncoding.GetString(bytes, 0, bytes.Length);
			return s;
		}

		public byte[] ReadStringAsBytes()
		{
			byte[] readBytes; ArraySegment<byte> tmp;

			if (buffer.TryGetBuffer(out tmp))
			{
				byte[] tempBuffer = tmp.Array;
		 
				int end = (int) buffer.Position;

				while (end < (int) buffer.Length &&
				       tempBuffer[end] != 0 && (int) tempBuffer[end] != -1)
					end++;

				readBytes = new byte[end - buffer.Position];
				Array.Copy(tempBuffer, (int) buffer.Position, readBytes, 0, (int) (end - buffer.Position));
				buffer.Position = end + 1;

				return readBytes;
			}
			throw new IOException("Read bit value failed.");
		}

		#endregion
	}
}
