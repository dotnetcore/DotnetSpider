using DotnetSpider.Core.Downloader;
using System.IO;
using Xunit;

namespace DotnetSpider.Core.Test.Downloader
{
	public class FileDownloaderTest
	{
		[Fact]
		public void DownloadRelativePathFile()
		{
			FileDownloader downloader = new FileDownloader();
			var path = Path.Combine("Downloader", "1.html");
			var request = new Request($"file://{path}");
			var spider = new DefaultSpider();
			var page = downloader.Download(request, spider);
			Assert.Equal("hello", page.Content);
		}

		[Fact]
		public void DownloadRelativeAbsolutePathFile()
		{
			FileDownloader downloader = new FileDownloader();
			var path = Path.Combine(Env.BaseDirectory, "Downloader", "1.html");
			var request = new Request($"file://{path}");
			var spider = new DefaultSpider();
			var page = downloader.Download(request, spider);
			Assert.Equal("hello", page.Content);
		}

		[Fact]
		public void FileNotExists()
		{
			FileDownloader downloader = new FileDownloader();
			var request = new Request("file://Downloader/2.html");
			var spider = new DefaultSpider();
			var page = downloader.Download(request, spider);
			Assert.True(string.IsNullOrEmpty(page.Content));
			Assert.Equal("File downloader\\2.html unfound.", page.Exception.Message);
			Assert.True(page.Skip);
		}
	}
}
