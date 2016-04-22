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
		public DownloadValidationResult Result { get; set; }
		public abstract bool Validate(Page page, out DownloadValidationResult result);
	}

	public class ContainsDownloadValidation : DownloadValidation
	{
		public string ContainsString { get; set; }

		public override Types Type { get; internal set; } = Types.Contains;

		public override bool Validate(Page page, out DownloadValidationResult result)
		{
			string rawText = page.Content;
			if(string.IsNullOrEmpty(rawText))
			{
				throw new SpiderExceptoin("Download failed or response is null.");
			}
			if (rawText.Contains(ContainsString))
			{
				result = Result;
				return false;
			}
			else
			{
				result = DownloadValidationResult.Success;
				return true;
			}
		}
	}
}
