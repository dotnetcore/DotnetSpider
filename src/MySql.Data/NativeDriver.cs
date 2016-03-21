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
using System.Diagnostics;
using System.IO;
using MySql.Data.Common;
using MySql.Data.Types;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient.Authentication;
using System.Reflection;
using System.ComponentModel;
using System.Security.Authentication; 
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Globalization;
 

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for Driver.
	/// </summary>
	internal class NativeDriver : IDriver
	{
		private DBVersion version;
		private int threadId;
		protected byte[] encryptionSeed;
		protected ServerStatusFlags serverStatus;
		protected MySqlStream stream;
		protected Stream baseStream;
		private BitArray nullMap;
		private MySqlPacket packet;
		private ClientFlags connectionFlags;
		private Driver owner;
		private int warnings;
		private MySqlAuthenticationPlugin authPlugin;

		// Windows authentication method string, used by the protocol.
		// Also known as "client plugin name".
		const string AuthenticationWindowsPlugin = "authentication_windows_client";

		// Predefined username for IntegratedSecurity
		const string AuthenticationWindowsUser = "auth_windows";

		public NativeDriver(Driver owner)
		{
			this.owner = owner;
			threadId = -1;
		}

		public ClientFlags Flags
		{
			get { return connectionFlags; }
		}

		public int ThreadId
		{
			get { return threadId; }
		}

		public DBVersion Version
		{
			get { return version; }
		}

		public ServerStatusFlags ServerStatus
		{
			get { return serverStatus; }
		}

		public int WarningCount
		{
			get { return warnings; }
		}

		public MySqlPacket Packet
		{
			get { return packet; }
		}

		internal MySqlConnectionStringBuilder Settings
		{
			get { return owner.Settings; }
		}

		internal Encoding Encoding
		{
			get { return owner.Encoding; }
		}

		private void HandleException(MySqlException ex)
		{
			if (ex.IsFatal)
				owner.Close();
		}

		internal void SendPacket(MySqlPacket p)
		{
			stream.SendPacket(p);
		}

		internal void SendEmptyPacket()
		{
			byte[] buffer = new byte[4];
			stream.SendEntirePacketDirectly(buffer, 0);
		}

		internal MySqlPacket ReadPacket()
		{
			return packet = stream.ReadPacket();
		}

		internal void ReadOk(bool read)
		{
			try
			{
				if (read)
					packet = stream.ReadPacket();
				byte marker = (byte)packet.ReadByte();
				if (marker != 0)
				{
					throw new MySqlException("Out of sync with server", true, null);
				}

				packet.ReadFieldLength(); /* affected rows */
				packet.ReadFieldLength(); /* last insert id */
				if (packet.HasMoreData)
				{
					serverStatus = (ServerStatusFlags)packet.ReadInteger(2);
					packet.ReadInteger(2);  /* warning count */
					if (packet.HasMoreData)
					{
						packet.ReadLenString();  /* message */
					}
				}
			}
			catch (MySqlException ex)
			{
				HandleException(ex);
				throw;
			}
		}

		/// <summary>
		/// Sets the current database for the this connection
		/// </summary>
		/// <param name="dbName"></param>
		public void SetDatabase(string dbName)
		{
			byte[] dbNameBytes = Encoding.GetBytes(dbName);

			packet.Clear();
			packet.WriteByte((byte)DBCmd.INIT_DB);
			packet.Write(dbNameBytes);
			ExecutePacket(packet);

			ReadOk(true);
		}

		public void Configure()
		{
			stream.MaxPacketSize = (ulong)owner.MaxPacketSize;
			stream.Encoding = Encoding;
		}

		public void Open()
		{
			// connect to one of our specified hosts
			try
			{
				baseStream = StreamCreator.GetStream(Settings);
				//#if !CF && !RT
				//         if (Settings.IncludeSecurityAsserts)
				//            MySqlSecurityPermission.CreatePermissionSet(false).Assert();
				//#endif
			}
			catch (System.Security.SecurityException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new MySqlException("Resources.UnableToConnectToHost",
					(int)MySqlErrorCode.UnableToConnectToHost, ex);
			}
			if (baseStream == null)
				throw new MySqlException("Resources.UnableToConnectToHost",
					(int)MySqlErrorCode.UnableToConnectToHost);

			int maxSinglePacket = 255 * 255 * 255;
			stream = new MySqlStream(baseStream, Encoding, false);

			stream.ResetTimeout((int)Settings.ConnectionTimeout * 1000);

			// read off the welcome packet and parse out it's values
			packet = stream.ReadPacket();
			int protocol = packet.ReadByte();
			string versionString = packet.ReadString();
			owner.isFabric = versionString.EndsWith("fabric", StringComparison.OrdinalIgnoreCase);
			version = DBVersion.Parse(versionString);
			if (!owner.isFabric && !version.isAtLeast(5, 0, 0))
				throw new NotSupportedException("Resources.ServerTooOld");

			threadId = packet.ReadInteger(4);

			byte[] seedPart1 = packet.ReadStringAsBytes();

			maxSinglePacket = (256 * 256 * 256) - 1;

			// read in Server capabilities if they are provided
			ClientFlags serverCaps = 0;
			if (packet.HasMoreData)
				serverCaps = (ClientFlags)packet.ReadInteger(2);

			/* New protocol with 16 bytes to describe server characteristics */
			owner.ConnectionCharSetIndex = (int)packet.ReadByte();

			serverStatus = (ServerStatusFlags)packet.ReadInteger(2);

			// Since 5.5, high bits of server caps are stored after status.
			// Previously, it was part of reserved always 0x00 13-byte filler.
			uint serverCapsHigh = (uint)packet.ReadInteger(2);
			serverCaps |= (ClientFlags)(serverCapsHigh << 16);

			packet.Position += 11;
			byte[] seedPart2 = packet.ReadStringAsBytes();
			encryptionSeed = new byte[seedPart1.Length + seedPart2.Length];
			seedPart1.CopyTo(encryptionSeed, 0);
			seedPart2.CopyTo(encryptionSeed, seedPart1.Length);

			string authenticationMethod = "";
			if ((serverCaps & ClientFlags.PLUGIN_AUTH) != 0)
			{
				authenticationMethod = packet.ReadString();
			}
			else
			{
				// Some MySql versions like 5.1, don't give name of plugin, default to native password.
				authenticationMethod = "mysql_native_password";
			}

			// based on our settings, set our connection flags
			SetConnectionFlags(serverCaps);

			packet.Clear();
			packet.WriteInteger((int)connectionFlags, 4);
			packet.WriteInteger(maxSinglePacket, 4);
			packet.WriteByte(33); //character set utf-8
			packet.Write(new byte[23]);

#if !CF && !RT
			if ((serverCaps & ClientFlags.SSL) == 0)
			{
				if ((Settings.SslMode != MySqlSslMode.None)
				&& (Settings.SslMode != MySqlSslMode.Preferred))
				{
					// Client requires SSL connections.
					string message = String.Format("Resources.NoServerSSLSupport",

						Settings.Server);
					throw new MySqlException(message);
				}
			}
			else if (Settings.SslMode != MySqlSslMode.None)
			{
				stream.SendPacket(packet);
				StartSSL();
				packet.Clear();
				packet.WriteInteger((int)connectionFlags, 4);
				packet.WriteInteger(maxSinglePacket, 4);
				packet.WriteByte(33); //character set utf-8
				packet.Write(new byte[23]);
			}
#endif

#if RT
      if (Settings.SslMode != MySqlSslMode.None)
      {
        throw new NotImplementedException("SSL not supported in this WinRT release.");
      }
#endif

			Authenticate(authenticationMethod, false);

			// if we are using compression, then we use our CompressedStream class
			// to hide the ugliness of managing the compression
			if ((connectionFlags & ClientFlags.COMPRESS) != 0)
				stream = new MySqlStream(baseStream, Encoding, true);

			// give our stream the server version we are connected to.  
			// We may have some fields that are read differently based 
			// on the version of the server we are connected to.
			packet.Version = version;
			stream.MaxBlockSize = maxSinglePacket;
		}

#if !CF && !RT

		#region SSL

		/// <summary>
		/// Retrieve client SSL certificates. Dependent on connection string 
		/// settings we use either file or store based certificates.
		/// </summary>
		private X509CertificateCollection GetClientCertificates()
		{
			X509CertificateCollection certs = new X509CertificateCollection();

			// Check for file-based certificate
			if (Settings.CertificateFile != null)
			{
				if (!Version.isAtLeast(5, 1, 0))
					throw new MySqlException("Properties.Resources.FileBasedCertificateNotSupported");


				X509Certificate2 clientCert = new X509Certificate2(Settings.CertificateFile,
					Settings.CertificatePassword);
				certs.Add(clientCert);
				return certs;
			}

			if (Settings.CertificateStoreLocation == MySqlCertificateStoreLocation.None)
				return certs;

			StoreLocation location =
				(Settings.CertificateStoreLocation == MySqlCertificateStoreLocation.CurrentUser) ?
				StoreLocation.CurrentUser : StoreLocation.LocalMachine;

			// Check for store-based certificate
			X509Store store = new X509Store(StoreName.My, location);
			store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);


			if (Settings.CertificateThumbprint == null)
			{
				// Return all certificates from the store.
				certs.AddRange(store.Certificates);
				return certs;
			}

			// Find certificate with given thumbprint
			certs.AddRange(store.Certificates.Find(X509FindType.FindByThumbprint,
					  Settings.CertificateThumbprint, true));

			if (certs.Count == 0)
			{
				throw new MySqlException("Certificate with Thumbprint " +
				   Settings.CertificateThumbprint + " not found");
			}
			return certs;
		}


		private void StartSSL()
		{
			RemoteCertificateValidationCallback sslValidateCallback =
				new RemoteCertificateValidationCallback(ServerCheckValidation);
			SslStream ss = new SslStream(baseStream, true, sslValidateCallback, null);
			X509CertificateCollection certs = GetClientCertificates();
			//ss.AuthenticateAsClient(Settings.Server, certs, SslProtocols.Tls, false);
			baseStream = ss;
			stream = new MySqlStream(ss, Encoding, false);
			stream.SequenceByte = 2;

		}

		private bool ServerCheckValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None)
				return true;

			if (Settings.SslMode == MySqlSslMode.Preferred ||
				Settings.SslMode == MySqlSslMode.Required)
			{
				//Tolerate all certificate errors.
				return true;
			}

			if (Settings.SslMode == MySqlSslMode.VerifyCA &&
				sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
			{
				// Tolerate name mismatch in certificate, if full validation is not requested.
				return true;
			}

			return false;
		}


		#endregion

#endif

		#region Authentication

		/// <summary>
		/// Return the appropriate set of connection flags for our
		/// server capabilities and our user requested options.
		/// </summary>
		private void SetConnectionFlags(ClientFlags serverCaps)
		{
			// allow load data local infile
			ClientFlags flags = ClientFlags.LOCAL_FILES;

			if (!Settings.UseAffectedRows)
				flags |= ClientFlags.FOUND_ROWS;

			flags |= ClientFlags.PROTOCOL_41;
			// Need this to get server status values
			flags |= ClientFlags.TRANSACTIONS;

			// user allows/disallows batch statements
			if (Settings.AllowBatch)
				flags |= ClientFlags.MULTI_STATEMENTS;

			// We always allow multiple result sets
			flags |= ClientFlags.MULTI_RESULTS;

			// if the server allows it, tell it that we want long column info
			if ((serverCaps & ClientFlags.LONG_FLAG) != 0)
				flags |= ClientFlags.LONG_FLAG;

			// if the server supports it and it was requested, then turn on compression
			if ((serverCaps & ClientFlags.COMPRESS) != 0 && Settings.UseCompression)
				flags |= ClientFlags.COMPRESS;

			flags |= ClientFlags.LONG_PASSWORD; // for long passwords

			// did the user request an interactive session?
			if (Settings.InteractiveSession)
				flags |= ClientFlags.INTERACTIVE;

			// if the server allows it and a database was specified, then indicate
			// that we will connect with a database name
			if ((serverCaps & ClientFlags.CONNECT_WITH_DB) != 0 &&
				Settings.Database != null && Settings.Database.Length > 0)
				flags |= ClientFlags.CONNECT_WITH_DB;

			// if the server is requesting a secure connection, then we oblige
			if ((serverCaps & ClientFlags.SECURE_CONNECTION) != 0)
				flags |= ClientFlags.SECURE_CONNECTION;

#if !CF
			// if the server is capable of SSL and the user is requesting SSL
			if ((serverCaps & ClientFlags.SSL) != 0 && Settings.SslMode != MySqlSslMode.None)
				flags |= ClientFlags.SSL;
#endif

			// if the server supports output parameters, then we do too
			if ((serverCaps & ClientFlags.PS_MULTI_RESULTS) != 0)
				flags |= ClientFlags.PS_MULTI_RESULTS;

			if ((serverCaps & ClientFlags.PLUGIN_AUTH) != 0)
				flags |= ClientFlags.PLUGIN_AUTH;

			// if the server supports connection attributes
			if ((serverCaps & ClientFlags.CONNECT_ATTRS) != 0)
				flags |= ClientFlags.CONNECT_ATTRS;

			if ((serverCaps & ClientFlags.CAN_HANDLE_EXPIRED_PASSWORD) != 0)
				flags |= ClientFlags.CAN_HANDLE_EXPIRED_PASSWORD;

			connectionFlags = flags;
		}


		internal void SetConnectAttrs()
		{
			// Sets connect attributes
			if ((connectionFlags & ClientFlags.CONNECT_ATTRS) != 0)
			{
				string connectAttrs = string.Empty;
				MySqlConnectAttrs attrs = new MySqlConnectAttrs();
				foreach (PropertyInfo property in attrs.GetType().GetProperties())
				{
					string name = property.Name;
 
          object[] customAttrs = property.GetCustomAttributes(typeof(DisplayNameAttribute), false).ToArray<object>();
 
					if (customAttrs.Length > 0)
						name = (customAttrs[0] as DisplayNameAttribute).DisplayName;

					string value = (string)property.GetValue(attrs, null);
					connectAttrs += string.Format("{0}{1}", (char)name.Length, name);
					connectAttrs += string.Format("{0}{1}", (char)value.Length, value);
				}
				packet.WriteLenString(connectAttrs);
			}
		}
	 
	
		public void Authenticate(string authMethod, bool reset)
		{
			if (authMethod != null)
			{
				// Integrated security is a shortcut for windows auth
				if (Settings.IntegratedSecurity)
					authMethod = "authentication_windows_client";

				authPlugin = MySqlAuthenticationPlugin.GetPlugin(authMethod, this, encryptionSeed);
			}

			authPlugin.Authenticate(reset);
		}

		#endregion

		public void Reset()
		{
			warnings = 0;
			stream.Encoding = this.Encoding;
			stream.SequenceByte = 0;
			packet.Clear();
			packet.WriteByte((byte)DBCmd.CHANGE_USER);
			Authenticate(null, true);
		}

		/// <summary>
		/// Query is the method that is called to send all queries to the server
		/// </summary>
		public void SendQuery(MySqlPacket queryPacket)
		{
			warnings = 0;
			queryPacket.SetByte(4, (byte)DBCmd.QUERY);
			ExecutePacket(queryPacket);
			// the server will respond in one of several ways with the first byte indicating
			// the type of response.
			// 0 == ok packet.  This indicates non-select queries
			// 0xff == error packet.  This is handled in stream.OpenPacket
			// > 0 = number of columns in select query
			// We don't actually read the result here since a single query can generate
			// multiple resultsets and we don't want to duplicate code.  See ReadResult
			// Instead we set our internal server status flag to indicate that we have a query waiting.
			// This flag will be maintained by ReadResult
			serverStatus |= ServerStatusFlags.AnotherQuery;
		}

		public void Close(bool isOpen)
		{
			try
			{
				if (isOpen)
				{
					try
					{
						packet.Clear();
						packet.WriteByte((byte)DBCmd.QUIT);
						ExecutePacket(packet);
					}
					catch (Exception)
					{
						// Eat exception here. We should try to closing 
						// the stream anyway.
					}
				}

				if (stream != null)
					stream.Close();
				stream = null;
			}
			catch (Exception)
			{
				// we are just going to eat any exceptions
				// generated here
			}
		}

		public bool Ping()
		{
			try
			{
				packet.Clear();
				packet.WriteByte((byte)DBCmd.PING);
				ExecutePacket(packet);
				ReadOk(true);
				return true;
			}
			catch (Exception)
			{
				owner.Close();
				return false;
			}
		}

		public int GetResult(ref int affectedRow, ref long insertedId)
		{
			try
			{
				packet = stream.ReadPacket();
			}
			catch (TimeoutException)
			{
				// Do not reset serverStatus, allow to reenter, e.g when
				// ResultSet is closed.
				throw;
			}
			catch (Exception)
			{
				serverStatus = 0;
				throw;
			}

			int fieldCount = (int)packet.ReadFieldLength();
			if (-1 == fieldCount)
			{
				string filename = packet.ReadString();
				SendFileToServer(filename);

				return GetResult(ref affectedRow, ref insertedId);
			}
			else if (fieldCount == 0)
			{
				// the code to read last packet will set these server status vars 
				// again if necessary.
				serverStatus &= ~(ServerStatusFlags.AnotherQuery |
								  ServerStatusFlags.MoreResults);
				affectedRow = (int)packet.ReadFieldLength();
				insertedId = (long)packet.ReadFieldLength();

				serverStatus = (ServerStatusFlags)packet.ReadInteger(2);
				warnings += packet.ReadInteger(2);
				if (packet.HasMoreData)
				{
					packet.ReadLenString(); //TODO: server message
				}
			}
			return fieldCount;
		}

		/// <summary>
		/// Sends the specified file to the server. 
		/// This supports the LOAD DATA LOCAL INFILE
		/// </summary>
		/// <param name="filename"></param>
		private void SendFileToServer(string filename)
		{
			byte[] buffer = new byte[8196];

			long len = 0;
			try
			{
				using (FileStream fs = new FileStream(filename, FileMode.Open))
				{
					len = fs.Length;
					while (len > 0)
					{
						int count = fs.Read(buffer, 4, (int)(len > 8192 ? 8192 : len));
						stream.SendEntirePacketDirectly(buffer, count);
						len -= count;
					}
					stream.SendEntirePacketDirectly(buffer, 0);
				}
			}
			catch (Exception ex)
			{
				throw new MySqlException("Error during LOAD DATA LOCAL INFILE", ex);
			}
		}

		private void ReadNullMap(int fieldCount)
		{
			// if we are binary, then we need to load in our null bitmap
			nullMap = null;
			byte[] nullMapBytes = new byte[(fieldCount + 9) / 8];
			packet.ReadByte();
			packet.Read(nullMapBytes, 0, nullMapBytes.Length);
			nullMap = new BitArray(nullMapBytes);
		}

		public IMySqlValue ReadColumnValue(int index, MySqlField field, IMySqlValue valObject)
		{
			long length = -1;
			bool isNull;

			if (nullMap != null)
				isNull = nullMap[index + 2];
			else
			{
				length = packet.ReadFieldLength();
				isNull = length == -1;
			}

			packet.Encoding = field.Encoding;
			packet.Version = version;
			return valObject.ReadValue(packet, length, isNull);
		}

		public void SkipColumnValue(IMySqlValue valObject)
		{
			int length = -1;
			if (nullMap == null)
			{
				length = (int)packet.ReadFieldLength();
				if (length == -1) return;
			}
			if (length > -1)
				packet.Position += length;
			else
				valObject.SkipValue(packet);
		}

		public void GetColumnsData(MySqlField[] columns)
		{
			for (int i = 0; i < columns.Length; i++)
				GetColumnData(columns[i]);
			ReadEOF();
		}

		private void GetColumnData(MySqlField field)
		{
			stream.Encoding = Encoding;
			packet = stream.ReadPacket();
			field.Encoding = Encoding;
			field.CatalogName = packet.ReadLenString();
			field.DatabaseName = packet.ReadLenString();
			field.TableName = packet.ReadLenString();
			field.RealTableName = packet.ReadLenString();
			field.ColumnName = packet.ReadLenString();
			field.OriginalColumnName = packet.ReadLenString();
			packet.ReadByte();
			field.CharacterSetIndex = packet.ReadInteger(2);
			field.ColumnLength = packet.ReadInteger(4);
			MySqlDbType type = (MySqlDbType)packet.ReadByte();
			ColumnFlags colFlags;
			if ((connectionFlags & ClientFlags.LONG_FLAG) != 0)
				colFlags = (ColumnFlags)packet.ReadInteger(2);
			else
				colFlags = (ColumnFlags)packet.ReadByte();
			field.Scale = (byte)packet.ReadByte();

			if (packet.HasMoreData)
			{
				packet.ReadInteger(2); // reserved
			}

			if (type == MySqlDbType.Decimal || type == MySqlDbType.NewDecimal)
			{
				field.Precision = (byte)(field.ColumnLength - 2);
				if ((colFlags & ColumnFlags.UNSIGNED) != 0)
					field.Precision++;
			}

			field.SetTypeAndFlags(type, colFlags);
		}

		private void ExecutePacket(MySqlPacket packetToExecute)
		{
			try
			{
				warnings = 0;
				stream.SequenceByte = 0;
				stream.SendPacket(packetToExecute);
			}
			catch (MySqlException ex)
			{
				HandleException(ex);
				throw;
			}
		}

		public void ExecuteStatement(MySqlPacket packetToExecute)
		{
			warnings = 0;
			packetToExecute.SetByte(4, (byte)DBCmd.EXECUTE);
			ExecutePacket(packetToExecute);
			serverStatus |= ServerStatusFlags.AnotherQuery;
		}

		private void CheckEOF()
		{
			if (!packet.IsLastPacket)
				throw new MySqlException("Expected end of data packet");

			packet.ReadByte(); // read off the 254

			if (packet.HasMoreData)
			{
				warnings += packet.ReadInteger(2);
				serverStatus = (ServerStatusFlags)packet.ReadInteger(2);

				// if we are at the end of this cursor based resultset, then we remove
				// the last row sent status flag so our next fetch doesn't abort early
				// and we remove this command result from our list of active CommandResult objects.
				//                if ((serverStatus & ServerStatusFlags.LastRowSent) != 0)
				//              {
				//                serverStatus &= ~ServerStatusFlags.LastRowSent;
				//              commandResults.Remove(lastCommandResult);
				//        }
			}
		}

		private void ReadEOF()
		{
			packet = stream.ReadPacket();
			CheckEOF();
		}

		public int PrepareStatement(string sql, ref MySqlField[] parameters)
		{
			//TODO: check this
			//ClearFetchedRow();

			packet.Length = sql.Length * 4 + 5;
			byte[] buffer = packet.Buffer;
			int len = Encoding.GetBytes(sql, 0, sql.Length, packet.Buffer, 5);
			packet.Position = len + 5;
			buffer[4] = (byte)DBCmd.PREPARE;
			ExecutePacket(packet);

			packet = stream.ReadPacket();

			int marker = packet.ReadByte();
			if (marker != 0)
				throw new MySqlException("Expected prepared statement marker");

			int statementId = packet.ReadInteger(4);
			int numCols = packet.ReadInteger(2);
			int numParams = packet.ReadInteger(2);
			//TODO: find out what this is needed for
			packet.ReadInteger(3);
			if (numParams > 0)
			{
				parameters = owner.GetColumns(numParams);
				// we set the encoding for each parameter back to our connection encoding
				// since we can't trust what is coming back from the server
				for (int i = 0; i < parameters.Length; i++)
					parameters[i].Encoding = Encoding;
			}

			if (numCols > 0)
			{
				while (numCols-- > 0)
				{
					packet = stream.ReadPacket();
					//TODO: handle streaming packets
				}

				ReadEOF();
			}

			return statementId;
		}

		//		private void ClearFetchedRow() 
		//		{
		//			if (lastCommandResult == 0) return;

		//TODO
		/*			CommandResult result = (CommandResult)commandResults[lastCommandResult];
					result.ReadRemainingColumns();

					stream.OpenPacket();
					if (! stream.IsLastPacket)
						throw new MySqlException("Cursor reading out of sync");

					ReadEOF(false);
					lastCommandResult = 0;*/
		//		}

		/// <summary>
		/// FetchDataRow is the method that the data reader calls to see if there is another 
		/// row to fetch.  In the non-prepared mode, it will simply read the next data packet.
		/// In the prepared mode (statementId > 0), it will 
		/// </summary>
		public bool FetchDataRow(int statementId, int columns)
		{
			/*			ClearFetchedRow();

						if (!commandResults.ContainsKey(statementId)) return false;

						if ( (serverStatus & ServerStatusFlags.LastRowSent) != 0)
							return false;

						stream.StartPacket(9, true);
						stream.WriteByte((byte)DBCmd.FETCH);
						stream.WriteInteger(statementId, 4);
						stream.WriteInteger(1, 4);
						stream.Flush();

						lastCommandResult = statementId;
							*/
			packet = stream.ReadPacket();
			if (packet.IsLastPacket)
			{
				CheckEOF();
				return false;
			}
			nullMap = null;
			if (statementId > 0)
				ReadNullMap(columns);

			return true;
		}

		public void CloseStatement(int statementId)
		{
			packet.Clear();
			packet.WriteByte((byte)DBCmd.CLOSE_STMT);
			packet.WriteInteger((long)statementId, 4);
			stream.SequenceByte = 0;
			stream.SendPacket(packet);
		}

		/// <summary>
		/// Execution timeout, in milliseconds. When the accumulated time for network IO exceeds this value
		/// TimeoutException is thrown. This timeout needs to be reset for every new command
		/// </summary>
		/// 
		public void ResetTimeout(int timeout)
		{
			if (stream != null)
				stream.ResetTimeout(timeout);
		}

	}

}
