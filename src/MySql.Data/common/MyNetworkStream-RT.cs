//// Copyright (c) 2009 Sun Microsystems, Inc.
////
//// MySQL Connector/NET is licensed under the terms of the GPLv2
//// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
//// MySQL Connectors. There are special exceptions to the terms and 
//// conditions of the GPLv2 as it is applied to this software, see the 
//// FLOSS License Exception
//// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
////
//// This program is free software; you can redistribute it and/or modify 
//// it under the terms of the GNU General Public License as published 
//// by the Free Software Foundation; version 2 of the License.
////
//// This program is distributed in the hope that it will be useful, but 
//// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
//// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
//// for more details.
////
//// You should have received a copy of the GNU General Public License along 
//// with this program; if not, write to the Free Software Foundation, Inc., 
//// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

//using System;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;
//using Windows.Networking;
//using Windows.Networking.Sockets;
//using Windows.Storage.Streams;
//using System.Runtime.InteropServices.WindowsRuntime;
//using MySql.Data.MySqlClient;

//using Windows.Foundation;

//namespace MySql.Data.Common
//{
//  internal class MyNetworkStream : Stream, IDisposable
//  {
//    DataReader dataReader;
//    DataWriter dataWriter;
//    StreamSocket streamSocket;
//    private int timeout;
//    private bool readable, writeable;
//    private int readTimeout = System.Threading.Timeout.Infinite;
//    private int writeTimeout = System.Threading.Timeout.Infinite;

//    public MyNetworkStream(string host, int port, int timeout)
//    {
//      Server = host;
//      Port = port;
//      this.timeout = timeout;
//      readable = writeable = false;
//    }

//    public string Server { get; private set; }
//    public int Port { get; private set; }

//    public void Open()
//    {
//      OpenConnection();
//      dataReader = new DataReader(streamSocket.InputStream);
//      dataWriter = new DataWriter(streamSocket.OutputStream);
//      readable = writeable = true;
//    }

//    private void OpenConnection()
//    {
//      streamSocket = new StreamSocket();

//      HostName host = new HostName(Server);

//      CancellationTokenSource cts = new CancellationTokenSource();

//      try
//      {
//        cts.CancelAfter(timeout*1000);
//        streamSocket.Control.KeepAlive = true;
//        var task = streamSocket.ConnectAsync(host, Port.ToString()).AsTask(cts.Token);
//        task.Wait();
//      }
//      catch (TaskCanceledException)
//      {
//        // we timed out the connection
//        streamSocket.Dispose();
//        streamSocket = null;
//        throw new TimeoutException(Resources.Timeout);
//      }
//    }

//    public override int ReadTimeout
//    {
//      get  { return readTimeout; }
//      set  { readTimeout = value; }
//    }

//    public override int WriteTimeout
//    {
//      get { return writeTimeout; }
//      set { writeTimeout = value; }
//    }

//    public override bool CanRead
//    {
//      get { return readable; }
//    }

//    public override bool CanWrite 
//    { 
//      get { return writeable; }
//    }

//    public override bool CanSeek
//    {
//      get { return false; }
//    }

//    public override bool CanTimeout
//    {
//      get { return true; }
//    }

//    public override long Position
//    {
//      get  { throw new NotImplementedException();  }
//      set  { throw new NotImplementedException();  }
//    }

//    public override long Length
//    {
//      get { throw new NotImplementedException(); }
//    }

//    public override long Seek(long offset, SeekOrigin origin)
//    {
//      throw new NotImplementedException();
//    }

//    public override void SetLength(long value)
//    {
//      throw new NotImplementedException();
//    }

//    public override void Flush()
//    {
//    }

//    public override int Read(byte[] buffer, int offset, int count)
//    {
//      dataReader.InputStreamOptions = InputStreamOptions.Partial;

//      try
//      {
//        CancellationTokenSource cts = new CancellationTokenSource(readTimeout);
//        DataReaderLoadOperation op = dataReader.LoadAsync((uint)count);
//        Task<uint> read = op.AsTask<uint>(cts.Token);
//        read.Wait();
//        // here we need to put the bytes read into the buffer
//        dataReader.ReadBuffer(read.Result).CopyTo(0, buffer, offset, (int)read.Result);
//        return (int)read.Result;
//      }
//      catch (TaskCanceledException)
//      {
//        streamSocket.Dispose();
//        streamSocket = null;
//        throw new TimeoutException(Resources.Timeout);
//      }
//    }

//    public override void Write(byte[] byteBuffer, int offset, int count)
//    {
//      try
//      {
//        CancellationTokenSource cts = new CancellationTokenSource(writeTimeout);
//        dataWriter.WriteBuffer(byteBuffer.AsBuffer(), (uint)offset, (uint)count);
//        DataWriterStoreOperation op = dataWriter.StoreAsync();
//        Task<uint> write = op.AsTask<uint>(cts.Token);
//        write.Wait();
//      }
//      catch (TaskCanceledException)
//      {
//        streamSocket.Dispose();
//        streamSocket = null;
//        throw new TimeoutException(Resources.Timeout);
//      }
//    }

//    public static MyNetworkStream CreateStream(MySqlConnectionStringBuilder settings, bool unix)
//    {
//      MyNetworkStream s = new MyNetworkStream(settings.Server, (int)settings.Port, (int)settings.ConnectionTimeout);
//      s.Open();
//      return s;
//    }

//    void Dispose()
//    {
//      streamSocket.Dispose();
//      streamSocket = null;
//      readable = writeable = false;
//    }
//  }
//}