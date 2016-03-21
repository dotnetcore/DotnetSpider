// Copyright © 2012, Oracle and/or its affiliates. All rights reserved.
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

using System.IO;
using System;

using MySql.Data.Common;
using System.Text;
using System.Diagnostics;

namespace MySql.Data.MySqlClient.Authentication
{
  public abstract class MySqlAuthenticationPlugin
  {
    private NativeDriver driver;
    protected byte[] AuthenticationData;

    /// <summary>
    /// This is a factory method that is used only internally.  It creates an auth plugin based on the method type
    /// </summary>
    /// <param name="method"></param>
    /// <param name="flags"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    internal static MySqlAuthenticationPlugin GetPlugin(string method, NativeDriver driver, byte[] authData)
    {
      if (method == "mysql_old_password")
      {
        driver.Close(true);
        throw new MySqlException("Resources.OldPasswordsNotSupported");

	  }
      MySqlAuthenticationPlugin plugin = AuthenticationPluginManager.GetPlugin(method);
      if (plugin == null)
        throw new MySqlException(String.Format("Resources.UnknownAuthenticationMethod", method));


	  plugin.driver = driver;
      plugin.SetAuthData(authData);
      return plugin;
    }

    protected MySqlConnectionStringBuilder Settings
    {
      get { return driver.Settings; }
    }

    protected Version ServerVersion
    {
      get { return new Version(driver.Version.Major, driver.Version.Minor, driver.Version.Build); }
    }

    internal ClientFlags Flags
    {
      get { return driver.Flags; }
    }

    protected Encoding Encoding 
    { 
      get { return driver.Encoding; } 
    }

    protected virtual void SetAuthData(byte[] data)
    {
      AuthenticationData = data;
    }

    protected virtual void CheckConstraints()
    {
    }

    protected virtual void AuthenticationFailed(Exception ex)
    {
      string msg = String.Format("Resources.AuthenticationFailed", Settings.Server, GetUsername(), PluginName, ex.Message);

	  throw new MySqlException(msg, ex);
    }

    protected virtual void AuthenticationSuccessful()
    {
    }

    protected virtual byte[] MoreData(byte[] data)
    {
      return null;
    }

    internal void Authenticate(bool reset)
    {
      CheckConstraints();

      MySqlPacket packet = driver.Packet;

      // send auth response
      packet.WriteString(GetUsername());

      // now write the password
      WritePassword(packet);

      if ((Flags & ClientFlags.CONNECT_WITH_DB) != 0 || reset)
      {
        if (!String.IsNullOrEmpty(Settings.Database))
          packet.WriteString(Settings.Database);
      }

      if (reset)
        packet.WriteInteger(8, 2);

      if ((Flags & ClientFlags.PLUGIN_AUTH) != 0)
        packet.WriteString(PluginName);

     driver.SetConnectAttrs();
      driver.SendPacket(packet);
      //read server response
      packet = ReadPacket();
      byte[] b = packet.Buffer;
      if (b[0] == 0xfe)
      {
        if (packet.IsLastPacket)
        {
          driver.Close(true);
          throw new MySqlException("Resources.OldPasswordsNotSupported");

		}
        else
        {
          HandleAuthChange(packet);
        }
      }
      driver.ReadOk(false);
      AuthenticationSuccessful();
    }

    private void WritePassword(MySqlPacket packet)
    {
      bool secure = (Flags & ClientFlags.SECURE_CONNECTION) != 0;
      object password = GetPassword();
      if (password is string)
      {
        if (secure)
          packet.WriteLenString((string)password);
        else
          packet.WriteString((string)password);
      }
      else if (password == null)
        packet.WriteByte(0);
      else if (password is byte[])
        packet.Write(password as byte[]);
      else throw new MySqlException("Unexpected password format: " + password.GetType());
    }

    private MySqlPacket ReadPacket()
    {
      try
      {
        MySqlPacket p = driver.ReadPacket();
        return p;
      }
      catch (MySqlException ex)
      {
        // make sure this is an auth failed ex
        AuthenticationFailed(ex);
        return null;
      }
    }

    private void HandleAuthChange(MySqlPacket packet)
    {
      byte b = packet.ReadByte();
      Debug.Assert(b == 0xfe);

      string method = packet.ReadString();
      byte[] authData = new byte[packet.Length - packet.Position];
      Array.Copy(packet.Buffer, packet.Position, authData, 0, authData.Length);

      MySqlAuthenticationPlugin plugin = MySqlAuthenticationPlugin.GetPlugin(method, driver, authData);
      plugin.AuthenticationChange();
    }

    private void AuthenticationChange()
    {
      MySqlPacket packet = driver.Packet;
      packet.Clear();
      byte[] moreData = MoreData(null);
      while (moreData != null && moreData.Length > 0)
      {
        packet.Clear();
        packet.Write(moreData);
        driver.SendPacket(packet);

        packet = ReadPacket();
        byte prefixByte = packet.Buffer[0];
        if (prefixByte != 1) return;

        // a prefix of 0x01 means need more auth data
        byte[] responseData = new byte[packet.Length - 1];
        Array.Copy(packet.Buffer, 1, responseData, 0, responseData.Length);
        moreData = MoreData(responseData);
      }
      // we get here if MoreData returned null but the last packet read was a more data packet
      ReadPacket();
    }

    public abstract string PluginName { get; }

    public virtual string GetUsername()
    {
      return Settings.UserID;
    }

    public virtual object GetPassword()
    {
      return null;
    }
  }
}
