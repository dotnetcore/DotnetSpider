using System;
using System.IO;
using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension
{
	public static class SpiderEnvironment
	{
		public static string GetDataFilePath(ISpider spider, string name)
		{
#if !NET_CORE
			string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", spider.Identity);
#else
			string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "data", spider.Identity);
#endif
			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			return Path.Combine(folderPath, name + ".sql");
		}
	}
}
