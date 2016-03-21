// Copyright © 2013, 2015, Oracle and/or its affiliates. All rights reserved.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;
using System.Reflection;
using MySql.Data.MySqlClient;


namespace MySql.Data.MySqlClient
{
	public sealed partial class MySqlConnectionStringBuilder
	{
		internal Dictionary<string, object> values = new Dictionary<string, object>();

		private static MySqlConnectionStringOptionCollection options = new MySqlConnectionStringOptionCollection();

		static MySqlConnectionStringBuilder()
		{
			// Server options
			options.Add(new MySqlConnectionStringOption("server", "host,data source,datasource,address,addr,network address", typeof(string), "" /*"localhost"*/, false));
			options.Add(new MySqlConnectionStringOption("database", "initial catalog", typeof(string), string.Empty, false));
			options.Add(new MySqlConnectionStringOption("protocol", "connection protocol, connectionprotocol", typeof(MySqlConnectionProtocol), MySqlConnectionProtocol.Sockets, false));
			options.Add(new MySqlConnectionStringOption("port", null, typeof(uint), (uint)3306, false));
			options.Add(new MySqlConnectionStringOption("pipe", "pipe name,pipename", typeof(string), "MYSQL", false));
			options.Add(new MySqlConnectionStringOption("compress", "use compression,usecompression", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("allowbatch", "allow batch", typeof(bool), true, false));
			options.Add(new MySqlConnectionStringOption("logging", null, typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("sharedmemoryname", "shared memory name", typeof(string), "MYSQL", false));
			options.Add(new MySqlConnectionStringOption("useoldsyntax", "old syntax,oldsyntax,use old syntax", typeof(bool), false, true,
			  delegate (MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender, object value)
			  {
			// MySqlTrace.LogWarning(-1, "Use Old Syntax is now obsolete.  Please see documentation");
			msb.SetValue("useoldsyntax", value);
			  },
			  delegate (MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender)
			  {
				  return (bool)msb.values["useoldsyntax"];
			  }
			  ));
			options.Add(new MySqlConnectionStringOption("connectiontimeout", "connection timeout,connect timeout", typeof(uint), (uint)15, false,
			  delegate (MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender, object Value)
			  {

				  uint value = (uint)Convert.ChangeType(Value, sender.BaseType);

			// Timeout in milliseconds should not exceed maximum for 32 bit
			// signed integer (~24 days). We truncate the value if it exceeds 
			// maximum (MySqlCommand.CommandTimeout uses the same technique
			uint timeout = Math.Min(value, Int32.MaxValue / 1000);
				  if (timeout != value)
				  {

				  }
				  msb.SetValue("connectiontimeout", timeout);
			  },
			  delegate (MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender)
			  {
				  return (uint)msb.values["connectiontimeout"];
			  }
			  ));
			options.Add(new MySqlConnectionStringOption("defaultcommandtimeout", "command timeout,default command timeout", typeof(uint), (uint)30, false));
			options.Add(new MySqlConnectionStringOption("usedefaultcommandtimeoutforef", "use default command timeout for ef", typeof(bool), false, false));

			// authentication options
			options.Add(new MySqlConnectionStringOption("user id", "uid,username,user name,user,userid", typeof(string), "", false));
			options.Add(new MySqlConnectionStringOption("password", "pwd", typeof(string), "", false));
			options.Add(new MySqlConnectionStringOption("persistsecurityinfo", "persist security info", typeof(bool), false, false));
#if !CF
			options.Add(new MySqlConnectionStringOption("encrypt", null, typeof(bool), false, true,
			  delegate (MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender, object value)
			  {
			// just for this case, reuse the logic to translate string to bool
			sender.ValidateValue(ref value);
			//MySqlTrace.LogWarning(-1, "Encrypt is now obsolete. Use Ssl Mode instead");
			msb.SetValue("Ssl Mode", (bool)value ? MySqlSslMode.Prefered : MySqlSslMode.None);
			  },
			  delegate (MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender)
			  {
				  return msb.SslMode != MySqlSslMode.None;
			  }
			  ));
			options.Add(new MySqlConnectionStringOption("certificatefile", "certificate file", typeof(string), null, false));
			options.Add(new MySqlConnectionStringOption("certificatepassword", "certificate password", typeof(string), null, false));
			options.Add(new MySqlConnectionStringOption("certificatestorelocation", "certificate store location", typeof(MySqlCertificateStoreLocation), MySqlCertificateStoreLocation.None, false));
			options.Add(new MySqlConnectionStringOption("certificatethumbprint", "certificate thumb print", typeof(string), null, false));
#endif
			options.Add(new MySqlConnectionStringOption("integratedsecurity", "integrated security", typeof(bool), false, false,
			  delegate (MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender, object value)
			  {
				  if (!MySql.Data.Common.Platform.IsWindows())
					  throw new MySqlException("IntegratedSecurity is supported on Windows only");

				  msb.SetValue("Integrated Security", value);
			  },
			  delegate (MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender)
			  {
				  object val = msb.values["Integrated Security"];
				  return (bool)val;
			  }
			  ));

			// Other properties
			options.Add(new MySqlConnectionStringOption("allowzerodatetime", "allow zero datetime", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("convertzerodatetime", "convert zero datetime", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("useusageadvisor", "use usage advisor,usage advisor", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("procedurecachesize", "procedure cache size,procedure cache,procedurecache", typeof(uint), (uint)25, false));
			options.Add(new MySqlConnectionStringOption("useperformancemonitor", "use performance monitor,useperfmon,perfmon", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("ignoreprepare", "ignore prepare", typeof(bool), true, false));
			options.Add(new MySqlConnectionStringOption("useprocedurebodies", "use procedure bodies,procedure bodies", typeof(bool), true, true,
			  delegate (MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender, object value)
			  {
				  sender.ValidateValue(ref value);
			//MySqlTrace.LogWarning(-1, "Use Procedure Bodies is now obsolete.  Use Check Parameters instead");
			msb.SetValue("checkparameters", value);
				  msb.SetValue("useprocedurebodies", value);
			  },
			  delegate (MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender)
			  {
				  return (bool)msb.values["useprocedurebodies"];
			  }
			  ));
			options.Add(new MySqlConnectionStringOption("autoenlist", "auto enlist", typeof(bool), true, false));
			options.Add(new MySqlConnectionStringOption("respectbinaryflags", "respect binary flags", typeof(bool), true, false));
			options.Add(new MySqlConnectionStringOption("treattinyasboolean", "treat tiny as boolean", typeof(bool), true, false));
			options.Add(new MySqlConnectionStringOption("allowuservariables", "allow user variables", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("interactivesession", "interactive session,interactive", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("functionsreturnstring", "functions return string", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("useaffectedrows", "use affected rows", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("oldguids", "old guids", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("keepalive", "keep alive", typeof(uint), (uint)0, false));
			options.Add(new MySqlConnectionStringOption("sqlservermode", "sql server mode", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("tablecaching", "table cache,tablecache", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("defaulttablecacheage", "default table cache age", typeof(int), (int)60, false));
			options.Add(new MySqlConnectionStringOption("checkparameters", "check parameters", typeof(bool), true, false));
			options.Add(new MySqlConnectionStringOption("replication", null, typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("exceptioninterceptors", "exception interceptors", typeof(string), null, false));
			options.Add(new MySqlConnectionStringOption("commandinterceptors", "command interceptors", typeof(string), null, false));
#if !CF
			options.Add(new MySqlConnectionStringOption("includesecurityasserts", "include security asserts", typeof(bool), false, false));
#endif

			// pooling options
			options.Add(new MySqlConnectionStringOption("connectionlifetime", "connection lifetime", typeof(uint), (uint)0, false));
			options.Add(new MySqlConnectionStringOption("pooling", null, typeof(bool), true, false));
			options.Add(new MySqlConnectionStringOption("minpoolsize", "minimumpoolsize,min pool size,minimum pool size", typeof(uint), (uint)0, false));
			options.Add(new MySqlConnectionStringOption("maxpoolsize", "maximumpoolsize,max pool size,maximum pool size", typeof(uint), (uint)100, false));
			options.Add(new MySqlConnectionStringOption("connectionreset", "connection reset", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("cacheserverproperties", "cache server properties", typeof(bool), false, false));

			// language and charset options
			options.Add(new MySqlConnectionStringOption("characterset", "character set,charset", typeof(string), "", false));
			options.Add(new MySqlConnectionStringOption("treatblobsasutf8", "treat blobs as utf8", typeof(bool), false, false));
			options.Add(new MySqlConnectionStringOption("blobasutf8includepattern", null, typeof(string), "", false));
			options.Add(new MySqlConnectionStringOption("blobasutf8excludepattern", null, typeof(string), "", false));
#if !CF
			options.Add(new MySqlConnectionStringOption("sslmode", "ssl mode", typeof(MySqlSslMode), MySqlSslMode.Preferred, false));
#endif
		}

		public MySqlConnectionStringBuilder()
		{
			HasProcAccess = true;
			// Populate initial values
			lock (this)
			{
				for (int i = 0; i < options.Options.Count; i++)
				{
					values[options.Options[i].Keyword] = options.Options[i].DefaultValue;
				}
			}
		}

		public MySqlConnectionStringBuilder(string connStr)
		  : base()
		{
			lock (this)
			{
				ConnectionString = connStr;
			}
		}

		#region Server Properties

		/// <summary>
		/// Gets or sets the name of the server.
		/// </summary>
		/// <value>The server.</value>
		[Category("Connection")]
		[Description("Server to connect to")]
		[RefreshProperties(RefreshProperties.All)]
		public string Server
		{
			get { return this["server"] as string; }
			set { this["server"] = value; }
		}

		/// <summary>
		/// Gets or sets the name of the database the connection should 
		/// initially connect to.
		/// </summary>
		[Category("Connection")]
		[Description("Database to use initially")]
		[RefreshProperties(RefreshProperties.All)]
		public string Database
		{
			get { return values["database"] as string; }
			set { SetValue("database", value); }
		}

		/// <summary>
		/// Gets or sets the protocol that should be used for communicating
		/// with MySQL.
		/// </summary>
		[Category("Connection")]
		[DisplayName("Connection Protocol")]
		[Description("Protocol to use for connection to MySQL")]
		[RefreshProperties(RefreshProperties.All)]
		public MySqlConnectionProtocol ConnectionProtocol
		{
			get { return (MySqlConnectionProtocol)values["protocol"]; }
			set { SetValue("protocol", value); }
		}

		/// <summary>
		/// Gets or sets the name of the named pipe that should be used
		/// for communicating with MySQL.
		/// </summary>
		[Category("Connection")]
		[DisplayName("Pipe Name")]
		[Description("Name of pipe to use when connecting with named pipes (Win32 only)")]
		[RefreshProperties(RefreshProperties.All)]
		public string PipeName
		{
			get { return (string)values["pipe"]; }
			set { SetValue("pipe", value); }
		}

		/// <summary>
		/// Gets or sets a boolean value that indicates whether this connection
		/// should use compression.
		/// </summary>
		[Category("Connection")]
		[DisplayName("Use Compression")]
		[Description("Should the connection use compression")]
		[RefreshProperties(RefreshProperties.All)]
		public bool UseCompression
		{
			get { return (bool)values["compress"]; }
			set { SetValue("compress", value); }
		}

		/// <summary>
		/// Gets or sets a boolean value that indicates whether this connection will allow
		/// commands to send multiple SQL statements in one execution.
		/// </summary>
		[Category("Connection")]
		[DisplayName("Allow Batch")]
		[Description("Allows execution of multiple SQL commands in a single statement")]
		[RefreshProperties(RefreshProperties.All)]
		public bool AllowBatch
		{
			get { return (bool)values["allowbatch"]; }
			set { SetValue("allowbatch", value); }
		}

		/// <summary>
		/// Gets or sets a boolean value that indicates whether logging is enabled.
		/// </summary>
		[Category("Connection")]
		[Description("Enables output of diagnostic messages")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Logging
		{
			get { return (bool)values["logging"]; }
			set { SetValue("logging", value); }
		}

		/// <summary>
		/// Gets or sets the base name of the shared memory objects used to 
		/// communicate with MySQL when the shared memory protocol is being used.
		/// </summary>
		[Category("Connection")]
		[DisplayName("Shared Memory Name")]
		[Description("Name of the shared memory object to use")]
		[RefreshProperties(RefreshProperties.All)]
		public string SharedMemoryName
		{
			get { return (string)values["sharedmemoryname"]; }
			set { SetValue("sharedmemoryname", value); }
		}

		/// <summary>
		/// Gets or sets a boolean value that indicates whether this connection uses
		/// the old style (@) parameter markers or the new (?) style.
		/// </summary>
		[Category("Connection")]
		[DisplayName("Use Old Syntax")]
		[Description("Allows the use of old style @ syntax for parameters")]
		[RefreshProperties(RefreshProperties.All)]
		[Obsolete("Use Old Syntax is no longer needed.  See documentation")]
		public bool UseOldSyntax
		{
			get { return (bool)values["useoldsyntax"]; }
			set { SetValue("useoldsyntax", value); }
		}

		/// <summary>
		/// Gets or sets the port number that is used when the socket
		/// protocol is being used.
		/// </summary>
		[Category("Connection")]
		[Description("Port to use for TCP/IP connections")]
		[RefreshProperties(RefreshProperties.All)]
		public uint Port
		{
			get { return (uint)values["port"]; }
			set { SetValue("port", value); }
		}

		/// <summary>
		/// Gets or sets the connection timeout.
		/// </summary>
		[Category("Connection")]
		[DisplayName("Connect Timeout")]
		[Description("The length of time (in seconds) to wait for a connection " +
					 "to the server before terminating the attempt and generating an error.")]
		[RefreshProperties(RefreshProperties.All)]
		public uint ConnectionTimeout
		{
			get { return (uint)values["connectiontimeout"]; }

			set
			{
				// Timeout in milliseconds should not exceed maximum for 32 bit
				// signed integer (~24 days). We truncate the value if it exceeds 
				// maximum (MySqlCommand.CommandTimeout uses the same technique
				uint timeout = Math.Min(value, Int32.MaxValue / 1000);
				if (timeout != value)
				{
					//MySqlTrace.LogWarning(-1, "Connection timeout value too large ("
					//   + value + " seconds). Changed to max. possible value" +
					//   +timeout + " seconds)");
				}
				SetValue("connectiontimeout", timeout);
			}
		}

		/// <summary>
		/// Gets or sets the default command timeout.
		/// </summary>
		[Category("Connection")]
		[DisplayName("Default Command Timeout")]
		[Description(@"The default timeout that MySqlCommand objects will use
                     unless changed.")]
		[RefreshProperties(RefreshProperties.All)]
		public uint DefaultCommandTimeout
		{
			get { return (uint)values["defaultcommandtimeout"]; }
			set { SetValue("defaultcommandtimeout", value); }
		}

		#endregion

		#region Authentication Properties

		/// <summary>
		/// Gets or sets the user id that should be used to connect with.
		/// </summary>
		[Category("Security")]
		[DisplayName("User Id")]
		[Description("Indicates the user ID to be used when connecting to the data source.")]
		[RefreshProperties(RefreshProperties.All)]
		public string UserID
		{
			get { return (string)values["user id"]; }
			set { SetValue("user id", value); }
		}

		/// <summary>
		/// Gets or sets the password that should be used to connect with.
		/// </summary>
		[Category("Security")]
		[Description("Indicates the password to be used when connecting to the data source.")]
		[PasswordPropertyText(true)]
		[RefreshProperties(RefreshProperties.All)]
		public string Password
		{
			get { return (string)values["password"]; }
			set { SetValue("password", value); }
		}

		/// <summary>
		/// Gets or sets a boolean value that indicates if the password should be persisted
		/// in the connection string.
		/// </summary>
		[Category("Security")]
		[DisplayName("Persist Security Info")]
		[Description("When false, security-sensitive information, such as the password, " +
					 "is not returned as part of the connection if the connection is open or " +
					 "has ever been in an open state.")]
		[RefreshProperties(RefreshProperties.All)]
		public bool PersistSecurityInfo
		{
			get { return (bool)values["persistsecurityinfo"]; }
			set { SetValue("persistsecurityinfo", value); }
		}

#if !CF
		[Category("Authentication")]
		[Description("Should the connection use SSL.")]
		[Obsolete("Use Ssl Mode instead.")]
		internal bool Encrypt
		{
			get { return SslMode != MySqlSslMode.None; }
			set
			{
				SetValue("Ssl Mode", value ? MySqlSslMode.Prefered : MySqlSslMode.None);
			}
		}

		[Category("Authentication")]
		[DisplayName("Certificate File")]
		[Description("Certificate file in PKCS#12 format (.pfx)")]
		public string CertificateFile
		{
			get { return (string)values["certificatefile"]; }
			set { SetValue("certificatefile", value); }
		}

		[Category("Authentication")]
		[DisplayName("Certificate Password")]
		[Description("Password for certificate file")]
		public string CertificatePassword
		{
			get { return (string)values["certificatepassword"]; }
			set { SetValue("certificatepassword", value); }
		}

		[Category("Authentication")]
		[DisplayName("Certificate Store Location")]
		[Description("Certificate Store Location for client certificates")]
		[DefaultValue(MySqlCertificateStoreLocation.None)]
		public MySqlCertificateStoreLocation CertificateStoreLocation
		{
			get { return (MySqlCertificateStoreLocation)values["certificatestorelocation"]; }
			set { SetValue("certificatestorelocation", value); }
		}

		[Category("Authentication")]
		[DisplayName("Certificate Thumbprint")]
		[Description("Certificate thumbprint. Can be used together with Certificate " +
			"Store Location parameter to uniquely identify certificate to be used " +
			"for SSL authentication.")]
		public string CertificateThumbprint
		{
			get { return (string)values["certificatethumbprint"]; }
			set { SetValue("certificatethumbprint", value); }
		}

#endif

		[Category("Authentication")]
		[DisplayName("Integrated Security")]
		[Description("Use windows authentication when connecting to server")]
		[DefaultValue(false)]
		public bool IntegratedSecurity
		{
			get { return (bool)values["integratedsecurity"]; }
			set
			{
				if (!MySql.Data.Common.Platform.IsWindows())
					throw new MySqlException("IntegratedSecurity is supported on Windows only");

				SetValue("integratedsecurity", value);
			}
		}

		#endregion

		#region Other Properties

		/// <summary>
		/// Gets or sets a boolean value that indicates if zero date time values are supported.
		/// </summary>
		[Category("Advanced")]
		[DisplayName("Allow Zero Datetime")]
		[Description("Should zero datetimes be supported")]
		[DefaultValue(false)]
		[RefreshProperties(RefreshProperties.All)]
		public bool AllowZeroDateTime
		{
			get { return (bool)values["allowzerodatetime"]; }
			set { SetValue("allowzerodatetime", value); }
		}

		/// <summary>
		/// Gets or sets a boolean value indicating if zero datetime values should be 
		/// converted to DateTime.MinValue.
		/// </summary>
		[Category("Advanced")]
		[DisplayName("Convert Zero Datetime")]
		[Description("Should illegal datetime values be converted to DateTime.MinValue")]
		[DefaultValue(false)]
		[RefreshProperties(RefreshProperties.All)]
		public bool ConvertZeroDateTime
		{
			get { return (bool)values["convertzerodatetime"]; }
			set { SetValue("convertzerodatetime", value); }
		}

		/// <summary>
		/// Gets or sets a boolean value indicating if the Usage Advisor should be enabled.
		/// </summary>
		[Category("Advanced")]
		[DisplayName("Use Usage Advisor")]
		[Description("Logs inefficient database operations")]
		[DefaultValue(false)]
		[RefreshProperties(RefreshProperties.All)]
		public bool UseUsageAdvisor
		{
			get { return (bool)values["useusageadvisor"]; }
			set { SetValue("useusageadvisor", value); }
		}

		/// <summary>
		/// Gets or sets the size of the stored procedure cache.
		/// </summary>
		[Category("Advanced")]
		[DisplayName("Procedure Cache Size")]
		[Description("Indicates how many stored procedures can be cached at one time. " +
					 "A value of 0 effectively disables the procedure cache.")]
		[DefaultValue(25)]
		[RefreshProperties(RefreshProperties.All)]
		public uint ProcedureCacheSize
		{
			get { return (uint)values["procedurecachesize"]; }
			set { SetValue("procedurecachesize", value); }
		}

		/// <summary>
		/// Gets or sets a boolean value indicating if the permon hooks should be enabled.
		/// </summary>
		[Category("Advanced")]
		[DisplayName("Use Performance Monitor")]
		[Description("Indicates that performance counters should be updated during execution.")]
		[DefaultValue(false)]
		[RefreshProperties(RefreshProperties.All)]
		public bool UsePerformanceMonitor
		{
			get { return (bool)values["useperformancemonitor"]; }
			set { SetValue("useperformancemonitor", value); }
		}

		/// <summary>
		/// Gets or sets a boolean value indicating if calls to Prepare() should be ignored.
		/// </summary>
		[Category("Advanced")]
		[DisplayName("Ignore Prepare")]
		[Description("Instructs the provider to ignore any attempts to prepare a command.")]
		[DefaultValue(true)]
		[RefreshProperties(RefreshProperties.All)]
		public bool IgnorePrepare
		{
			get { return (bool)values["ignoreprepare"]; }
			set { SetValue("ignoreprepare", value); }
		}

		[Category("Advanced")]
		[DisplayName("Use Procedure Bodies")]
		[Description("Indicates if stored procedure bodies will be available for parameter detection.")]
		[DefaultValue(true)]
		[Obsolete("Use CheckParameters instead")]
		public bool UseProcedureBodies
		{
			get { return (bool)values["useprocedurebodies"]; }
			set { SetValue("useprocedurebodies", value); }
		}

		[Category("Advanced")]
		[DisplayName("Auto Enlist")]
		[Description("Should the connetion automatically enlist in the active connection, if there are any.")]
		[DefaultValue(true)]
		[RefreshProperties(RefreshProperties.All)]
		public bool AutoEnlist
		{
			get { return (bool)values["autoenlist"]; }
			set { SetValue("autoenlist", value); }
		}

		[Category("Advanced")]
		[DisplayName("Respect Binary Flags")]
		[Description("Should binary flags on column metadata be respected.")]
		[DefaultValue(true)]
		[RefreshProperties(RefreshProperties.All)]
		public bool RespectBinaryFlags
		{
			get { return (bool)values["respectbinaryflags"]; }
			set { SetValue("respectbinaryflags", value); }
		}

		[Category("Advanced")]
		[DisplayName("Treat Tiny As Boolean")]
		[Description("Should the provider treat TINYINT(1) columns as boolean.")]
		[DefaultValue(true)]
		[RefreshProperties(RefreshProperties.All)]
		public bool TreatTinyAsBoolean
		{
			get { return (bool)values["treattinyasboolean"]; }
			set { SetValue("treattinyasboolean", value); }
		}

		[Category("Advanced")]
		[DisplayName("Allow User Variables")]
		[Description("Should the provider expect user variables to appear in the SQL.")]
		[DefaultValue(false)]
		[RefreshProperties(RefreshProperties.All)]
		public bool AllowUserVariables
		{
			get { return (bool)values["allowuservariables"]; }
			set { SetValue("allowuservariables", value); }
		}

		[Category("Advanced")]
		[DisplayName("Interactive Session")]
		[Description("Should this session be considered interactive?")]
		[DefaultValue(false)]
		[RefreshProperties(RefreshProperties.All)]
		public bool InteractiveSession
		{
			get { return (bool)values["interactivesession"]; }
			set { SetValue("interactivesession", value); }
		}

		[Category("Advanced")]
		[DisplayName("Functions Return String")]
		[Description("Should all server functions be treated as returning string?")]
		[DefaultValue(false)]
		public bool FunctionsReturnString
		{
			get { return (bool)values["functionsreturnstring"]; }
			set { SetValue("functionsreturnstring", value); }
		}

		[Category("Advanced")]
		[DisplayName("Use Affected Rows")]
		[Description("Should the returned affected row count reflect affected rows instead of found rows?")]
		[DefaultValue(false)]
		public bool UseAffectedRows
		{
			get { return (bool)values["useaffectedrows"]; }
			set { SetValue("useaffectedrows", value); }
		}


		[Category("Advanced")]
		[DisplayName("Old Guids")]
		[Description("Treat binary(16) columns as guids")]
		[DefaultValue(false)]
		public bool OldGuids
		{
			get { return (bool)values["oldguids"]; }
			set { SetValue("oldguids", value); }
		}

		[DisplayName("Keep Alive")]
		[Description("For TCP connections, idle connection time measured in seconds, before the first keepalive packet is sent." +
			"A value of 0 indicates that keepalive is not used.")]
		[DefaultValue(0)]
		public uint Keepalive
		{
			get { return (uint)values["keepalive"]; }
			set { SetValue("keepalive", value); }
		}

		[Category("Advanced")]
		[DisplayName("Sql Server Mode")]
		[Description("Allow Sql Server syntax.  " +
			"A value of yes allows symbols to be enclosed with [] instead of ``.  This does incur " +
			"a performance hit so only use when necessary.")]
		[DefaultValue(false)]
		public bool SqlServerMode
		{
			get { return (bool)values["sqlservermode"]; }
			set { SetValue("sqlservermode", value); }
		}

		[Category("Advanced")]
		[DisplayName("Table Cache")]
		[Description(@"Enables or disables caching of TableDirect command.  
            A value of yes enables the cache while no disables it.")]
		[DefaultValue(false)]
		public bool TableCaching
		{
			get { return (bool)values["tablecaching"]; }
			set { SetValue("tablecachig", value); }
		}

		[Category("Advanced")]
		[DisplayName("Default Table Cache Age")]
		[Description(@"Specifies how long a TableDirect result should be cached in seconds.")]
		[DefaultValue(60)]
		public int DefaultTableCacheAge
		{
			get { return (int)values["defaulttablecacheage"]; }
			set { SetValue("defaulttablecacheage", value); }
		}

		[Category("Advanced")]
		[DisplayName("Check Parameters")]
		[Description("Indicates if stored routine parameters should be checked against the server.")]
		[DefaultValue(true)]
		public bool CheckParameters
		{
			get { return (bool)values["checkparameters"]; }
			set { SetValue("checkparameters", value); }
		}

		[Category("Advanced")]
		[DisplayName("Replication")]
		[Description("Indicates if this connection is to use replicated servers.")]
		[DefaultValue(false)]
		public bool Replication
		{
			get { return (bool)values["replication"]; }
			set { SetValue("replication", value); }
		}

		[Category("Advanced")]
		[DisplayName("Exception Interceptors")]
		[Description("The list of interceptors that can triage thrown MySqlExceptions.")]
		public string ExceptionInterceptors
		{
			get { return (string)values["exceptioninterceptors"]; }
			set { SetValue("exceptioninterceptors", value); }
		}

		[Category("Advanced")]
		[DisplayName("Command Interceptors")]
		[Description("The list of interceptors that can intercept command operations.")]
		public string CommandInterceptors
		{
			get { return (string)values["commandinterceptors"]; }
			set { SetValue("commandinterceptors", value); }
		}

#if !CF
		[Category("Advanced")]
		[DisplayName("Include Security Asserts")]
		[Description("Include security asserts to support Medium Trust")]
		[DefaultValue(false)]
		public bool IncludeSecurityAsserts
		{
			get { return (bool)values["includesecurityasserts"]; }
			set { SetValue("includesecurityasserts", value); }
		}
#endif

		#endregion

		#region Pooling Properties

		/// <summary>
		/// Gets or sets the lifetime of a pooled connection.
		/// </summary>
		[Category("Pooling")]
		[DisplayName("Connection Lifetime")]
		[Description("The minimum amount of time (in seconds) for this connection to " +
					 "live in the pool before being destroyed.")]
		[DefaultValue(0)]
		[RefreshProperties(RefreshProperties.All)]
		public uint ConnectionLifeTime
		{
			get { return (uint)values["connectionlifetime"]; }
			set { SetValue("connectionlifetime", value); }
		}

		/// <summary>
		/// Gets or sets a boolean value indicating if connection pooling is enabled.
		/// </summary>
		[Category("Pooling")]
		[Description("When true, the connection object is drawn from the appropriate " +
					 "pool, or if necessary, is created and added to the appropriate pool.")]
		[DefaultValue(true)]
		[RefreshProperties(RefreshProperties.All)]
		public bool Pooling
		{
			get { return (bool)values["pooling"]; }
			set { SetValue("pooling", value); }
		}

		/// <summary>
		/// Gets the minimum connection pool size.
		/// </summary>
		[Category("Pooling")]
		[DisplayName("Minimum Pool Size")]
		[Description("The minimum number of connections allowed in the pool.")]
		[DefaultValue(0)]
		[RefreshProperties(RefreshProperties.All)]
		public uint MinimumPoolSize
		{
			get { return (uint)values["minpoolsize"]; }
			set { SetValue("minpoolsize", value); }
		}

		/// <summary>
		/// Gets or sets the maximum connection pool setting.
		/// </summary>
		[Category("Pooling")]
		[DisplayName("Maximum Pool Size")]
		[Description("The maximum number of connections allowed in the pool.")]
		[DefaultValue(100)]
		[RefreshProperties(RefreshProperties.All)]
		public uint MaximumPoolSize
		{
			get { return (uint)values["maxpoolsize"]; }
			set { SetValue("maxpoolsize", value); }
		}

		/// <summary>
		/// Gets or sets a boolean value indicating if the connection should be reset when retrieved
		/// from the pool.
		/// </summary>
		[Category("Pooling")]
		[DisplayName("Connection Reset")]
		[Description("When true, indicates the connection state is reset when removed from the pool.")]
		[DefaultValue(false)]
		[RefreshProperties(RefreshProperties.All)]
		public bool ConnectionReset
		{
			get { return (bool)values["connectionreset"]; }
			set { SetValue("connectionreset", value); }
		}

		[Category("Pooling")]
		[DisplayName("Cache Server Properties")]
		[Description("When true, server properties will be cached after the first server in the pool is created")]
		[DefaultValue(false)]
		[RefreshProperties(RefreshProperties.All)]
		public bool CacheServerProperties
		{
			get { return (bool)values["cacheserverproperties"]; }
			set { SetValue("cacheserverproperties", value); }
		}

		#endregion

		#region Language and Character Set Properties

		/// <summary>
		/// Gets or sets the character set that should be used for sending queries to the server.
		/// </summary>
		[DisplayName("Character Set")]
		[Category("Advanced")]
		[Description("Character set this connection should use")]
		[DefaultValue("")]
		[RefreshProperties(RefreshProperties.All)]
		public string CharacterSet
		{
			get { return (string)values["characterset"]; }
			set { SetValue("characterset", value); }
		}

		/// <summary>
		/// Indicates whether the driver should treat binary blobs as UTF8
		/// </summary>
		[DisplayName("Treat Blobs As UTF8")]
		[Category("Advanced")]
		[Description("Should binary blobs be treated as UTF8")]
		[DefaultValue(false)]
		[RefreshProperties(RefreshProperties.All)]
		public bool TreatBlobsAsUTF8
		{
			get { return (bool)values["treatblobsasutf8"]; }
			set { SetValue("treatblobsasutf8", value); }
		}

		/// <summary>
		/// Gets or sets the pattern that matches the columns that should be treated as UTF8
		/// </summary>
		[Category("Advanced")]
		[Description("Pattern that matches columns that should be treated as UTF8")]
		[RefreshProperties(RefreshProperties.All)]
		public string BlobAsUTF8IncludePattern
		{
			get { return (string)values["blobasutf8includepattern"]; }
			set { SetValue("blobasutf8includepattern", value); }
		}

		/// <summary>
		/// Gets or sets the pattern that matches the columns that should not be treated as UTF8
		/// </summary>
		[Category("Advanced")]
		[Description("Pattern that matches columns that should not be treated as UTF8")]
		[RefreshProperties(RefreshProperties.All)]
		public string BlobAsUTF8ExcludePattern
		{
			get { return (string)values["blobasutf8excludepattern"]; }
			set { SetValue("blobasutf8excludepattern", value); }
		}

#if !CF
		/// <summary>
		/// Indicates whether to use SSL connections and how to handle server certificate errors.
		/// </summary>
		[DisplayName("Ssl Mode")]
		[Category("Security")]
		[Description("SSL properties for connection")]
		[DefaultValue(MySqlSslMode.None)]
		public MySqlSslMode SslMode
		{
			get { return (MySqlSslMode)values["sslmode"]; }
			set { SetValue("sslmode", value); }
		}
#endif

		#endregion

		#region Backwards compatibility properties
		[DisplayName("Use Default Command Timeout For EF")]
		[Category("Backwards Compatibility")]
		[Description("Enforces the command timeout of EFMySqlCommand to the value provided in 'DefaultCommandTimeout' property")]
		[DefaultValue(false)]
		public bool UseDefaultCommandTimeoutForEF
		{
			get { return (bool)values["usedefaultcommandtimeoutforef"]; }
			set { SetValue("usedefaultcommandtimeoutforef", value); }
		}
		#endregion

		#region Fabric Properties

		public string FabricGroup { get; internal set; }

		public string ShardingTable { get; internal set; }

		public object ShardingKey { get; internal set; }

		public int? FabricServerMode { get; internal set; }

		public int? FabricScope { get; internal set; }

		#endregion

		internal bool HasProcAccess { get; set; }

		public override object this[string keyword]
		{
			get { MySqlConnectionStringOption opt = GetOption(keyword); return opt.Getter(this, opt); }
			set { MySqlConnectionStringOption opt = GetOption(keyword); opt.Setter(this, opt, value); }
		}

		internal Regex GetBlobAsUTF8IncludeRegex()
		{
			if (String.IsNullOrEmpty(BlobAsUTF8IncludePattern)) return null;
			return new Regex(BlobAsUTF8IncludePattern);
		}

		internal Regex GetBlobAsUTF8ExcludeRegex()
		{
			if (String.IsNullOrEmpty(BlobAsUTF8ExcludePattern)) return null;
			return new Regex(BlobAsUTF8ExcludePattern);
		}

		public override void Clear()
		{
			base.Clear();
			lock (this)
			{
				foreach (var option in options.Options)
					if (option.DefaultValue != null)
						values[option.Keyword] = option.DefaultValue;
					else
						values[option.Keyword] = null;
			}
		}

		internal void SetValue(string keyword, object value)
		{
			MySqlConnectionStringOption option = GetOption(keyword);
			option.ValidateValue(ref value);

			// remove all related keywords
			option.Clean(this);

			if (value != null)
			{
				lock (this)
				{
					// set value for the given keyword
					values[option.Keyword] = value;
					base[keyword] = value;
				}
			}
		}

		private MySqlConnectionStringOption GetOption(string key)
		{
			MySqlConnectionStringOption option = options.Get(key);
			if (option == null)
				throw new ArgumentException("Resources.KeywordNotSupported", key);
			else
				return option;
		}

		public override bool ContainsKey(string keyword)
		{
			MySqlConnectionStringOption option = options.Get(keyword);
			return option != null;
		}

		public override bool Remove(string keyword)
		{
			bool removed = false;
			lock (this) { removed = base.Remove(keyword); }
			if (!removed) return false;
			MySqlConnectionStringOption option = GetOption(keyword);
			lock (this)
			{
				values[option.Keyword] = option.DefaultValue;
			}
			return true;
		}

		public string GetConnectionString(bool includePass)
		{
			if (includePass) return ConnectionString;

			StringBuilder conn = new StringBuilder();
			string delimiter = "";
			foreach (string key in this.Keys)
			{
				if (String.Compare(key, "password", StringComparison.OrdinalIgnoreCase) == 0 ||
					String.Compare(key, "pwd", StringComparison.OrdinalIgnoreCase) == 0) continue;
				conn.AppendFormat(CultureInfo.CurrentCulture, "{0}{1}={2}",
					delimiter, key, this[key]);
				delimiter = ";";
			}
			return conn.ToString();
		}

		public override bool Equals(object obj)
		{
			MySqlConnectionStringBuilder other = obj as MySqlConnectionStringBuilder;
			if (obj == null)
				return false;

			if (this.values.Count != other.values.Count) return false;

			foreach (KeyValuePair<string, object> kvp in this.values)
			{
				if (other.values.ContainsKey(kvp.Key))
				{
					object v = other.values[kvp.Key];
					if (v == null && kvp.Value != null) return false;
					if (kvp.Value == null && v != null) return false;
					if (kvp.Value == null && v == null) return true;
					if (!v.Equals(kvp.Value)) return false;
				}
				else
				{
					return false;
				}
			}

			return true;
		}
	}

	class MySqlConnectionStringOption
	{
		public MySqlConnectionStringOption(string keyword, string synonyms, Type baseType, object defaultValue, bool obsolete,
		  SetterDelegate setter, GetterDelegate getter)
		{
			Keyword = keyword.ToLower();
			if (synonyms != null)
				Synonyms = synonyms.ToLower().Split(',');
			BaseType = baseType;
			Obsolete = obsolete;
			DefaultValue = defaultValue;
			Setter = setter;
			Getter = getter;
		}

		public MySqlConnectionStringOption(string keyword, string synonyms, Type baseType, object defaultValue, bool obsolete) :
		  this(keyword, synonyms, baseType, defaultValue, obsolete,
		   delegate (MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender, object value)
		   {
			   sender.ValidateValue(ref value);
		   //if ( sender.BaseType.IsEnum )
		   //  msb.SetValue( sender.Keyword, Enum.Parse( sender.BaseType, ( string )value, true ));
		   //else
#if !CF
		   msb.SetValue(sender.Keyword, Convert.ChangeType(value, sender.BaseType));
#else
           msb.SetValue( sender.Keyword, Convert.ChangeType(value, sender.BaseType, System.Globalization.CultureInfo.CurrentCulture));
#endif
	   },
		   delegate (MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender)
		   {
			   return msb.values[sender.Keyword];
		   }
		 )
		{
		}

		public string[] Synonyms { get; private set; }
		public bool Obsolete { get; private set; }
		public Type BaseType { get; private set; }
		public string Keyword { get; private set; }
		public object DefaultValue { get; private set; }
		public SetterDelegate Setter { get; private set; }
		public GetterDelegate Getter { get; private set; }

		public delegate void SetterDelegate(MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender, object value);
		public delegate object GetterDelegate(MySqlConnectionStringBuilder msb, MySqlConnectionStringOption sender);

		public bool HasKeyword(string key)
		{
			if (Keyword == key) return true;
			if (Synonyms == null) return false;
			foreach (var syn in Synonyms)
				if (syn == key) return true;
			return false;
		}

		public void Clean(MySqlConnectionStringBuilder builder)
		{
			builder.Remove(Keyword);
			if (Synonyms == null) return;
			foreach (var syn in Synonyms)
				builder.Remove(syn);
		}

		public void ValidateValue(ref object value)
		{
			bool b;
			if (value == null) return;
			string typeName = BaseType.Name;
			Type valueType = value.GetType();
			if (valueType.Name == "String")
			{
				if (BaseType == valueType) return;
				else if (BaseType == typeof(bool))
				{
					if (string.Compare("yes", (string)value, StringComparison.OrdinalIgnoreCase) == 0) value = true;
					else if (string.Compare("no", (string)value, StringComparison.OrdinalIgnoreCase) == 0) value = false;
 
					else if (Boolean.TryParse(value.ToString(), out b)) value = b;
 
					else throw new ArgumentException(String.Format("Resources.ValueNotCorrectType", value));
					return;
				}
			}

 
			if (typeName == "Boolean" && Boolean.TryParse(value.ToString(), out b)) { value = b; return; }

			UInt64 uintVal;
			if (typeName.StartsWith("UInt64") && UInt64.TryParse(value.ToString(), out uintVal)) { value = uintVal; return; }

			UInt32 uintVal32;
			if (typeName.StartsWith("UInt32") && UInt32.TryParse(value.ToString(), out uintVal32)) { value = uintVal32; return; }

			Int64 intVal;
			if (typeName.StartsWith("Int64") && Int64.TryParse(value.ToString(), out intVal)) { value = intVal; return; }

			Int32 intVal32;
			if (typeName.StartsWith("Int32") && Int32.TryParse(value.ToString(), out intVal32)) { value = intVal32; return; }
 

			object objValue;
 
      Type baseType = BaseType.GetTypeInfo().BaseType;
 
			if (baseType != null && baseType.Name == "Enum" && ParseEnum(value.ToString(), out objValue))
			{
				value = objValue; return;
			}

			throw new ArgumentException(String.Format("Resources.ValueNotCorrectType", value));
		}

		private bool ParseEnum(string requestedValue, out object value)
		{
			value = null;
			try
			{
				value = Enum.Parse(BaseType, requestedValue, true);
				return true;
			}
			catch (ArgumentException)
			{
				return false;
			}
		}

	}

	internal class MySqlConnectionStringOptionCollection : Dictionary<string, MySqlConnectionStringOption>
	{
		List<MySqlConnectionStringOption> options;

		internal List<MySqlConnectionStringOption> Options { get { return options; } }

		internal MySqlConnectionStringOptionCollection() : base(StringComparer.OrdinalIgnoreCase)
		{
			options = new List<MySqlConnectionStringOption>();
		}

		internal void Add(MySqlConnectionStringOption option)
		{
			options.Add(option);
			// Register the option with all the keywords.
			base.Add(option.Keyword, option);
			if (option.Synonyms != null)
			{
				for (int i = 0; i < option.Synonyms.Length; i++)
					base.Add(option.Synonyms[i], option);
			}
		}

		internal MySqlConnectionStringOption Get(string keyword)
		{
			MySqlConnectionStringOption option = null;
			base.TryGetValue(keyword, out option);
			return option;
		}
	}
}
