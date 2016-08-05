using Java2Dotnet.Spider.Core.Downloader;
using Java2Dotnet.Spider.Ioc;
using Java2Dotnet.Spider.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Core.Pipeline
{
	public abstract class BasePipeline : IPipeline
	{
		protected ILogService Logger { get; set; }
		protected string BasePath { get; set; }

		public ISpider Spider { get; protected set; }

#if !NET_CORE
		public static string PathSeperator = "\\";
#else
		public static string PathSeperator = "/";
#endif

		public virtual void InitPipeline(ISpider spider)
		{
			Logger = ServiceProvider.Get<ILogService>().First();
			Spider = spider;
		}

		public abstract void Process(ResultItems resultItems);

		protected void SetPath(string path)
		{
			if (!path.EndsWith(PathSeperator))
			{
				path += PathSeperator;
			}

#if !NET_CORE
			BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
#else
			BasePath = Path.Combine(AppContext.BaseDirectory, path);
#endif
		}

		public virtual void Dispose()
		{
		}

		public static FileInfo PrepareFile(string fullName)
		{
			CheckAndMakeParentDirecotry(fullName);
			return new FileInfo(fullName);
		}

		public static DirectoryInfo PrepareDirectory(string fullName)
		{
			return new DirectoryInfo(CheckAndMakeParentDirecotry(fullName));
		}

		private static string CheckAndMakeParentDirecotry(string fullName)
		{
			string path = Path.GetDirectoryName(fullName);

			if (path != null)
			{
				DirectoryInfo directory = new DirectoryInfo(path);
				if (!directory.Exists)
				{
					directory.Create();
				}
			}
			return path;
		}
	}
}
