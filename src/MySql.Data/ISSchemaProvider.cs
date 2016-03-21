// Copyright ?2004, 2013, Oracle and/or its affiliates. All rights reserved.
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
using System.Data;
using System.Text;
using MySql.Data.Common;
using System.Globalization;
using System.Diagnostics;
using MySql.Data.Types;
using System.Collections;

using System.Collections.Generic;

namespace MySql.Data.MySqlClient
{
	internal class ISSchemaProvider : SchemaProvider
	{
		public ISSchemaProvider(MySqlConnection connection)
		  : base(connection)
		{
		}

		protected override MySqlSchemaCollection GetCollections()
		{
			MySqlSchemaCollection dt = base.GetCollections();

			object[][] collections = new object[][]
				  {
				new object[] {"Views", 2, 3},
				new object[] {"ViewColumns", 3, 4},
				new object[] {"Procedure Parameters", 5, 1},
				new object[] {"Procedures", 4, 3},
				new object[] {"Triggers", 2, 4}
				  };

			FillTable(dt, collections);
			return dt;
		}

		protected override MySqlSchemaCollection GetRestrictions()
		{
			MySqlSchemaCollection dt = base.GetRestrictions();

			object[][] restrictions = new object[][]
				  {
				new object[] {"Procedure Parameters", "Database", "", 0},
				new object[] {"Procedure Parameters", "Schema", "", 1},
				new object[] {"Procedure Parameters", "Name", "", 2},
				new object[] {"Procedure Parameters", "Type", "", 3},
				new object[] {"Procedure Parameters", "Parameter", "", 4},
				new object[] {"Procedures", "Database", "", 0},
				new object[] {"Procedures", "Schema", "", 1},
				new object[] {"Procedures", "Name", "", 2},
				new object[] {"Procedures", "Type", "", 3},
				new object[] {"Views", "Database", "", 0},
				new object[] {"Views", "Schema", "", 1},
				new object[] {"Views", "Table", "", 2},
				new object[] {"ViewColumns", "Database", "", 0},
				new object[] {"ViewColumns", "Schema", "", 1},
				new object[] {"ViewColumns", "Table", "", 2},
				new object[] {"ViewColumns", "Column", "", 3},
				new object[] {"Triggers", "Database", "", 0},
				new object[] {"Triggers", "Schema", "", 1},
				new object[] {"Triggers", "Name", "", 2},
				new object[] {"Triggers", "EventObjectTable", "", 3},
				  };
			FillTable(dt, restrictions);
			return dt;
		}

		public override MySqlSchemaCollection GetDatabases(string[] restrictions)
		{
			string[] keys = new string[1];
			keys[0] = "SCHEMA_NAME";
			MySqlSchemaCollection dt = Query("SCHEMATA", "", keys, restrictions);
			dt.Columns[1].Name = "database_name";
			dt.Name = "Databases";
			return dt;
		}

		public override MySqlSchemaCollection GetTables(string[] restrictions)
		{
			string[] keys = new string[4];
			keys[0] = "TABLE_CATALOG";
			keys[1] = "TABLE_SCHEMA";
			keys[2] = "TABLE_NAME";
			keys[3] = "TABLE_TYPE";
			MySqlSchemaCollection dt = Query("TABLES", "TABLE_TYPE != 'VIEW'", keys, restrictions);
			dt.Name = "Tables";
			return dt;
		}

		public override MySqlSchemaCollection GetColumns(string[] restrictions)
		{
			string[] keys = new string[4];
			keys[0] = "TABLE_CATALOG";
			keys[1] = "TABLE_SCHEMA";
			keys[2] = "TABLE_NAME";
			keys[3] = "COLUMN_NAME";
			MySqlSchemaCollection dt = Query("COLUMNS", null, keys, restrictions);
			dt.RemoveColumn("CHARACTER_OCTET_LENGTH");
			dt.Name = "Columns";
			QuoteDefaultValues(dt);
			return dt;
		}

		private MySqlSchemaCollection GetViews(string[] restrictions)
		{
			string[] keys = new string[3];
			keys[0] = "TABLE_CATALOG";
			keys[1] = "TABLE_SCHEMA";
			keys[2] = "TABLE_NAME";
			MySqlSchemaCollection dt = Query("VIEWS", null, keys, restrictions);
			dt.Name = "Views";
			return dt;
		}

		private MySqlSchemaCollection GetViewColumns(string[] restrictions)
		{
			StringBuilder where = new StringBuilder();
			StringBuilder sql = new StringBuilder(
				"SELECT C.* FROM information_schema.columns C");
			sql.Append(" JOIN information_schema.views V ");
			sql.Append("ON C.table_schema=V.table_schema AND C.table_name=V.table_name ");
			if (restrictions != null && restrictions.Length >= 2 &&
				restrictions[1] != null)
				where.AppendFormat(CultureInfo.InvariantCulture, "C.table_schema='{0}' ", restrictions[1]);
			if (restrictions != null && restrictions.Length >= 3 &&
				restrictions[2] != null)
			{
				if (where.Length > 0)
					where.Append("AND ");
				where.AppendFormat(CultureInfo.InvariantCulture, "C.table_name='{0}' ", restrictions[2]);
			}
			if (restrictions != null && restrictions.Length == 4 &&
				restrictions[3] != null)
			{
				if (where.Length > 0)
					where.Append("AND ");
				where.AppendFormat(CultureInfo.InvariantCulture, "C.column_name='{0}' ", restrictions[3]);
			}
			if (where.Length > 0)
				sql.AppendFormat(CultureInfo.InvariantCulture, " WHERE {0}", where);
			MySqlSchemaCollection dt = GetTable(sql.ToString());
			dt.Name = "ViewColumns";
			dt.Columns[0].Name = "VIEW_CATALOG";
			dt.Columns[1].Name = "VIEW_SCHEMA";
			dt.Columns[2].Name = "VIEW_NAME";
			QuoteDefaultValues(dt);
			return dt;
		}

		private MySqlSchemaCollection GetTriggers(string[] restrictions)
		{
			string[] keys = new string[4];
			keys[0] = "TRIGGER_CATALOG";
			keys[1] = "TRIGGER_SCHEMA";
			keys[2] = "EVENT_OBJECT_TABLE";
			keys[3] = "TRIGGER_NAME";
			MySqlSchemaCollection dt = Query("TRIGGERS", null, keys, restrictions);
			dt.Name = "Triggers";
			return dt;
		}

		/// <summary>
		/// Return schema information about procedures and functions
		/// Restrictions supported are:
		/// schema, name, type
		/// </summary>
		/// <param name="restrictions"></param>
		/// <returns></returns>
		public override MySqlSchemaCollection GetProcedures(string[] restrictions)
		{
			try
			{
				if (connection.Settings.HasProcAccess)
					return base.GetProcedures(restrictions);
			}
			catch (MySqlException ex)
			{
				if (ex.Number == (int)MySqlErrorCode.TableAccessDenied)
					connection.Settings.HasProcAccess = false;
				else
					throw;
			}

			string[] keys = new string[4];
			keys[0] = "ROUTINE_CATALOG";
			keys[1] = "ROUTINE_SCHEMA";
			keys[2] = "ROUTINE_NAME";
			keys[3] = "ROUTINE_TYPE";

			MySqlSchemaCollection dt = Query("ROUTINES", null, keys, restrictions);
			dt.Name = "Procedures";
			return dt;
		}

		private MySqlSchemaCollection GetProceduresWithParameters(string[] restrictions)
		{
			MySqlSchemaCollection dt = GetProcedures(restrictions);
			dt.AddColumn("ParameterList", typeof(string));

			foreach (MySqlSchemaRow row in dt.Rows)
			{
				row["ParameterList"] = GetProcedureParameterLine(row);
			}
			return dt;
		}

		private string GetProcedureParameterLine(MySqlSchemaRow isRow)
		{
			string sql = "SHOW CREATE {0} `{1}`.`{2}`";
			sql = String.Format(sql, isRow["ROUTINE_TYPE"], isRow["ROUTINE_SCHEMA"],
				isRow["ROUTINE_NAME"]);
			MySqlCommand cmd = new MySqlCommand(sql, connection);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();

				// if we are not the owner of this proc or have permissions
				// then we will get null for the body
				if (reader.IsDBNull(2)) return null;

				string sql_mode = reader.GetString(1);

				string body = reader.GetString(2);
				MySqlTokenizer tokenizer = new MySqlTokenizer(body);
				tokenizer.AnsiQuotes = sql_mode.IndexOf("ANSI_QUOTES") != -1;
				tokenizer.BackslashEscapes = sql_mode.IndexOf("NO_BACKSLASH_ESCAPES") == -1;

				string token = tokenizer.NextToken();
				while (token != "(")
					token = tokenizer.NextToken();
				int start = tokenizer.StartIndex + 1;
				token = tokenizer.NextToken();
				while (token != ")" || tokenizer.Quoted)
				{
					token = tokenizer.NextToken();
					// if we see another ( and we are not quoted then we
					// are in a size element and we need to look for the closing paren
					if (token == "(" && !tokenizer.Quoted)
					{
						while (token != ")" || tokenizer.Quoted)
							token = tokenizer.NextToken();
						token = tokenizer.NextToken();
					}
				}
				return body.Substring(start, tokenizer.StartIndex - start);
			}
		}

		private MySqlSchemaCollection GetParametersForRoutineFromIS(string[] restrictions)
		{
			string[] keys = new string[5];
			keys[0] = "SPECIFIC_CATALOG";
			keys[1] = "SPECIFIC_SCHEMA";
			keys[2] = "SPECIFIC_NAME";
			keys[3] = "ROUTINE_TYPE";
			keys[4] = "PARAMETER_NAME";

			StringBuilder sql = new StringBuilder(@"SELECT * FROM INFORMATION_SCHEMA.PARAMETERS");
			// now get our where clause and append it if there is one
			string where = GetWhereClause(null, keys, restrictions);
			if (!String.IsNullOrEmpty(where))
				sql.AppendFormat(CultureInfo.InvariantCulture, " WHERE {0}", where);

			MySqlSchemaCollection coll = QueryCollection("parameters", sql.ToString());

			if ((coll.Rows.Count != 0) && ((string)coll.Rows[0]["routine_type"] == "FUNCTION"))
			{
				// update missing data for the first row (function return value).
				// (using sames valus than GetParametersFromShowCreate).
				coll.Rows[0]["parameter_mode"] = "IN";
				coll.Rows[0]["parameter_name"] = "return_value"; // "FUNCTION";
			}
			return coll;
		}

		private MySqlSchemaCollection GetParametersFromIS(string[] restrictions, MySqlSchemaCollection routines)
		{
			MySqlSchemaCollection parms = null;

			if (routines == null || routines.Rows.Count == 0)
			{
				if (restrictions == null)
				{
					parms = QueryCollection("parameters", "SELECT * FROM INFORMATION_SCHEMA.PARAMETERS WHERE 1=2");
				}
				else
					parms = GetParametersForRoutineFromIS(restrictions);
			}
			else foreach (MySqlSchemaRow routine in routines.Rows)
				{
					if (restrictions != null && restrictions.Length >= 3)
						restrictions[2] = routine["ROUTINE_NAME"].ToString();

					parms = GetParametersForRoutineFromIS(restrictions);
				}
			parms.Name = "Procedure Parameters";
			return parms;
		}

		internal MySqlSchemaCollection CreateParametersTable()
		{
			MySqlSchemaCollection dt = new MySqlSchemaCollection("Procedure Parameters");
			dt.AddColumn("SPECIFIC_CATALOG", typeof(string));
			dt.AddColumn("SPECIFIC_SCHEMA", typeof(string));
			dt.AddColumn("SPECIFIC_NAME", typeof(string));
			dt.AddColumn("ORDINAL_POSITION", typeof(Int32));
			dt.AddColumn("PARAMETER_MODE", typeof(string));
			dt.AddColumn("PARAMETER_NAME", typeof(string));
			dt.AddColumn("DATA_TYPE", typeof(string));
			dt.AddColumn("CHARACTER_MAXIMUM_LENGTH", typeof(Int32));
			dt.AddColumn("CHARACTER_OCTET_LENGTH", typeof(Int32));
			dt.AddColumn("NUMERIC_PRECISION", typeof(byte));
			dt.AddColumn("NUMERIC_SCALE", typeof(Int32));
			dt.AddColumn("CHARACTER_SET_NAME", typeof(string));
			dt.AddColumn("COLLATION_NAME", typeof(string));
			dt.AddColumn("DTD_IDENTIFIER", typeof(string));
			dt.AddColumn("ROUTINE_TYPE", typeof(string));
			return dt;
		}

		/// <summary>
		/// Return schema information about parameters for procedures and functions
		/// Restrictions supported are:
		/// schema, name, type, parameter name
		/// </summary>
		public virtual MySqlSchemaCollection GetProcedureParameters(string[] restrictions,
			MySqlSchemaCollection routines)
		{
			bool is55 = connection.driver.Version.isAtLeast(5, 5, 3);

			try
			{
				// we want to avoid using IS if  we can as it is painfully slow
				MySqlSchemaCollection dt = CreateParametersTable();
				GetParametersFromShowCreate(dt, restrictions, routines);
				return dt;
			}
			catch (Exception)
			{
				if (!is55) throw;

				// we get here by not having access and we are on 5.5 or later so just use IS
				return GetParametersFromIS(restrictions, routines);
			}
		}

		protected override MySqlSchemaCollection GetSchemaInternal(string collection, string[] restrictions)
		{
			MySqlSchemaCollection dt = base.GetSchemaInternal(collection, restrictions);
			if (dt != null)
				return dt;

			switch (collection)
			{
				case "VIEWS":
					return GetViews(restrictions);
				case "PROCEDURES":
					return GetProcedures(restrictions);
				case "PROCEDURES WITH PARAMETERS":
					return GetProceduresWithParameters(restrictions);
				case "PROCEDURE PARAMETERS":
					return GetProcedureParameters(restrictions, null);
				case "TRIGGERS":
					return GetTriggers(restrictions);
				case "VIEWCOLUMNS":
					return GetViewColumns(restrictions);
			}
			return null;
		}

		private static string GetWhereClause(string initial_where, string[] keys, string[] values)
		{
			StringBuilder where = new StringBuilder(initial_where);
			if (values != null)
			{
				for (int i = 0; i < keys.Length; i++)
				{
					if (i >= values.Length) break;
					if (values[i] == null || values[i] == String.Empty) continue;
					if (where.Length > 0)
						where.Append(" AND ");
					where.AppendFormat(CultureInfo.InvariantCulture,
						"{0} LIKE '{1}'", keys[i], values[i]);
				}
			}
			return where.ToString();
		}

		private MySqlSchemaCollection Query(string table_name, string initial_where,
			string[] keys, string[] values)
		{
			StringBuilder query = new StringBuilder("SELECT * FROM INFORMATION_SCHEMA.");
			query.Append(table_name);

			string where = GetWhereClause(initial_where, keys, values);

			if (where.Length > 0)
				query.AppendFormat(CultureInfo.InvariantCulture, " WHERE {0}", where);

			return GetTable(query.ToString());
		}

		private MySqlSchemaCollection GetTable(string sql)
		{
			MySqlSchemaCollection c = new MySqlSchemaCollection();
			MySqlCommand cmd = new MySqlCommand(sql, connection);
			MySqlDataReader reader = cmd.ExecuteReader();

			// add columns
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

		public override MySqlSchemaCollection GetForeignKeys(string[] restrictions)
		{
			if (!connection.driver.Version.isAtLeast(5, 1, 16))
				return base.GetForeignKeys(restrictions);

			string sql = @"SELECT rc.constraint_catalog, rc.constraint_schema,
                rc.constraint_name, kcu.table_catalog, kcu.table_schema, rc.table_name,
                rc.match_option, rc.update_rule, rc.delete_rule, 
                NULL as referenced_table_catalog,
                kcu.referenced_table_schema, rc.referenced_table_name 
                FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
                LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu ON 
                kcu.constraint_catalog <=> rc.constraint_catalog AND
                kcu.constraint_schema <=> rc.constraint_schema AND 
                kcu.constraint_name <=> rc.constraint_name AND
                kcu.ORDINAL_POSITION=1 WHERE 1=1";

			StringBuilder where = new StringBuilder();
			if (restrictions.Length >= 2 && !String.IsNullOrEmpty(restrictions[1]))
				where.AppendFormat(CultureInfo.InvariantCulture,
					" AND rc.constraint_schema LIKE '{0}'", restrictions[1]);
			if (restrictions.Length >= 3 && !String.IsNullOrEmpty(restrictions[2]))
				where.AppendFormat(CultureInfo.InvariantCulture,
					" AND rc.table_name LIKE '{0}'", restrictions[2]);
			if (restrictions.Length >= 4 && !String.IsNullOrEmpty(restrictions[3]))
				where.AppendFormat(CultureInfo.InvariantCulture,
					" AND rc.constraint_name LIKE '{0}'", restrictions[2]);

			sql += where.ToString();

			return GetTable(sql);
		}

		public override MySqlSchemaCollection GetForeignKeyColumns(string[] restrictions)
		{
			if (!connection.driver.Version.isAtLeast(5, 0, 6))
				return base.GetForeignKeyColumns(restrictions);

			string sql = @"SELECT kcu.* FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
                WHERE kcu.referenced_table_name IS NOT NULL";

			StringBuilder where = new StringBuilder();
			if (restrictions.Length >= 2 && !String.IsNullOrEmpty(restrictions[1]))
				where.AppendFormat(CultureInfo.InvariantCulture,
					" AND kcu.constraint_schema LIKE '{0}'", restrictions[1]);
			if (restrictions.Length >= 3 && !String.IsNullOrEmpty(restrictions[2]))
				where.AppendFormat(CultureInfo.InvariantCulture,
					" AND kcu.table_name LIKE '{0}'", restrictions[2]);
			if (restrictions.Length >= 4 && !String.IsNullOrEmpty(restrictions[3]))
				where.AppendFormat(CultureInfo.InvariantCulture,
					" AND kcu.constraint_name LIKE '{0}'", restrictions[3]);

			sql += where.ToString();

			return GetTable(sql);
		}

		#region Procedures Support Rouines

		internal void GetParametersFromShowCreate(MySqlSchemaCollection parametersTable,
			string[] restrictions, MySqlSchemaCollection routines)
		{
			// this allows us to pass in a pre-populated routines table
			// and avoid the querying for them again.
			// we use this when calling a procedure or function
			if (routines == null)
				routines = GetSchema("procedures", restrictions);

			MySqlCommand cmd = connection.CreateCommand();

			foreach (MySqlSchemaRow routine in routines.Rows)
			{
				string showCreateSql = String.Format("SHOW CREATE {0} `{1}`.`{2}`",
					routine["ROUTINE_TYPE"], routine["ROUTINE_SCHEMA"],
					routine["ROUTINE_NAME"]);
				cmd.CommandText = showCreateSql;
				try
				{
					string nameToRestrict = null;
					if (restrictions != null && restrictions.Length == 5 &&
						restrictions[4] != null)
						nameToRestrict = restrictions[4];
					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						reader.Read();
						string body = reader.GetString(2);
						reader.Close();
						ParseProcedureBody(parametersTable, body, routine, nameToRestrict);
					}
				}

				catch (MySqlNullValueException snex)

				{
					throw new InvalidOperationException(
						String.Format("Resources.UnableToRetrieveParameters", routine["ROUTINE_NAME"]), snex);
				}
			}
		}

		private void ParseProcedureBody(MySqlSchemaCollection parametersTable, string body,
			MySqlSchemaRow row, string nameToRestrict)
		{
			List<string> modes = new List<string>(new string[3] { "IN", "OUT", "INOUT" });

			string sqlMode = row["SQL_MODE"].ToString();

			int pos = 1;
			MySqlTokenizer tokenizer = new MySqlTokenizer(body);
			tokenizer.AnsiQuotes = sqlMode.IndexOf("ANSI_QUOTES") != -1;
			tokenizer.BackslashEscapes = sqlMode.IndexOf("NO_BACKSLASH_ESCAPES") == -1;
			tokenizer.ReturnComments = false;
			string token = tokenizer.NextToken();

			// this block will scan for the opening paren while also determining
			// if this routine is a function.  If so, then we need to add a
			// parameter row for the return parameter since it is ordinal position
			// 0 and should appear first.
			while (token != "(")
			{
				if (String.Compare(token, "FUNCTION", StringComparison.OrdinalIgnoreCase) == 0 &&
					nameToRestrict == null)
				{
					parametersTable.AddRow();
					InitParameterRow(row, parametersTable.Rows[0]);
				}
				token = tokenizer.NextToken();
			}
			token = tokenizer.NextToken();  // now move to the next token past the (

			while (token != ")")
			{
				MySqlSchemaRow parmRow = parametersTable.NewRow();
				InitParameterRow(row, parmRow);
				parmRow["ORDINAL_POSITION"] = pos++;

				// handle mode and name for the parameter
				string mode = token.ToUpper();
				if (!tokenizer.Quoted && modes.Contains(mode))
				{
					parmRow["PARAMETER_MODE"] = mode;
					token = tokenizer.NextToken();
				}
				if (tokenizer.Quoted)
					token = token.Substring(1, token.Length - 2);
				parmRow["PARAMETER_NAME"] = token;

				// now parse data type
				token = ParseDataType(parmRow, tokenizer);
				if (token == ",")
					token = tokenizer.NextToken();

				// now determine if we should include this row after all
				// we need to parse it before this check so we are correctly
				// positioned for the next parameter
				if (nameToRestrict == null ||
					String.Compare(parmRow["PARAMETER_NAME"].ToString(), nameToRestrict, StringComparison.OrdinalIgnoreCase) == 0)
					parametersTable.Rows.Add(parmRow);
			}

			// now parse out the return parameter if there is one.
			token = tokenizer.NextToken().ToUpper();
			if (String.Compare(token, "RETURNS", StringComparison.OrdinalIgnoreCase) == 0)
			{
				MySqlSchemaRow parameterRow = parametersTable.Rows[0];
				parameterRow["PARAMETER_NAME"] = "RETURN_VALUE";
				ParseDataType(parameterRow, tokenizer);
			}
		}

		/// <summary>
		/// Initializes a new row for the procedure parameters table.
		/// </summary>
		private static void InitParameterRow(MySqlSchemaRow procedure, MySqlSchemaRow parameter)
		{
			parameter["SPECIFIC_CATALOG"] = null;
			parameter["SPECIFIC_SCHEMA"] = procedure["ROUTINE_SCHEMA"];
			parameter["SPECIFIC_NAME"] = procedure["ROUTINE_NAME"];
			parameter["PARAMETER_MODE"] = "IN";
			parameter["ORDINAL_POSITION"] = 0;
			parameter["ROUTINE_TYPE"] = procedure["ROUTINE_TYPE"];
		}

		/// <summary>
		///  Parses out the elements of a procedure parameter data type.
		/// </summary>
		private string ParseDataType(MySqlSchemaRow row, MySqlTokenizer tokenizer)
		{
			StringBuilder dtd = new StringBuilder(
				tokenizer.NextToken().ToUpper());
			row["DATA_TYPE"] = dtd.ToString();
			string type = row["DATA_TYPE"].ToString();

			string token = tokenizer.NextToken();
			if (token == "(")
			{
				token = tokenizer.ReadParenthesis();
				dtd.AppendFormat(CultureInfo.InvariantCulture, "{0}", token);
				if (type != "ENUM" && type != "SET")
					ParseDataTypeSize(row, token);
				token = tokenizer.NextToken();
			}
			else
				dtd.Append(GetDataTypeDefaults(type, row));

			while (token != ")" &&
				   token != "," &&
				   String.Compare(token, "begin", StringComparison.OrdinalIgnoreCase) != 0 &&
				   String.Compare(token, "return", StringComparison.OrdinalIgnoreCase) != 0)
			{
				if (String.Compare(token, "CHARACTER", StringComparison.OrdinalIgnoreCase) == 0 ||
					String.Compare(token, "BINARY", StringComparison.OrdinalIgnoreCase) == 0)
				{ }  // we don't need to do anything with this
				else if (String.Compare(token, "SET", StringComparison.OrdinalIgnoreCase) == 0 ||
						 String.Compare(token, "CHARSET", StringComparison.OrdinalIgnoreCase) == 0)
					row["CHARACTER_SET_NAME"] = tokenizer.NextToken();
				else if (String.Compare(token, "ASCII", StringComparison.OrdinalIgnoreCase) == 0)
					row["CHARACTER_SET_NAME"] = "latin1";
				else if (String.Compare(token, "UNICODE", StringComparison.OrdinalIgnoreCase) == 0)
					row["CHARACTER_SET_NAME"] = "ucs2";
				else if (String.Compare(token, "COLLATE", StringComparison.OrdinalIgnoreCase) == 0)
					row["COLLATION_NAME"] = tokenizer.NextToken();
				else
					dtd.AppendFormat(CultureInfo.InvariantCulture, " {0}", token);
				token = tokenizer.NextToken();
			}

			if (dtd.Length > 0)
				row["DTD_IDENTIFIER"] = dtd.ToString();

			// now default the collation if one wasn't given
			if (string.IsNullOrEmpty((string)row["COLLATION_NAME"]) &&
				!string.IsNullOrEmpty((string)row["CHARACTER_SET_NAME"]))
				row["COLLATION_NAME"] = CharSetMap.GetDefaultCollation(
					row["CHARACTER_SET_NAME"].ToString(), connection);

			// now set the octet length
			if (row["CHARACTER_MAXIMUM_LENGTH"] != null)
			{
				if (row["CHARACTER_SET_NAME"] == null)
					row["CHARACTER_SET_NAME"] = "";
				row["CHARACTER_OCTET_LENGTH"] =
					CharSetMap.GetMaxLength((string)row["CHARACTER_SET_NAME"], connection) *
					(int)row["CHARACTER_MAXIMUM_LENGTH"];
			}

			return token;
		}

		private static string GetDataTypeDefaults(string type, MySqlSchemaRow row)
		{
			string format = "({0},{1})";
			object precision = row["NUMERIC_PRECISION"];
			if (MetaData.IsNumericType(type) &&
				string.IsNullOrEmpty((string)row["NUMERIC_PRECISION"]))
			{
				row["NUMERIC_PRECISION"] = 10;
				row["NUMERIC_SCALE"] = 0;
				if (!MetaData.SupportScale(type))
					format = "({0})";
				return String.Format(format, row["NUMERIC_PRECISION"],
					row["NUMERIC_SCALE"]);
			}
			return String.Empty;
		}

		private static void ParseDataTypeSize(MySqlSchemaRow row, string size)
		{
			size = size.Trim('(', ')');
			string[] parts = size.Split(',');

			if (!MetaData.IsNumericType(row["DATA_TYPE"].ToString()))
			{
				row["CHARACTER_MAXIMUM_LENGTH"] = Int32.Parse(parts[0]);
				// will set octet length in a minute
			}
			else
			{
				row["NUMERIC_PRECISION"] = Int32.Parse(parts[0]);
				if (parts.Length == 2)
					row["NUMERIC_SCALE"] = Int32.Parse(parts[1]);
			}
		}

		#endregion

	}
}
