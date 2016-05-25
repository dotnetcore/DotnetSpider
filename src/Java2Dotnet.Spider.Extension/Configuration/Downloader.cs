using System;
using Java2Dotnet.Spider.Core.Downloader;
using Java2Dotnet.Spider.Extension.Downloader.WebDriver;
using System.Collections.Generic;

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
		public List<DownloadValidation> DownloadValidations { get; set; }

		public abstract IDownloader GetDownloader();

		public GeneratePostBody GeneratePostBody { get; set; }
	}

	public class HttpDownloader : Downloader
	{
		public override Types Type { get; internal set; } = Types.HttpClientDownloader;

		public override IDownloader GetDownloader()
		{
			var downloader = new HttpClientDownloader();
			if (DownloadValidations != null)
			{
				downloader.DownloadValidation = page =>
				{
					DownloadValidationResult result = DownloadValidationResult.Success;
					foreach (var downloadValidation in DownloadValidations)
					{
						var r = downloadValidation.Validate(page, out result);
						if(!r)
						{
							break;
						}
					}

					return result;
				};
			}
			if (GeneratePostBody != null)
			{
				downloader.GeneratePostBody = (s, r) =>
				{
					List<string> arguments = new List<string>();
					foreach (var arg in GeneratePostBody.ArgumnetNames)
					{
						var tmp = s.Arguments.ContainsKey(arg) ? s.Arguments[arg] : "";
						arguments.Add(tmp);
					}
					r.PostBody = string.Format(r.PostBody, arguments.ToArray());
				};
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
			if (DownloadValidations != null)
			{
				downloader.DownloadValidation = page =>
				{
					DownloadValidationResult result = DownloadValidationResult.Success;
					foreach (var downloadValidation in DownloadValidations)
					{
						var r = downloadValidation.Validate(page, out result);
						if (!r)
						{
							break;
						}
					}

					return result;
				};
			}
			return downloader;
		}
	}

#if !NET_CORE
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
			if (DownloadValidations != null)
			{
				downloader.DownloadValidation = page =>
				{
					DownloadValidationResult result = DownloadValidationResult.Success;
					foreach (var downloadValidation in DownloadValidations)
					{
						var r = downloadValidation.Validate(page, out result);
						if(!r)
						{
							break;
						}
					}

					return result;
				};
			}
			if (VerifyCode != null)
			{
				downloader.VerifyCode = VerifyCode.Verify;
			}
			return downloader;
		}

		public Browser Browser { get; set; }

		public Loginer Login { get; set; }

		public VerifyCode VerifyCode { get; set; }
	}
#endif
}
