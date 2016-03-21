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

using System;

namespace MySql.Data.Types
{
  [Serializable]
  public partial struct MySqlDateTime : IConvertible
  {
    #region IConvertible Members

    ulong IConvertible.ToUInt64(IFormatProvider provider)
    {
      return 0;
    }

    sbyte IConvertible.ToSByte(IFormatProvider provider)
    {
      // TODO:  Add MySqlDateTime.ToSByte implementation
      return 0;
    }

    double IConvertible.ToDouble(IFormatProvider provider)
    {
      return 0;
    }

    DateTime IConvertible.ToDateTime(IFormatProvider provider)
    {
      return GetDateTime();
    }

    float IConvertible.ToSingle(IFormatProvider provider)
    {
      return 0;
    }

    bool IConvertible.ToBoolean(IFormatProvider provider)
    {
      return false;
    }

    int IConvertible.ToInt32(IFormatProvider provider)
    {
      return 0;
    }

    ushort IConvertible.ToUInt16(IFormatProvider provider)
    {
      return 0;
    }

    short IConvertible.ToInt16(IFormatProvider provider)
    {
      return 0;
    }

    string System.IConvertible.ToString(IFormatProvider provider)
    {
      return null;
    }

    byte IConvertible.ToByte(IFormatProvider provider)
    {
      return 0;
    }

    char IConvertible.ToChar(IFormatProvider provider)
    {
      return '\0';
    }

    long IConvertible.ToInt64(IFormatProvider provider)
    {
      return 0;
    }

    System.TypeCode IConvertible.GetTypeCode()
    {
      return new System.TypeCode();
    }

    decimal IConvertible.ToDecimal(IFormatProvider provider)
    {
      return 0;
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider provider)
    {
      return null;
    }

    uint IConvertible.ToUInt32(IFormatProvider provider)
    {
      return 0;
    }

    #endregion

  }
}
