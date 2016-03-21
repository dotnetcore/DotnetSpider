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
using System.IO;
using System.Runtime.InteropServices;

namespace MySql.Data.Common
{
	internal class Platform
	{
		private static bool inited;

		/// <summary>
		/// By creating a private ctor, we keep the compiler from creating a default ctor
		/// </summary>
		private Platform()
		{
		}

		public static bool IsWindows()
		{
			return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		}

		public static char DirectorySeparatorChar
		{
			get
			{ 
				return Path.DirectorySeparatorChar; 
			}
		}


		private static void Init()
		{
			inited = true;
		}
	}
}
