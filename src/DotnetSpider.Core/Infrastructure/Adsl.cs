using System;
using System.Runtime.InteropServices;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// Adsl 拨号类
	/// </summary>
	public class Adsl
	{
		private struct Rasconn
		{
			public int DwSize;
			public IntPtr Hrasconn;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
			public string SzEntryName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
			public string SzDeviceType;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
			public string SzDeviceName;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct RasStats
		{
			public int dwSize;
			public int dwBytesXmited;
			public int dwBytesRcved;
			public int dwFramesXmited;
			public int dwFramesRcved;
			public int dwCrcErr;
			public int dwTimeoutErr;
			public int dwAlignmentErr;
			public int dwHardwareOverrunErr;
			public int dwFramingErr;
			public int dwBufferOverrunErr;
			public int dwCompressionRatioIn;
			public int dwCompressionRatioOut;
			public int dwBps;
			public int dwConnectionDuration;
		}

		[DllImport("rasapi32.dll")]
		private static extern uint RasHangUp(IntPtr hrasconn);

		[DllImport("wininet.dll")]
		private static extern int InternetDial(IntPtr hwnd, [In]string lpszConnectionid, uint dwFlags, ref int lpdwConnection, uint dwReserved);

		[DllImport("Rasapi32.dll", EntryPoint = "RasEnumConnectionsA", SetLastError = true)]
		private static extern int RasEnumConnections
			(
			ref Rasconn lprasconn, // buffer to receive connections data
			ref int lpcb, // size in bytes of buffer
			ref int lpcConnections // number of connections written to buffer
			);

		/// <summary>
		/// 拨号
		/// </summary>
		/// <param name="adslName">ADSL名称, 默认为: 宽带连接</param>
		/// <returns></returns>
		public int Connect(string adslName)
		{
			int lpdwConnection = 0;
			return InternetDial(IntPtr.Zero, adslName, 2, ref lpdwConnection, 0);
		}

		/// <summary>
		/// 断开当前ADSL拨号
		/// </summary>
		public void Disconnect()
		{
			var intprt = GetAdslIntPrt();
			RasHangUp(intprt);
		}

		private IntPtr GetAdslIntPrt()
		{
			Rasconn lprasConn = new Rasconn
			{
#if NET45
				DwSize = Marshal.SizeOf(typeof(Rasconn)),
#else
				DwSize = Marshal.SizeOf<Rasconn>(),
#endif
				Hrasconn = IntPtr.Zero
			};


			int lpcConnections = 0;
#if NET45
			var lpcb = Marshal.SizeOf(typeof(Rasconn));
#else
			var lpcb = Marshal.SizeOf<Rasconn>();
#endif

			var nRet = RasEnumConnections(ref lprasConn, ref lpcb, ref lpcConnections);

			if (nRet != 0)
			{
				return IntPtr.Zero;
			}

			return lprasConn.Hrasconn;
		}
	}
}
