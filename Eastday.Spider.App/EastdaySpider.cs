using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Selector;
using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Eastday
{
    public class EastdaySpider
    {
        public static void CrawlerPagesTraversal()
        {
            // Config encoding, header, cookie, proxy etc... 定义采集的 Site 对象, 设置 Header、Cookie、代理等
            var site = new Site { EncodingName = "gb2312", RemoveOutboundLinks = true };

            // Set start/seed url
            site.AddStartUrl("http://www.eastday.com/");
            site.Domain = null;

            Spider spider = Spider.Create(site,
                // crawler identity
                "cnblogs_" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                // use memoery queue scheduler
                new QueueDuplicateRemovedScheduler(),
                // default page processor will save whole html, and extract urls to target urls via regex
                new EastdayNewsProcessor())
                // save crawler result to file in the folder: \{running directory}\data\{crawler identity}\{guid}.dsd
                .AddPipeline(new NewsPipeline())
                // dowload html by http client
                //.SetDownloader(new HttpClientDownloader())
                // 4 threads 4线程
                .SetThreadNum(4);

            // traversal deep 遍历深度
            spider.Deep = 10;

            // stop crawler if it can't get url from the scheduler after 30000 ms 当爬虫连续30秒无法从调度中心取得需要采集的链接时结束.
            spider.EmptySleepTime = 30000;

            // start crawler 启动爬虫
            spider.Run();
        }
    }

    class EastdayNewsProcessor : BasePageProcessor
    {
        private string[] excludeUrl = new string[] {
            "^https?://big5\\.eastday\\.com",
            "^https?://usa\\.eastday\\.com",
            "^https?://canada\\.eastday\\.com",
            "^https?://sports\\.eastday\\.com",
            "^https?://imedia\\.eastday\\.com",
            "^https?://me\\.eastday\\.com",
            "^https?://t\\.eastday\\.com",
            "^https?://photo\\.eastday\\.com",
            "^https?://mil\\.eastday\\.com",
            "^https?://shurufa\\.eastday\\.com",
            "^https?://tianqi\\.eastday\\.com",
            "^https?://cp\\.eastday\\.com",
            "^https?://.+\\.eastday\\.com.+\\.php.+",
            //pdf,jpg.....
        };

        public EastdayNewsProcessor()
        {
            // 定义目标页的筛选
            //AddTargetUrlExtractor(null, "^https?://.+\\.eastday\\.co(m|m/)$", "^https?://.+\\.eastday\\.com/.+/u1ai?\\d+\\.ht(m|ml)$");
            AddTargetUrlExtractor(null, "^https?://.+\\.eastday\\.com");
            AddExcludeTargetUrlPattern(excludeUrl);
        }

        protected override void Handle(Page page)
        {
            // 利用 Selectable 查询并构造自己想要的数据对象
            var regex = Regex.Match(page.TargetUrl, "^https?://(.+)\\.eastday\\.com/.+/u1ai?(\\d+)\\.ht(m|ml)$", RegexOptions.IgnoreCase);
            if (regex.Success)
            {
                var news = new EastdayNews();
                news.Url = page.TargetUrl;
                news.Domain = regex.Groups[1].Value;
                news.UrlId = regex.Groups[regex.Groups.Count - 2].Value;
                regex = Regex.Match(page.TargetUrl, "^https?://(.+)\\.eastday\\.com/(.+)/(\\d+)/u1ai?(\\d+)\\.ht(m|ml)$", RegexOptions.IgnoreCase);
                if (regex.Success)
                {
                    news.SubDomain = regex.Groups[2].Value;
                    news.PublishTime = regex.Groups[3].Value;
                }
                news.Title = page.Selectable.Select(Selectors.XPath("//title")).GetValue();
                // 以自定义KEY存入page对象中供Pipeline调用
                page.AddResultItem("EastdayNews", news);
            }
        }
    }

    public class EastdayNews
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public string UrlId { get; set; }
        public string Domain { get; set; }
        public string SubDomain { get; set; }
        public string PublishTime { get; set; }
        public string Url { get; set; }
        public override string ToString()
        {
            return $"{Id}|{Url}|{Title}";
        }
    }

    class NewsPipeline : BasePipeline
    {
        public override void Process(params ResultItems[] resultItems)
        {
            foreach (var resultItem in resultItems)
            {
                var news = resultItem.GetResultItem("EastdayNews");
                if ( news != null)
                {
                    string sql = @"INSERT dbo.News( UrlId, Url, Title,Domain,SubDomain,PublishTime ) 
                                  VALUES  (@UrlId,@Url,@Title,@Domain,@SubDomain,@PublishTime)";
                    SqlParameter[] pParamList =
                    {
                        new SqlParameter("@UrlId", news.UrlId),
                        new SqlParameter("@Url", news.Url),
                        new SqlParameter("@Title", news.Title),
                        new SqlParameter("@Domain", news.Domain),
                        new SqlParameter("@SubDomain", news.SubDomain),
                        new SqlParameter("@PublishTime", news.PublishTime),
                    };
                    SQLHelper.ExecuteNonQuery(sql, pParamList);
                    //Console.WriteLine($"News {news}");
                }
            }
        }
    }
}
