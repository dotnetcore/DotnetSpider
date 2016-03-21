// Copyright (c) 2004-2008 MySQL AB, 2008-2009 Sun Microsystems, Inc.
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
using MySql.Data.MySqlClient;


namespace MySql.Data.Common
{
    internal class BufferedStream : Stream
    {
        private byte[] writeBuffer;
        private byte[] readBuffer;
        private int writePos;
        private int readLength;
        private int readPos;
        private int bufferSize;
        private Stream baseStream;

        public BufferedStream(Stream stream)
        {
            baseStream = stream;
            bufferSize = 4096;
            readBuffer = new byte[bufferSize];
            writeBuffer = new byte[bufferSize];
        }

        #region Stream Implementation

        public override bool CanRead
        {
            get
            {
                if (baseStream != null) return baseStream.CanRead;
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (baseStream != null) return baseStream.CanSeek;
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (baseStream != null) return baseStream.CanWrite;
                return false;
            }
        }

        public override void Flush()
        {
            if (baseStream == null)
                throw new InvalidOperationException("ObjectDisposed");
            if (writePos == 0) return;

            baseStream.Write(writeBuffer, 0, writePos);
            baseStream.Flush();
            writePos = 0;
        }

        public override long Length
        {
            get
            {
                if (baseStream == null)
                    throw new InvalidOperationException("Resources.ObjectDisposed");
                Flush();
                return baseStream.Length;
            }

        }

        public override long Position
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer", "Resources.ParameterCannotBeNull");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Resources.OffsetCannotBeNegative");

			if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Resources.CountCannotBeNegative");

			if ((buffer.Length - offset) < count)
                throw new ArgumentException("Resources.OffsetMustBeValid");

			if (baseStream == null)
                throw new InvalidOperationException("Resources.ObjectDisposed");


			if ((readLength - readPos) == 0)
            {
                TryToFillReadBuffer();
                if (readLength == 0) return 0;
            }

            int inBuffer = readLength - readPos;
            int toRead = count;
            if (toRead > inBuffer)
                toRead = inBuffer;
            Buffer.BlockCopy(readBuffer, readPos, buffer, offset, toRead);
            readPos += toRead;
            count -= toRead;
            if (count > 0)
            {
                int read = baseStream.Read(buffer, offset + toRead, count);
                toRead += read;
                readPos = readLength = 0;
            }
            return toRead;
        }

        private void TryToFillReadBuffer()
        {
            int read = baseStream.Read(readBuffer, 0, bufferSize);
            readPos = 0;
            readLength = read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void SetLength(long value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer", "Resources.ParameterCannotBeNull");

			if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Resources.OffsetCannotBeNegative");

			if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Resources.CountCannotBeNegative");

			if ((buffer.Length - offset) < count)
                throw new ArgumentException("Resources.OffsetMustBeValid");

			if (baseStream == null)
                throw new InvalidOperationException("Resources.ObjectDisposed");

			// if we don't have enough room in our current write buffer for the data
			// then flush the data
			int roomLeft = bufferSize - writePos;
            if (count > roomLeft)
                Flush();

            // if the data will not fit into a entire buffer, then there is no need to buffer it.
            // We just send it down
            if (count > bufferSize)
                baseStream.Write(buffer, offset, count);
            else
            {
                // if we get here then there is room in our buffer for the data.  We store it and 
                // adjust our internal lengths.
                Buffer.BlockCopy(buffer, offset, writeBuffer, writePos, count);
                writePos += count;
            }
        }

        #endregion

    }
}
