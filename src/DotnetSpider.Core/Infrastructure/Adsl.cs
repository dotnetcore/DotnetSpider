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
			public IntPtr Hrasconn;
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
				Hrasconn = IntPtr.Zero
			};


			int lpcConnections = 0;
#if !NETSTANDARD
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
