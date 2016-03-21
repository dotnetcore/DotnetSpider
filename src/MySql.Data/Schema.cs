// Copyright © 2013, 2014 Oracle and/or its affiliates. All rights reserved.
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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MySql.Data.MySqlClient
{
	public class MySqlSchemaCollection
	{
		private List<SchemaColumn> columns = new List<SchemaColumn>();
		private List<MySqlSchemaRow> rows = new List<MySqlSchemaRow>();
 

		public MySqlSchemaCollection()
		{
			Mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			LogicalMappings = new Dictionary<int, int>();
		}

		public MySqlSchemaCollection(string name) : this()
		{
			Name = name;
		}

 

		internal Dictionary<string, int> Mapping;
		internal Dictionary<int, int> LogicalMappings;
		public string Name { get; set; }
		public IList<SchemaColumn> Columns { get { return columns; } }
		public IList<MySqlSchemaRow> Rows { get { return rows; } }

		internal SchemaColumn AddColumn(string name, Type t)
		{
			SchemaColumn c = new SchemaColumn();
			c.Name = name;
			c.Type = t;
			columns.Add(c);
			Mapping.Add(name, columns.Count - 1);
			LogicalMappings[columns.Count - 1] = columns.Count - 1;
			return c;
		}

		internal int ColumnIndex(string name)
		{
			int index = -1;
			for (int i = 0; i < columns.Count; i++)
			{
				SchemaColumn c = columns[i];
				if (String.Compare(c.Name, name, StringComparison.OrdinalIgnoreCase) != 0) continue;
				index = i;
				break;
			}
			return index;
		}

		internal void RemoveColumn(string name)
		{
			int index = ColumnIndex(name);
			if (index == -1)
				throw new InvalidOperationException();
			columns.RemoveAt(index);
			for (int i = index; i < Columns.Count; i++)
				LogicalMappings[i] = LogicalMappings[i] + 1;
		}

		internal bool ContainsColumn(string name)
		{
			return ColumnIndex(name) >= 0;
		}

		internal MySqlSchemaRow AddRow()
		{
			MySqlSchemaRow r = new MySqlSchemaRow(this);
			rows.Add(r);
			return r;
		}

		internal MySqlSchemaRow NewRow()
		{
			MySqlSchemaRow r = new MySqlSchemaRow(this);
			return r;
		}

 
	}

	public class MySqlSchemaRow
	{
		private Dictionary<int, object> data;

		public MySqlSchemaRow(MySqlSchemaCollection c)
		{
			Collection = c;
			InitMetadata();
		}

		internal void InitMetadata()
		{
			data = new Dictionary<int, object>();
		}

		internal MySqlSchemaCollection Collection { get; private set; }

		internal object this[string s]
		{
			get { return GetValueForName(s); }
			set { SetValueForName(s, value); }
		}

		internal object this[int i]
		{
			get
			{
				int idx = Collection.LogicalMappings[i];
				if (!data.ContainsKey(idx))
					data[idx] = null;
				return data[idx];
			}
			set { data[Collection.LogicalMappings[i]] = value; }
		}

		private void SetValueForName(string colName, object value)
		{
			int index = Collection.Mapping[colName];
			this[index] = value;
		}

		private object GetValueForName(string colName)
		{
			int index = Collection.Mapping[colName];
			if (!data.ContainsKey(index))
				data[index] = null;
			return this[index];
		}

		internal void CopyRow(MySqlSchemaRow row)
		{
			if (Collection.Columns.Count != row.Collection.Columns.Count)
				throw new InvalidOperationException("column count doesn't match");
			for (int i = 0; i < Collection.Columns.Count; i++)
				row[i] = this[i];
		}
	}

	public class SchemaColumn
	{
		public string Name { get; set; }
		public Type Type { get; set; }
	}
}
