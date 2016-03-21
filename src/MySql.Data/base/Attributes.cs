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
using System.ComponentModel;
using System.Text;

namespace System
{
  internal class SerializableAttribute : Attribute
  {
  }
}

namespace System.ComponentModel
{
  internal class BrowsableAttribute : Attribute
  {
    public BrowsableAttribute(bool value)
    {
    }
  }

  internal class CategoryAttribute : Attribute
  {
    public CategoryAttribute(string s)
    {
    }
  }

  internal class DescriptionAttribute : Attribute
  {
    public DescriptionAttribute(string s)
    {
    }
  }

  internal class DisplayNameAttribute : Attribute
  {
    public DisplayNameAttribute(string s)
    {
      DisplayName = s;
    }

    public virtual string DisplayName { get; private set; }
  }

  internal class PasswordPropertyTextAttribute : Attribute
  {
    public PasswordPropertyTextAttribute(bool b)
    {
    }
  }

  internal class EditorAttribute : Attribute
  {
    public EditorAttribute(string s, Type t)
    {
    }
  }

  internal class RefreshPropertiesAttribute : Attribute
  {
    public RefreshPropertiesAttribute(RefreshProperties e)
    {
    }
  }

  internal class DesignerSerializationVisibilityAttribute : Attribute
  {
    public DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility x)
    {
    }
  }

  internal class ListBindableAttribute : Attribute
  {
    public ListBindableAttribute(bool b)
    {
    }
  }

  internal class ToolboxBitmapAttribute : Attribute
  {
    public ToolboxBitmapAttribute(Type t, string s)
    {
    }
  }

  internal class DesignerCategoryAttribute : Attribute
  {
    public DesignerCategoryAttribute(string s)
    {
    }
  }

  internal class ToolboxItemAttribute : Attribute
  {
    public ToolboxItemAttribute(bool b)
    {
    }
  }

  internal class DbProviderSpecificTypePropertyAttribute : Attribute
  {
    public DbProviderSpecificTypePropertyAttribute(bool b)
    {
    }
  }

#if CF
  internal  class TypeConverterAttribute : Attribute
  {
    public TypeConverterAttribute( Type t )
    {
    }
  }
#endif
}
