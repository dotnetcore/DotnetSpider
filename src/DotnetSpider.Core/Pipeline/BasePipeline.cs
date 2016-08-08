using System;
using System.IO;
using NLog;

namespace DotnetSpider.Core.Pipeline
{
	public abstract class BasePipeline : IPipeline
	{
		protected ILogger Logger { get; set; }
		protected string BasePath { get; set; }

		public ISpider Spider { get; protected set; }

#if !NET_CORE
		public static string PathSeperator = "\\";
#else
		public static string PathSeperator = "/";
#endif

		public virtual void InitPipeline(ISpider spider)
		{
			Logger = LogManager.GetCurrentClassLogger();
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
