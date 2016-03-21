// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; version 3 of the License.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License
// for more details.
//
// You should have received a copy of the GNU Lesser General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using MySql.Data.Common;

namespace MySql.Data.MySqlClient
{
	/// <include file='docs/mysqlcommand.xml' path='docs/ClassSummary/*'/>
	public sealed class MySqlCommand : DbCommand
	{
		private MySqlConnection connection;
		private MySqlTransaction curTransaction;
		private string cmdText;
		private CommandType cmdType;
		private long updatedRowCount;
		private UpdateRowSource updatedRowSource;
		private MySqlParameterCollection parameters;
		private IAsyncResult asyncResult;
		private bool designTimeVisible;
		internal Int64 lastInsertedId;
		private PreparableStatement statement;
		private int commandTimeout;
		private bool canceled;
		private bool resetSqlSelect;
		private List<MySqlCommand> batch;
		private string batchableCommandText;
		private CommandTimer commandTimer;
		private bool useDefaultTimeout;
		private bool shouldCache;
		private int cacheAge;
		private bool internallyCreated;

		/// <include file='docs/mysqlcommand.xml' path='docs/ctor1/*'/>
		public MySqlCommand()
		{
			designTimeVisible = true;
			cmdType = CommandType.Text;
			parameters = new MySqlParameterCollection(this);
			updatedRowSource = UpdateRowSource.Both;
			cmdText = String.Empty;
			useDefaultTimeout = true;
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ctor2/*'/>
		public MySqlCommand(string cmdText)
			: this()
		{
			CommandText = cmdText;
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ctor3/*'/>
		public MySqlCommand(string cmdText, MySqlConnection connection)
			: this(cmdText)
		{
			Connection = connection;
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ctor4/*'/>
		public MySqlCommand(string cmdText, MySqlConnection connection,
				MySqlTransaction transaction)
			:
			this(cmdText, connection)
		{
			curTransaction = transaction;
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/LastInseredId/*'/>
		public Int64 LastInsertedId
		{
			get { return lastInsertedId; }
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/CommandText/*'/>        
		public override string CommandText
		{
			get { return cmdText; }
			set
			{
				cmdText = value;
				statement = null;
				batchableCommandText = null;
				if (cmdText != null && cmdText.EndsWith("DEFAULT VALUES"))
				{
					cmdText = cmdText.Substring(0, cmdText.Length - 14);
					cmdText = cmdText + "() VALUES ()";
				}
			}
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/CommandTimeout/*'/>
		public override int CommandTimeout
		{
			get { return useDefaultTimeout ? 30 : commandTimeout; }
			set
			{
				if (commandTimeout < 0)
					throw new ArgumentException("Command timeout must not be negative");

				// Timeout in milliseconds should not exceed maximum for 32 bit
				// signed integer (~24 days), because underlying driver (and streams)
				// use milliseconds expressed ints for timeout values.
				// Hence, truncate the value.
				int timeout = Math.Min(value, Int32.MaxValue / 1000);
				commandTimeout = timeout;
				useDefaultTimeout = false;
			}
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/CommandType/*'/>        
		public override CommandType CommandType
		{
			get { return cmdType; }
			set { cmdType = value; }
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/IsPrepared/*'/>
		public bool IsPrepared
		{
			get { return statement != null && statement.IsPrepared; }
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/Connection/*'/>
		public new MySqlConnection Connection
		{
			get { return connection; }
			set
			{
				/*
                * The connection is associated with the transaction
                * so set the transaction object to return a null reference if the connection
                * is reset.
                */
				if (connection != value)
					Transaction = null;

				connection = value;

				// if the user has not already set the command timeout, then
				// take the default from the connection
				if (connection != null)
				{
					if (useDefaultTimeout)
					{
						commandTimeout = (int)connection.Settings.DefaultCommandTimeout;
						useDefaultTimeout = false;
					}

					EnableCaching = connection.Settings.TableCaching;
					CacheAge = connection.Settings.DefaultTableCacheAge;
				}
			}
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/Parameters/*'/>
		public new MySqlParameterCollection Parameters
		{
			get { return parameters; }
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/Transaction/*'/>
		public new MySqlTransaction Transaction
		{
			get { return curTransaction; }
			set { curTransaction = value; }
		}

		public bool EnableCaching
		{
			get { return shouldCache; }
			set { shouldCache = value; }
		}

		public int CacheAge
		{
			get { return cacheAge; }
			set { cacheAge = value; }
		}

		internal List<MySqlCommand> Batch
		{
			get { return batch; }
		}

		internal bool Canceled
		{
			get { return canceled; }
		}

		internal string BatchableCommandText
		{
			get { return batchableCommandText; }
		}

		internal bool InternallyCreated
		{
			get { return internallyCreated; }
			set { internallyCreated = value; }
		}

		/// <summary>
		/// Attempts to cancel the execution of a currently active command
		/// </summary>
		/// <remarks>
		/// Cancelling a currently active query only works with MySQL versions 5.0.0 and higher.
		/// </remarks>
		public override void Cancel()
		{
			connection.CancelQuery(connection.ConnectionTimeout);
			canceled = true;
		}

		/// <summary>
		/// Creates a new instance of a <see cref="MySqlParameter"/> object.
		/// </summary>
		/// <remarks>
		/// This method is a strongly-typed version of <see cref="IDbCommand.CreateParameter"/>.
		/// </remarks>
		/// <returns>A <see cref="MySqlParameter"/> object.</returns>
		///
		public new MySqlParameter CreateParameter()
		{
			return (MySqlParameter)CreateDbParameter();
		}

		/// <summary>
		/// Check the connection to make sure
		///		- it is open
		///		- it is not currently being used by a reader
		///		- and we have the right version of MySQL for the requested command type
		/// </summary>
		private void CheckState()
		{
			// There must be a valid and open connection.
			if (connection == null)
				throw new InvalidOperationException("Connection must be valid and open.");

			if (connection.State != ConnectionState.Open && !connection.SoftClosed)
				throw new InvalidOperationException("Connection must be valid and open.");

			// Data readers have to be closed first
			if (connection.IsInUse && !this.internallyCreated)
				throw new MySqlException("There is already an open DataReader associated with this Connection which must be closed first.");
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ExecuteNonQuery/*'/>
		public override int ExecuteNonQuery()
		{
			using (MySqlDataReader reader = ExecuteReader())
			{
				reader.Close();
				return reader.RecordsAffected;
			}
		}

		internal void ClearCommandTimer()
		{
			if (commandTimer != null)
			{
				commandTimer.Dispose();
				commandTimer = null;
			}
		}

		internal void Close(MySqlDataReader reader)
		{
			if (statement != null)
				statement.Close(reader);
			ResetSqlSelectLimit();
			if (statement != null && connection != null && connection.driver != null)
				connection.driver.CloseQuery(connection, statement.StatementId);
			ClearCommandTimer();
		}

		/// <summary>
		/// Reset reader to null, to avoid "There is already an open data reader"
		/// on the next ExecuteReader(). Used in error handling scenarios.
		/// </summary>
		private void ResetReader()
		{
			if (connection != null && connection.Reader != null)
			{
				connection.Reader.Close();
				connection.Reader = null;
			}
		}

		/// <summary>
		/// Reset SQL_SELECT_LIMIT that could have been modified by CommandBehavior.
		/// </summary>
		internal void ResetSqlSelectLimit()
		{
			// if we are supposed to reset the sql select limit, do that here
			if (resetSqlSelect)
			{
				resetSqlSelect = false;
				MySqlCommand command = new MySqlCommand("SET SQL_SELECT_LIMIT=DEFAULT", connection);
				command.internallyCreated = true;
				command.ExecuteNonQuery();
			}
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ExecuteReader/*'/>
		public new MySqlDataReader ExecuteReader()
		{
			return ExecuteReader(CommandBehavior.Default);
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ExecuteReader1/*'/>
		public new MySqlDataReader ExecuteReader(CommandBehavior behavior)
		{
 

			// interceptors didn't handle this so we fall through
			bool success = false;
			CheckState();
			Driver driver = connection.driver;

			cmdText = cmdText.Trim();
			 

			string sql = cmdText.Trim(';');

 

			lock (driver)
			{

				// We have to recheck that there is no reader, after we got the lock
				if (connection.Reader != null)
				{
					throw (new MySqlException("Resources.DataReaderOpen"));
				}

 
				commandTimer = new CommandTimer(connection, CommandTimeout);

				lastInsertedId = -1;

				if (CommandType == CommandType.TableDirect)
					sql = "SELECT * FROM " + sql;
				else if (CommandType == CommandType.Text)
				{
					// validates single word statetment (maybe is a stored procedure call)
					if (sql.IndexOf(" ") == -1)
					{
						if (AddCallStatement(sql))
							sql = "call " + sql;
					}
				}

				// if we are on a replicated connection, we are only allow readonly statements
				if (connection.Settings.Replication && !InternallyCreated)
					EnsureCommandIsReadOnly(sql);

				if (statement == null || !statement.IsPrepared)
				{
					if (CommandType == CommandType.StoredProcedure)
						statement = new StoredProcedure(this, sql);
					else
						statement = new PreparableStatement(this, sql);
				}

				// stored procs are the only statement type that need do anything during resolve
				statement.Resolve(false);

				// Now that we have completed our resolve step, we can handle our
				// command behaviors
				HandleCommandBehaviors(behavior);

				updatedRowCount = -1;
				try
				{
					MySqlDataReader reader = new MySqlDataReader(this, statement, behavior);
					connection.Reader = reader;
					canceled = false;
					// execute the statement
					statement.Execute();
					// wait for data to return
					reader.NextResult();
					success = true;
					return reader;
				}
				catch (TimeoutException tex)
				{
					connection.HandleTimeoutOrThreadAbort(tex);
					throw; //unreached
				}
				catch (ThreadAbortException taex)
				{
					connection.HandleTimeoutOrThreadAbort(taex);
					throw;
				}
				catch (IOException ioex)
				{
					connection.Abort(); // Closes connection without returning it to the pool
					throw new MySqlException("Resources.FatalErrorDuringExecute", ioex);
				}
				catch (MySqlException ex)
				{

					if (ex.InnerException is TimeoutException)
						throw; // already handled

					try
					{
						ResetReader();
						ResetSqlSelectLimit();
					}
					catch (Exception)
					{
						// Reset SqlLimit did not work, connection is hosed.
						Connection.Abort();
						throw new MySqlException(ex.Message, true, ex);
					}

					// if we caught an exception because of a cancel, then just return null
					if (ex.IsQueryAborted)
						return null;
					if (ex.IsFatal)
						Connection.Close();
					if (ex.Number == 0)
						throw new MySqlException("Resources.FatalErrorDuringExecute", ex);
					throw;
				}
				finally
				{
					if (connection != null)
					{
						if (connection.Reader == null)
						{
							// Something went seriously wrong,  and reader would not
							// be able to clear timeout on closing.
							// So we clear timeout here.
							ClearCommandTimer();
						}
						if (!success)
						{
							// ExecuteReader failed.Close Reader and set to null to 
							// prevent subsequent errors with DataReaderOpen
							ResetReader();
						}
					}
				}
			}
		}
		private static List<string> keywords = null;
		/// <summary>
		/// Verifies if a query is valid even if it has not spaces or is a stored procedure call
		/// </summary>
		/// <param name="query">Query to validate</param>
		/// <returns>If it is necessary to add call statement</returns>
		private bool AddCallStatement(string query)
		{
			if (string.IsNullOrEmpty(query)) return false;

			string keyword = query.ToUpper();
			int indexChar = keyword.IndexOfAny(new char[] { '(', '"', '@', '\'', '`' });
			if (indexChar > 0)
				keyword = keyword.Substring(0, indexChar);

			if (keywords == null)
				keywords = new List<string>(@"ACCESSIBLE
ADD
ALL
ALTER
ANALYZE
AND
AS
ASC
ASENSITIVE
BEFORE
BETWEEN
BIGINT
BINARY
BLOB
BOTH
BY
CALL
CASCADE
CASE
CHANGE
CHAR
CHARACTER
CHECK
COLLATE
COLUMN
CONDITION
CONNECTION
CONSTRAINT
CONTINUE
CONVERT
CREATE
CROSS
CURRENT_DATE
CURRENT_TIME
CURRENT_TIMESTAMP
CURRENT_USER
CURSOR
DATABASE
DATABASES
DAY_HOUR
DAY_MICROSECOND
DAY_MINUTE
DAY_SECOND
DEC
DECIMAL
DECLARE
DEFAULT
DELAYED
DELETE
DESC
DESCRIBE
DETERMINISTIC
DISTINCT
DISTINCTROW
DIV
DOUBLE
DROP
DUAL
EACH
ELSE
ELSEIF
ENCLOSED
ESCAPED
EXISTS
EXIT
EXPLAIN
FALSE
FETCH
FLOAT
FLOAT4
FLOAT8
FOR
FORCE
FOREIGN
FROM
FULLTEXT
GOTO
GRANT
GROUP
HAVING
HIGH_PRIORITY
HOUR_MICROSECOND
HOUR_MINUTE
HOUR_SECOND
IF
IGNORE
IN
INDEX
INFILE
INNER
INOUT
INSENSITIVE
INSERT
INT
INT1
INT2
INT3
INT4
INT8
INTEGER
INTERVAL
INTO
IS
ITERATE
JOIN
KEY
KEYS
KILL
LABEL
LEADING
LEAVE
LEFT
LIKE
LIMIT
LINEAR
LINES
LOAD
LOCALTIME
LOCALTIMESTAMP
LOCK
LONG
LONGBLOB
LONGTEXT
LOOP
LOW_PRIORITY
MASTER_SSL_VERIFY_SERVER_CERT
MATCH
MEDIUMBLOB
MEDIUMINT
MEDIUMTEXT
MIDDLEINT
MINUTE_MICROSECOND
MINUTE_SECOND
MOD
MODIFIES
NATURAL
NOT
NO_WRITE_TO_BINLOG
NULL
NUMERIC
ON
OPTIMIZE
OPTION
OPTIONALLY
OR
ORDER
OUT
OUTER
OUTFILE
PRECISION
PRIMARY
PROCEDURE
PURGE
RANGE
READ
READS
READ_ONLY
READ_WRITE
REAL
REFERENCES
REGEXP
RELEASE
RENAME
REPEAT
REPLACE
REQUIRE
RESTRICT
RETURN
REVOKE
RIGHT
RLIKE
SCHEMA
SCHEMAS
SECOND_MICROSECOND
SELECT
SENSITIVE
SEPARATOR
SET
SHOW
SMALLINT
SPATIAL
SPECIFIC
SQL
SQLEXCEPTION
SQLSTATE
SQLWARNING
SQL_BIG_RESULT
SQL_CALC_FOUND_ROWS
SQL_SMALL_RESULT
SSL
STARTING
STRAIGHT_JOIN
TABLE
TERMINATED
THEN
TINYBLOB
TINYINT
TINYTEXT
TO
TRAILING
TRIGGER
TRUE
UNDO
UNION
UNIQUE
UNLOCK
UNSIGNED
UPDATE
UPGRADE
USAGE
USE
USING
UTC_DATE
UTC_TIME
UTC_TIMESTAMP
VALUE
VALUES
VARBINARY
VARCHAR
VARCHARACTER
VARYING
WHEN
WHERE
WHILE
WITH
WRITE
XOR
YEAR_MONTH
ZEROFILL".Replace("\r", "").Split('\n'));

			return !keywords.Contains(keyword);
		}

		private void EnsureCommandIsReadOnly(string sql)
		{
			sql = sql.ToLower();
			if (!sql.StartsWith("select") && !sql.StartsWith("show"))
				throw new MySqlException("ResourceStrings.ReplicatedConnectionsAllowOnlyReadonlyStatements");
			if (sql.EndsWith("for update") || sql.EndsWith("lock in share mode"))
				throw new MySqlException("ResourceStrings.ReplicatedConnectionsAllowOnlyReadonlyStatements");
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ExecuteScalar/*'/>
		public override object ExecuteScalar()
		{
			lastInsertedId = -1;
			object val = null;

			using (MySqlDataReader reader = ExecuteReader())
			{
				if (reader.Read())
					val = reader.GetValue(0);
			}

			return val;
		}

		private void HandleCommandBehaviors(CommandBehavior behavior)
		{
			if ((behavior & CommandBehavior.SchemaOnly) != 0)
			{
				new MySqlCommand("SET SQL_SELECT_LIMIT=0", connection).ExecuteNonQuery();
				resetSqlSelect = true;
			}
			else if ((behavior & CommandBehavior.SingleRow) != 0)
			{
				new MySqlCommand("SET SQL_SELECT_LIMIT=1", connection).ExecuteNonQuery();
				resetSqlSelect = true;
			}
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/Prepare2/*'/>
		private void Prepare(int cursorPageSize)
		{
			using (new CommandTimer(Connection, CommandTimeout))
			{
				// if the length of the command text is zero, then just return
				string psSQL = CommandText;
				if (psSQL == null ||
					 psSQL.Trim().Length == 0)
					return;
				statement = new PreparableStatement(this, CommandText);
				statement.Resolve(true);
				statement.Prepare();
			}
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/Prepare/*'/>
		public override void Prepare()
		{
			if (connection == null)
				throw new InvalidOperationException("The connection property has not been set.");
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException("The connection is not open.");
			if (connection.Settings.IgnorePrepare)
				return;

			Prepare(0);
		}

		internal delegate object AsyncDelegate(int type, CommandBehavior behavior);

		internal AsyncDelegate caller = null;
		internal Exception thrownException;

		internal object AsyncExecuteWrapper(int type, CommandBehavior behavior)
		{
			thrownException = null;
			try
			{
				if (type == 1)
					return ExecuteReader(behavior);
				return ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				thrownException = ex;
			}
			return null;
		}

		/// <summary>
		/// Initiates the asynchronous execution of the SQL statement or stored procedure
		/// that is described by this <see cref="MySqlCommand"/>, and retrieves one or more
		/// result sets from the server.
		/// </summary>
		/// <returns>An <see cref="IAsyncResult"/> that can be used to poll, wait for results,
		/// or both; this value is also needed when invoking EndExecuteReader,
		/// which returns a <see cref="MySqlDataReader"/> instance that can be used to retrieve
		/// the returned rows. </returns>
		public IAsyncResult BeginExecuteReader()
		{
			return BeginExecuteReader(CommandBehavior.Default);
		}

		/// <summary>
		/// Initiates the asynchronous execution of the SQL statement or stored procedure
		/// that is described by this <see cref="MySqlCommand"/> using one of the
		/// <b>CommandBehavior</b> values.
		/// </summary>
		/// <param name="behavior">One of the <see cref="CommandBehavior"/> values, indicating
		/// options for statement execution and data retrieval.</param>
		/// <returns>An <see cref="IAsyncResult"/> that can be used to poll, wait for results,
		/// or both; this value is also needed when invoking EndExecuteReader,
		/// which returns a <see cref="MySqlDataReader"/> instance that can be used to retrieve
		/// the returned rows. </returns>
		public IAsyncResult BeginExecuteReader(CommandBehavior behavior)
		{
			if (caller != null)
				throw new MySqlException("ResourceStrings.UnableToStartSecondAsyncOp");

			caller = new AsyncDelegate(AsyncExecuteWrapper);
			asyncResult = caller.BeginInvoke(1, behavior, null, null);
			return asyncResult;
		}

		/// <summary>
		/// Finishes asynchronous execution of a SQL statement, returning the requested
		/// <see cref="MySqlDataReader"/>.
		/// </summary>
		/// <param name="result">The <see cref="IAsyncResult"/> returned by the call to
		/// <see cref="BeginExecuteReader()"/>.</param>
		/// <returns>A <b>MySqlDataReader</b> object that can be used to retrieve the requested rows. </returns>
		public MySqlDataReader EndExecuteReader(IAsyncResult result)
		{
			result.AsyncWaitHandle.WaitOne();
			AsyncDelegate c = caller;
			caller = null;
			if (thrownException != null)
				throw thrownException;
			return (MySqlDataReader)c.EndInvoke(result);
		}

		/// <summary>
		/// Initiates the asynchronous execution of the SQL statement or stored procedure
		/// that is described by this <see cref="MySqlCommand"/>.
		/// </summary>
		/// <param name="callback">
		/// An <see cref="AsyncCallback"/> delegate that is invoked when the command's
		/// execution has completed. Pass a null reference (<b>Nothing</b> in Visual Basic)
		/// to indicate that no callback is required.</param>
		/// <param name="stateObject">A user-defined state object that is passed to the
		/// callback procedure. Retrieve this object from within the callback procedure
		/// using the <see cref="IAsyncResult.AsyncState"/> property.</param>
		/// <returns>An <see cref="IAsyncResult"/> that can be used to poll or wait for results,
		/// or both; this value is also needed when invoking <see cref="EndExecuteNonQuery"/>,
		/// which returns the number of affected rows. </returns>
		public IAsyncResult BeginExecuteNonQuery(AsyncCallback callback, object stateObject)
		{
			if (caller != null)
				throw new MySqlException("ResourceStrings.UnableToStartSecondAsyncOp");

			caller = new AsyncDelegate(AsyncExecuteWrapper);
			asyncResult = caller.BeginInvoke(2, CommandBehavior.Default,
				callback, stateObject);
			return asyncResult;
		}

		/// <summary>
		/// Initiates the asynchronous execution of the SQL statement or stored procedure
		/// that is described by this <see cref="MySqlCommand"/>.
		/// </summary>
		/// <returns>An <see cref="IAsyncResult"/> that can be used to poll or wait for results,
		/// or both; this value is also needed when invoking <see cref="EndExecuteNonQuery"/>,
		/// which returns the number of affected rows. </returns>
		public IAsyncResult BeginExecuteNonQuery()
		{
			if (caller != null)
				throw new MySqlException("ResourceStrings.UnableToStartSecondAsyncOp");

			caller = new AsyncDelegate(AsyncExecuteWrapper);
			asyncResult = caller.BeginInvoke(2, CommandBehavior.Default, null, null);
			return asyncResult;
		}

		/// <summary>
		/// Finishes asynchronous execution of a SQL statement.
		/// </summary>
		/// <param name="asyncResult">The <see cref="IAsyncResult"/> returned by the call
		/// to <see cref="BeginExecuteNonQuery()"/>.</param>
		/// <returns></returns>
		public int EndExecuteNonQuery(IAsyncResult asyncResult)
		{
			asyncResult.AsyncWaitHandle.WaitOne();
			AsyncDelegate c = caller;
			caller = null;
			if (thrownException != null)
				throw thrownException;
			return (int)c.EndInvoke(asyncResult);
		}

		internal long EstimatedSize()
		{
			long size = CommandText.Length;
			foreach (MySqlParameter parameter in Parameters)
				size += parameter.EstimatedSize();
			return size;
		}

		/// <summary>
		/// Creates a clone of this MySqlCommand object.  CommandText, Connection, and Transaction properties
		/// are included as well as the entire parameter list.
		/// </summary>
		/// <returns>The cloned MySqlCommand object</returns>
		public MySqlCommand Clone()
		{
			MySqlCommand clone = new MySqlCommand(cmdText, connection, curTransaction);
			clone.CommandType = CommandType;
			clone.commandTimeout = commandTimeout;
			clone.useDefaultTimeout = useDefaultTimeout;
			clone.batchableCommandText = batchableCommandText;
			clone.UpdatedRowSource = UpdatedRowSource;
			clone.EnableCaching = EnableCaching;
			clone.CacheAge = CacheAge;

			foreach (MySqlParameter p in parameters)
			{
				clone.Parameters.Add(p.Clone());
			}
			return clone;
		}

		internal void AddToBatch(MySqlCommand command)
		{
			if (batch == null)
				batch = new List<MySqlCommand>();
			batch.Add(command);
		}

		internal string GetCommandTextForBatching()
		{
			if (batchableCommandText == null)
			{
				// if the command starts with insert and is "simple" enough, then
				// we can use the multi-value form of insert
				if (String.Compare(CommandText.Substring(0, 6), "INSERT", true) == 0)
				{
					MySqlCommand cmd = new MySqlCommand("SELECT @@sql_mode", Connection);
					string sql_mode = cmd.ExecuteScalar().ToString().ToUpper();
					MySqlTokenizer tokenizer = new MySqlTokenizer(CommandText);
					tokenizer.AnsiQuotes = sql_mode.IndexOf("ANSI_QUOTES") != -1;
					tokenizer.BackslashEscapes = sql_mode.IndexOf("NO_BACKSLASH_ESCAPES") == -1;
					string token = tokenizer.NextToken().ToLower();
					while (token != null)
					{
						if (token.ToUpper() == "VALUES" &&
							!tokenizer.Quoted)
						{
							token = tokenizer.NextToken();
							Debug.Assert(token == "(");

							// find matching right parameter, and ensure that pares
							// are balanced.
							int openParenCount = 1;
							while (token != null)
							{
								batchableCommandText += token;
								token = tokenizer.NextToken();

								if (token == "(")
									openParenCount++;
								else if (token == ")")
									openParenCount--;

								if (openParenCount == 0)
									break;
							}

							if (token != null)
								batchableCommandText += token;
							token = tokenizer.NextToken();
							if (token != null && (token == "," ||
								token.ToUpper() == "ON"))
							{
								batchableCommandText = null;
								break;
							}
						}
						token = tokenizer.NextToken();
					}
				}
				// Otherwise use the command verbatim
				else batchableCommandText = CommandText;
			}

			return batchableCommandText;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (statement != null && statement.IsPrepared)
					statement.CloseStatement();
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the command object should be visible in a Windows Form Designer control.
		/// </summary>
		public override bool DesignTimeVisible
		{
			get
			{
				return designTimeVisible;
			}
			set
			{
				designTimeVisible = value;
			}
		}

		/// <summary>
		/// Gets or sets how command results are applied to the DataRow when used by the
		/// Update method of the DbDataAdapter.
		/// </summary>
		public override UpdateRowSource UpdatedRowSource
		{
			get
			{
				return updatedRowSource;
			}
			set
			{
				updatedRowSource = value;
			}
		}

		protected override DbParameter CreateDbParameter()
		{
			return new MySqlParameter();
		}

		protected override DbConnection DbConnection
		{
			get { return Connection; }
			set { Connection = (MySqlConnection)value; }
		}

		protected override DbParameterCollection DbParameterCollection
		{
			get { return Parameters; }
		}

		protected override DbTransaction DbTransaction
		{
			get { return Transaction; }
			set { Transaction = (MySqlTransaction)value; }
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			return ExecuteReader(behavior);
		}
	}
}