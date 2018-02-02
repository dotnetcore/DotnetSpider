using System;
using System.IO;
using System.Text;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 打印爬虫框架信息的帮助类
	/// </summary>
	public static class PrintInfo
	{
		private static readonly object Locker = new object();

		/// <summary>
		/// 打印爬虫框架信息
		/// </summary>
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
				}
			}
		}

		/// <summary>
		/// 打印一整行word到控制台中
		/// </summary>
		/// <param name="word">打印的字符</param>
		public static void PrintLine(char word = '=')
		{
			var width = 120;
			
			try
			{
				width = Console.WindowWidth;
			}
			catch
			{
				// ignore
			}
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < width; ++i)
			{
				builder.Append(word);
			}

			Console.Write(builder.ToString());
		}
	}
}
