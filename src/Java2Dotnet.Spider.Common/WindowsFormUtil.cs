#if !NET_CORE

using System;
using System.Runtime.InteropServices;

namespace Java2Dotnet.Spider.Common
{
	public static class WindowsFormUtil
	{
		[DllImport("User32.dll", EntryPoint = "FindWindow")]
		public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("User32.dll", EntryPoint = "FindWindowEx")]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);
		[DllImport("user32.dll", EntryPoint = "SendMessageA")]
		public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

		public const int WmClose = 0x10;
	}
}
#endif
