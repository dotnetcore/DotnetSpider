using DotnetSpider.Core.Infrastructure;
using NLog;
using System.Collections.Generic;
using System.IO;
#if NET_CORE
#endif

namespace DotnetSpider.Core.Pipeline
{
	public abstract class BasePipeline : IPipeline
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();
		protected string BasePath { get; set; }

		public ISpider Spider { get; protected set; }

		public virtual void InitPipeline(ISpider spider)
		{
			Spider = spider;
		}

		public abstract void Process(IEnumerable<ResultItems> resultItems);

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

		protected void SetPath(string path)
		{
			if (!path.EndsWith(Env.PathSeperator))
			{
				path += Env.PathSeperator;
			}

			BasePath = Path.Combine(Env.BaseDirectory, path);
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
