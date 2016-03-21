// Copyright © 2013, Oracle and/or its affiliates. All rights reserved.
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

namespace MySql.Data.MySqlClient.Memcached
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.IO;

  /// <summary>
  /// Implementation of memcached binary client protocol.
  /// </summary>
  /// <remarks>According to http://code.google.com/p/memcached/wiki/BinaryProtocolRevamped </remarks>
  public class BinaryClient : Client
  {
    private Encoding encoding;

    private enum OpCodes : byte
    {
        Get = 0x00,
        Set = 0x01,
        Add = 0x02,
        Replace = 0x03,
        Delete = 0x04,
        Increment = 0x05,
        Decrement = 0x06,
        Quit = 0x07,
        Flush = 0x08, 
        GetK = 0x0c,
        GetKQ = 0x0d,
        Append = 0x0e,
        Prepend = 0x0f,
        SASL_list_mechs = 0x20,
        SASL_Auth = 0x21,
        SASL_Step = 0x22
    }

    private enum MagicByte : byte
    {
      Request = 0x80,
      Response = 0x81
    }

    private enum ResponseStatus : ushort
    {
      NoError = 0x0000,
      KeyNotFound = 0x0001,
      KeyExists = 0x0002,
      ValueTooLarge = 0x0003,  
      InvalidArguments = 0x0004,
      ItemNotStored = 0x0005,
      IncrDecrOnNonNumericValue = 0x0006,
      VbucketBelongsToAnotherServer = 0x0007,
      AuthenticationError = 0x0008,
      AuthenticationContinue = 0x0009,
      UnknownCommand = 0x0081,
      OutOfMemory = 0x0082,
      NotSupported = 0x0083,
      InternalError = 0x0084,
      Busy = 0x0085,
      TemporaryFailure = 0x0086
    }

    public BinaryClient( string server, uint port ) : base( server, port )
    {
      encoding = Encoding.UTF8;
    }

    #region Memcached protocol interface

    public override void Add(string key, object data, TimeSpan expiration)
    {
      SendCommand((byte)MagicByte.Request, (byte)OpCodes.Add, key, data, expiration, true);
    }

    public override void Append(string key, object data)
    {
      SendCommand((byte)MagicByte.Request, (byte)OpCodes.Append, key, data, TimeSpan.Zero, false);
    }

    public override void Cas(string key, object data, TimeSpan expiration, ulong casUnique)
    {
      throw new NotImplementedException("Not available in binary protocol");
      //SendCommand((byte)MagicByte.Request, (byte)OpCodes.Cas, key, data, expiration, true, casUnique);
    }

    public override void Decrement(string key, int amount)
    {
      SendCommand((byte)MagicByte.Request, (byte)OpCodes.Decrement, key, amount);
    }

    public override void Delete(string key)
    {
      SendCommand((byte)MagicByte.Request, (byte)OpCodes.Delete, key );
    }

    public override void FlushAll(TimeSpan delay)
    {
      SendCommand((byte)MagicByte.Request, (byte)OpCodes.Flush, delay);
    }

    public override KeyValuePair<string, object> Get(string key)
    {
      string val;
      SendCommand((byte)MagicByte.Request, (byte)OpCodes.Get, key, out val );
      return new KeyValuePair<string, object>(key, val);
    }

    public override void Increment(string key, int amount)
    {
      SendCommand((byte)MagicByte.Request, (byte)OpCodes.Increment, key, amount);
    }

    public override void Prepend(string key, object data)
    {
      SendCommand((byte)MagicByte.Request, (byte)OpCodes.Prepend, key, data, TimeSpan.Zero, false);
    }

    public override void Replace(string key, object data, TimeSpan expiration)
    {
      SendCommand((byte)MagicByte.Request, (byte)OpCodes.Replace, key, data, expiration, true);
    }

    public override void Set(string key, object data, TimeSpan expiration)
    {
      SendCommand( ( byte )MagicByte.Request, ( byte )OpCodes.Set, key, data, expiration, true );
    }

    #endregion

    /// <summary>
    /// Sends an store command (add, replace, set).
    /// </summary>
    /// <param name="magic"></param>
    /// <param name="opcode"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="expiration"></param>
    private void SendCommand( 
      byte magic, byte opcode, string key, object data, TimeSpan expiration, bool hasExtra )
    {
      // Send data
      byte[] dataToSend = EncodeStoreCommand(magic, opcode, key, data, expiration, hasExtra );
      stream.Write(dataToSend, 0, dataToSend.Length);
      byte[] res = GetResponse();    
    }

    /// <summary>
    /// Sends a get command.
    /// </summary>
    /// <param name="magic"></param>
    /// <param name="opcode"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    private void SendCommand(byte magic, byte opcode, string key, out string value)
    {
      // Send data
      byte[] dataToSend = EncodeGetCommand(magic, opcode, key );
      stream.Write(dataToSend, 0, dataToSend.Length);
      byte[] res = GetResponse();
      byte[] bValue = new byte[res[4] - 4];
      Array.Copy(res, 28, bValue, 0, res[4] - 4);
      value = encoding.GetString(bValue, 0, bValue.Length);
    }

    /// <summary>
    /// Sends a delete command.
    /// </summary>
    /// <param name="magic"></param>
    /// <param name="opcode"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    private void SendCommand(byte magic, byte opcode, string key )
    {
      // Send data
      byte[] dataToSend = EncodeGetCommand(magic, opcode, key );
      stream.Write(dataToSend, 0, dataToSend.Length);
      byte[] res = GetResponse();
    }

    /// <summary>
    /// Sends a command without args (like flush).
    /// </summary>
    /// <param name="magic"></param>
    /// <param name="opcode"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    private void SendCommand(byte magic, byte opcode, TimeSpan expiration )
    {
      // Send data
      byte[] dataToSend = EncodeFlushCommand(magic, opcode, expiration);
      stream.Write(dataToSend, 0, dataToSend.Length);
      byte[] res = GetResponse();
    }

    /// <summary>
    /// Sends a command with amount (INCR/DECR)
    /// </summary>
    /// <param name="magic"></param>
    /// <param name="opcode"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    private void SendCommand(byte magic, byte opcode, string key, int amount )
    {
      // Send data
      byte[] dataToSend = EncodeIncrCommand(magic, opcode, key, amount);
      stream.Write(dataToSend, 0, dataToSend.Length);
      byte[] res = GetResponse();
    }

    private byte[] GetResponse()
    {
      byte[] response = new byte[24];
      stream.Read(response, 0, response.Length);
      ValidateResponse( response );
      return response;
    }

    private void ValidateResponse(byte[] res)
    {
      // Memcached returns words in big endian.
      ushort status = (ushort)(( res[ 6 ] << 8 ) | res[ 7 ] );
      if( status != 0 )
      {
        throw new MemcachedException(((ResponseStatus)status).ToString());
      }
    }

    /// <summary>
    /// Encodes in the binary protocol the a command of the kind set, add or replace.
    /// </summary>
    /// <param name="magic"></param>
    /// <param name="opcode"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="expiration"></param>
    /// <param name="hasExtra">If true applies to set, add or replace commands; if false applies
    /// to append & prepend commands.</param>
    /// <returns></returns>
    private byte[] EncodeStoreCommand(
      byte magic, byte opcode, string key, object data, TimeSpan expiration, 
      bool hasExtra )
    {
      /*
       * Field        (offset) (value)
    Magic        (0)    : 0x80
    Opcode       (1)    : 0x02
    Key length   (2,3)  : 0x0005
    Extra length (4)    : 0x08
    Data type    (5)    : 0x00
    VBucket      (6,7)  : 0x0000
    Total body   (8-11) : 0x00000012
    Opaque       (12-15): 0x00000000
    CAS          (16-23): 0x0000000000000000
    Extras              :
      Flags      (24-27): 0xdeadbeef
      Expiry     (28-31): 0x00000e10
    Key          (32-36): The textual string "Hello"
    Value        (37-41): The textual string "World"
       * */
      byte[] bKey = encoding.GetBytes(key);
      byte[] bData = encoding.GetBytes(data.ToString());
      MemoryStream ms = new MemoryStream();
      // write magic
      ms.WriteByte(magic);
      // write opcode
      ms.WriteByte(opcode);
      // write keylength
      WriteToMemoryStream(BitConverter.GetBytes((ushort)bKey.Length), ms);
      // write extra length
      ms.WriteByte(8);
      // write data type
      ms.WriteByte(0);
      // write status
      ms.WriteByte(0); ms.WriteByte(0);
      // write total body length
      WriteToMemoryStream(BitConverter.GetBytes((uint)
        (bKey.Length + bData.Length + ( hasExtra? 8 : 0))), ms);
      // write opaque
      WriteToMemoryStream(BitConverter.GetBytes((uint)0), ms);
      // write CAS
      // NOTE: For some reason in the Innodb implementation of Memcached the CAS
      // is 4 bytes long (instead of 8 bytes).
      WriteToMemoryStream(BitConverter.GetBytes((ushort)0), ms);
      // write extras, flags
      if (hasExtra)
      {
        ms.Write(new byte[4], 0, 4);
        WriteToMemoryStream(BitConverter.GetBytes((uint)(expiration.TotalSeconds)), ms);
      }
      // write key
      ms.Write(bKey, 0, bKey.Length);
      // write value
      ms.Write(bData, 0, bData.Length);
      return ms.ToArray();
    }

    private byte[] EncodeGetCommand(byte magic, byte opcode, string key )
    {
      /*
       * Field        (offset) (value)
    Magic        (0)    : 0x80
    Opcode       (1)    : 0x00
    Key length   (2,3)  : 0x0005
    Extra length (4)    : 0x00
    Data type    (5)    : 0x00
    VBucket      (6,7)  : 0x0000
    Total body   (8-11) : 0x00000005
    Opaque       (12-15): 0x00000000
    CAS          (16-23): 0x0000000000000000
    Extras              : None
    Key          (24-29): The textual string: "Hello"
    Value               : None
       * */
      byte[] bKey = encoding.GetBytes(key);
      MemoryStream ms = new MemoryStream();
      // write magic
      ms.WriteByte(magic);
      // write opcode
      ms.WriteByte(opcode);
      // write keylength
      WriteToMemoryStream(BitConverter.GetBytes((ushort)bKey.Length), ms);
      // write extra length
      ms.WriteByte(8);
      // write data type
      ms.WriteByte(0);
      // write status
      ms.WriteByte(0); ms.WriteByte(0);
      // write total body length
      WriteToMemoryStream(BitConverter.GetBytes((ushort)bKey.Length), ms);
      // write opaque
      WriteToMemoryStream(BitConverter.GetBytes((uint)0), ms);
      // write CAS
      WriteToMemoryStream(BitConverter.GetBytes((uint)0), ms);
      // write key
      ms.Write(bKey, 0, bKey.Length);
      return ms.ToArray();
    }

    private byte[] EncodeFlushCommand(byte magic, byte opcode, TimeSpan expiration )
    {
      /*
       *   Field        (offset) (value)
    Magic        (0)    : 0x80
    Opcode       (1)    : 0x08
    Key length   (2,3)  : 0x0000
    Extra length (4)    : 0x04
    Data type    (5)    : 0x00
    VBucket      (6,7)  : 0x0000
    Total body   (8-11) : 0x00000004
    Opaque       (12-15): 0x00000000
    CAS          (16-23): 0x0000000000000000
    Extras              :
       Expiry    (24-27): 0x000e10
    Key                 : None
    Value               : None
       * */
      MemoryStream ms = new MemoryStream();
      // write magic
      ms.WriteByte(magic);
      // write opcode
      ms.WriteByte(opcode);
      // write keylength
      ms.WriteByte(0); ms.WriteByte(0);
      // write extra length
      ms.WriteByte(4);
      // write data type
      ms.WriteByte(0);
      // write status
      ms.WriteByte(0); ms.WriteByte(0);
      // write total body length
      WriteToMemoryStream(BitConverter.GetBytes((ushort)4), ms);
      // write opaque
      WriteToMemoryStream(BitConverter.GetBytes((uint)0), ms);
      // write CAS
      WriteToMemoryStream(BitConverter.GetBytes((uint)0), ms);
      // write extra (flags)
      WriteToMemoryStream(BitConverter.GetBytes((uint)(expiration.TotalSeconds)), ms);
      return ms.ToArray();
    }

    private byte[] EncodeIncrCommand(byte magic, byte opcode, string key, int amount )
    {
      /*
       *    Field        (offset) (value)
    Magic        (0)    : 0x80
    Opcode       (1)    : 0x05
    Key length   (2,3)  : 0x0007
    Extra length (4)    : 0x14
    Data type    (5)    : 0x00
    VBucket      (6,7)  : 0x0000
    Total body   (8-11) : 0x0000001b
    Opaque       (12-15): 0x00000000
    CAS          (16-23): 0x0000000000000000
    Extras              :
      delta      (24-31): 0x0000000000000001
      initial    (32-39): 0x0000000000000000
      exipration (40-43): 0x00000e10
    Key                 : Textual string "counter"
    Value               : None
       * */
      byte[] bKey = encoding.GetBytes(key);
      MemoryStream ms = new MemoryStream();
      // write magic
      ms.WriteByte(magic);
      // write opcode
      ms.WriteByte(opcode);
      // write keylength
      WriteToMemoryStream(BitConverter.GetBytes((ushort)bKey.Length), ms);
      // write extra length
      ms.WriteByte(20);
      // write data type
      ms.WriteByte(0);
      // write status
      ms.WriteByte(0); ms.WriteByte(0);
      // write total body length
      WriteToMemoryStream(BitConverter.GetBytes((ushort)( bKey.Length + 20 )), ms);
      // write opaque
      WriteToMemoryStream(BitConverter.GetBytes((uint)0), ms);
      // write CAS
      WriteToMemoryStream(BitConverter.GetBytes((uint)0), ms);
      // write extra (flags)
      long delta = amount;
      if ((OpCodes)opcode == OpCodes.Decrement)
        delta *= -1;
      WriteToMemoryStream(BitConverter.GetBytes((long)0), ms);
      WriteToMemoryStream(BitConverter.GetBytes((uint)(TimeSpan.Zero.TotalSeconds)), ms);
      // write key
      ms.Write(bKey, 0, bKey.Length);
      return ms.ToArray();
    }

    private void WriteToMemoryStream(byte[] data, MemoryStream ms)
    {
      // .NET runs in x86 which uses little endian, and Memcached runs in big endian.
      Array.Reverse(data);
      ms.Write(data, 0, data.Length);
    }
  }
}
