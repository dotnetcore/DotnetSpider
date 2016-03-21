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
using MySql.Data.MySqlClient;
using System.Globalization;

namespace MySql.Data.Types
{
  internal class MetaData
  {
    public static bool IsNumericType(string typename)
    {
      string lowerType = typename.ToLower( );
      switch (lowerType)
      {
        case "int":
        case "integer":
        case "numeric":
        case "decimal":
        case "dec":
        case "fixed":
        case "tinyint":
        case "mediumint":
        case "bigint":
        case "real":
        case "double":
        case "float":
        case "serial":
        case "smallint": return true;
      }
      return false;
    }

    public static bool IsTextType(string typename)
    {
      string lowerType = typename.ToLower( );
      switch (lowerType)
      {
        case "varchar":
        case "char":
        case "text":
        case "longtext":
        case "tinytext":
        case "mediumtext":
        case "nchar":
        case "nvarchar":
        case "enum":
        case "set":
          return true;
      }
      return false;
    }

    public static bool SupportScale(string typename)
    {
      string lowerType = typename.ToLower ();
      switch (lowerType)
      {
        case "numeric":
        case "decimal":
        case "dec":
        case "real": return true;
      }
      return false;
    }

    public static MySqlDbType NameToType(string typeName, bool unsigned,
       bool realAsFloat, MySqlConnection connection)
    {
      switch (typeName.ToUpperInvariant())
      {
        case "CHAR": return MySqlDbType.String;
        case "VARCHAR": return MySqlDbType.VarChar;
        case "DATE": return MySqlDbType.Date;
        case "DATETIME": return MySqlDbType.DateTime;
        case "NUMERIC":
        case "DECIMAL":
        case "DEC":
        case "FIXED":
          if (connection.driver.Version.isAtLeast(5, 0, 3))
            return MySqlDbType.NewDecimal;
          else
            return MySqlDbType.Decimal;
        case "YEAR":
          return MySqlDbType.Year;
        case "TIME":
          return MySqlDbType.Time;
        case "TIMESTAMP":
          return MySqlDbType.Timestamp;
        case "SET": return MySqlDbType.Set;
        case "ENUM": return MySqlDbType.Enum;
        case "BIT": return MySqlDbType.Bit;

        case "TINYINT":
          return unsigned ? MySqlDbType.UByte : MySqlDbType.Byte;
        case "BOOL":
        case "BOOLEAN":
          return MySqlDbType.Byte;
        case "SMALLINT":
          return unsigned ? MySqlDbType.UInt16 : MySqlDbType.Int16;
        case "MEDIUMINT":
          return unsigned ? MySqlDbType.UInt24 : MySqlDbType.Int24;
        case "INT":
        case "INTEGER":
          return unsigned ? MySqlDbType.UInt32 : MySqlDbType.Int32;
        case "SERIAL":
          return MySqlDbType.UInt64;
        case "BIGINT":
          return unsigned ? MySqlDbType.UInt64 : MySqlDbType.Int64;
        case "FLOAT": return MySqlDbType.Float;
        case "DOUBLE": return MySqlDbType.Double;
        case "REAL": return
           realAsFloat ? MySqlDbType.Float : MySqlDbType.Double;
        case "TEXT":
          return MySqlDbType.Text;
        case "BLOB":
          return MySqlDbType.Blob;
        case "LONGBLOB":
          return MySqlDbType.LongBlob;
        case "LONGTEXT":
          return MySqlDbType.LongText;
        case "MEDIUMBLOB":
          return MySqlDbType.MediumBlob;
        case "MEDIUMTEXT":
          return MySqlDbType.MediumText;
        case "TINYBLOB":
          return MySqlDbType.TinyBlob;
        case "TINYTEXT":
          return MySqlDbType.TinyText;
        case "BINARY":
          return MySqlDbType.Binary;
        case "VARBINARY":
          return MySqlDbType.VarBinary;
      }
      throw new MySqlException("Unhandled type encountered");
    }

  }
}
