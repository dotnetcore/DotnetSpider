#if !NET_CORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.Infrastructure
{
	public class WindowsUserUtil
	{
		[DllImport("Advapi32.dll", EntryPoint = "GetUserName", ExactSpelling = false, SetLastError = true)]
		static extern bool GetUserName([MarshalAs(UnmanagedType.LPArray)] byte[] lpBuffer, [MarshalAs(UnmanagedType.LPArray)] Int32[] nSize);

		public static string CurrentUser
		{
			get
			{
				byte[] bytes = new byte[256];
				Int32[] len = new Int32[1];
				len[0] = 256;
				GetUserName(bytes, len);
				return Encoding.ASCII.GetString(bytes).Trim('\0');
			}
		}
	}
}
#endif