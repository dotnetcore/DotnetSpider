using System.IO;

namespace DotnetSpider.Core.Pipeline
{
	public abstract class BaseFilePipeline : BasePipeline
	{
		public string RootDataFolder { get; protected set; }
		public string Interval { get; protected set; }
		public string DataFolder { get; protected set; }

		protected BaseFilePipeline() { }

		protected BaseFilePipeline(string interval)
		{
			InitFolder(interval);
		}

		protected void InitFolder(string interval)
		{
			if (string.IsNullOrEmpty(interval) || string.IsNullOrWhiteSpace(interval))
			{
				throw new SpiderException("Interval path should not be null.");
			}
			if (!interval.EndsWith(Env.PathSeperator))
			{
				interval += Env.PathSeperator;
			}

			RootDataFolder = Path.Combine(Env.BaseDirectory, interval);
			Interval = interval;
		}

		public override void Init(ISpider spider)
		{
			base.Init(spider);

			DataFolder = Path.Combine(RootDataFolder, spider.Identity);
			if (!Directory.Exists(DataFolder))
			{
				Directory.CreateDirectory(DataFolder);
			}
		}
	}
}
