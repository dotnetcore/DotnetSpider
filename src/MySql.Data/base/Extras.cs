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
using System.Collections.Generic;
 
using System.Text;

namespace System.Drawing.Design
{
  internal class UITypeEditor
  {
  }
}

namespace System.ComponentModel
{
#if CF
  internal interface ITypeDescriptorContext { }

  internal class InstanceDescriptor { }
#endif
}

#if CF
namespace System
{
  internal static class TryParseUtility
  {
    internal static bool TryParse(string s, out bool value)
    {
      try
      {
        value = bool.Parse(s);
        return true;
      }
      catch (FormatException fe)
      {
      }
      catch (InvalidCastException ice)
      { 
      }
      value = false;
      return false;
    }

    internal static bool TryParse(string s, out int value)
    {
      try
      {
        value = int.Parse(s);
        return true;
      }
      catch (FormatException fe)
      {
      }
      catch (InvalidCastException ice)
      {
      }
      value = 0;
      return false;
    }

    internal static bool TryParse(string s, out uint value)
    {
      try
      {
        value = uint.Parse(s);
        return true;
      }
      catch (FormatException fe)
      {
      }
      catch (InvalidCastException ice)
      {
      }
      value = 0;
      return false;
    }

    internal static bool TryParse(string s, out long value)
    {
      try
      {
        value = long.Parse(s);
        return true;
      }
      catch (FormatException fe)
      {
      }
      catch (InvalidCastException ice)
      {
      }
      value = 0;
      return false;
    }

    internal static bool TryParse(string s, out ulong value)
    {
      try
      {
        value = ulong.Parse(s);
        return true;
      }
      catch (FormatException fe)
      {
      }
      catch (InvalidCastException ice)
      {
      }
      value = 0;
      return false;
    }
  }
}
#endif
