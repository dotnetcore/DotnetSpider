//// Copyright ?2004, 2014, Oracle and/or its affiliates. All rights reserved.
////
//// MySQL Connector/NET is licensed under the terms of the GPLv2
//// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
//// MySQL Connectors. There are special exceptions to the terms and 
//// conditions of the GPLv2 as it is applied to this software, see the 
//// FLOSS License Exception
//// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
////
//// This program is free software; you can redistribute it and/or modify 
//// it under the terms of the GNU General Public License as published 
//// by the Free Software Foundation; version 2 of the License.
////
//// This program is distributed in the hope that it will be useful, but 
//// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
//// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
//// for more details.
////
//// You should have received a copy of the GNU General Public License along 
//// with this program; if not, write to the Free Software Foundation, Inc., 
//// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

//using MySql.Data.Common;
//using System.Collections.Generic;
//using System.Text;
//using System;
//using System.Data;
//using System.Globalization;
//using System.IO;

//#if NET_40_OR_GREATER
//using System.Threading.Tasks;
//using System.Threading;
//#endif
//namespace MySql.Data.MySqlClient
//{
//  /// <summary>
//  /// Provides a class capable of executing a SQL script containing
//  /// multiple SQL statements including CREATE PROCEDURE statements
//  /// that require changing the delimiter
//  /// </summary>
//  public class MySqlScript
//  {
//    private MySqlConnection connection;
//    private string query;
//    private string delimiter;

//    public event MySqlStatementExecutedEventHandler StatementExecuted;
//    public event MySqlScriptErrorEventHandler Error;
//    public event EventHandler ScriptCompleted;

//    #region Constructors

//    /// <summary>
//    /// Initializes a new instance of the 
//    /// <see cref="MySqlScript"/> class.
//    /// </summary>
//    public MySqlScript()
//    {
//      Delimiter = ";";
//    }

//    /// <summary>
//    /// Initializes a new instance of the 
//    /// <see cref="MySqlScript"/> class.
//    /// </summary>
//    /// <param name="connection">The connection.</param>
//    public MySqlScript(MySqlConnection connection)
//      : this()
//    {
//      this.connection = connection;
//    }

//    /// <summary>
//    /// Initializes a new instance of the 
//    /// <see cref="MySqlScript"/> class.
//    /// </summary>
//    /// <param name="query">The query.</param>
//    public MySqlScript(string query)
//      : this()
//    {
//      this.query = query;
//    }

//    /// <summary>
//    /// Initializes a new instance of the 
//    /// <see cref="MySqlScript"/> class.
//    /// </summary>
//    /// <param name="connection">The connection.</param>
//    /// <param name="query">The query.</param>
//    public MySqlScript(MySqlConnection connection, string query)
//      : this()
//    {
//      this.connection = connection;
//      this.query = query;
//    }

//    #endregion

//    #region Properties

//    /// <summary>
//    /// Gets or sets the connection.
//    /// </summary>
//    /// <value>The connection.</value>
//    public MySqlConnection Connection
//    {
//      get { return connection; }
//      set { connection = value; }
//    }

//    /// <summary>
//    /// Gets or sets the query.
//    /// </summary>
//    /// <value>The query.</value>
//    public string Query
//    {
//      get { return query; }
//      set { query = value; }
//    }

//    /// <summary>
//    /// Gets or sets the delimiter.
//    /// </summary>
//    /// <value>The delimiter.</value>
//    public string Delimiter
//    {
//      get { return delimiter; }
//      set { delimiter = value; }
//    }

//    #endregion

//    #region Public Methods

//    /// <summary>
//    /// Executes this instance.
//    /// </summary>
//    /// <returns>The number of statements executed as part of the script.</returns>
//    public int Execute()
//    {
//      bool openedConnection = false;

//      if (connection == null)
//        throw new InvalidOperationException(Resources.ConnectionNotSet);
//      if (query == null || query.Length == 0)
//        return 0;

//      // next we open up the connetion if it is not already open
//      if (connection.State != ConnectionState.Open)
//      {
//        openedConnection = true;
//        connection.Open();
//      }

//      // since we don't allow setting of parameters on a script we can 
//      // therefore safely allow the use of user variables.  no one should be using
//      // this connection while we are using it so we can temporarily tell it
//      // to allow the use of user variables
//      bool allowUserVars = connection.Settings.AllowUserVariables;
//      connection.Settings.AllowUserVariables = true;

//      try
//      {
//        string mode = connection.driver.Property("sql_mode");
//        mode = StringUtility.ToUpperInvariant(mode);
//        bool ansiQuotes = mode.IndexOf("ANSI_QUOTES") != -1;
//        bool noBackslashEscapes = mode.IndexOf("NO_BACKSLASH_ESCAPES") != -1;

//        // first we break the query up into smaller queries
//        List<ScriptStatement> statements = BreakIntoStatements(ansiQuotes, noBackslashEscapes);

//        int count = 0;
//        MySqlCommand cmd = new MySqlCommand(null, connection);
//        foreach (ScriptStatement statement in statements)
//        {
//          if (String.IsNullOrEmpty(statement.text)) continue;
//          cmd.CommandText = statement.text;
//          try
//          {
//            cmd.ExecuteNonQuery();
//            count++;
//            OnQueryExecuted(statement);
//          }
//          catch (Exception ex)
//          {
//            if (Error == null)
//              throw;
//            if (!OnScriptError(ex))
//              break;
//          }
//        }
//        OnScriptCompleted();
//        return count;
//      }
//      finally
//      {
//        connection.Settings.AllowUserVariables = allowUserVars;
//        if (openedConnection)
//        {
//          connection.Close();
//        }
//      }
//    }

//    #endregion

//    private void OnQueryExecuted(ScriptStatement statement)
//    {
//      if (StatementExecuted != null)
//      {
//        MySqlScriptEventArgs args = new MySqlScriptEventArgs();
//        args.Statement = statement;
//        StatementExecuted(this, args);
//      }
//    }

//    private void OnScriptCompleted()
//    {
//      if (ScriptCompleted != null)
//        ScriptCompleted(this, EventArgs.Empty);
//    }

//    private bool OnScriptError(Exception ex)
//    {
//      if (Error != null)
//      {
//        MySqlScriptErrorEventArgs args = new MySqlScriptErrorEventArgs(ex);
//        Error(this, args);
//        return args.Ignore;
//      }
//      return false;
//    }

//    private List<int> BreakScriptIntoLines()
//    {
//      List<int> lineNumbers = new List<int>();

//      StringReader sr = new StringReader(query);
//      string line = sr.ReadLine();
//      int pos = 0;
//      while (line != null)
//      {
//        lineNumbers.Add(pos);
//        pos += line.Length;
//        line = sr.ReadLine();
//      }
//      return lineNumbers;
//    }

//    private static int FindLineNumber(int position, List<int> lineNumbers)
//    {
//      int i = 0;
//      while (i < lineNumbers.Count && position < lineNumbers[i])
//        i++;
//      return i;
//    }

//    private List<ScriptStatement> BreakIntoStatements(bool ansiQuotes, bool noBackslashEscapes)
//    {
//      string currentDelimiter = Delimiter;
//      int startPos = 0;
//      List<ScriptStatement> statements = new List<ScriptStatement>();
//      List<int> lineNumbers = BreakScriptIntoLines();
//      MySqlTokenizer tokenizer = new MySqlTokenizer(query);

//      tokenizer.AnsiQuotes = ansiQuotes;
//      tokenizer.BackslashEscapes = !noBackslashEscapes;

//      string token = tokenizer.NextToken();
//      while (token != null)
//      {
//        if (!tokenizer.Quoted)
//        {
//          if (token.ToLower(CultureInfo.InvariantCulture) == "delimiter")
//          {
//            tokenizer.NextToken();
//            AdjustDelimiterEnd(tokenizer);
//            currentDelimiter = query.Substring(tokenizer.StartIndex,
//              tokenizer.StopIndex - tokenizer.StartIndex).Trim();
//            startPos = tokenizer.StopIndex;
//          }
//          else
//          {
//            // this handles the case where our tokenizer reads part of the
//            // delimiter
//            if (currentDelimiter.StartsWith(token, StringComparison.OrdinalIgnoreCase))
//            {
//              if ((tokenizer.StartIndex + currentDelimiter.Length) <= query.Length)
//              {
//                if (query.Substring(tokenizer.StartIndex, currentDelimiter.Length) == currentDelimiter)
//                {
//                  token = currentDelimiter;
//                  tokenizer.Position = tokenizer.StartIndex + currentDelimiter.Length;
//                  tokenizer.StopIndex = tokenizer.Position;
//                }
//              }
//            }

//            int delimiterPos = token.IndexOf(currentDelimiter, StringComparison.OrdinalIgnoreCase);
//            if (delimiterPos != -1)
//            {
//              int endPos = tokenizer.StopIndex - token.Length + delimiterPos;
//              if (tokenizer.StopIndex == query.Length - 1)
//                endPos++;
//              string currentQuery = query.Substring(startPos, endPos - startPos);
//              ScriptStatement statement = new ScriptStatement();
//              statement.text = currentQuery.Trim();
//              statement.line = FindLineNumber(startPos, lineNumbers);
//              statement.position = startPos - lineNumbers[statement.line];
//              statements.Add(statement);
//              startPos = endPos + currentDelimiter.Length;
//            }
//          }
//        }
//        token = tokenizer.NextToken();
//      }

//      // now clean up the last statement
//      if (startPos < query.Length - 1)
//      {
//        string sqlLeftOver = query.Substring(startPos).Trim();
//        if (!String.IsNullOrEmpty(sqlLeftOver))
//        {
//          ScriptStatement statement = new ScriptStatement();
//          statement.text = sqlLeftOver;
//          statement.line = FindLineNumber(startPos, lineNumbers);
//          statement.position = startPos - lineNumbers[statement.line];
//          statements.Add(statement);
//        }
//      }
//      return statements;
//    }

//    private void AdjustDelimiterEnd(MySqlTokenizer tokenizer)
//    {
//      if (tokenizer.StopIndex < query.Length)
//      {
//        int pos = tokenizer.StopIndex;
//        char c = query[pos];

//        while (!Char.IsWhiteSpace(c) && pos < (query.Length - 1))
//        {
//          c = query[++pos];
//        }
//        tokenizer.StopIndex = pos;
//        tokenizer.Position = pos;
//      }
//    }

//#if NET_40_OR_GREATER
//    #region Async
//    /// <summary>
//    /// Async version of Execute
//    /// </summary>
//    /// <returns>The number of statements executed as part of the script inside.</returns>
//    public Task<int> ExecuteAsync()
//    {
//      return ExecuteAsync(CancellationToken.None);
//    }

//    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
//    {
//      var result = new TaskCompletionSource<int>();
//      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
//      {
//        try
//        {
//          var executeResult = Execute();
//          result.SetResult(executeResult);
//        }
//        catch (Exception ex)
//        {
//          result.SetException(ex);
//        }
//      }
//      else
//      {
//        result.SetCanceled();
//      }
//      return result.Task;
//    }
//    #endregion
//#endif
//  }

//  /// <summary>
//  /// 
//  /// </summary>
//  public delegate void MySqlStatementExecutedEventHandler(object sender, MySqlScriptEventArgs args);
//  /// <summary>
//  /// 
//  /// </summary>
//  public delegate void MySqlScriptErrorEventHandler(object sender, MySqlScriptErrorEventArgs args);

//  /// <summary>
//  /// 
//  /// </summary>
//  public class MySqlScriptEventArgs : EventArgs
//  {
//    private ScriptStatement statement;

//    internal ScriptStatement Statement
//    {
//      set { this.statement = value; }
//    }

//    /// <summary>
//    /// Gets the statement text.
//    /// </summary>
//    /// <value>The statement text.</value>
//    public string StatementText
//    {
//      get { return statement.text; }
//    }

//    /// <summary>
//    /// Gets the line.
//    /// </summary>
//    /// <value>The line.</value>
//    public int Line
//    {
//      get { return statement.line; }
//    }

//    /// <summary>
//    /// Gets the position.
//    /// </summary>
//    /// <value>The position.</value>
//    public int Position
//    {
//      get { return statement.position; }
//    }
//  }

//  /// <summary>
//  /// 
//  /// </summary>
//  public class MySqlScriptErrorEventArgs : MySqlScriptEventArgs
//  {
//    private Exception exception;
//    private bool ignore;

//    /// <summary>
//    /// Initializes a new instance of the <see cref="MySqlScriptErrorEventArgs"/> class.
//    /// </summary>
//    /// <param name="exception">The exception.</param>
//    public MySqlScriptErrorEventArgs(Exception exception)
//      : base()
//    {
//      this.exception = exception;
//    }

//    /// <summary>
//    /// Gets the exception.
//    /// </summary>
//    /// <value>The exception.</value>
//    public Exception Exception
//    {
//      get { return exception; }
//    }

//    /// <summary>
//    /// Gets or sets a value indicating whether this <see cref="MySqlScriptErrorEventArgs"/> is ignore.
//    /// </summary>
//    /// <value><c>true</c> if ignore; otherwise, <c>false</c>.</value>
//    public bool Ignore
//    {
//      get { return ignore; }
//      set { ignore = value; }
//    }
//  }

//  struct ScriptStatement
//  {
//    public string text;
//    public int line;
//    public int position;
//  }
//}
