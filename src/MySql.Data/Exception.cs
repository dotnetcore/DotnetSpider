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
 

namespace MySql.Data.MySqlClient
{
  /// <summary>
  /// The exception that is thrown when MySQL returns an error. This class cannot be inherited.
  /// </summary>
  /// <include file='docs/MySqlException.xml' path='MyDocs/MyMembers[@name="Class"]/*'/>
 
  public sealed class MySqlException : DbException
  {
    private int errorCode;
    private bool isFatal;

    internal MySqlException()
    {
    }

    internal MySqlException(string msg)
      : base(msg)
    {
    }

    internal MySqlException(string msg, Exception ex)
      : base(msg, ex)
    {
    }

    internal MySqlException(string msg, bool isFatal, Exception inner)
      : base(msg, inner)
    {
      this.isFatal = isFatal;
    }

    internal MySqlException(string msg, int errno, Exception inner)
      : this(msg, inner)
    {
      errorCode = errno;
#if !CF
      Data.Add("Server Error Code", errno);
#endif
    }

    internal MySqlException(string msg, int errno)
      : this(msg, errno, null)
    {
    }

 

    /// <summary>
    /// Gets a number that identifies the type of error.
    /// </summary>
    public int Number
    {
      get { return errorCode; }
    }

    /// <summary>
    /// True if this exception was fatal and cause the closing of the connection, false otherwise.
    /// </summary>
    internal bool IsFatal
    {
      get { return isFatal; }
    }

    internal bool IsQueryAborted
    {
      get
      {
        return (errorCode == (int)MySqlErrorCode.QueryInterrupted ||
          errorCode == (int)MySqlErrorCode.FileSortAborted);
      }
    }
  }
}
