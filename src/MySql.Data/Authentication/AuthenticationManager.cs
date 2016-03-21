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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace MySql.Data.MySqlClient.Authentication
{
    internal class AuthenticationPluginManager
    {
      static Dictionary<string, PluginInfo> plugins = new Dictionary<string, PluginInfo>();

      static AuthenticationPluginManager()
      {
        plugins["mysql_native_password"] = new PluginInfo("MySql.Data.MySqlClient.Authentication.MySqlNativePasswordPlugin");
        plugins["sha256_password"] = new PluginInfo("MySql.Data.MySqlClient.Authentication.Sha256AuthenticationPlugin");
//#if !CF && !RT
//        plugins["authentication_windows_client"] = new PluginInfo("MySql.Data.MySqlClient.Authentication.MySqlWindowsAuthenticationPlugin");
//        if (MySqlConfiguration.Settings != null && MySqlConfiguration.Settings.AuthenticationPlugins != null)
//        {
//          foreach (AuthenticationPluginConfigurationElement e in MySqlConfiguration.Settings.AuthenticationPlugins)
//            plugins[e.Name] = new PluginInfo(e.Type);
//        }
//#endif
      }

      public static MySqlAuthenticationPlugin GetPlugin(string method)
      {
        if (!plugins.ContainsKey(method))
          throw new MySqlException(String.Format("AuthenticationMethodNotSupported", method));
        return CreatePlugin(method);
      }

      private static MySqlAuthenticationPlugin CreatePlugin(string method)
      {
        PluginInfo pi = plugins[method];

        try
        {
          Type t = Type.GetType(pi.Type);
          MySqlAuthenticationPlugin o = (MySqlAuthenticationPlugin)Activator.CreateInstance(t);
          return o;
        }
        catch (Exception e)
        {
          throw new MySqlException(String.Format("Resources.UnableToCreateAuthPlugin", method), e);
		}
      }
    }

    struct PluginInfo
    {
      public string Type;
      public Assembly Assembly;

      public PluginInfo(string type)
      {
        Type = type;
        Assembly = null;
      }
    }
}
