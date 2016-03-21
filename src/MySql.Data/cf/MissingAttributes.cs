// Copyright (c) 2009 Sun Microsystems, Inc.
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
#if CF

    class CategoryAttribute : Attribute
    {
        public CategoryAttribute(string cat) { }
    }

    class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string desc) { }
    }

    class DisplayNameAttribute : Attribute
    {
        private string displayName;

        public DisplayNameAttribute(string name)
        {
            displayName = name;
        }

        public string DisplayName
        {
            get { return displayName; }
        }
    }

    class RefreshPropertiesAttribute : Attribute
    {
        public RefreshPropertiesAttribute(RefreshProperties prop) { }
    }

    class PasswordPropertyTextAttribute : Attribute
    {
        public PasswordPropertyTextAttribute(bool v) { }
    }

    public enum RefreshProperties
    {
        None = 0,
        All = 1,
        Repaint = 2,
    }

#endif
}
