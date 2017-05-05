#if !NET_CORE
using System;
using System.Runtime.InteropServices;

namespace DotnetSpider.Extension.Infrastructure
{
	public struct Rasconn
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

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct RasStats
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

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct RasEntryName
	{
		public int dwSize;
		//[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasFieldSizeConstants.RAS_MaxEntryName + 1)]
		public string szEntryName;
		//#if WINVER5
		//  public int dwFlags;
		//  [MarshalAs(UnmanagedType.ByValTStr,SizeConst=260+1)]
		//  public string szPhonebookPath;
		//#endif
	}


	public enum DelCacheType //要删除的类型。
	{
		File,//表示internet临时文件
		Cookie //表示Cookie
	}

	public class Adsl
	{
		[DllImport("rasapi32.dll", CharSet = CharSet.Auto)]
		public static extern uint RasHangUp(IntPtr hrasconn);

		[DllImport("wininet.dll", CharSet = CharSet.Auto)]
		public static extern int InternetDial(IntPtr hwnd, [In]string lpszConnectoid, uint dwFlags, ref int lpdwConnection, uint dwReserved);

		[DllImport("Rasapi32.dll", EntryPoint = "RasEnumConnectionsA",
			 SetLastError = true)]
		internal static extern int RasEnumConnections
			(
			ref Rasconn lprasconn, // buffer to receive connections data
			ref int lpcb, // size in bytes of buffer
			ref int lpcConnections // number of connections written to buffer
			);

		[DllImport("wininet.dll", CharSet = CharSet.Auto)]
		public static extern bool DeleteUrlCacheEntry(
			DelCacheType type
			);

		[DllImport("rasapi32.dll", CharSet = CharSet.Auto)]
		internal static extern uint RasGetConnectionStatistics(
			IntPtr hRasConn,       // handle to the connection
			[In, Out]RasStats lpStatistics  // buffer to receive statistics
			);

		[DllImport("rasapi32.dll", CharSet = CharSet.Auto)]
		public static extern uint RasEnumEntries(
			string reserved,              // reserved, must be NULL
			string lpszPhonebook,         // pointer to full path and
										  //  file name of phone-book file
			[In, Out]RasEntryName[] lprasentryname, // buffer to receive
													//  phone-book entries
			ref int lpcb,                  // size in bytes of buffer
			out int lpcEntries             // number of entries written
										   //  to buffer
			);

		private readonly string _mDuration;
		private readonly string _mConnectionName;
		private readonly string[] _mConnectionNames;
		private readonly double _mTx;
		private readonly double _mRx;
		private readonly bool _mConnected;
		private readonly IntPtr _mConnectedRasHandle;

		public Adsl()
		{
			_mConnected = true;

			Rasconn lprasConn = new Rasconn
			{
				DwSize = Marshal.SizeOf(typeof(Rasconn)),
				Hrasconn = IntPtr.Zero
			};


			int lpcConnections = 0;
			var lpcb = Marshal.SizeOf(typeof(Rasconn));

			var nRet = RasEnumConnections(ref lprasConn, ref lpcb, ref
				lpcConnections);

			if (nRet != 0)
			{
				_mConnected = false;
				return;

			}

			if (lpcConnections > 0)
			{
				//for (int i = 0; i < lpcConnections; i++)

				//{
				RasStats stats = new RasStats();

				_mConnectedRasHandle = lprasConn.Hrasconn;
				RasGetConnectionStatistics(lprasConn.Hrasconn, stats);


				_mConnectionName = lprasConn.SzEntryName;

				var hours = stats.dwConnectionDuration / 1000 / 3600;
				var minutes = stats.dwConnectionDuration / 1000 / 60 - hours * 60;
				var seconds = stats.dwConnectionDuration / 1000 - minutes * 60 - hours * 3600;


				_mDuration = hours + " hours " + minutes + " minutes " + seconds + " secs";
				_mTx = stats.dwBytesXmited;
				_mRx = stats.dwBytesRcved;
				//}
			}
			else
			{
				_mConnected = false;
			}


			int lpNames = 1;

			var entryNameSize = Marshal.SizeOf(typeof(RasEntryName));
			var lpSize = lpNames * entryNameSize;

			var names = new RasEntryName[lpNames];
			names[0].dwSize = entryNameSize;

			RasEnumEntries(null, null, names, ref lpSize, out lpNames);

			//if we have more than one connection, we need to do it again
			if (lpNames > 1)
			{
				names = new RasEntryName[lpNames];
				for (int i = 0; i < names.Length; i++)
				{
					names[i].dwSize = entryNameSize;
				}

				RasEnumEntries(null, null, names, ref lpSize, out lpNames);
			}
			_mConnectionNames = new string[names.Length];


			if (lpNames > 0)
			{
				for (int i = 0; i < names.Length; i++)
				{
					_mConnectionNames[i] = names[i].szEntryName;
				}
			}
		}

		public string Duration => _mConnected ? _mDuration : "";

		public string[] Connections => _mConnectionNames;

		public double BytesTransmitted => _mConnected ? _mTx : 0;
		public double BytesReceived => _mConnected ? _mRx : 0;
		public string ConnectionName => _mConnected ? _mConnectionName : "";
		public bool IsConnected => _mConnected;

		public int Connect(string connection)
		{
			int temp = 0;
			uint internetAutoDialUnattended = 2;
			return InternetDial(IntPtr.Zero, connection, internetAutoDialUnattended, ref temp, 0);
		}
		public void Disconnect()
		{
			RasHangUp(_mConnectedRasHandle);
		}
	}
}
#endif