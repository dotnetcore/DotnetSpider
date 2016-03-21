// Copyright ?2004, 2015, Oracle and/or its affiliates. All rights reserved.
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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using MySql.Data.Common;
using MySql.Data.Types;
using System.Collections.Specialized;
using System.Collections;
using System.Text.RegularExpressions;

using System.Collections.Generic;
#if !RT
using System.Data;
using System.Data.Common;
#endif

namespace MySql.Data.MySqlClient
{
	internal class SchemaProvider
	{
		protected MySqlConnection connection;
		public static string MetaCollection = "MetaDataCollections";

		public SchemaProvider(MySqlConnection connectionToUse)
		{
			connection = connectionToUse;
		}

		public virtual MySqlSchemaCollection GetSchema(string collection, String[] restrictions)
		{
			if (connection.State != ConnectionState.Open)
				throw new MySqlException("GetSchema can only be called on an open connection.");

			collection = collection.ToUpper();

			MySqlSchemaCollection c = GetSchemaInternal(collection, restrictions);

			if (c == null)
				throw new ArgumentException("Invalid collection name");
			return c;
		}

		public virtual MySqlSchemaCollection GetDatabases(string[] restrictions)
		{
			Regex regex = null;
			int caseSetting = Int32.Parse(connection.driver.Property("lower_case_table_names"));

			string sql = "SHOW DATABASES";

			// if lower_case_table_names is zero, then case lookup should be sensitive
			// so we can use LIKE to do the matching.
			if (caseSetting == 0)
			{
				if (restrictions != null && restrictions.Length >= 1)
					sql = sql + " LIKE '" + restrictions[0] + "'";
			}

			MySqlSchemaCollection c = QueryCollection("Databases", sql);

			if (caseSetting != 0 && restrictions != null && restrictions.Length >= 1 && restrictions[0] != null)
				regex = new Regex(restrictions[0], RegexOptions.IgnoreCase);

			MySqlSchemaCollection c2 = new MySqlSchemaCollection("Databases");
			c2.AddColumn("CATALOG_NAME", typeof(string));
			c2.AddColumn("SCHEMA_NAME", typeof(string));

			foreach (MySqlSchemaRow row in c.Rows)
			{
				if (regex != null && !regex.Match(row[0].ToString()).Success) continue;
				MySqlSchemaRow newRow = c2.AddRow();
				newRow[1] = row[0];
			}
			return c2;
		}

		public virtual MySqlSchemaCollection GetTables(string[] restrictions)
		{
			MySqlSchemaCollection c = new MySqlSchemaCollection("Tables");
			c.AddColumn("TABLE_CATALOG", typeof(string));
			c.AddColumn("TABLE_SCHEMA", typeof(string));
			c.AddColumn("TABLE_NAME", typeof(string));
			c.AddColumn("TABLE_TYPE", typeof(string));
			c.AddColumn("ENGINE", typeof(string));
			c.AddColumn("VERSION", typeof(ulong));
			c.AddColumn("ROW_FORMAT", typeof(string));
			c.AddColumn("TABLE_ROWS", typeof(ulong));
			c.AddColumn("AVG_ROW_LENGTH", typeof(ulong));
			c.AddColumn("DATA_LENGTH", typeof(ulong));
			c.AddColumn("MAX_DATA_LENGTH", typeof(ulong));
			c.AddColumn("INDEX_LENGTH", typeof(ulong));
			c.AddColumn("DATA_FREE", typeof(ulong));
			c.AddColumn("AUTO_INCREMENT", typeof(ulong));
			c.AddColumn("CREATE_TIME", typeof(DateTime));
			c.AddColumn("UPDATE_TIME", typeof(DateTime));
			c.AddColumn("CHECK_TIME", typeof(DateTime));
			c.AddColumn("TABLE_COLLATION", typeof(string));
			c.AddColumn("CHECKSUM", typeof(ulong));
			c.AddColumn("CREATE_OPTIONS", typeof(string));
			c.AddColumn("TABLE_COMMENT", typeof(string));

			// we have to new up a new restriction array here since
			// GetDatabases takes the database in the first slot
			string[] dbRestriction = new string[4];
			if (restrictions != null && restrictions.Length >= 2)
				dbRestriction[0] = restrictions[1];
			MySqlSchemaCollection databases = GetDatabases(dbRestriction);

			if (restrictions != null)
				Array.Copy(restrictions, dbRestriction,
					   Math.Min(dbRestriction.Length, restrictions.Length));

			foreach (MySqlSchemaRow row in databases.Rows)
			{
				dbRestriction[1] = row["SCHEMA_NAME"].ToString();
				FindTables(c, dbRestriction);
			}
			return c;
		}

		protected void QuoteDefaultValues(MySqlSchemaCollection schemaCollection)
		{
			if (schemaCollection == null) return;
			if (!schemaCollection.ContainsColumn("COLUMN_DEFAULT")) return;

			foreach (MySqlSchemaRow row in schemaCollection.Rows)
			{
				object defaultValue = row["COLUMN_DEFAULT"];
				if (MetaData.IsTextType(row["DATA_TYPE"].ToString()))
					row["COLUMN_DEFAULT"] = String.Format("{0}", defaultValue);
			}
		}

		public virtual MySqlSchemaCollection GetColumns(string[] restrictions)
		{
			MySqlSchemaCollection c = new MySqlSchemaCollection("Columns");
			c.AddColumn("TABLE_CATALOG", typeof(string));
			c.AddColumn("TABLE_SCHEMA", typeof(string));
			c.AddColumn("TABLE_NAME", typeof(string));
			c.AddColumn("COLUMN_NAME", typeof(string));
			c.AddColumn("ORDINAL_POSITION", typeof(ulong));
			c.AddColumn("COLUMN_DEFAULT", typeof(string));
			c.AddColumn("IS_NULLABLE", typeof(string));
			c.AddColumn("DATA_TYPE", typeof(string));
			c.AddColumn("CHARACTER_MAXIMUM_LENGTH", typeof(ulong));
			c.AddColumn("CHARACTER_OCTET_LENGTH", typeof(ulong));
			c.AddColumn("NUMERIC_PRECISION", typeof(ulong));
			c.AddColumn("NUMERIC_SCALE", typeof(ulong));
			c.AddColumn("CHARACTER_SET_NAME", typeof(string));
			c.AddColumn("COLLATION_NAME", typeof(string));
			c.AddColumn("COLUMN_TYPE", typeof(string));
			c.AddColumn("COLUMN_KEY", typeof(string));
			c.AddColumn("EXTRA", typeof(string));
			c.AddColumn("PRIVILEGES", typeof(string));
			c.AddColumn("COLUMN_COMMENT", typeof(string));
			c.AddColumn("GENERATION_EXPRESSION", typeof(string));

			// we don't allow restricting on table type here
			string columnName = null;
			if (restrictions != null && restrictions.Length == 4)
			{
				columnName = restrictions[3];
				restrictions[3] = null;
			}
			MySqlSchemaCollection tables = GetTables(restrictions);

			foreach (MySqlSchemaRow row in tables.Rows)
				LoadTableColumns(c, row["TABLE_SCHEMA"].ToString(),
						 row["TABLE_NAME"].ToString(), columnName);

			QuoteDefaultValues(c);
			return c;
		}

		private void LoadTableColumns(MySqlSchemaCollection schemaCollection, string schema,
						string tableName, string columnRestriction)
		{
			string sql = String.Format("SHOW FULL COLUMNS FROM `{0}`.`{1}`",
						   schema, tableName);
			MySqlCommand cmd = new MySqlCommand(sql, connection);

			int pos = 1;
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					string colName = reader.GetString(0);
					if (columnRestriction != null && colName != columnRestriction)
						continue;
					MySqlSchemaRow row = schemaCollection.AddRow();
					row["TABLE_CATALOG"] = DBNull.Value;
					row["TABLE_SCHEMA"] = schema;
					row["TABLE_NAME"] = tableName;
					row["COLUMN_NAME"] = colName;
					row["ORDINAL_POSITION"] = pos++;
					row["COLUMN_DEFAULT"] = reader.GetValue(5);
					row["IS_NULLABLE"] = reader.GetString(3);
					row["DATA_TYPE"] = reader.GetString(1);
					row["CHARACTER_MAXIMUM_LENGTH"] = DBNull.Value;
					row["CHARACTER_OCTET_LENGTH"] = DBNull.Value;
					row["NUMERIC_PRECISION"] = DBNull.Value;
					row["NUMERIC_SCALE"] = DBNull.Value;
					row["CHARACTER_SET_NAME"] = reader.GetValue(2);
					row["COLLATION_NAME"] = row["CHARACTER_SET_NAME"];
					row["COLUMN_TYPE"] = reader.GetString(1);
					row["COLUMN_KEY"] = reader.GetString(4);
					row["EXTRA"] = reader.GetString(6);
					row["PRIVILEGES"] = reader.GetString(7);
					row["COLUMN_COMMENT"] = reader.GetString(8);
#if !CF && !RT
					row["GENERATION_EXPRESION"] = reader.GetString(6).Contains("VIRTUAL") ? reader.GetString(9) : string.Empty;
#endif
					ParseColumnRow(row);
				}
			}
		}

		private static void ParseColumnRow(MySqlSchemaRow row)
		{
			// first parse the character set name
			string charset = row["CHARACTER_SET_NAME"].ToString();
			int index = charset.IndexOf('_');
			if (index != -1)
				row["CHARACTER_SET_NAME"] = charset.Substring(0, index);

			// now parse the data type
			string dataType = row["DATA_TYPE"].ToString();
			index = dataType.IndexOf('(');
			if (index == -1)
				return;
			row["DATA_TYPE"] = dataType.Substring(0, index);
			int stop = dataType.IndexOf(')', index);
			string dataLen = dataType.Substring(index + 1, stop - (index + 1));
			string lowerType = row["DATA_TYPE"].ToString().ToLower();
			if (lowerType == "char" || lowerType == "varchar")
				row["CHARACTER_MAXIMUM_LENGTH"] = dataLen;
			else if (lowerType == "real" || lowerType == "decimal")
			{
				string[] lenparts = dataLen.Split(new char[] { ',' });
				row["NUMERIC_PRECISION"] = lenparts[0];
				if (lenparts.Length == 2)
					row["NUMERIC_SCALE"] = lenparts[1];
			}
		}

		public virtual MySqlSchemaCollection GetIndexes(string[] restrictions)
		{
			MySqlSchemaCollection dt = new MySqlSchemaCollection("Indexes");
			dt.AddColumn("INDEX_CATALOG", typeof(string));
			dt.AddColumn("INDEX_SCHEMA", typeof(string));
			dt.AddColumn("INDEX_NAME", typeof(string));
			dt.AddColumn("TABLE_NAME", typeof(string));
			dt.AddColumn("UNIQUE", typeof(bool));
			dt.AddColumn("PRIMARY", typeof(bool));
			dt.AddColumn("TYPE", typeof(string));
			dt.AddColumn("COMMENT", typeof(string));

			// Get the list of tables first
			int max = restrictions == null ? 4 : restrictions.Length;
			string[] tableRestrictions = new string[Math.Max(max, 4)];
			if (restrictions != null)
				restrictions.CopyTo(tableRestrictions, 0);
			tableRestrictions[3] = "BASE TABLE";
			MySqlSchemaCollection tables = GetTables(tableRestrictions);

			foreach (MySqlSchemaRow table in tables.Rows)
			{
				string sql = String.Format("SHOW INDEX FROM `{0}`.`{1}`",
				  MySqlHelper.DoubleQuoteString((string)table["TABLE_SCHEMA"]),
				  MySqlHelper.DoubleQuoteString((string)table["TABLE_NAME"]));
				MySqlSchemaCollection indexes = QueryCollection("indexes", sql);

				foreach (MySqlSchemaRow index in indexes.Rows)
				{
					long seq_index = (long)index["SEQ_IN_INDEX"];
					if (seq_index != 1) continue;
					if (restrictions != null && restrictions.Length == 4 &&
					  restrictions[3] != null &&
					  !index["KEY_NAME"].Equals(restrictions[3])) continue;
					MySqlSchemaRow row = dt.AddRow();
					row["INDEX_CATALOG"] = null;
					row["INDEX_SCHEMA"] = table["TABLE_SCHEMA"];
					row["INDEX_NAME"] = index["KEY_NAME"];
					row["TABLE_NAME"] = index["TABLE"];
					row["UNIQUE"] = (long)index["NON_UNIQUE"] == 0;
					row["PRIMARY"] = index["KEY_NAME"].Equals("PRIMARY");
					row["TYPE"] = index["INDEX_TYPE"];
					row["COMMENT"] = index["COMMENT"];
				}
			}

			return dt;
		}

		public virtual MySqlSchemaCollection GetIndexColumns(string[] restrictions)
		{
			MySqlSchemaCollection dt = new MySqlSchemaCollection("IndexColumns");
			dt.AddColumn("INDEX_CATALOG", typeof(string));
			dt.AddColumn("INDEX_SCHEMA", typeof(string));
			dt.AddColumn("INDEX_NAME", typeof(string));
			dt.AddColumn("TABLE_NAME", typeof(string));
			dt.AddColumn("COLUMN_NAME", typeof(string));
			dt.AddColumn("ORDINAL_POSITION", typeof(int));
			dt.AddColumn("SORT_ORDER", typeof(string));

			int max = restrictions == null ? 4 : restrictions.Length;
			string[] tableRestrictions = new string[Math.Max(max, 4)];
			if (restrictions != null)
				restrictions.CopyTo(tableRestrictions, 0);
			tableRestrictions[3] = "BASE TABLE";
			MySqlSchemaCollection tables = GetTables(tableRestrictions);

			foreach (MySqlSchemaRow table in tables.Rows)
			{
				string sql = String.Format("SHOW INDEX FROM `{0}`.`{1}`",
							   table["TABLE_SCHEMA"], table["TABLE_NAME"]);
				MySqlCommand cmd = new MySqlCommand(sql, connection);
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string key_name = GetString(reader, reader.GetOrdinal("KEY_NAME"));
						string col_name = GetString(reader, reader.GetOrdinal("COLUMN_NAME"));

						if (restrictions != null)
						{
							if (restrictions.Length >= 4 && restrictions[3] != null &&
							  key_name != restrictions[3]) continue;
							if (restrictions.Length >= 5 && restrictions[4] != null &&
							  col_name != restrictions[4]) continue;
						}
						MySqlSchemaRow row = dt.AddRow();
						row["INDEX_CATALOG"] = null;
						row["INDEX_SCHEMA"] = table["TABLE_SCHEMA"];
						row["INDEX_NAME"] = key_name;
						row["TABLE_NAME"] = GetString(reader, reader.GetOrdinal("TABLE"));
						row["COLUMN_NAME"] = col_name;
						row["ORDINAL_POSITION"] = reader.GetValue(reader.GetOrdinal("SEQ_IN_INDEX"));
						row["SORT_ORDER"] = reader.GetString("COLLATION");
					}
				}
			}

			return dt;
		}

		public virtual MySqlSchemaCollection GetForeignKeys(string[] restrictions)
		{
			MySqlSchemaCollection dt = new MySqlSchemaCollection("Foreign Keys");
			dt.AddColumn("CONSTRAINT_CATALOG", typeof(string));
			dt.AddColumn("CONSTRAINT_SCHEMA", typeof(string));
			dt.AddColumn("CONSTRAINT_NAME", typeof(string));
			dt.AddColumn("TABLE_CATALOG", typeof(string));
			dt.AddColumn("TABLE_SCHEMA", typeof(string));
			dt.AddColumn("TABLE_NAME", typeof(string));
			dt.AddColumn("MATCH_OPTION", typeof(string));
			dt.AddColumn("UPDATE_RULE", typeof(string));
			dt.AddColumn("DELETE_RULE", typeof(string));
			dt.AddColumn("REFERENCED_TABLE_CATALOG", typeof(string));
			dt.AddColumn("REFERENCED_TABLE_SCHEMA", typeof(string));
			dt.AddColumn("REFERENCED_TABLE_NAME", typeof(string));

			// first we use our restrictions to get a list of tables that should be
			// consulted.  We save the keyname restriction since GetTables doesn't 
			// understand that.
			string keyName = null;
			if (restrictions != null && restrictions.Length >= 4)
			{
				keyName = restrictions[3];
				restrictions[3] = null;
			}

			MySqlSchemaCollection tables = GetTables(restrictions);

			// now for each table retrieved, we call our helper function to
			// parse it's foreign keys
			foreach (MySqlSchemaRow table in tables.Rows)
				GetForeignKeysOnTable(dt, table, keyName, false);

			return dt;
		}

		public virtual MySqlSchemaCollection GetForeignKeyColumns(string[] restrictions)
		{
			MySqlSchemaCollection dt = new MySqlSchemaCollection("Foreign Keys");
			dt.AddColumn("CONSTRAINT_CATALOG", typeof(string));
			dt.AddColumn("CONSTRAINT_SCHEMA", typeof(string));
			dt.AddColumn("CONSTRAINT_NAME", typeof(string));
			dt.AddColumn("TABLE_CATALOG", typeof(string));
			dt.AddColumn("TABLE_SCHEMA", typeof(string));
			dt.AddColumn("TABLE_NAME", typeof(string));
			dt.AddColumn("COLUMN_NAME", typeof(string));
			dt.AddColumn("ORDINAL_POSITION", typeof(int));
			dt.AddColumn("REFERENCED_TABLE_CATALOG", typeof(string));
			dt.AddColumn("REFERENCED_TABLE_SCHEMA", typeof(string));
			dt.AddColumn("REFERENCED_TABLE_NAME", typeof(string));
			dt.AddColumn("REFERENCED_COLUMN_NAME", typeof(string));

			// first we use our restrictions to get a list of tables that should be
			// consulted.  We save the keyname restriction since GetTables doesn't 
			// understand that.
			string keyName = null;
			if (restrictions != null && restrictions.Length >= 4)
			{
				keyName = restrictions[3];
				restrictions[3] = null;
			}

			MySqlSchemaCollection tables = GetTables(restrictions);

			// now for each table retrieved, we call our helper function to
			// parse it's foreign keys
			foreach (MySqlSchemaRow table in tables.Rows)
				GetForeignKeysOnTable(dt, table, keyName, true);
			return dt;
		}


		private string GetSqlMode()
		{
			MySqlCommand cmd = new MySqlCommand("SELECT @@SQL_MODE", connection);
			return cmd.ExecuteScalar().ToString();
		}

		#region Foreign Key routines

		/// <summary>
		/// GetForeignKeysOnTable retrieves the foreign keys on the given table.
		/// Since MySQL supports foreign keys on versions prior to 5.0, we can't  use
		/// information schema.  MySQL also does not include any type of SHOW command
		/// for foreign keys so we have to resort to use SHOW CREATE TABLE and parsing
		/// the output.
		/// </summary>
		/// <param name="fkTable">The table to store the key info in.</param>
		/// <param name="tableToParse">The table to get the foeign key info for.</param>
		/// <param name="filterName">Only get foreign keys that match this name.</param>
		/// <param name="includeColumns">Should column information be included in the table.</param>
		private void GetForeignKeysOnTable(MySqlSchemaCollection fkTable, MySqlSchemaRow tableToParse,
						   string filterName, bool includeColumns)
		{
			string sqlMode = GetSqlMode();

			if (filterName != null)
				filterName = filterName.ToLower();

			string sql = string.Format("SHOW CREATE TABLE `{0}`.`{1}`",
						   tableToParse["TABLE_SCHEMA"], tableToParse["TABLE_NAME"]);
			string lowerBody = null, body = null;
			MySqlCommand cmd = new MySqlCommand(sql, connection);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				body = reader.GetString(1);
				lowerBody = body.ToLower();
			}

			MySqlTokenizer tokenizer = new MySqlTokenizer(lowerBody);
			tokenizer.AnsiQuotes = sqlMode.IndexOf("ANSI_QUOTES") != -1;
			tokenizer.BackslashEscapes = sqlMode.IndexOf("NO_BACKSLASH_ESCAPES") != -1;

			while (true)
			{
				string token = tokenizer.NextToken();
				// look for a starting contraint
				while (token != null && (token != "constraint" || tokenizer.Quoted))
					token = tokenizer.NextToken();
				if (token == null) break;

				ParseConstraint(fkTable, tableToParse, tokenizer, includeColumns);
			}
		}

		private static void ParseConstraint(MySqlSchemaCollection fkTable, MySqlSchemaRow table,
		  MySqlTokenizer tokenizer, bool includeColumns)
		{
			string name = tokenizer.NextToken();
			MySqlSchemaRow row = fkTable.AddRow();

			// make sure this constraint is a FK
			string token = tokenizer.NextToken();
			if (token != "foreign" || tokenizer.Quoted)
				return;
			tokenizer.NextToken(); // read off the 'KEY' symbol
			tokenizer.NextToken(); // read off the '(' symbol

			row["CONSTRAINT_CATALOG"] = table["TABLE_CATALOG"];
			row["CONSTRAINT_SCHEMA"] = table["TABLE_SCHEMA"];
			row["TABLE_CATALOG"] = table["TABLE_CATALOG"];
			row["TABLE_SCHEMA"] = table["TABLE_SCHEMA"];
			row["TABLE_NAME"] = table["TABLE_NAME"];
			row["REFERENCED_TABLE_CATALOG"] = null;
			row["CONSTRAINT_NAME"] = name.Trim(new char[] { '\'', '`' });

			List<string> srcColumns = includeColumns ? ParseColumns(tokenizer) : null;

			// now look for the references section
			while (token != "references" || tokenizer.Quoted)
				token = tokenizer.NextToken();
			string target1 = tokenizer.NextToken();
			string target2 = tokenizer.NextToken();
			if (target2.StartsWith(".", StringComparison.Ordinal))
			{
				row["REFERENCED_TABLE_SCHEMA"] = target1;
				row["REFERENCED_TABLE_NAME"] = target2.Substring(1).Trim(new char[] { '\'', '`' });
				tokenizer.NextToken();  // read off the '('
			}
			else
			{
				row["REFERENCED_TABLE_SCHEMA"] = table["TABLE_SCHEMA"];
				row["REFERENCED_TABLE_NAME"] = target1.Substring(1).Trim(new char[] { '\'', '`' }); ;
			}

			// if we are supposed to include columns, read the target columns
			List<string> targetColumns = includeColumns ? ParseColumns(tokenizer) : null;

			if (includeColumns)
				ProcessColumns(fkTable, row, srcColumns, targetColumns);
			else
				fkTable.Rows.Add(row);
		}

		private static List<string> ParseColumns(MySqlTokenizer tokenizer)
		{
			List<string> sc = new List<string>();
			string token = tokenizer.NextToken();
			while (token != ")")
			{
				if (token != ",")
					sc.Add(token);
				token = tokenizer.NextToken();
			}
			return sc;
		}

		private static void ProcessColumns(MySqlSchemaCollection fkTable, MySqlSchemaRow row, List<string> srcColumns, List<string> targetColumns)
		{
			for (int i = 0; i < srcColumns.Count; i++)
			{
				MySqlSchemaRow newRow = fkTable.AddRow();
				row.CopyRow(newRow);
				newRow["COLUMN_NAME"] = srcColumns[i];
				newRow["ORDINAL_POSITION"] = i;
				newRow["REFERENCED_COLUMN_NAME"] = targetColumns[i];
				fkTable.Rows.Add(newRow);
			}
		}

		#endregion

		public virtual MySqlSchemaCollection GetUsers(string[] restrictions)
		{
			StringBuilder sb = new StringBuilder("SELECT Host, User FROM mysql.user");
			if (restrictions != null && restrictions.Length > 0)
				sb.AppendFormat(CultureInfo.InvariantCulture, " WHERE User LIKE '{0}'", restrictions[0]);

			MySqlSchemaCollection c = QueryCollection("Users", sb.ToString());
			c.Columns[0].Name = "HOST";
			c.Columns[1].Name = "USERNAME";

			return c;
		}

		public virtual MySqlSchemaCollection GetProcedures(string[] restrictions)
		{
			MySqlSchemaCollection dt = new MySqlSchemaCollection("Procedures");
			dt.AddColumn("SPECIFIC_NAME", typeof(string));
			dt.AddColumn("ROUTINE_CATALOG", typeof(string));
			dt.AddColumn("ROUTINE_SCHEMA", typeof(string));
			dt.AddColumn("ROUTINE_NAME", typeof(string));
			dt.AddColumn("ROUTINE_TYPE", typeof(string));
			dt.AddColumn("DTD_IDENTIFIER", typeof(string));
			dt.AddColumn("ROUTINE_BODY", typeof(string));
			dt.AddColumn("ROUTINE_DEFINITION", typeof(string));
			dt.AddColumn("EXTERNAL_NAME", typeof(string));
			dt.AddColumn("EXTERNAL_LANGUAGE", typeof(string));
			dt.AddColumn("PARAMETER_STYLE", typeof(string));
			dt.AddColumn("IS_DETERMINISTIC", typeof(string));
			dt.AddColumn("SQL_DATA_ACCESS", typeof(string));
			dt.AddColumn("SQL_PATH", typeof(string));
			dt.AddColumn("SECURITY_TYPE", typeof(string));
			dt.AddColumn("CREATED", typeof(DateTime));
			dt.AddColumn("LAST_ALTERED", typeof(DateTime));
			dt.AddColumn("SQL_MODE", typeof(string));
			dt.AddColumn("ROUTINE_COMMENT", typeof(string));
			dt.AddColumn("DEFINER", typeof(string));

			StringBuilder sql = new StringBuilder("SELECT * FROM mysql.proc WHERE 1=1");
			if (restrictions != null)
			{
				if (restrictions.Length >= 2 && restrictions[1] != null)
					sql.AppendFormat(CultureInfo.InvariantCulture,
					  " AND db LIKE '{0}'", restrictions[1]);
				if (restrictions.Length >= 3 && restrictions[2] != null)
					sql.AppendFormat(CultureInfo.InvariantCulture,
					  " AND name LIKE '{0}'", restrictions[2]);
				if (restrictions.Length >= 4 && restrictions[3] != null)
					sql.AppendFormat(CultureInfo.InvariantCulture,
					  " AND type LIKE '{0}'", restrictions[3]);
			}

			MySqlCommand cmd = new MySqlCommand(sql.ToString(), connection);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					MySqlSchemaRow row = dt.AddRow();
					row["SPECIFIC_NAME"] = reader.GetString("specific_name");
					row["ROUTINE_CATALOG"] = DBNull.Value;
					row["ROUTINE_SCHEMA"] = reader.GetString("db");
					row["ROUTINE_NAME"] = reader.GetString("name");
					string routineType = reader.GetString("type");
					row["ROUTINE_TYPE"] = routineType;
					row["DTD_IDENTIFIER"] = routineType.ToLowerInvariant() == "function" ?
					  (object)reader.GetString("returns") : DBNull.Value;
					row["ROUTINE_BODY"] = "SQL";
					row["ROUTINE_DEFINITION"] = reader.GetString("body");
					row["EXTERNAL_NAME"] = DBNull.Value;
					row["EXTERNAL_LANGUAGE"] = DBNull.Value;
					row["PARAMETER_STYLE"] = "SQL";
					row["IS_DETERMINISTIC"] = reader.GetString("is_deterministic");
					row["SQL_DATA_ACCESS"] = reader.GetString("sql_data_access");
					row["SQL_PATH"] = DBNull.Value;
					row["SECURITY_TYPE"] = reader.GetString("security_type");
					row["CREATED"] = reader.GetDateTime("created");
					row["LAST_ALTERED"] = reader.GetDateTime("modified");
					row["SQL_MODE"] = reader.GetString("sql_mode");
					row["ROUTINE_COMMENT"] = reader.GetString("comment");
					row["DEFINER"] = reader.GetString("definer");
				}
			}

			return dt;
		}

		protected virtual MySqlSchemaCollection GetCollections()
		{
			object[][] collections = new object[][]
			  {
		  new object[] {"MetaDataCollections", 0, 0},
		  new object[] {"DataSourceInformation", 0, 0},
		  new object[] {"DataTypes", 0, 0},
		  new object[] {"Restrictions", 0, 0},
		  new object[] {"ReservedWords", 0, 0},
		  new object[] {"Databases", 1, 1},
		  new object[] {"Tables", 4, 2},
		  new object[] {"Columns", 4, 4},
		  new object[] {"Users", 1, 1},
		  new object[] {"Foreign Keys", 4, 3},
		  new object[] {"IndexColumns", 5, 4},
		  new object[] {"Indexes", 4, 3},
		  new object[] {"Foreign Key Columns", 4, 3},
		  new object[] {"UDF", 1, 1}
			  };

			MySqlSchemaCollection dt = new MySqlSchemaCollection("MetaDataCollections");
			dt.AddColumn("CollectionName", typeof(string));
			dt.AddColumn("NumberOfRestrictions", typeof(int));
			dt.AddColumn("NumberOfIdentifierParts", typeof(int));

			FillTable(dt, collections);

			return dt;
		}



		private static MySqlSchemaCollection GetDataTypes()
		{
			MySqlSchemaCollection dt = new MySqlSchemaCollection("DataTypes");
			dt.AddColumn("TypeName", typeof(string));
			dt.AddColumn("ProviderDbType", typeof(int));
			dt.AddColumn("ColumnSize", typeof(long));
			dt.AddColumn("CreateFormat", typeof(string));
			dt.AddColumn("CreateParameters", typeof(string));
			dt.AddColumn("DataType", typeof(string));
			dt.AddColumn("IsAutoincrementable", typeof(bool));
			dt.AddColumn("IsBestMatch", typeof(bool));
			dt.AddColumn("IsCaseSensitive", typeof(bool));
			dt.AddColumn("IsFixedLength", typeof(bool));
			dt.AddColumn("IsFixedPrecisionScale", typeof(bool));
			dt.AddColumn("IsLong", typeof(bool));
			dt.AddColumn("IsNullable", typeof(bool));
			dt.AddColumn("IsSearchable", typeof(bool));
			dt.AddColumn("IsSearchableWithLike", typeof(bool));
			dt.AddColumn("IsUnsigned", typeof(bool));
			dt.AddColumn("MaximumScale", typeof(short));
			dt.AddColumn("MinimumScale", typeof(short));
			dt.AddColumn("IsConcurrencyType", typeof(bool));
			dt.AddColumn("IsLiteralSupported", typeof(bool));
			dt.AddColumn("LiteralPrefix", typeof(string));
			dt.AddColumn("LiteralSuffix", typeof(string));
			dt.AddColumn("NativeDataType", typeof(string));

			// have each one of the types contribute to the datatypes collection
			MySqlBit.SetDSInfo(dt);
			MySqlBinary.SetDSInfo(dt);
			MySqlDateTime.SetDSInfo(dt);
			MySqlTimeSpan.SetDSInfo(dt);
			MySqlString.SetDSInfo(dt);
			MySqlDouble.SetDSInfo(dt);
			MySqlSingle.SetDSInfo(dt);
			MySqlByte.SetDSInfo(dt);
			MySqlInt16.SetDSInfo(dt);
			MySqlInt32.SetDSInfo(dt);
			MySqlInt64.SetDSInfo(dt);
			MySqlDecimal.SetDSInfo(dt);
			MySqlUByte.SetDSInfo(dt);
			MySqlUInt16.SetDSInfo(dt);
			MySqlUInt32.SetDSInfo(dt);
			MySqlUInt64.SetDSInfo(dt);

			return dt;
		}

		protected virtual MySqlSchemaCollection GetRestrictions()
		{
			object[][] restrictions = new object[][]
			  {
		  new object[] {"Users", "Name", "", 0},
		  new object[] {"Databases", "Name", "", 0},
		  new object[] {"Tables", "Database", "", 0},
		  new object[] {"Tables", "Schema", "", 1},
		  new object[] {"Tables", "Table", "", 2},
		  new object[] {"Tables", "TableType", "", 3},
		  new object[] {"Columns", "Database", "", 0},
		  new object[] {"Columns", "Schema", "", 1},
		  new object[] {"Columns", "Table", "", 2},
		  new object[] {"Columns", "Column", "", 3},
		  new object[] {"Indexes", "Database", "", 0},
		  new object[] {"Indexes", "Schema", "", 1},
		  new object[] {"Indexes", "Table", "", 2},
		  new object[] {"Indexes", "Name", "", 3},
		  new object[] {"IndexColumns", "Database", "", 0},
		  new object[] {"IndexColumns", "Schema", "", 1},
		  new object[] {"IndexColumns", "Table", "", 2},
		  new object[] {"IndexColumns", "ConstraintName", "", 3},
		  new object[] {"IndexColumns", "Column", "", 4},
		  new object[] {"Foreign Keys", "Database", "", 0},
		  new object[] {"Foreign Keys", "Schema", "", 1},
		  new object[] {"Foreign Keys", "Table", "", 2},
		  new object[] {"Foreign Keys", "Constraint Name", "", 3},
		  new object[] {"Foreign Key Columns", "Catalog", "", 0},
		  new object[] {"Foreign Key Columns", "Schema", "", 1},
		  new object[] {"Foreign Key Columns", "Table", "", 2},
		  new object[] {"Foreign Key Columns", "Constraint Name", "", 3},
		  new object[] {"UDF", "Name", "", 0}
			  };

			MySqlSchemaCollection dt = new MySqlSchemaCollection("Restrictions");
			dt.AddColumn("CollectionName", typeof(string));
			dt.AddColumn("RestrictionName", typeof(string));
			dt.AddColumn("RestrictionDefault", typeof(string));
			dt.AddColumn("RestrictionNumber", typeof(int));

			FillTable(dt, restrictions);

			return dt;
		}

		private static MySqlSchemaCollection GetReservedWords()
		{
			MySqlSchemaCollection dt = new MySqlSchemaCollection("ReservedWords");


			string[] lines = @"ACCESSIBLE ADD ALL 
ALTER ANALYZE AND 
AS ASC ASENSITIVE 
BEFORE BETWEEN BIGINT 
BINARY BLOB BOTH 
BY CALL CASCADE 
CASE CHANGE CHAR 
CHARACTER CHECK COLLATE 
COLUMN CONDITION CONNECTION 
CONSTRAINT CONTINUE CONTRIBUTORS 
CONVERT CREATE CROSS 
CURRENT_DATE CURRENT_TIME CURRENT_TIMESTAMP 
CURRENT_USER CURSOR DATABASE 
DATABASES DAY_HOUR DAY_MICROSECOND 
DAY_MINUTE DAY_SECOND DEC 
DECIMAL DECLARE DEFAULT 
DELAYED DELETE DESC 
DESCRIBE DETERMINISTIC DISTINCT 
DISTINCTROW DIV DOUBLE 
DROP DUAL EACH 
ELSE ELSEIF ENCLOSED 
ESCAPED EXISTS EXIT 
EXPLAIN FALSE FETCH 
FLOAT FLOAT4 FLOAT8 
FOR FORCE FOREIGN 
FROM FULLTEXT GRANT 
GROUP HAVING HIGH_PRIORITY 
HOUR_MICROSECOND HOUR_MINUTE HOUR_SECOND 
IF IGNORE IN 
INDEX INFILE INNER 
INOUT INSENSITIVE INSERT 
INT INT1 INT2 
INT3 INT4 INT8 
INTEGER INTERVAL INTO 
IS ITERATE JOIN 
KEY KEYS KILL 
LEADING LEAVE LEFT 
LIKE LIMIT LINEAR 
LINES LOAD LOCALTIME 
LOCALTIMESTAMP LOCK LONG 
LONGBLOB LONGTEXT LOOP 
LOW_PRIORITY MATCH MEDIUMBLOB 
MEDIUMINT MEDIUMTEXT MIDDLEINT 
MINUTE_MICROSECOND MINUTE_SECOND MOD 
MODIFIES NATURAL NOT 
NO_WRITE_TO_BINLOG NULL NUMERIC 
ON OPTIMIZE OPTION 
OPTIONALLY OR ORDER 
OUT OUTER OUTFILE 
PRECISION PRIMARY PROCEDURE 
PURGE RANGE READ 
READS READ_ONLY READ_WRITE 
REAL REFERENCES REGEXP 
RELEASE RENAME REPEAT 
REPLACE REQUIRE RESTRICT 
RETURN REVOKE RIGHT 
RLIKE SCHEMA SCHEMAS 
SECOND_MICROSECOND SELECT SENSITIVE 
SEPARATOR SET SHOW 
SMALLINT SPATIAL SPECIFIC 
SQL SQLEXCEPTION SQLSTATE 
SQLWARNING SQL_BIG_RESULT SQL_CALC_FOUND_ROWS 
SQL_SMALL_RESULT SSL STARTING 
STRAIGHT_JOIN TABLE TERMINATED 
THEN TINYBLOB TINYINT 
TINYTEXT TO TRAILING 
TRIGGER TRUE UNDO 
UNION UNIQUE UNLOCK 
UNSIGNED UPDATE UPGRADE 
USAGE USE USING 
UTC_DATE UTC_TIME UTC_TIMESTAMP 
VALUES VARBINARY VARCHAR 
VARCHARACTER VARYING WHEN 
WHERE WHILE WITH 
WRITE X509 XOR 
YEAR_MONTH ZEROFILL   
".Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				string[] keywords = line.Split(new char[] { ' ' });
				foreach (string s in keywords)
				{
					if (String.IsNullOrEmpty(s)) continue;
					MySqlSchemaRow row = dt.AddRow();
					row[0] = s;
				}
			}

				 
			 


			return dt;
		}

		protected static void FillTable(MySqlSchemaCollection dt, object[][] data)
		{
			foreach (object[] dataItem in data)
			{
				MySqlSchemaRow row = dt.AddRow();
				for (int i = 0; i < dataItem.Length; i++)
					row[i] = dataItem[i];
			}
		}

		private void FindTables(MySqlSchemaCollection schema, string[] restrictions)
		{
			StringBuilder sql = new StringBuilder();
			StringBuilder where = new StringBuilder();
			sql.AppendFormat(CultureInfo.InvariantCulture,
					 "SHOW TABLE STATUS FROM `{0}`", restrictions[1]);
			if (restrictions != null && restrictions.Length >= 3 &&
			  restrictions[2] != null)
				where.AppendFormat(CultureInfo.InvariantCulture,
						   " LIKE '{0}'", restrictions[2]);
			sql.Append(where.ToString());

			string table_type = restrictions[1].ToLower() == "information_schema"
						?
					  "SYSTEM VIEW"
						: "BASE TABLE";

			MySqlCommand cmd = new MySqlCommand(sql.ToString(), connection);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					MySqlSchemaRow row = schema.AddRow();
					row["TABLE_CATALOG"] = null;
					row["TABLE_SCHEMA"] = restrictions[1];
					row["TABLE_NAME"] = reader.GetString(0);
					row["TABLE_TYPE"] = table_type;
					row["ENGINE"] = GetString(reader, 1);
					row["VERSION"] = reader.GetValue(2);
					row["ROW_FORMAT"] = GetString(reader, 3);
					row["TABLE_ROWS"] = reader.GetValue(4);
					row["AVG_ROW_LENGTH"] = reader.GetValue(5);
					row["DATA_LENGTH"] = reader.GetValue(6);
					row["MAX_DATA_LENGTH"] = reader.GetValue(7);
					row["INDEX_LENGTH"] = reader.GetValue(8);
					row["DATA_FREE"] = reader.GetValue(9);
					row["AUTO_INCREMENT"] = reader.GetValue(10);
					row["CREATE_TIME"] = reader.GetValue(11);
					row["UPDATE_TIME"] = reader.GetValue(12);
					row["CHECK_TIME"] = reader.GetValue(13);
					row["TABLE_COLLATION"] = GetString(reader, 14);
					row["CHECKSUM"] = reader.GetValue(15);
					row["CREATE_OPTIONS"] = GetString(reader, 16);
					row["TABLE_COMMENT"] = GetString(reader, 17);
				}
			}
		}

		private static string GetString(MySqlDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
				return null;
			return reader.GetString(index);
		}

		public virtual MySqlSchemaCollection GetUDF(string[] restrictions)
		{
			string sql = "SELECT name,ret,dl FROM mysql.func";
			if (restrictions != null)
			{
				if (restrictions.Length >= 1 && !String.IsNullOrEmpty(restrictions[0]))
					sql += String.Format(" WHERE name LIKE '{0}'", restrictions[0]);
			}

			MySqlSchemaCollection dt = new MySqlSchemaCollection("User-defined Functions");
			dt.AddColumn("NAME", typeof(string));
			dt.AddColumn("RETURN_TYPE", typeof(int));
			dt.AddColumn("LIBRARY_NAME", typeof(string));

			MySqlCommand cmd = new MySqlCommand(sql, connection);
			try
			{
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						MySqlSchemaRow row = dt.AddRow();
						row[0] = reader.GetString(0);
						row[1] = reader.GetInt32(1);
						row[2] = reader.GetString(2);
					}
				}
			}
			catch (MySqlException ex)
			{
				if (ex.Number != (int)MySqlErrorCode.TableAccessDenied)
					throw;
				throw new MySqlException("Resources.UnableToEnumerateUDF", ex);
			}

			return dt;
		}

		protected virtual MySqlSchemaCollection GetSchemaInternal(string collection, string[] restrictions)
		{
			switch (collection)
			{
				// common collections
				case "METADATACOLLECTIONS":
					return GetCollections();
				case "DATASOURCEINFORMATION":
					return null;
				case "DATATYPES":
					return GetDataTypes();
				case "RESTRICTIONS":
					return GetRestrictions();
				case "RESERVEDWORDS":
					return GetReservedWords();

				// collections specific to our provider
				case "USERS":
					return GetUsers(restrictions);
				case "DATABASES":
					return GetDatabases(restrictions);
				case "UDF":
					return GetUDF(restrictions);
			}

			// if we have a current database and our users have
			// not specified a database, then default to the currently
			// selected one.
			if (restrictions == null)
				restrictions = new string[2];
			if (connection != null &&
			  connection.Database != null &&
			  connection.Database.Length > 0 &&
			  restrictions.Length > 1 &&
			  restrictions[1] == null)
				restrictions[1] = connection.Database;

			switch (collection)
			{
				case "TABLES":
					return GetTables(restrictions);
				case "COLUMNS":
					return GetColumns(restrictions);
				case "INDEXES":
					return GetIndexes(restrictions);
				case "INDEXCOLUMNS":
					return GetIndexColumns(restrictions);
				case "FOREIGN KEYS":
					return GetForeignKeys(restrictions);
				case "FOREIGN KEY COLUMNS":
					return GetForeignKeyColumns(restrictions);
			}
			return null;
		}

		internal string[] CleanRestrictions(string[] restrictionValues)
		{
			string[] restrictions = null;
			if (restrictionValues != null)
			{
				restrictions = (string[])restrictionValues.Clone();

				for (int x = 0; x < restrictions.Length; x++)
				{
					string s = restrictions[x];
					if (s == null) continue;
					restrictions[x] = s.Trim('`');
				}
			}
			return restrictions;
		}

		protected MySqlSchemaCollection QueryCollection(string name, string sql)
		{
			MySqlSchemaCollection c = new MySqlSchemaCollection(name);
			MySqlCommand cmd = new MySqlCommand(sql, connection);
			MySqlDataReader reader = cmd.ExecuteReader();

			for (int i = 0; i < reader.FieldCount; i++)
				c.AddColumn(reader.GetName(i), reader.GetFieldType(i));

			using (reader)
			{
				while (reader.Read())
				{
					MySqlSchemaRow row = c.AddRow();
					for (int i = 0; i < reader.FieldCount; i++)
						row[i] = reader.GetValue(i);
				}
			}
			return c;
		}
	}
}
