// Copyright © 2004, 2013, Oracle and/or its affiliates. All rights reserved.
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
  /// Implementation of the Memcached text client protocol.
  /// </summary>
  public class TextClient : Client
  {
    private Encoding encoding;

    private static readonly string PROTOCOL_ADD = "add";
    private static readonly string PROTOCOL_APPEND = "append";
    private static readonly string PROTOCOL_CAS = "cas";
    private static readonly string PROTOCOL_DECREMENT = "decr";
    private static readonly string PROTOCOL_DELETE = "delete";
    private static readonly string PROTOCOL_FLUSHALL = "flush_all";
    private static readonly string PROTOCOL_GET = "get";
    private static readonly string PROTOCOL_GETS = "gets";
    private static readonly string PROTOCOL_INCREMENT = "incr";
    private static readonly string PROTOCOL_PREPEND = "prepend";
    private static readonly string PROTOCOL_REPLACE = "replace";
    private static readonly string PROTOCOL_SET = "set";

    private static readonly string VALUE = "VALUE";
    private static readonly string END = "END";
    // Errors
    private static readonly string ERR_ERROR = "ERROR";
    private static readonly string ERR_CLIENT_ERROR = "CLIENT_ERROR";
    private static readonly string ERR_SERVER_ERROR = "SERVER_ERROR";

    internal protected TextClient( string server, uint port ) : base( server, port )
    {
      encoding = Encoding.UTF8;
    }

    #region Memcached protocol interface

    public override void Add(string key, object data, TimeSpan expiration)
    {
      SendCommand(PROTOCOL_ADD, key, data, expiration);
    }

    public override void Append(string key, object data )
    {
      SendCommand(PROTOCOL_APPEND, key, data);
    }

    public override void Cas(string key, object data, TimeSpan expiration, ulong casUnique)
    {
      SendCommand(PROTOCOL_CAS, key, data, expiration, casUnique);
    }

    public override void Decrement(string key, int amount)
    {
      SendCommand(PROTOCOL_DECREMENT, key, amount);
    }

    public override void Delete(string key)
    {
      SendCommand(PROTOCOL_DELETE, key);
    }

    public override void FlushAll(TimeSpan delay)
    {
      SendCommand(PROTOCOL_FLUSHALL, delay);
    }

    public override KeyValuePair<string, object> Get(string key)
    {
      KeyValuePair<string, object>[] kvp = Gets(key);
      if (kvp.Length == 0)
        throw new MemcachedException("Item does not exists.");
      else
        return kvp[0];
    }

    private KeyValuePair<string, object>[] Gets(params string[] keys) 
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(string.Format("{0}", PROTOCOL_GETS));
      for (int i = 0; i < keys.Length; i++)
      {
        sb.Append(string.Format(" {0}", keys[i]));
      }
      sb.Append("\r\n");
      SendData(sb.ToString());
      byte[] res = GetResponse();
      return ParseGetResponse(res);
    }

    public override void Increment(string key, int amount)
    {
      SendCommand(PROTOCOL_INCREMENT, key, amount);
    }

    public override void Prepend(string key, object data)
    {
      SendCommand(PROTOCOL_PREPEND, key, data);
    }

    public override void Replace(string key, object data, TimeSpan expiration)
    {
      SendCommand(PROTOCOL_REPLACE, key, data, expiration);
    }

    public override void Set(string key, object data, TimeSpan expiration)
    {
      SendCommand(PROTOCOL_SET, key, data, expiration);
    }

    #endregion

    #region Support methods


    /// <summary>
    /// Sends a command to the memcached server.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="expiration"></param>
    /// <remarks>This version is for commands that take a key, data, expiration & casUnique.</remarks>
    private void SendCommand(string cmd, string key, object data, TimeSpan expiration, ulong casUnique)
    {
      StringBuilder sb = new StringBuilder();
      // set key flags exptime 
      sb.Append(string.Format("{0} {1} 0 {2} ", cmd, key, (int)(expiration.TotalSeconds)));
      byte[] buf = encoding.GetBytes(data.ToString());
      string s = encoding.GetString(buf, 0, buf.Length);
      sb.Append(s.Length.ToString());
      sb.AppendFormat(" {0}", casUnique);
      sb.Append("\r\n");
      sb.Append(s);
      sb.Append("\r\n");
      SendData(sb.ToString());
      GetResponse();
    }

    /// <summary>
    /// Sends a command to the memcached server.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="expiration"></param>
    /// <remarks>This version is for commands that take a key, data & expiration</remarks>
    private void SendCommand(string cmd, string key, object data, TimeSpan expiration)
    {
      StringBuilder sb = new StringBuilder();
      // set key flags exptime 
      sb.Append(string.Format("{0} {1} 0 {2} ", cmd, key, (int)(expiration.TotalSeconds)));
      byte[] buf = encoding.GetBytes(data.ToString());
      string s = encoding.GetString(buf, 0, buf.Length);
      sb.Append(s.Length.ToString());
      sb.Append("\r\n");
      sb.Append(s);
      sb.Append("\r\n");
      SendData(sb.ToString());
      GetResponse();
    }

    /// <summary>
    /// Send a command to memcached server.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="?"></param>
    /// <remarks>This version is for commands that don't need flags neither expiration fields.</remarks>
    private void SendCommand(string cmd, string key, object data )
    {
      StringBuilder sb = new StringBuilder();
      // set key 
      sb.Append(string.Format("{0} {1} ", cmd, key ));
      byte[] buf = encoding.GetBytes(data.ToString());
      string s = encoding.GetString(buf, 0, buf.Length);
      if ((cmd == PROTOCOL_APPEND) || (cmd == PROTOCOL_PREPEND))
      {
        sb.Append("0 0 ");
      }
      sb.Append(s.Length.ToString());
      sb.Append("\r\n");
      sb.Append(s);
      sb.Append("\r\n");
      SendData(sb.ToString());
      GetResponse();
    }

    /// <summary>
    /// Sends a command to the server.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="key"></param>
    /// <remarks>This version is for commands that only require a key</remarks>
    private void SendCommand(string cmd, string key )
    {
      StringBuilder sb = new StringBuilder();
      // set key 
      sb.Append(string.Format("{0} {1} ", cmd, key ));      
      sb.Append("\r\n");
      SendData(sb.ToString());
      GetResponse();
    }

    /// <summary>
    /// Sends a command to the server.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="key"></param>
    /// <remarks>This version is for commands that only require a key & an integer value.</remarks>
    private void SendCommand(string cmd, string key, int amount )
    {
      StringBuilder sb = new StringBuilder();
      // set key 
      sb.Append(string.Format("{0} {1} {2}", cmd, key, amount));
      sb.Append("\r\n");
      SendData(sb.ToString());
      GetResponse();
    }

    /// <summary>
    /// Sends a command to the server.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="key"></param>
    /// <param name="expiration"></param>
    /// <remarks>This version is for commands that only require a key & expiration.</remarks>
    private void SendCommand(string cmd, TimeSpan expiration)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(string.Format("{0} {1}\r\n", PROTOCOL_FLUSHALL, expiration.TotalSeconds));
      SendData(sb.ToString());
      GetResponse();
    }

    private void ValidateErrorResponse(byte[] res)
    {
      string s = encoding.GetString(res, 0, res.Length);
      if ((s.StartsWith(ERR_ERROR, StringComparison.OrdinalIgnoreCase)) ||
          (s.StartsWith(ERR_CLIENT_ERROR, StringComparison.OrdinalIgnoreCase)) ||
          (s.StartsWith(ERR_SERVER_ERROR, StringComparison.OrdinalIgnoreCase)))
      {
        throw new MemcachedException(s);
      }
    }

    private void SendData(string sData)
    {
      byte[] data = encoding.GetBytes(sData);
      stream.Write(data, 0, data.Length);
    }

    private KeyValuePair<string, object>[] ParseGetResponse(byte[] input)
    {
      // VALUE key2 10 9 2\r\n111222333\r\nEND\r\n
      string[] sInput = encoding.GetString(input, 0, input.Length).Split(new string[] { "\r\n" }, StringSplitOptions.None);
      List<KeyValuePair<string, object>> l = new List<KeyValuePair<string, object>>();
      int i = 0;
      string key = "";
      KeyValuePair<string, object> kvp;
      while ((sInput[i] != END) && (i < sInput.Length))
      {
        if (sInput[i].StartsWith(VALUE, StringComparison.OrdinalIgnoreCase))
        {
          key = sInput[i].Split(' ')[1];
        }
        else
        {
          kvp = new KeyValuePair<string, object>(key, sInput[i]);
          l.Add(kvp);
        }
        i++;
      }
      return l.ToArray();
    }

    private byte[] GetResponse()
    {
      byte[] res = new byte[1024];
      MemoryStream ms = new MemoryStream();
      int cnt = stream.Read(res, 0, 1024);
      while (cnt > 0)
      {
        ms.Write(res, 0, cnt);
        if (cnt < 1024) break;
        cnt = stream.Read(res, 0, 1024);
      }
      byte[] res2 = ms.ToArray();
      ValidateErrorResponse(res2);
      return res2;
    }

    #endregion
  }
}
