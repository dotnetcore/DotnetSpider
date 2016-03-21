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
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security;
using  System.Runtime.InteropServices.ComTypes;

namespace MySql.Data.MySqlClient.Authentication
{

	internal class MySqlWindowsAuthenticationPlugin : MySqlAuthenticationPlugin
	{
		SECURITY_HANDLE outboundCredentials = new SECURITY_HANDLE(0);
		SECURITY_HANDLE clientContext = new SECURITY_HANDLE(0);
		SECURITY_INTEGER lifetime = new SECURITY_INTEGER(0);
		bool continueProcessing;
		string targetName = null;

		protected override void CheckConstraints()
		{ 
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				throw new MySqlException(String.Format("WinAuthNotSupportOnPlatform"));

			base.CheckConstraints();
		}

		public override string GetUsername()
		{
			string username = base.GetUsername();
			if (String.IsNullOrEmpty(username))
				return "auth_windows";
			return username;
		}

		public override string PluginName
		{
			get { return "authentication_windows_client"; }
		}

		protected override byte[] MoreData(byte[] moreData)
		{
			if (moreData == null)
				AcquireCredentials();

			byte[] clientBlob = null;

			if (continueProcessing)
				InitializeClient(out clientBlob, moreData, out continueProcessing);

			if (!continueProcessing || clientBlob == null || clientBlob.Length == 0)
			{
				FreeCredentialsHandle(ref outboundCredentials);
				DeleteSecurityContext(ref clientContext);
				return null;
			}
			return clientBlob;
		}

		void InitializeClient(out byte[] clientBlob, byte[] serverBlob, out bool continueProcessing)
		{
			clientBlob = null;
			continueProcessing = true;
			SecBufferDesc clientBufferDesc = new SecBufferDesc(MAX_TOKEN_SIZE);
			SECURITY_INTEGER initLifetime = new SECURITY_INTEGER(0);
			int ss = -1;
			try
			{
				uint ContextAttributes = 0;

				if (serverBlob == null)
				{
					ss = InitializeSecurityContext(
						ref outboundCredentials,
						IntPtr.Zero,
						targetName,
						STANDARD_CONTEXT_ATTRIBUTES,
						0,
						SECURITY_NETWORK_DREP,
						IntPtr.Zero, /* always zero first time around */
						0,
						out clientContext,
						out clientBufferDesc,
						out ContextAttributes,
						out initLifetime);

				}
				else
				{
					SecBufferDesc serverBufferDesc = new SecBufferDesc(serverBlob);

					try
					{
						ss = InitializeSecurityContext(ref outboundCredentials,
							ref clientContext,
							targetName,
							STANDARD_CONTEXT_ATTRIBUTES,
							0,
							SECURITY_NETWORK_DREP,
							ref serverBufferDesc,
							0,
							out clientContext,
							out clientBufferDesc,
							out ContextAttributes,
							out initLifetime);
					}
					finally
					{
						serverBufferDesc.Dispose();
					}
				}


				if ((SEC_I_COMPLETE_NEEDED == ss)
					|| (SEC_I_COMPLETE_AND_CONTINUE == ss))
				{
					CompleteAuthToken(ref clientContext, ref clientBufferDesc);
				}

				if (ss != SEC_E_OK &&
					ss != SEC_I_CONTINUE_NEEDED &&
					ss != SEC_I_COMPLETE_NEEDED &&
					ss != SEC_I_COMPLETE_AND_CONTINUE)
				{
					throw new MySqlException(
						"InitializeSecurityContext() failed  with errorcode " + ss);
				}

				clientBlob = clientBufferDesc.GetSecBufferByteArray();
			}
			finally
			{
				clientBufferDesc.Dispose();
			}
			continueProcessing = (ss != SEC_E_OK && ss != SEC_I_COMPLETE_NEEDED);
		}

		/// <summary>
		/// Currently this method is unused
		/// </summary>
		/// <returns></returns>
		private string GetTargetName()
		{
			return null;
			if (AuthenticationData == null) return String.Empty;

			int index = -1;
			for (int i = 0; i < AuthenticationData.Length; i++)
			{
				if (AuthenticationData[i] != 0) continue;
				index = i;
				break;
			}
			if (index == -1)
#if RT
        targetName = System.Text.Encoding.UTF8.GetString(AuthenticationData, 0, AuthenticationData.Length);
#else
				targetName = System.Text.Encoding.UTF8.GetString(AuthenticationData);
#endif
			else
				targetName = System.Text.Encoding.UTF8.GetString(AuthenticationData, 0, index);
			return targetName;
		}

		private void AcquireCredentials()
		{

			continueProcessing = true;

			int ss = AcquireCredentialsHandle(null, "Negotiate", SECPKG_CRED_OUTBOUND,
					IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, ref outboundCredentials,
					ref lifetime);
			if (ss != SEC_E_OK)
				throw new MySqlException("AcquireCredentialsHandle failed with errorcode" + ss);
		}

		#region SSPI Constants and Imports

		const int SEC_E_OK = 0;
		const int SEC_I_CONTINUE_NEEDED = 0x90312;
		const int SEC_I_COMPLETE_NEEDED = 0x1013;
		const int SEC_I_COMPLETE_AND_CONTINUE = 0x1014;

		const int SECPKG_CRED_OUTBOUND = 2;
		const int SECURITY_NETWORK_DREP = 0;
		const int SECURITY_NATIVE_DREP = 0x10;
		const int SECPKG_CRED_INBOUND = 1;
		const int MAX_TOKEN_SIZE = 12288;
		const int SECPKG_ATTR_SIZES = 0;
		const int STANDARD_CONTEXT_ATTRIBUTES = 0;

		[DllImport("secur32", CharSet = CharSet.Unicode)]
		static extern int AcquireCredentialsHandle(
			string pszPrincipal,
			string pszPackage,
			int fCredentialUse,
			IntPtr PAuthenticationID,
			IntPtr pAuthData,
			int pGetKeyFn,
			IntPtr pvGetKeyArgument,
			ref SECURITY_HANDLE phCredential,
			ref SECURITY_INTEGER ptsExpiry);

		[DllImport("secur32", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern int InitializeSecurityContext(
			ref SECURITY_HANDLE phCredential,
			IntPtr phContext,
			string pszTargetName,
			int fContextReq,
			int Reserved1,
			int TargetDataRep,
			IntPtr pInput,
			int Reserved2,
			out SECURITY_HANDLE phNewContext,
			out SecBufferDesc pOutput,
			out uint pfContextAttr,
			out SECURITY_INTEGER ptsExpiry);

		[DllImport("secur32", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern int InitializeSecurityContext(
			ref SECURITY_HANDLE phCredential,
			ref SECURITY_HANDLE phContext,
			string pszTargetName,
			int fContextReq,
			int Reserved1,
			int TargetDataRep,
			ref SecBufferDesc SecBufferDesc,
			int Reserved2,
			out SECURITY_HANDLE phNewContext,
			out SecBufferDesc pOutput,
			out uint pfContextAttr,
			out SECURITY_INTEGER ptsExpiry);

		[DllImport("secur32", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern int CompleteAuthToken(
			ref SECURITY_HANDLE phContext,
			ref SecBufferDesc pToken);

		[DllImport("secur32.Dll", CharSet = CharSet.Unicode, SetLastError = false)]
		public static extern int QueryContextAttributes(
			ref SECURITY_HANDLE phContext,
			uint ulAttribute,
			out SecPkgContext_Sizes pContextAttributes);

		[DllImport("secur32.Dll", CharSet = CharSet.Unicode, SetLastError = false)]
		public static extern int FreeCredentialsHandle(ref SECURITY_HANDLE pCred);

		[DllImport("secur32.Dll", CharSet = CharSet.Unicode, SetLastError = false)]
		public static extern int DeleteSecurityContext(ref SECURITY_HANDLE pCred);

		#endregion
	}

	[StructLayout(LayoutKind.Sequential)]
	struct SecBufferDesc : IDisposable
	{

		public int ulVersion;
		public int cBuffers;
		public IntPtr pBuffers; //Point to SecBuffer

		public SecBufferDesc(int bufferSize)
		{
			ulVersion = (int)SecBufferType.SECBUFFER_VERSION;
			cBuffers = 1;
			SecBuffer secBuffer = new SecBuffer(bufferSize);
			pBuffers = Marshal.AllocHGlobal(Marshal.SizeOf(secBuffer));
			Marshal.StructureToPtr(secBuffer, pBuffers, false);
		}

		public SecBufferDesc(byte[] secBufferBytes)
		{
			ulVersion = (int)SecBufferType.SECBUFFER_VERSION;
			cBuffers = 1;
			SecBuffer ThisSecBuffer = new SecBuffer(secBufferBytes);
			pBuffers = Marshal.AllocHGlobal(Marshal.SizeOf(ThisSecBuffer));
			Marshal.StructureToPtr(ThisSecBuffer, pBuffers, false);
		}

		public void Dispose()
		{
			if (pBuffers != IntPtr.Zero)
			{
				Debug.Assert(cBuffers == 1);
				SecBuffer ThisSecBuffer =
					(SecBuffer)Marshal.PtrToStructure(pBuffers, typeof(SecBuffer));
				ThisSecBuffer.Dispose();
				Marshal.FreeHGlobal(pBuffers);
				pBuffers = IntPtr.Zero;
			}
		}

		public byte[] GetSecBufferByteArray()
		{
			byte[] Buffer = null;

			if (pBuffers == IntPtr.Zero)
			{
				throw new InvalidOperationException("Object has already been disposed!!!");
			}
			Debug.Assert(cBuffers == 1);
			SecBuffer secBuffer = (SecBuffer)Marshal.PtrToStructure(pBuffers,
				typeof(SecBuffer));
			if (secBuffer.cbBuffer > 0)
			{
				Buffer = new byte[secBuffer.cbBuffer];
				Marshal.Copy(secBuffer.pvBuffer, Buffer, 0, secBuffer.cbBuffer);
			}
			return (Buffer);
		}

	}

	public enum SecBufferType
	{
		SECBUFFER_VERSION = 0,
		SECBUFFER_EMPTY = 0,
		SECBUFFER_DATA = 1,
		SECBUFFER_TOKEN = 2
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SecHandle //=PCtxtHandle
	{
		IntPtr dwLower; // ULONG_PTR translates to IntPtr not to uint
		IntPtr dwUpper; // this is crucial for 64-Bit Platforms
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SecBuffer : IDisposable
	{
		public int cbBuffer;
		public int BufferType;
		public IntPtr pvBuffer;


		public SecBuffer(int bufferSize)
		{
			cbBuffer = bufferSize;
			BufferType = (int)SecBufferType.SECBUFFER_TOKEN;
			pvBuffer = Marshal.AllocHGlobal(bufferSize);
		}

		public SecBuffer(byte[] secBufferBytes)
		{
			cbBuffer = secBufferBytes.Length;
			BufferType = (int)SecBufferType.SECBUFFER_TOKEN;
			pvBuffer = Marshal.AllocHGlobal(cbBuffer);
			Marshal.Copy(secBufferBytes, 0, pvBuffer, cbBuffer);
		}

		public SecBuffer(byte[] secBufferBytes, SecBufferType bufferType)
		{
			cbBuffer = secBufferBytes.Length;
			BufferType = (int)bufferType;
			pvBuffer = Marshal.AllocHGlobal(cbBuffer);
			Marshal.Copy(secBufferBytes, 0, pvBuffer, cbBuffer);
		}

		public void Dispose()
		{
			if (pvBuffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(pvBuffer);
				pvBuffer = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SECURITY_INTEGER
	{
		public uint LowPart;
		public int HighPart;
		public SECURITY_INTEGER(int dummy)
		{
			LowPart = 0;
			HighPart = 0;
		}
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct SECURITY_HANDLE
	{
		public IntPtr LowPart;
		public IntPtr HighPart;
		public SECURITY_HANDLE(int dummy)
		{
			LowPart = HighPart = new IntPtr(0);
		}
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct SecPkgContext_Sizes
	{
		public uint cbMaxToken;
		public uint cbMaxSignature;
		public uint cbBlockSize;
		public uint cbSecurityTrailer;
	};

}
