// Copyright © 2013, Oracle and/or its affiliates. All rights reserved.
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

//#define BOUNCY_CASTLE_INCLUDED

using System;
using System.Collections.Generic;
using System.IO;
#if RT
using Windows.Security.Cryptography;
#else
using System.Security.Cryptography;
#endif
using System.Text;
using MySql.Data.Common;

#if BOUNCY_CASTLE_INCLUDED
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
#endif

namespace MySql.Data.MySqlClient.Authentication
{
  /// <summary>
  /// The implementation of the sha256_password authentication plugin.
  /// </summary>
  public class Sha256AuthenticationPlugin : MySqlAuthenticationPlugin
  {
#if BOUNCY_CASTLE_INCLUDED
    private RsaKeyParameters publicKey;
#endif
    private byte[] rawPubkey;

    public override string PluginName
    {
      get { return "sha256_password"; }
    }

    protected override byte[] MoreData(byte[] data)
    {
      rawPubkey = data;
      byte[] buffer = GetPassword() as byte[];
      return buffer;
    }

    public override object GetPassword()
    {
#if !CF && !RT
      if (Settings.SslMode != MySqlSslMode.None)
      {
        // send as clear text, since the channel is already encrypted
        byte[] passBytes = this.Encoding.GetBytes(Settings.Password);
        byte[] buffer = new byte[passBytes.Length + 1];
        Array.Copy(passBytes, 0, buffer, 0, passBytes.Length);
        buffer[passBytes.Length] = 0;
        return buffer;
      }
      else
      {
#endif
#if BOUNCY_CASTLE_INCLUDED
        // send RSA encrypted, since the channel is not protected
        if (rawPubkey != null)
        {
          publicKey = GenerateKeysFromPem(rawPubkey);
        }
        if (publicKey == null) return new byte[] { 0x01 }; //RequestPublicKey();
        else
        {
          byte[] bytes = GetRsaPassword(Settings.Password, AuthenticationData);
          if (bytes != null && bytes.Length == 1 && bytes[0] == 0) return null;
          return bytes;
        }
#else
        throw new NotImplementedException( "You can use sha256 plugin only in SSL connections in this implementation." );
#endif 
#if !CF && !RT
      }
#endif
    }

#if BOUNCY_CASTLE_INCLUDED
    private void RequestPublicKey()
    {
      RsaKeyParameters keys = GenerateKeysFromPem(rawPubkey);
      publicKey = keys;
    }

    private RsaKeyParameters GenerateKeysFromPem(byte[] rawData)
    {
      PemReader pem = new PemReader(new StreamReader(new MemoryStream( rawData )));
      RsaKeyParameters keyPair = (RsaKeyParameters)pem.ReadObject();
      return keyPair;
    }

    private byte[] GetRsaPassword(string password, byte[] seedBytes)
    {
      // Obfuscate the plain text password with the session scramble
      byte[] ofuscated = GetXor(this.Encoding.GetBytes(password), seedBytes);
      // Encrypt the password and send it to the server
      byte[] result = Encrypt(ofuscated, publicKey );
      return result;
    }

    private byte[] GetXor( byte[] src, byte[] pattern )
    {
      byte[] src2 = new byte[src.Length + 1];
      Array.Copy(src, 0, src2, 0, src.Length);
      src2[src.Length] = 0;
      byte[] result = new byte[src2.Length];
      for (int i = 0; i < src2.Length; i++)
      {
        result[ i ] = ( byte )( src2[ i ] ^ ( pattern[ i % pattern.Length ] ));
      }
      return result;
    }

    private byte[] Encrypt(byte[] data, RsaKeyParameters key)
    { 
      IBufferedCipher c = CipherUtilities.GetCipher("RSA/NONE/OAEPPadding");
      c.Init(true, key);
      byte[] result = c.DoFinal(data);
      return result;
    }
#endif
  }
}
