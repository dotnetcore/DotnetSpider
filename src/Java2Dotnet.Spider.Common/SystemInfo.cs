#if !NET_CORE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Common
{
	public class SystemInfo
	{
		private readonly int _processorCount;   //CPU个数
		private readonly PerformanceCounter _pcCpuLoad;   //CPU计数器
		private readonly long _physicalMemory;   //物理内存

		private const int GwHwndfirst = 0;
		private const int GwHwndnext = 2;
		private const int GwlStyle = (-16);
		private const int WsVisible = 268435456;
		private const int WsBorder = 8388608;

		#region AIP声明
		[DllImport("IpHlpApi.dll")]
		extern static public uint GetIfTable(byte[] pIfTable, ref uint pdwSize, bool bOrder);

		[DllImport("User32")]
		private extern static int GetWindow(int hWnd, int wCmd);

		[DllImport("User32")]
		private extern static int GetWindowLongA(int hWnd, int wIndx);

		[DllImport("user32.dll")]
		private static extern bool GetWindowText(int hWnd, StringBuilder title, int maxBufSize);

		[DllImport("user32", CharSet = CharSet.Auto)]
		private extern static int GetWindowTextLength(IntPtr hWnd);
		#endregion

		#region 构造函数
		/// <summary>
		/// 构造函数，初始化计数器等
		/// </summary>
		public SystemInfo()
		{
			//初始化CPU计数器
			_pcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total") { MachineName = "." };
			_pcCpuLoad.NextValue();

			//CPU个数
			_processorCount = Environment.ProcessorCount;

			//获得物理内存
			ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
			ManagementObjectCollection moc = mc.GetInstances();
			foreach (var o in moc)
			{
				var mo = (ManagementObject)o;
				if (mo["TotalPhysicalMemory"] != null)
				{
					_physicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
				}
			}
		}
		#endregion

		#region CPU个数
		/// <summary>
		/// 获取CPU个数
		/// </summary>
		public int ProcessorCount => _processorCount;

		#endregion

		#region CPU占用率
		/// <summary>
		/// 获取CPU占用率
		/// </summary>
		public float CpuLoad => _pcCpuLoad.NextValue();

		#endregion

		#region 可用内存
		/// <summary>
		/// 获取可用内存
		/// </summary>
		public long MemoryAvailable
		{
			get
			{
				long availablebytes = 0;
				//ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_PerfRawData_PerfOS_Memory");
				//foreach (ManagementObject mo in mos.Get())
				//{
				//    availablebytes = long.Parse(mo["Availablebytes"].ToString());
				//}
				ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
				foreach (var o in mos.GetInstances())
				{
					var mo = (ManagementObject)o;
					if (mo["FreePhysicalMemory"] != null)
					{
						availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
					}
				}
				return availablebytes;
			}
		}
		#endregion

		#region 物理内存
		/// <summary>
		/// 获取物理内存
		/// </summary>
		public long PhysicalMemory => _physicalMemory;

		#endregion

		#region 结束指定进程
		/// <summary>
		/// 结束指定进程
		/// </summary>
		/// <param name="pid">进程的 Process ID</param>
		public static void EndProcess(int pid)
		{
			try
			{
				Process process = Process.GetProcessById(pid);
				process.Kill();
			}
			catch
			{
				// ignored
			}
		}
		#endregion

		#region 查找所有应用程序标题
		/// <summary>
		/// 查找所有应用程序标题
		/// </summary>
		/// <returns>应用程序标题范型</returns>
		public static List<string> FindAllApps(int handle)
		{
			List<string> apps = new List<string>();

			int hwCurr;
			hwCurr = GetWindow(handle, GwHwndfirst);

			while (hwCurr > 0)
			{
				int isTask = (WsVisible | WsBorder);
				int lngStyle = GetWindowLongA(hwCurr, GwlStyle);
				bool taskWindow = ((lngStyle & isTask) == isTask);
				if (taskWindow)
				{
					int length = GetWindowTextLength(new IntPtr(hwCurr));
					StringBuilder sb = new StringBuilder(2 * length + 1);
					GetWindowText(hwCurr, sb, sb.Capacity);
					string strTitle = sb.ToString();
					if (!string.IsNullOrEmpty(strTitle))
					{
						apps.Add(strTitle);
					}
				}
				hwCurr = GetWindow(hwCurr, GwHwndnext);
			}

			return apps;
		}
		#endregion
	}
}

#endif