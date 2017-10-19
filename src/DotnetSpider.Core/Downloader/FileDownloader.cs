using System.IO;

namespace DotnetSpider.Core.Downloader
{
    public class FileDownloader : BaseDownloader
    {
        protected override Page DowloadContent(Request request, ISpider spider)
        {
            var filePath = request.GetExtra("__FilePath");

            if (!string.IsNullOrEmpty(filePath))
            {
                if (File.Exists(filePath))
                {
                    return new Page(request)
                    {
                        Content = File.ReadAllText(filePath)
                    };
                }
            }

            return null;
        }
    }
}