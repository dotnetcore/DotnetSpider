//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using Java2Dotnet.Spider.Log;

//namespace Java2Dotnet.Spider.Core.Utils
//{
//	public static class LogUtils
//	{
//		public static ILogService GetLogger(ISpider spider)
//		{
//			return LogManager.GetLogger($"{spider.Identity}&{spider.UserId}&{spider.TaskGroup}");
//		}

//		public static ILogService GetLogger(string identity, string userid, string taskGroup)
//		{
//			return LogManager.GetLogger($"{identity}&{userid}&{taskGroup}");
//		}
//	}
//}