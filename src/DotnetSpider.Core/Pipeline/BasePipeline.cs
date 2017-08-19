using DotnetSpider.Core.Infrastructure;
using NLog;
using System;
using System.IO;
#if NET_CORE
#endif

namespace DotnetSpider.Core.Pipeline
{
	public abstract class BasePipeline : IPipeline
	{
		protected readonly static ILogger Logger = LogCenter.GetLogger();
		protected string BasePath { get; set; }

		public ISpider Spider { get; protected set; }

		public virtual void InitPipeline(ISpider spider)
		{
			Spider = spider;
		}

		public abstract void Process(params ResultItems[] resultItems);

		protected void SetPath(string path)
		{
			if (!path.EndsWith(Infrastructure.Environment.PathSeperator))
			{
				path += Infrastructure.Environment.PathSeperator;
			}

			BasePath = Path.Combine(Infrastructure.Environment.BaseDirectory, path);
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
