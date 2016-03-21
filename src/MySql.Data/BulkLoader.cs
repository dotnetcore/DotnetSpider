// Copyright (c) 2006-2008 MySQL AB, 2008-2009 Sun Microsystems, Inc. 2014, Oracle and/or its affiliates. All rights reserved.
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
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Data;

using MySql.Data.Common;
#if NET_40_OR_GREATER
using System.Threading.Tasks;
using System.Threading;
#endif

namespace MySql.Data.MySqlClient
{
  /// <summary>
  /// 
  /// </summary>
  public class MySqlBulkLoader
  {
    // constant values
    private const string defaultFieldTerminator = "\t";
    private const string defaultLineTerminator = "\n";
    private const char defaultEscapeCharacter = '\\';

    // fields
    private string fieldTerminator;
    private string lineTerminator;
    private string charSet;
    private string tableName;
    private int numLinesToIgnore;
    private MySqlConnection connection;
    private string filename;
    private int timeout;
    private bool local;
    private string linePrefix;
    private char fieldQuotationCharacter;
    private bool fieldQuotationOptional;
    private char escapeChar;
    private MySqlBulkLoaderPriority priority;
    private MySqlBulkLoaderConflictOption conflictOption;
    private List<string> columns;
    private List<string> expressions;

    public MySqlBulkLoader(MySqlConnection connection)
    {
      Connection = connection;
      Local = true;
      FieldTerminator = defaultFieldTerminator;
      LineTerminator = defaultLineTerminator;
      FieldQuotationCharacter = Char.MinValue;
      ConflictOption = MySqlBulkLoaderConflictOption.None;
      columns = new List<string>();
      expressions = new List<string>();
    }

    #region Properties

    /// <summary>
    /// Gets or sets the connection.
    /// </summary>
    /// <value>The connection.</value>
    public MySqlConnection Connection
    {
      get { return connection; }
      set { connection = value; }
    }

    /// <summary>
    /// Gets or sets the field terminator.
    /// </summary>
    /// <value>The field terminator.</value>
    public string FieldTerminator
    {
      get { return fieldTerminator; }
      set { fieldTerminator = value; }
    }

    /// <summary>
    /// Gets or sets the line terminator.
    /// </summary>
    /// <value>The line terminator.</value>
    public string LineTerminator
    {
      get { return lineTerminator; }
      set { lineTerminator = value; }
    }

    /// <summary>
    /// Gets or sets the name of the table.
    /// </summary>
    /// <value>The name of the table.</value>
    public string TableName
    {
      get { return tableName; }
      set { tableName = value; }
    }

    /// <summary>
    /// Gets or sets the character set.
    /// </summary>
    /// <value>The character set.</value>
    public string CharacterSet
    {
      get { return charSet; }
      set { charSet = value; }
    }

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    public string FileName
    {
      get { return filename; }
      set { filename = value; }
    }

    /// <summary>
    /// Gets or sets the timeout.
    /// </summary>
    /// <value>The timeout.</value>
    public int Timeout
    {
      get { return timeout; }
      set { timeout = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the filename that is to be loaded
    /// is local to the client or not
    /// </summary>
    /// <value><c>true</c> if local; otherwise, <c>false</c>.</value>
    public bool Local
    {
      get { return local; }
      set { local = value; }
    }

    /// <summary>
    /// Gets or sets the number of lines to skip.
    /// </summary>
    /// <value>The number of lines to skip.</value>
    public int NumberOfLinesToSkip
    {
      get { return numLinesToIgnore; }
      set { numLinesToIgnore = value; }
    }

    /// <summary>
    /// Gets or sets the line prefix.
    /// </summary>
    /// <value>The line prefix.</value>
    public string LinePrefix
    {
      get { return linePrefix; }
      set { linePrefix = value; }
    }

    /// <summary>
    /// Gets or sets the field quotation character.
    /// </summary>
    /// <value>The field quotation character.</value>
    public char FieldQuotationCharacter
    {
      get { return fieldQuotationCharacter; }
      set { fieldQuotationCharacter = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether [field quotation optional].
    /// </summary>
    /// <value>
    /// 	<c>true</c> if [field quotation optional]; otherwise, <c>false</c>.
    /// </value>
    public bool FieldQuotationOptional
    {
      get { return fieldQuotationOptional; }
      set { fieldQuotationOptional = value; }
    }

    /// <summary>
    /// Gets or sets the escape character.
    /// </summary>
    /// <value>The escape character.</value>
    public char EscapeCharacter
    {
      get { return escapeChar; }
      set { escapeChar = value; }
    }

    /// <summary>
    /// Gets or sets the conflict option.
    /// </summary>
    /// <value>The conflict option.</value>
    public MySqlBulkLoaderConflictOption ConflictOption
    {
      get { return conflictOption; }
      set { conflictOption = value; }
    }

    /// <summary>
    /// Gets or sets the priority.
    /// </summary>
    /// <value>The priority.</value>
    public MySqlBulkLoaderPriority Priority
    {
      get { return priority; }
      set { priority = value; }
    }

    /// <summary>
    /// Gets the columns.
    /// </summary>
    /// <value>The columns.</value>
    public List<string> Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// Gets the expressions.
    /// </summary>
    /// <value>The expressions.</value>
    public List<string> Expressions
    {
      get { return expressions; }
    }

    #endregion

    /// <summary>
    /// Execute the load operation
    /// </summary>
    /// <returns>The number of rows inserted.</returns>
    public int Load()
    {
      bool openedConnection = false;

      if (Connection == null)
        throw new InvalidOperationException("Resources.ConnectionNotSet");

	  // next we open up the connetion if it is not already open
			if (connection.State != ConnectionState.Open)
      {
        openedConnection = true;
        connection.Open();
      }

      try
      {
        string sql = BuildSqlCommand();
        MySqlCommand cmd = new MySqlCommand(sql, Connection);
        cmd.CommandTimeout = Timeout;
        return cmd.ExecuteNonQuery();
      }
      finally
      {
        if (openedConnection)
          connection.Close();
      }
    }

#if NET_40_OR_GREATER
    #region Async
    /// <summary>
    /// Async version of Load
    /// </summary>
    /// <returns>The number of rows inserted.</returns>
    public Task<int> LoadAsync()
    {
      return LoadAsync(CancellationToken.None);
    }

    public Task<int> LoadAsync(CancellationToken cancellationToken)
    {
      var result = new TaskCompletionSource<int>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          int loadResult = Load();
          result.SetResult(loadResult);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }
    #endregion
#endif

    private string BuildSqlCommand()
    {
      StringBuilder sql = new StringBuilder("LOAD DATA ");
      if (Priority == MySqlBulkLoaderPriority.Low)
        sql.Append("LOW_PRIORITY ");
      else if (Priority == MySqlBulkLoaderPriority.Concurrent)
        sql.Append("CONCURRENT ");

      if (Local)
        sql.Append("LOCAL ");
      sql.Append("INFILE ");
      if (Platform.DirectorySeparatorChar == '\\')
        sql.AppendFormat("'{0}' ", FileName.Replace(@"\", @"\\"));
      else
        sql.AppendFormat("'{0}' ", FileName);

      if (ConflictOption == MySqlBulkLoaderConflictOption.Ignore)
        sql.Append("IGNORE ");
      else if (ConflictOption == MySqlBulkLoaderConflictOption.Replace)
        sql.Append("REPLACE ");

      sql.AppendFormat("INTO TABLE {0} ", TableName);

      if (CharacterSet != null)
        sql.AppendFormat("CHARACTER SET {0} ", CharacterSet);

      StringBuilder optionSql = new StringBuilder(String.Empty);
      if (FieldTerminator != defaultFieldTerminator)
        optionSql.AppendFormat("TERMINATED BY '{0}' ", FieldTerminator);
      if (FieldQuotationCharacter != Char.MinValue)
        optionSql.AppendFormat("{0} ENCLOSED BY '{1}' ",
            FieldQuotationOptional ? "OPTIONALLY" : "", FieldQuotationCharacter);
      if (EscapeCharacter != defaultEscapeCharacter &&
          EscapeCharacter != Char.MinValue)
        optionSql.AppendFormat("ESCAPED BY '{0}' ", EscapeCharacter);
      if (optionSql.Length > 0)
        sql.AppendFormat("FIELDS {0}", optionSql.ToString());

      optionSql = new StringBuilder(String.Empty);
      if (LinePrefix != null && LinePrefix.Length > 0)
        optionSql.AppendFormat("STARTING BY '{0}' ", LinePrefix);
      if (LineTerminator != defaultLineTerminator)
        optionSql.AppendFormat("TERMINATED BY '{0}' ", LineTerminator);
      if (optionSql.Length > 0)
        sql.AppendFormat("LINES {0}", optionSql.ToString());

      if (NumberOfLinesToSkip > 0)
        sql.AppendFormat("IGNORE {0} LINES ", NumberOfLinesToSkip);

      if (Columns.Count > 0)
      {
        sql.Append("(");
        sql.Append(Columns[0]);
        for (int i = 1; i < Columns.Count; i++)
          sql.AppendFormat(",{0}", Columns[i]);
        sql.Append(") ");
      }

      if (Expressions.Count > 0)
      {
        sql.Append("SET ");
        sql.Append(Expressions[0]);
        for (int i = 1; i < Expressions.Count; i++)
          sql.AppendFormat(",{0}", Expressions[i]);
      }

      return sql.ToString();
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public enum MySqlBulkLoaderPriority
  {
    /// <summary>
    /// This is the default and indicates normal priority
    /// </summary>
    None,
    /// <summary>
    /// Low priority will cause the load operation to wait until all readers of the table
    /// have finished.  This only affects storage engines that use only table-level locking
    /// such as MyISAM, Memory, and Merge.
    /// </summary>
    Low,
    /// <summary>
    /// Concurrent priority is only relevant for MyISAM tables and signals that if the table
    /// has no free blocks in the middle that other readers can retrieve data from the table
    /// while the load operation is happening.
    /// </summary>
    Concurrent
  }

  /// <summary>
  /// 
  /// </summary>
  public enum MySqlBulkLoaderConflictOption
  {
    /// <summary>
    /// This is the default and indicates normal operation.  In the event of a LOCAL load, this
    /// is the same as ignore.  When the data file is on the server, then a key conflict will
    /// cause an error to be thrown and the rest of the data file ignored.
    /// </summary>
    None,
    /// <summary>
    /// Replace column values when a key conflict occurs.
    /// </summary>
    Replace,
    /// <summary>
    /// Ignore any rows where the primary key conflicts.
    /// </summary>
    Ignore
  }

}
