using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Downloader
{
	public class FileDownloader : BaseDownloader
	{
		protected override Page DowloadContent(Request request, ISpider spider)
		{
			var filePath = request.GetExtra("__FilePath");
			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				return new Page(request)
				{
					Content = File.ReadAllText(filePath)
				};
			}
			return null;
		}
	}
}
