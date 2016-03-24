using System;
using Java2Dotnet.Spider.Core.Downloader;
using Java2Dotnet.Spider.Extension.Downloader.WebDriver;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	/// <summary>
	/// 配置下载器
	/// Http, WebDriver, Fiddler
	/// </summary>
	public abstract class Downloader
	{
		[Flags]
		public enum Types
		{
			WebDriverDownloader,
			HttpClientDownloader,
			FiddlerDownloader,
			FileDownloader
		}

		public abstract Types Type { get; internal set; }

		/// <summary>
		/// Contains("anti_Spider")
		/// UrlContains("anti_Spider")
		/// </summary>
		public DownloadValidation DownloadValidation { get; set; }

		public abstract IDownloader GetDownloader();
	}

	public class HttpDownloader : Downloader
	{
		public override Types Type { get; internal set; } = Types.HttpClientDownloader;

		public override IDownloader GetDownloader()
		{
			var downloader = new HttpClientDownloader();
			if (DownloadValidation != null)
			{
				downloader.DownloadValidation = DownloadValidation.Validate;
			}
			return downloader;
		}
	}

	public class FileDownloader : Downloader
	{
		public override Types Type { get; internal set; } = Types.FileDownloader;

		public override IDownloader GetDownloader()
		{
			var downloader = new Extension.Downloader.FileDownloader();
			if (DownloadValidation != null)
			{
				downloader.DownloadValidation = DownloadValidation.Validate;
			}
			return downloader;
		}
	}

	public class WebDriverDownloader : Downloader
	{
		public override Types Type { get; internal set; } = Types.WebDriverDownloader;

		public override IDownloader GetDownloader()
		{
			Extension.Downloader.WebDriver.WebDriverDownloader downloader = new Extension.Downloader.WebDriver.WebDriverDownloader(Browser);
			if (Login != null)
			{
				downloader.Login = Login.Login;
			}
			if (DownloadValidation != null)
			{
				downloader.DownloadValidation = DownloadValidation.Validate;
			}
			return downloader;
		}

		public Browser Browser { get; set; }

		public Loginer Login { get; set; }
	}
}
