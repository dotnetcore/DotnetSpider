//using DotnetSpider.Core.Downloader;
//using System;
//using System.IO;
//using Xunit;

//namespace DotnetSpider.Core.Test.Downloader
//{
//	public class FileDownloaderTest
//	{
//		[Fact(DisplayName = "DownloadRelativePathFile")]
//		public void DownloadRelativePathFile()
//		{
//			if (Environment.GetEnvironmentVariable("TRAVIS") == "1")
//			{
//				return;
//			}
//			FileDownloader downloader = new FileDownloader();
//			var path = Path.Combine("source", "1.html");
//			var request = new Request($"file://{path}");
//			var spider = new DefaultSpider();
//			var page = downloader.Download(request, spider).Result;
//			Assert.Equal("hello", page.Content);
//		}

//		[Fact(DisplayName = "DownloadRelativeAbsolutePathFile")]
//		public void DownloadRelativeAbsolutePathFile()
//		{
//			if (Environment.GetEnvironmentVariable("TRAVIS") == "1")
//			{
//				return;
//			}
//			FileDownloader downloader = new FileDownloader();
//			var path = Path.Combine(Env.BaseDirectory, "source", "1.html");
//			var request = new Request($"file://{path}");
//			var spider = new DefaultSpider();
//			var page = downloader.Download(request, spider).Result;
//			Assert.Equal("hello", page.Content);
//		}

//		[Fact(DisplayName = "FileNotExists")]
//		public void FileNotExists()
//		{
//			FileDownloader downloader = new FileDownloader();
//			var request = new Request("file://Downloader/2.html");
//			var spider = new DefaultSpider();
//			var page = downloader.Download(request, spider).Result;
//			Assert.True(string.IsNullOrEmpty(page.Content));
//			Assert.True(page.Exception is FileNotFoundException);
//			Assert.True(page.Skip);
//		}
//	}
//}
