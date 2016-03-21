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


namespace MySql.Data.Common
{
  /// <summary>
  /// Summary description for Version.
  /// </summary>
  internal struct DBVersion
  {
    private int major;
    private int minor;
    private int build;
    private string srcString;

    public DBVersion(string s, int major, int minor, int build)
    {
      this.major = major;
      this.minor = minor;
      this.build = build;
      srcString = s;
    }

    public int Major
    {
      get { return major; }
    }

    public int Minor
    {
      get { return minor; }
    }

    public int Build
    {
      get { return build; }
    }

    public static DBVersion Parse(string versionString)
    {
      int start = 0;
      int index = versionString.IndexOf('.', start);
      if (index == -1)
        throw new MySqlException("BadVersionFormat");
      string val = versionString.Substring(start, index - start).Trim();
      int major = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

      start = index + 1;
      index = versionString.IndexOf('.', start);
      if (index == -1)
        throw new MySqlException("BadVersionFormat");
      val = versionString.Substring(start, index - start).Trim();
      int minor = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

      start = index + 1;
      int i = start;
      while (i < versionString.Length && Char.IsDigit(versionString, i))
        i++;
      val = versionString.Substring(start, i - start).Trim();
      int build = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

      return new DBVersion(versionString, major, minor, build);
    }

    public bool isAtLeast(int majorNum, int minorNum, int buildNum)
    {
      if (major > majorNum) return true;
      if (major == majorNum && minor > minorNum) return true;
      if (major == majorNum && minor == minorNum && build >= buildNum) return true;
      return false;
    }

    public override string ToString()
    {
      return srcString;
    }

  }
}
