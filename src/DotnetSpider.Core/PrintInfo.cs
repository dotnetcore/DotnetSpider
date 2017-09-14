using System;
using System.Text;

namespace DotnetSpider.Core
{
	public class PrintInfo
	{
		private static readonly object Locker = new object();

		public static void Print()
		{
			lock (Locker)
			{
				var key = "_DotnetSpider_Info";

				var isPrinted = AppDomain.CurrentDomain.GetData(key) != null;

				if (!isPrinted)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("=================================================================");
					Console.WriteLine("== DotnetSpider is an open source crawler developed by C#      ==");
					Console.WriteLine("== It's multi thread, light weight, stable and high performce  ==");
					Console.WriteLine("== Support storage data to file, mysql, mssql, mongodb etc     ==");
					Console.WriteLine("== License: LGPL3.0                                            ==");
					Console.WriteLine("== Author: zlzforever@163.com                                  ==");
					Console.WriteLine("=================================================================");
					Console.ForegroundColor = ConsoleColor.White;

					AppDomain.CurrentDomain.SetData(key, "True");

					Console.WriteLine();
					PrintLine('=');
				}
			}
		}

		public static void PrintLine(char word = '=')
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < Console.WindowWidth; ++i)
			{
				builder.Append(word);
			}

			Console.WriteLine(builder.ToString());
		}
	}
}
