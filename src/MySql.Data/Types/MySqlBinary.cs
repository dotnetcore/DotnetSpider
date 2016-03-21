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
using System.Data;
using MySql.Data.MySqlClient;

namespace MySql.Data.Types
{

  internal struct MySqlBinary : IMySqlValue
  {
    private MySqlDbType type;
    private byte[] mValue;
    private bool isNull;

    public MySqlBinary(MySqlDbType type, bool isNull)
    {
      this.type = type;
      this.isNull = isNull;
      mValue = null;
    }

    public MySqlBinary(MySqlDbType type, byte[] val)
    {
      this.type = type;
      this.isNull = false;
      mValue = val;
    }

    #region IMySqlValue Members

    public bool IsNull
    {
      get { return isNull; }
    }

    MySqlDbType IMySqlValue.MySqlDbType
    {
      get { return type; }
    }

    object IMySqlValue.Value
    {
      get { return mValue; }
    }

    public byte[] Value
    {
      get { return mValue; }
    }

    Type IMySqlValue.SystemType
    {
      get { return typeof(byte[]); }
    }

    string IMySqlValue.MySqlTypeName
    {
      get
      {
        switch (type)
        {
          case MySqlDbType.TinyBlob: return "TINY_BLOB";
          case MySqlDbType.MediumBlob: return "MEDIUM_BLOB";
          case MySqlDbType.LongBlob: return "LONG_BLOB";
          case MySqlDbType.Blob:
          default:
            return "BLOB";
        }
      }
    }

    void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
    {
      byte[] buffToWrite = (val as byte[]);
      if (buffToWrite == null)
      {
        char[] valAsChar = (val as Char[]);
        if (valAsChar != null)
          buffToWrite = packet.Encoding.GetBytes(valAsChar);
        else
        {
          string s = val.ToString();
          if (length == 0)
            length = s.Length;
          else
            s = s.Substring(0, length);
          buffToWrite = packet.Encoding.GetBytes(s);
        }
      }

      // we assume zero length means write all of the value
      if (length == 0)
        length = buffToWrite.Length;

      if (buffToWrite == null)
        throw new MySqlException("Only byte arrays and strings can be serialized by MySqlBinary");

      if (binary)
      {
        packet.WriteLength(length);
        packet.Write(buffToWrite, 0, length);
      }
      else
      {
        packet.WriteStringNoNull("_binary ");
        packet.WriteByte((byte)'\'');
        EscapeByteArray(buffToWrite, length, packet);
        packet.WriteByte((byte)'\'');
      }
    }

    private static void EscapeByteArray(byte[] bytes, int length, MySqlPacket packet)
    {
      for (int x = 0; x < length; x++)
      {
        byte b = bytes[x];
        if (b == '\0')
        {
          packet.WriteByte((byte)'\\');
          packet.WriteByte((byte)'0');
        }

        else if (b == '\\' || b == '\'' || b == '\"')
        {
          packet.WriteByte((byte)'\\');
          packet.WriteByte(b);
        }
        else
          packet.WriteByte(b);
      }
    }

    IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
    {
      MySqlBinary b;
      if (nullVal)
        b = new MySqlBinary(type, true);
      else
      {
        if (length == -1)
          length = (long)packet.ReadFieldLength();

        byte[] newBuff = new byte[length];
        packet.Read(newBuff, 0, (int)length);
        b = new MySqlBinary(type, newBuff);
      }
      return b;
    }

    void IMySqlValue.SkipValue(MySqlPacket packet)
    {
      int len = (int)packet.ReadFieldLength();
      packet.Position += len;
    }

    #endregion

    public static void SetDSInfo(MySqlSchemaCollection sc)
    {
      string[] types = new string[] { "BLOB", "TINYBLOB", "MEDIUMBLOB", "LONGBLOB", "BINARY", "VARBINARY" };
      MySqlDbType[] dbtype = new MySqlDbType[] { MySqlDbType.Blob, 
                MySqlDbType.TinyBlob, MySqlDbType.MediumBlob, MySqlDbType.LongBlob, MySqlDbType.Binary, MySqlDbType.VarBinary };
      long[] sizes = new long[] { 65535L, 255L, 16777215L, 4294967295L, 255L, 65535L };
      string[] format = new string[] { null, null, null, null, "binary({0})", "varbinary({0})" };
      string[] parms = new string[] { null, null, null, null, "length", "length" };

      // we use name indexing because this method will only be called
      // when GetSchema is called for the DataSourceInformation 
      // collection and then it wil be cached.
      for (int x = 0; x < types.Length; x++)
      {
        MySqlSchemaRow row = sc.AddRow();
        row["TypeName"] = types[x];
        row["ProviderDbType"] = dbtype[x];
        row["ColumnSize"] = sizes[x];
        row["CreateFormat"] = format[x];
        row["CreateParameters"] = parms[x];
        row["DataType"] = "System.Byte[]";
        row["IsAutoincrementable"] = false;
        row["IsBestMatch"] = true;
        row["IsCaseSensitive"] = false;
        row["IsFixedLength"] = x < 4 ? false : true;
        row["IsFixedPrecisionScale"] = false;
        row["IsLong"] = sizes[x] > 255;
        row["IsNullable"] = true;
        row["IsSearchable"] = false;
        row["IsSearchableWithLike"] = false;
        row["IsUnsigned"] = DBNull.Value;
        row["MaximumScale"] = DBNull.Value;
        row["MinimumScale"] = DBNull.Value;
        row["IsConcurrencyType"] = DBNull.Value;
        row["IsLiteralSupported"] = false;
        row["LiteralPrefix"] = "0x";
        row["LiteralSuffix"] = DBNull.Value;
        row["NativeDataType"] = DBNull.Value;
      }
    }
  }
}
