using System.IO;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Downloader
{
    public class FileDownloader : BaseDownloader
    {
        protected override Task<Page> DowloadContent(Request request, ISpider spider)
        {
            var filePath = request.GetExtra("__FilePath");

            if (!string.IsNullOrEmpty(filePath))
            {
                if (File.Exists(filePath))
                {

                    return Task.FromResult(new Page(request) { Content = File.ReadAllText(filePath) });
              
                }
            }

            return null;
        }
    }
}