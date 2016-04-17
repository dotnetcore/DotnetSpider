using System;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Downloader;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class DownloadValidation
	{
		[Flags]
		public enum Types
		{
			Contains
		}

		public abstract Types Type { get; internal set; }
		public abstract DownloadValidationResult Validate(Page page);
	}

	public class ContainsDownloadValidation : DownloadValidation
	{
		public string ContainsString { get; set; }
		public DownloadValidationResult Result { get; set; }

		public override Types Type { get; internal set; } = Types.Contains;
		public override DownloadValidationResult Validate(Page page)
		{
			string rawText = page.Content;
			if(string.IsNulOrEmpty(rawText))
			{
				throw new SpiderExceptoin("Download failed or response is null.");
			}
			if (rawText.Contains(ContainsString))
			{
				return Result;
			}
			else
			{
				return DownloadValidationResult.Success;
			}
		}
	}
}
