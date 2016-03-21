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
using System.Security.Cryptography;
#if RT
using AliasText = MySql.Data.MySqlClient.RT;
#else
using AliasText = System.Text;
#endif

namespace MySql.Data.MySqlClient.Authentication
{
	public class MySqlNativePasswordPlugin : MySqlAuthenticationPlugin
	{
		public override string PluginName
		{
			get { return "mysql_native_password"; }
		}

		protected override void SetAuthData(byte[] data)
		{
			// if the data given to us is a null terminated string, we need to trim off the trailing zero
			if (data[data.Length - 1] == 0)
			{
				byte[] b = new byte[data.Length - 1];
				Buffer.BlockCopy(data, 0, b, 0, data.Length - 1);
				base.SetAuthData(b);
			}
			else
				base.SetAuthData(data);
		}

		protected override byte[] MoreData(byte[] data)
		{
			byte[] passBytes = GetPassword() as byte[];
			byte[] buffer = new byte[passBytes.Length - 1];
			Array.Copy(passBytes, 1, buffer, 0, passBytes.Length - 1);
			return buffer;
		}

		public override object GetPassword()
		{
			//string[] tmp = "35,80,73,94,123,111,62,38,66,90,125,39,69,101,80,65,53,35,80,72".Split(',');
			//byte[] authenticationData=new byte[tmp.Length];
			//for(int i=0;i<authenticationData.Length;++i)
			//{
			//authenticationData[i] = byte.Parse(tmp[i]);
			//}

			//byte[] bytes1 = Get411Password(Settings.Password, authenticationData);
			// Console.WriteLine("CTRY11:"+bytes1.GetString());
			byte[] bytes = Get411Password(Settings.Password, AuthenticationData);
			if (bytes != null && bytes.Length == 1 && bytes[0] == 0) return null;
			return bytes;
		}

		/// <summary>
		/// Returns a byte array containing the proper encryption of the 
		/// given password/seed according to the new 4.1.1 authentication scheme.
		/// </summary>
		/// <param name="password"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		private byte[] Get411Password(string password, byte[] seedBytes)
		{
			// if we have no password, then we just return 1 zero byte
			if (password.Length == 0) return new byte[1];

			var sha = System.Security.Cryptography.SHA1.Create();

			byte[] firstHash = sha.ComputeHash(AliasText.Encoding.UTF8.GetBytes(password));
			byte[] secondHash = sha.ComputeHash(firstHash);

			byte[] input = new byte[seedBytes.Length + secondHash.Length];
			Array.Copy(seedBytes, 0, input, 0, seedBytes.Length);
			Array.Copy(secondHash, 0, input, seedBytes.Length, secondHash.Length);
			byte[] thirdHash = sha.ComputeHash(input);

			byte[] finalHash = new byte[thirdHash.Length + 1];
			finalHash[0] = 0x14;
			Array.Copy(thirdHash, 0, finalHash, 1, thirdHash.Length);

			for (int i = 1; i < finalHash.Length; i++)
				finalHash[i] = (byte)(finalHash[i] ^ firstHash[i - 1]);
			return finalHash;
		}
	}
}
