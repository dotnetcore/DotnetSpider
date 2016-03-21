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
using IsolationLevel = System.Data.IsolationLevel;
using MySql.Data.Common;


namespace MySql.Data.MySqlClient
{
	/// <include file='docs/MySqlConnection.xml' path='docs/ClassSummary/*'/>    
	public class MySqlConnection : DbConnection
	{
		private SchemaProvider schemaProvider;
		private ProcedureCache procedureCache;

		public MySqlSchemaCollection GetSchemaCollection(string collectionName, string[] restrictionValues)
		{
			if (collectionName == null)
				collectionName = SchemaProvider.MetaCollection;

			string[] restrictions = schemaProvider.CleanRestrictions(restrictionValues);
			MySqlSchemaCollection c = schemaProvider.GetSchema(collectionName, restrictions);
			return c;
		}

		internal ConnectionState connectionState;
		internal Driver driver;
		private MySqlConnectionStringBuilder settings;
		private bool hasBeenOpen;
		private bool isInUse;
		private bool isKillQueryConnection;
		private string database;
		private int commandTimeout;

		/// <include file='docs/MySqlConnection.xml' path='docs/InfoMessage/*'/>
		public event MySqlInfoMessageEventHandler InfoMessage;

		private static Cache<string, MySqlConnectionStringBuilder> connectionStringCache =
		   new Cache<string, MySqlConnectionStringBuilder>(0, 25);

		/// <include file='docs/MySqlConnection.xml' path='docs/DefaultCtor/*'/>
		public MySqlConnection()
		{
			//TODO: add event data to StateChange docs
			settings = new MySqlConnectionStringBuilder();
			database = String.Empty;
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/Ctor1/*'/>
		public MySqlConnection(string connectionString)
			: this()
		{
			ConnectionString = connectionString;
		}

		internal MySqlConnectionStringBuilder Settings
		{
			get { return settings; }
		}

		internal MySqlDataReader Reader
		{
			get
			{
				if (driver == null)
					return null;
				return driver.reader;
			}
			set
			{
				driver.reader = value;
				isInUse = driver.reader != null;
			}
		}

		internal void OnInfoMessage(MySqlInfoMessageEventArgs args)
		{
			if (InfoMessage != null)
			{
				InfoMessage(this, args);
			}
		}

		internal bool SoftClosed
		{
			get
			{
				return (State == ConnectionState.Closed) && driver != null;
			}
		}

		internal bool IsInUse
		{
			get { return isInUse; }
			set { isInUse = value; }
		}

		/// <summary>
		/// Returns the id of the server thread this connection is executing on
		/// </summary>
		public int ServerThread
		{
			get { return driver.ThreadID; }
		}

		/// <summary>
		/// Gets the name of the MySQL server to which to connect.
		/// </summary>
		public override string DataSource
		{
			get { return settings.Server; }
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/ConnectionTimeout/*'/>
		public override int ConnectionTimeout
		{
			get { return (int)settings.ConnectionTimeout; }
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/Database/*'/>
		public override string Database
		{
			get { return database; }
		}

		/// <summary>
		/// Indicates if this connection should use compression when communicating with the server.
		/// </summary>
		public bool UseCompression
		{
			get { return settings.UseCompression; }
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/State/*'/>
		public override ConnectionState State
		{
			get { return connectionState; }
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/ServerVersion/*'/>
		public override string ServerVersion
		{
			get { return driver.Version.ToString(); }
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/ConnectionString/*'/>
		public override string ConnectionString
		{
			get
			{
				// Always return exactly what the user set.
				// Security-sensitive information may be removed.
				return settings.GetConnectionString(!hasBeenOpen || settings.PersistSecurityInfo);
			}
			set
			{
				if (State != ConnectionState.Closed)
					throw new MySqlException(
						"Not allowed to change the 'ConnectionString' property while the connection (state=" + State +
						").");

				MySqlConnectionStringBuilder newSettings;
				lock (connectionStringCache)
				{
					if (value == null)
						newSettings = new MySqlConnectionStringBuilder();
					else
					{
						newSettings = (MySqlConnectionStringBuilder)connectionStringCache[value];
						if (null == newSettings)
						{
							newSettings = new MySqlConnectionStringBuilder(value);
							connectionStringCache.Add(value, newSettings);
						}
					}
				}

				settings = newSettings;

				if (settings.Database != null && settings.Database.Length > 0)
					this.database = settings.Database;

				if (driver != null)
					driver.Settings = newSettings;
			}
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/BeginTransaction/*'/>
		public new MySqlTransaction BeginTransaction()
		{
			return BeginTransaction(IsolationLevel.RepeatableRead);
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/BeginTransaction1/*'/>
		public new MySqlTransaction BeginTransaction(IsolationLevel iso)
		{
			//TODO: check note in help
			if (State != ConnectionState.Open)
				throw new InvalidOperationException("ConnectionNotOpen");

			// First check to see if we are in a current transaction
			if (driver.HasStatus(ServerStatusFlags.InTransaction))
				throw new InvalidOperationException("NoNestedTransactions");

			MySqlTransaction t = new MySqlTransaction(this, iso);

			MySqlCommand cmd = new MySqlCommand("", this);

			cmd.CommandText = "SET SESSION TRANSACTION ISOLATION LEVEL ";
			switch (iso)
			{
				case IsolationLevel.ReadCommitted:
					cmd.CommandText += "READ COMMITTED";
					break;

				case IsolationLevel.ReadUncommitted:
					cmd.CommandText += "READ UNCOMMITTED";
					break;

				case IsolationLevel.RepeatableRead:
					cmd.CommandText += "REPEATABLE READ";
					break;

				case IsolationLevel.Serializable:
					cmd.CommandText += "SERIALIZABLE";
					break;

				case IsolationLevel.Chaos:
					throw new NotSupportedException("ChaosNotSupported");
				case IsolationLevel.Snapshot:
					throw new NotSupportedException("ResourceStrings.SnapshotNotSupported");
			}

			cmd.ExecuteNonQuery();

			cmd.CommandText = "BEGIN";
			cmd.ExecuteNonQuery();

			return t;
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/ChangeDatabase/*'/>
		public override void ChangeDatabase(string databaseName)
		{
			if (databaseName == null || databaseName.Trim().Length == 0)
				throw new ArgumentException("ResourceStrings.ParameterIsInvalid", "databaseName");

			if (State != ConnectionState.Open)
				throw new InvalidOperationException("ResourceStrings.ConnectionNotOpen");

			// This lock  prevents promotable transaction rollback to run
			// in parallel
			lock (driver)
			{
				// We use default command timeout for SetDatabase
				using (new CommandTimer(this, (int)Settings.DefaultCommandTimeout))
				{
					driver.SetDatabase(databaseName);
				}
			}
			this.database = databaseName;
		}

		internal void SetState(ConnectionState newConnectionState, bool broadcast)
		{
			if (newConnectionState == connectionState && !broadcast)
				return;
				
			ConnectionState oldConnectionState = connectionState;
			connectionState = newConnectionState;
			if (broadcast)
				OnStateChange(new StateChangeEventArgs(oldConnectionState, connectionState));
		}

		/// <summary>
		/// Ping
		/// </summary>
		/// <returns></returns>
		public bool Ping()
		{
			if (Reader != null)
				throw new MySqlException("ResourceStrings.DataReaderOpen");
			if (driver != null && driver.Ping())
				return true;
			driver = null;
			SetState(ConnectionState.Closed, true);
			return false;
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/Open/*'/>
		public override void Open()
		{
			if (State == ConnectionState.Open)
				throw new InvalidOperationException("Resources.ConnectionAlreadyOpen");
 
			SetState(ConnectionState.Connecting, true);

			//AssertPermissions();

			try
			{
				MySqlConnectionStringBuilder currentSettings = Settings;
 

				if (Settings.Pooling)
				{
					MySqlPool pool = MySqlPoolManager.GetPool(currentSettings);
					if (driver == null || !driver.IsOpen){
						driver = pool.GetConnection();
					}
					procedureCache = pool.ProcedureCache;

				}
				else
				{
					if (driver == null || !driver.IsOpen)
						driver = Driver.Create(currentSettings);
					procedureCache = new ProcedureCache((int)Settings.ProcedureCacheSize);
				}
			}
			catch (Exception ex)
			{
				SetState(ConnectionState.Closed, true);
				throw;
			}

			// if the user is using old syntax, let them know
			//if (driver.Settings.UseOldSyntax)
				//MySqlTrace.LogWarning(ServerThread,
				//  "You are using old syntax that will be removed in future versions");

			SetState(ConnectionState.Open, false);

			driver.Configure(this);

			if (!(driver.SupportsPasswordExpiration && driver.IsPasswordExpired))
			{
				if (Settings.Database != null && Settings.Database != String.Empty)
					ChangeDatabase(Settings.Database);
			}

			// setup our schema provider
			schemaProvider = new ISSchemaProvider(this);

 

//			// if we are opening up inside a current transaction, then autoenlist
//			// TODO: control this with a connection string option
//#if !MONO && !CF && !RT
//			if (Transaction.Current != null && Settings.AutoEnlist)
//				EnlistTransaction(Transaction.Current);
//#endif

			hasBeenOpen = true;
			SetState(ConnectionState.Open, true);
		}

		//private void AssertPermissions()
		//{
		//	// Security Asserts can only be done when the assemblies 
		//	// are put in the GAC as documented in 
		//	// http://msdn.microsoft.com/en-us/library/ff648665.aspx
		//	if (this.Settings.IncludeSecurityAsserts)
		//	{
		//		PermissionSet set = new PermissionSet(PermissionState.None);
		//		set.AddPermission(new MySqlClientPermission(ConnectionString));
		//		set.Demand();
		//		MySqlSecurityPermission.CreatePermissionSet(true).Assert();
		//	}
		//}

		internal ProcedureCache ProcedureCache
		{
			get { return procedureCache; }
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/CreateCommand/*'/>
		public new MySqlCommand CreateCommand()
		{
			// Return a new instance of a command object.
			MySqlCommand c = new MySqlCommand();
			c.Connection = this;
			return c;
		}

		/// <summary>
		/// Creates a new MySqlConnection object with the exact same ConnectionString value
		/// </summary>
		/// <returns>A cloned MySqlConnection object</returns>
		public MySqlConnection Clone()
		{
			MySqlConnection clone = new MySqlConnection();
			string connectionString = settings.ConnectionString;
			if (connectionString != null)
				clone.ConnectionString = connectionString;
			return clone;
		}

		protected override void Dispose(bool disposing)
		{
			if (State == ConnectionState.Open)
				Close();
			base.Dispose(disposing);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			if (isolationLevel == IsolationLevel.Unspecified)
				return BeginTransaction();
			return BeginTransaction(isolationLevel);
		}

		protected override DbCommand CreateDbCommand()
		{
			return CreateCommand();
		}

		internal void Abort()
		{
			try
			{
				driver.Close();
			}
			catch { }
			finally
			{
				this.isInUse = false;
			}
			SetState(ConnectionState.Closed, true);
		}

		internal void CloseFully()
		{
			if (settings.Pooling && driver.IsOpen)
			{
				// if we are in a transaction, roll it back
				if (driver.HasStatus(ServerStatusFlags.InTransaction))
				{
					MySqlTransaction t = new MySqlTransaction(this, IsolationLevel.Unspecified);
					t.Rollback();
				}

				MySqlPoolManager.ReleaseConnection(driver);
			}
			else
				driver.Close();
			driver = null;
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/Close/*'/>
		public override void Close()
		{
			if (State == ConnectionState.Closed) return;

			if (Reader != null)
				Reader.Close();

			// if the reader was opened with CloseConnection then driver
			// will be null on the second time through
			if (driver != null)
			{
				CloseFully();
				 
			}

			SetState(ConnectionState.Closed, true);
		}

		internal string CurrentDatabase()
		{
			if (Database != null && Database.Length > 0)
				return Database;
			MySqlCommand cmd = new MySqlCommand("SELECT database()", this);
			return cmd.ExecuteScalar().ToString();
		}

		internal void HandleTimeoutOrThreadAbort(Exception ex)
		{
			bool isFatal = false;

			if (isKillQueryConnection)
			{
				// Special connection started to cancel a query.
				// Abort will prevent recursive connection spawning
				Abort();
				if (ex is TimeoutException)
				{
					throw new MySqlException("ResourceStrings.Timeout", true, ex);
				}
				else
				{
					return;
				}
			}

			try
			{
				// Do a fast cancel.The reason behind small values for connection
				// and command timeout is that we do not want user to wait longer
				// after command has already expired.
				// Microsoft's SqlClient seems to be using 5 seconds timeouts
				// here as well.
				// Read the  error packet with "interrupted" message.
				CancelQuery(5);
				driver.ResetTimeout(5000);
				if (Reader != null)
				{
					Reader.Close();
					Reader = null;
				}
			}
			catch
			{
				Abort();
				isFatal = true;
			}
			if (ex is TimeoutException)
			{
				throw new MySqlException("ResourceStrings.Timeout", isFatal, ex);
			}
		}

		public void CancelQuery(int timeout)
		{
			MySqlConnectionStringBuilder cb = new MySqlConnectionStringBuilder(
				Settings.ConnectionString);
			cb.Pooling = false;
			cb.AutoEnlist = false;
			cb.ConnectionTimeout = (uint)timeout;

			using (MySqlConnection c = new MySqlConnection(cb.ConnectionString))
			{
				c.isKillQueryConnection = true;
				c.Open();
				string commandText = "KILL QUERY " + ServerThread;
				MySqlCommand cmd = new MySqlCommand(commandText, c);
				cmd.CommandTimeout = timeout;
				cmd.ExecuteNonQuery();
			}
		}

		// Problem description:
		// Sometimes, ExecuteReader is called recursively. This is the case if
		// command behaviors are used and we issue "set sql_select_limit"
		// before and after command. This is also the case with prepared
		// statements , where we set session variables. In these situations, we
		// have to prevent  recursive ExecuteReader calls from overwriting
		// timeouts set by the top level command.

		// To solve the problem, SetCommandTimeout() and ClearCommandTimeout() are
		// introduced . Query timeout here is  "sticky", that is once set with
		// SetCommandTimeout, it only be overwritten after ClearCommandTimeout
		// (SetCommandTimeout would return false if it timeout has not been
		// cleared).

		// The proposed usage pattern of there routines is following:
		// When timed operations starts, issue SetCommandTimeout(). When it
		// finishes, issue ClearCommandTimeout(), but _only_ if call to
		// SetCommandTimeout() was successful.

		/// <summary>
		/// Sets query timeout. If timeout has been set prior and not
		/// yet cleared ClearCommandTimeout(), it has no effect.
		/// </summary>
		/// <param name="value">timeout in seconds</param>
		/// <returns>true if </returns>
		internal bool SetCommandTimeout(int value)
		{
			if (!hasBeenOpen)
				// Connection timeout is handled by driver
				return false;

			if (commandTimeout != 0)
				// someone is trying to set a timeout while command is already
				// running. It could be for example recursive call to ExecuteReader
				// Ignore the request, as only top-level (non-recursive commands)
				// can set timeouts.
				return false;

			if (driver == null)
				return false;

			commandTimeout = value;
			driver.ResetTimeout(commandTimeout * 1000);
			return true;
		}

		/// <summary>
		/// Clears query timeout, allowing next SetCommandTimeout() to succeed.
		/// </summary>
		internal void ClearCommandTimeout()
		{
			if (!hasBeenOpen)
				return;
			commandTimeout = 0;
			if (driver != null)
			{
				driver.ResetTimeout(0);
			}
		}

		/*
        // Due to the DNXCore replacement for DataReader.GetSchemaTable()
        // haven't implemented (refer to https://github.com/dotnet/corefx/issues/3423)
        // this method should be remove till GetSchema is back.
        //

        /// <summary>
        /// Returns schema information for the data source of this <see cref="DbConnection"/>.
        /// </summary>
        /// <returns>A <see cref="DataTable"/> that contains schema information. </returns>
        public override DataTable GetSchema()
        {
            return GetSchema(null);
        }

        /// <summary>
        /// Returns schema information for the data source of this
        /// <see cref="DbConnection"/> using the specified string for the schema name.
        /// </summary>
        /// <param name="collectionName">Specifies the name of the schema to return. </param>
        /// <returns>A <see cref="DataTable"/> that contains schema information. </returns>
        public override DataTable GetSchema(string collectionName)
        {
            if (collectionName == null)
                collectionName = SchemaProvider.MetaCollection;

            return GetSchema(collectionName, null);
        }

        /// <summary>
        /// Returns schema information for the data source of this <see cref="DbConnection"/>
        /// using the specified string for the schema name and the specified string array
        /// for the restriction values.
        /// </summary>
        /// <param name="collectionName">Specifies the name of the schema to return.</param>
        /// <param name="restrictionValues">Specifies a set of restriction values for the requested schema.</param>
        /// <returns>A <see cref="DataTable"/> that contains schema information.</returns>
        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            if (collectionName == null)
                collectionName = SchemaProvider.MetaCollection;

            string[] restrictions = schemaProvider.CleanRestrictions(restrictionValues);
            DataTable dt = schemaProvider.GetSchema(collectionName, restrictions);
            return dt;
        }
        */

		/// <include file='docs/MySqlConnection.xml' path='docs/ClearPool/*'/>
		public static void ClearPool(MySqlConnection connection)
		{
			MySqlPoolManager.ClearPool(connection.Settings);
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/ClearAllPools/*'/>
		public static void ClearAllPools()
		{
			MySqlPoolManager.ClearAllPools();
		}

	}

	/// <summary>
	/// Represents the method that will handle the <see cref="MySqlConnection.InfoMessage"/> event of a
	/// <see cref="MySqlConnection"/>.
	/// </summary>
	public delegate void MySqlInfoMessageEventHandler(object sender, MySqlInfoMessageEventArgs args);

	/// <summary>
	/// Provides data for the InfoMessage event. This class cannot be inherited.
	/// </summary>
	public class MySqlInfoMessageEventArgs : EventArgs
	{
		/// <summary>
		///
		/// </summary>
		public MySqlError[] errors;
	}

	/// <summary>
	/// IDisposable wrapper around SetCommandTimeout and ClearCommandTimeout
	/// functionality
	/// </summary>
	internal class CommandTimer : IDisposable
	{
		private bool timeoutSet;
		private MySqlConnection connection;

		public CommandTimer(MySqlConnection connection, int timeout)
		{
			this.connection = connection;
			if (connection != null)
			{
				timeoutSet = connection.SetCommandTimeout(timeout);
			}
		}

		public void Dispose()
		{
			if (timeoutSet)
			{
				timeoutSet = false;
				connection.ClearCommandTimeout();
				connection = null;
			}
		}

	}
}
