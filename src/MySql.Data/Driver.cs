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
using System.Collections;
using System.Globalization;
using System.Text;
using MySql.Data.Common;
using MySql.Data.Types;

using System.Diagnostics;
using System.Collections.Generic;
using System.Security;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for BaseDriver.
	/// </summary>
	internal class Driver : IDisposable
	{
		protected Encoding encoding;
		protected MySqlConnectionStringBuilder connectionString;
		protected bool isOpen;
		protected DateTime creationTime;
		protected string serverCharSet;
		protected int serverCharSetIndex;
		protected Dictionary<string, string> serverProps;
		protected Dictionary<int, string> charSets;
		protected long maxPacketSize;
		internal int timeZoneOffset;
		private DateTime idleSince;

 
		protected MySqlPool pool;
		private bool firstResult;
		protected IDriver handler;
		internal MySqlDataReader reader;
		private bool disposeInProgress;
		internal bool isFabric;

		/// <summary>
		/// For pooled connections, time when the driver was
		/// put into idle queue
		/// </summary>
		public DateTime IdleSince
		{
			get { return idleSince; }
			set { idleSince = value; }
		}

		public Driver(MySqlConnectionStringBuilder settings)
		{
			encoding = Encoding.GetEncoding("UTF-8");
			if (encoding == null)
				throw new MySqlException("Resources.DefaultEncodingNotFound");
			connectionString = settings;
			serverCharSet = "utf8";
			serverCharSetIndex = -1;
			maxPacketSize = 1024;
			handler = new NativeDriver(this);
		}

		~Driver()
		{
			Dispose(false);
		}

		#region Properties

		public int ThreadID
		{
			get { return handler.ThreadId; }
		}

		public DBVersion Version
		{
			get { return handler.Version; }
		}

		public MySqlConnectionStringBuilder Settings
		{
			get { return connectionString; }
			set { connectionString = value; }
		}

		public Encoding Encoding
		{
			get { return encoding; }
			set { encoding = value; }
		}

 
		public bool IsOpen
		{
			get { return isOpen; }
		}

		public MySqlPool Pool
		{
			get { return pool; }
			set { pool = value; }
		}

		public long MaxPacketSize
		{
			get { return maxPacketSize; }
		}

		internal int ConnectionCharSetIndex
		{
			get { return serverCharSetIndex; }
			set { serverCharSetIndex = value; }
		}

		internal Dictionary<int, string> CharacterSets
		{
			get { return charSets; }
		}

		public bool SupportsOutputParameters
		{
			get { return Version.isAtLeast(5, 5, 0); }
		}

		public bool SupportsBatch
		{
			get { return (handler.Flags & ClientFlags.MULTI_STATEMENTS) != 0; }
		}

		public bool SupportsConnectAttrs
		{
			get { return (handler.Flags & ClientFlags.CONNECT_ATTRS) != 0; }
		}

		public bool SupportsPasswordExpiration
		{
			get { return (handler.Flags & ClientFlags.CAN_HANDLE_EXPIRED_PASSWORD) != 0; }
		}

		public bool IsPasswordExpired { get; internal set; }

		#endregion

		public string Property(string key)
		{
			return (string)serverProps[key];
		}

		public bool ConnectionLifetimeExpired()
		{
			TimeSpan ts = DateTime.Now.Subtract(creationTime);
			if (Settings.ConnectionLifeTime != 0 &&
			  ts.TotalSeconds > Settings.ConnectionLifeTime)
				return true;
			return false;
		}

		public static Driver Create(MySqlConnectionStringBuilder settings)
		{
			Driver d = new Driver(settings);

			//this try was added as suggested fix submitted on MySql Bug 72025, socket connections are left in CLOSE_WAIT status when connector fails to open a new connection.
			//the bug is present when the client try to get more connections that the server support or has configured in the max_connections variable.
			try
			{
				d.Open();
			}
			catch
			{
				d.Dispose();
				throw;
			}
			return d;
		}

		public bool HasStatus(ServerStatusFlags flag)
		{
			return (handler.ServerStatus & flag) != 0;
		}

		public virtual void Open()
		{
			creationTime = DateTime.Now;
			handler.Open();
			isOpen = true;
		}

		public virtual void Close()
		{
			Dispose();
		}

		public virtual void Configure(MySqlConnection connection)
		{
			bool firstConfigure = false;

			// if we have not already configured our server variables
			// then do so now
			if (serverProps == null)
			{
				firstConfigure = true;

				// if we are in a pool and the user has said it's ok to cache the
				// properties, then grab it from the pool
				try
				{
					if (Pool != null && Settings.CacheServerProperties)
					{
						if (Pool.ServerProperties == null)
							Pool.ServerProperties = LoadServerProperties(connection);
						serverProps = Pool.ServerProperties;
					}
					else{
						Console.WriteLine("LoadServerProperties");
						serverProps = LoadServerProperties(connection);
						
					}

					LoadCharacterSets(connection);
				}
				catch (MySqlException ex)
				{
					// expired password capability
					if (ex.Number == 1820)
					{
						IsPasswordExpired = true;
						return;
					}
					throw;
				}
			}


#if AUTHENTICATED
      string licenseType = serverProps["license"];
      if (licenseType == null || licenseType.Length == 0 || 
        licenseType != "commercial") 
        throw new MySqlException( "This client library licensed only for use with commercially-licensed MySQL servers." );
#endif
			// if the user has indicated that we are not to reset
			// the connection and this is not our first time through,
			// then we are done.
			if (!Settings.ConnectionReset && !firstConfigure) return;

			string charSet = connectionString.CharacterSet;
			if (charSet == null || charSet.Length == 0)
			{
				if (serverCharSetIndex >= 0 && charSets.ContainsKey(serverCharSetIndex))
					charSet = (string)charSets[serverCharSetIndex];
				else
					charSet = serverCharSet;
			}

			if (serverProps.ContainsKey("max_allowed_packet"))
				maxPacketSize = Convert.ToInt64(serverProps["max_allowed_packet"]);

			// now tell the server which character set we will send queries in and which charset we
			// want results in
			MySqlCommand charSetCmd = new MySqlCommand("SET character_set_results=NULL",
							  connection);
			charSetCmd.InternallyCreated = true;

			string clientCharSet;
			serverProps.TryGetValue("character_set_client", out clientCharSet);
			string connCharSet;
			serverProps.TryGetValue("character_set_connection", out connCharSet);
			if ((clientCharSet != null && clientCharSet.ToString() != charSet) ||
			  (connCharSet != null && connCharSet.ToString() != charSet))
			{
				MySqlCommand setNamesCmd = new MySqlCommand("SET NAMES " + charSet, connection);
				setNamesCmd.InternallyCreated = true;
				setNamesCmd.ExecuteNonQuery();
			}
			charSetCmd.ExecuteNonQuery();

			if (charSet != null)
				Encoding = CharSetMap.GetEncoding(Version, charSet);
			else
				Encoding = CharSetMap.GetEncoding(Version, "utf-8");

			handler.Configure();
		}

		/// <summary>
		/// Loads the properties from the connected server into a hashtable
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		private Dictionary<string, string> LoadServerProperties(MySqlConnection connection)
		{
			// load server properties
			Dictionary<string, string> hash = new Dictionary<string, string>();
			MySqlCommand cmd = new MySqlCommand("SHOW VARIABLES", connection);
			try
			{
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string key = reader.GetString(0);
						string value = reader.GetString(1);
						hash[key] = value;
					}
				}
				// Get time zone offset as numerical value
				timeZoneOffset = GetTimeZoneOffset(connection);
				return hash;
			}
			catch (Exception ex)
			{
				//MySqlTrace.LogError(ThreadID, ex.Message);
				throw;
			}
		}

		private int GetTimeZoneOffset(MySqlConnection con)
		{
			MySqlCommand cmd = new MySqlCommand("SELECT TIMEDIFF(NOW(), UTC_TIMESTAMP())", con);
			TimeSpan? timeZoneDiff = cmd.ExecuteScalar() as TimeSpan?;
			string timeZoneString = "0:00";
			if (timeZoneDiff.HasValue)
				timeZoneString = timeZoneDiff.ToString();

			return int.Parse(timeZoneString.Substring(0, timeZoneString.IndexOf(':')));
		}

		/// <summary>
		/// Loads all the current character set names and ids for this server 
		/// into the charSets hashtable
		/// </summary>
		private void LoadCharacterSets(MySqlConnection connection)
		{
			MySqlCommand cmd = new MySqlCommand("SHOW COLLATION", connection);

			// now we load all the currently active collations
			try
			{
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
					charSets = new Dictionary<int, string>();
					while (reader.Read())
					{
						charSets[Convert.ToInt32(reader["id"], NumberFormatInfo.InvariantInfo)] =
						  reader.GetString(reader.GetOrdinal("charset"));
					}
				}
			}
			catch (Exception ex)
			{
				//MySqlTrace.LogError(ThreadID, ex.Message);
				throw;
			}
		}

		public virtual List<MySqlError> ReportWarnings(MySqlConnection connection)
		{
			List<MySqlError> warnings = new List<MySqlError>();

			MySqlCommand cmd = new MySqlCommand("SHOW WARNINGS", connection);
			cmd.InternallyCreated = true;
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					warnings.Add(new MySqlError(reader.GetString(0),
								  reader.GetInt32(1), reader.GetString(2)));
				}
			}

			MySqlInfoMessageEventArgs args = new MySqlInfoMessageEventArgs();
			args.errors = warnings.ToArray();
			if (connection != null)
				connection.OnInfoMessage(args);
			return warnings;
		}

		public virtual void SendQuery(MySqlPacket p)
		{
			handler.SendQuery(p);
			firstResult = true;
		}

		public virtual ResultSet NextResult(int statementId, bool force)
		{
			if (!force && !firstResult && !HasStatus(ServerStatusFlags.AnotherQuery | ServerStatusFlags.MoreResults))
				return null;
			firstResult = false;

			int affectedRows = -1, warnings = 0;
			long insertedId = -1;
			int fieldCount = GetResult(statementId, ref affectedRows, ref insertedId);
			if (fieldCount == -1)
				return null;
			if (fieldCount > 0)
				return new ResultSet(this, statementId, fieldCount);
			else
				return new ResultSet(affectedRows, insertedId);
		}

		protected virtual int GetResult(int statementId, ref int affectedRows, ref long insertedId)
		{
			return handler.GetResult(ref affectedRows, ref insertedId);
		}

		public virtual bool FetchDataRow(int statementId, int columns)
		{
			return handler.FetchDataRow(statementId, columns);
		}

		public virtual bool SkipDataRow()
		{
			return FetchDataRow(-1, 0);
		}

		public virtual void ExecuteDirect(string sql)
		{
			MySqlPacket p = new MySqlPacket(Encoding);
			p.WriteString(sql);
			SendQuery(p);
			NextResult(0, false);
		}

		public MySqlField[] GetColumns(int count)
		{
			MySqlField[] fields = new MySqlField[count];
			for (int i = 0; i < count; i++)
				fields[i] = new MySqlField(this);
			handler.GetColumnsData(fields);

			return fields;
		}

		public virtual int PrepareStatement(string sql, ref MySqlField[] parameters)
		{
			return handler.PrepareStatement(sql, ref parameters);
		}

		public IMySqlValue ReadColumnValue(int index, MySqlField field, IMySqlValue value)
		{
			return handler.ReadColumnValue(index, field, value);
		}

		public void SkipColumnValue(IMySqlValue valObject)
		{
			handler.SkipColumnValue(valObject);
		}

		public void ResetTimeout(int timeoutMilliseconds)
		{
			handler.ResetTimeout(timeoutMilliseconds);
		}

		public bool Ping()
		{
			return handler.Ping();
		}

		public virtual void SetDatabase(string dbName)
		{
			handler.SetDatabase(dbName);
		}

		public virtual void ExecuteStatement(MySqlPacket packetToExecute)
		{
			handler.ExecuteStatement(packetToExecute);
		}


		public virtual void CloseStatement(int id)
		{
			handler.CloseStatement(id);
		}

		public virtual void Reset()
		{
			handler.Reset();
		}

		public virtual void CloseQuery(MySqlConnection connection, int statementId)
		{
			if (handler.WarningCount > 0)
				ReportWarnings(connection);
		}

		#region IDisposable Members

		protected virtual void Dispose(bool disposing)
		{
			// Avoid cyclic calls to Dispose.
			if (disposeInProgress)
				return;

			disposeInProgress = true;

			try
			{
				ResetTimeout(1000);
				if (disposing)
					handler.Close(isOpen);
				// if we are pooling, then release ourselves
				if (connectionString.Pooling)
					MySqlPoolManager.RemoveConnection(this);
			}
			catch (Exception)
			{
				if (disposing)
					throw;
			}
			finally
			{
				reader = null;
				isOpen = false;
				disposeInProgress = false;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}

	internal interface IDriver
	{
		int ThreadId { get; }
		DBVersion Version { get; }
		ServerStatusFlags ServerStatus { get; }
		ClientFlags Flags { get; }
		void Configure();
		void Open();
		void SendQuery(MySqlPacket packet);
		void Close(bool isOpen);
		bool Ping();
		int GetResult(ref int affectedRows, ref long insertedId);
		bool FetchDataRow(int statementId, int columns);
		int PrepareStatement(string sql, ref MySqlField[] parameters);
		void ExecuteStatement(MySqlPacket packet);
		void CloseStatement(int statementId);
		void SetDatabase(string dbName);
		void Reset();
		IMySqlValue ReadColumnValue(int index, MySqlField field, IMySqlValue valObject);
		void SkipColumnValue(IMySqlValue valueObject);
		void GetColumnsData(MySqlField[] columns);
		void ResetTimeout(int timeout);
		int WarningCount { get; }
	}
}
