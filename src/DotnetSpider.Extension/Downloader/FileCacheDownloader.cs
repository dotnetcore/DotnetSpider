//using System.IO;
//using System.Text;
//using DotnetSpider.Core.Common;
//using DotnetSpider.Core;
//using DotnetSpider.Core.Downloader;
//using DotnetSpider.Core.Pipeline;
//using DotnetSpider.Core.Processor;
//using DotnetSpider.Core.Utils;
//#if NET_CORE
//using System.Reflection;
//#endif

//namespace DotnetSpider.Extension.Downloader
//{
//	/// <summary>
//	/// Download file and saved to file for cache.
//	/// </summary>
//	public class FileCache : FilePersistentBase, IDownloader, IPipeline, IPageProcessor
//	{
//		private IDownloader _downloaderWhenFileMiss;
//		private readonly IPageProcessor _pageProcessor;

//		public Site Site
//		{
//			get { return _pageProcessor.Site; }
//			set { _pageProcessor.Site = value; }
//		}

//		public int ThreadNum { get; set; }

//		public FileCache(string startUrl, string urlPattern, string path = "/data/dotnetspider/temp/")
//		{
//			_pageProcessor = new SimplePageProcessor(startUrl, urlPattern);
//			SetPath(path);
//			_downloaderWhenFileMiss = new HttpClientDownloader();
//		}

//		public FileCache SetDownloaderWhenFileMiss(IDownloader downloaderWhenFileMiss)
//		{
//			_downloaderWhenFileMiss = downloaderWhenFileMiss;
//			return this;
//		}

//		public Page Download(Request request, ISpider spider)
//		{
//			// ReSharper disable once UnusedVariable
//			string path = BasePath + "/" + spider.Identity + "/";
//			Page page;
//			try
//			{
//				FileInfo file = PrepareFile(path + Encrypt.Md5Encrypt(request.Url.ToString()));

//				StreamReader bufferedReader = new StreamReader(file.OpenRead());
//				string line = bufferedReader.ReadLine();
//				if (("url:\t" + request.Url).Equals(line))
//				{
//					string html = GetHtml(bufferedReader);
//					page = new Page(request, spider.Site.ContentType);
//					page.Url = request.Url.ToString();
//					page.Content = html;
//				}
//			}
//			catch (IOException e)
//			{
//#if !NET_CORE
//				if (e.GetType().IsInstanceOfType(typeof(FileNotFoundException)))
//#else
//				if (typeof(FileNotFoundException).GetTypeInfo().IsAssignableFrom(e.GetType().GetTypeInfo()))
//#endif
//				{
//					spider.Logger.Info("File not exist for url: " + request.Url);
//				}
//				else
//				{
//					spider.Logger.Warn("File read error for url " + request.Url, e);
//				}
//			}
//			page = DownloadWhenMiss(request, spider);
//			return page;
//		}

//		public void Process(ResultItems resultItems, ISpider spider)
//		{
//			string path = BasePath + PathSeperator + spider.Identity + PathSeperator;
//			try
//			{
//				FileInfo fileInfo = PrepareFile(path + Encrypt.Md5Encrypt(resultItems.Request.Url.ToString()) + ".html");
//				using (StreamWriter writer = new StreamWriter(fileInfo.OpenWrite(), Encoding.UTF8))
//				{
//					writer.WriteLine("url:\t" + resultItems.Request.Url);
//					writer.WriteLine("html:\t" + resultItems.GetResultItem("html"));
//				}
//			}
//			catch (IOException e)
//			{
//				spider.Logger.Warn("Write file error.", e);
//			}
//		}

//		public void Process(Page page)
//		{
//			_pageProcessor.Process(page);
//		}

//		private string GetHtml(StreamReader bufferedReader)
//		{
//			StringBuilder htmlBuilder = new StringBuilder();
//			var line = bufferedReader.ReadLine();
//			//check
//			if (line != null)
//			{
//				line = line.Replace("html:\t", "");
//				htmlBuilder.Append(line);
//				while ((line = bufferedReader.ReadLine()) != null)
//				{
//					htmlBuilder.Append(line);
//				}
//			}
//			return htmlBuilder.ToString();
//		}

//		private Page DownloadWhenMiss(Request request, ISpider spider)
//		{
//			Page page = null;
//			if (_downloaderWhenFileMiss != null)
//			{
//				page = _downloaderWhenFileMiss.Download(request, spider);
//			}
//			return page;
//		}

//		public void Dispose()
//		{
//		}

//		public IDownloader Clone()
//		{
//			return (IDownloader)MemberwiseClone();
//		}
//	}
//}