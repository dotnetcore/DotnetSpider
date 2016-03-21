using Java2Dotnet.Spider.Extension.Downloader.WebDriver;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	/// <summary>
	/// 配置下载器
	/// Http, WebDriver, Fiddler
	/// </summary>
	public abstract class Downloader : IJobject
	{
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
	}

	public class HttpDownloader : Downloader
	{
		public override Types Type { get; internal set; } = Types.HttpClientDownloader;
	}

	public class WebDriverDownloader : Downloader
	{
		public override Types Type { get; internal set; } = Types.WebDriverDownloader;

		public Browser Browser { get; set; }

		public JObject Login { get; set; }
	}
}
