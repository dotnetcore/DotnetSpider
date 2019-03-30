using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Data;
using DotnetSpider.Data.Parser;
using DotnetSpider.Data.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.Selector;

namespace DotnetSpider.Sample.samples
{
    public class NvshensSpider
    {
        public static void Run()
        {
            var builder = new LocalSpiderBuilder();
            builder.UseSerilog(); // 可以配置任意日志组件
            builder.UseDistinctScheduler(); // 配置本地内存调度或者数据库调度

            var spider = builder.Build(); // 生成爬虫对象
            spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
            spider.Name = "宅男女神图片采集"; // 设置任务名称
            spider.Speed = 5; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
            spider.Depth = 5; // 设置采集深度
            spider.DownloaderType = DownloaderType.Default; // 使用普通下载器, 无关 Cookie, 干净的 HttpClient
            spider.AddDataParser(new NvshensRootDataParser());
            spider.AddDataParser(new NvshensTagDataParser());
            spider.AddDataParser(new NvshensTagPageDataParser());
            //spider.AddDataParser(new NvshensRootDataParser());

            spider.AddStorage(new ConsoleStorage()); // 控制台打印采集结果
            spider.AddRequests("https://www.nvshens.com/gallery/"); // 设置起始链接
            spider.RunAsync(); // 启动
        }

        class NvshensRootDataParser : DataParser
        {
            public NvshensRootDataParser()
            {
                CanParse = RegexCanParse("^((https|http)?:\\/\\/)www\\.nvshens\\.com\\/gallery\\/$");
                //Follow = XpathFollow(".");
            }

            public override Task<DataFlowResult> Parse(DataFlowContext context)
            {
               var response = context.GetResponse();
                context.AddItem("URL", response.Request.Url);
                context.AddItem("Title", context.GetSelectable().XPath(".//title").GetValue());

                Dictionary<string ,string> tags = new Dictionary<string, string>();
                var tagNodes = context.GetSelectable().XPath("//*[@id=\"post_rank\"]/div[2]/div/div[@class='tag_div']/ul/li/a").Nodes();
                foreach (var node in tagNodes)
                {
                    var url = node.XPath("./@href").GetValue();
                    var name = node.GetValue();
                    tags.Add(url,name);
                    Console.WriteLine("url:" + url +" - name:"+name);
                }

                var requests = new List<Request>();
                foreach (var sub in tags)
                {
                    var request = new Request
                    {
                        Url = sub.Key,
                        OwnerId = response.Request.OwnerId
                    };
                    requests.Add(request);

                    CreateDirByTag(sub.Value);
                }
                context.AddTargetRequests(requests.ToArray());

                /*var subs = context.GetSelectable().XPath("//*[@id=\"post_rank\"]/div[2]/div/div[@class='tag_div']/ul/li/a/@href").GetValues();
                var requests = new List<Request>();
                foreach (var sub in subs)
                {
                    var request = new Request();
                    request.Url = sub;
                    request.OwnerId = response.Request.OwnerId;
                    requests.Add(request);
                    Console.WriteLine("sub parse:" + sub);
                }
                context.AddTargetRequests(requests.ToArray());*/

                return Task.FromResult(DataFlowResult.Success);
            }
        }

        class NvshensTagDataParser : DataParser
        {
            public NvshensTagDataParser()
            {
                //CanParse = RegexCanParse("^((https|http) ?:\\/\\/)www\\.nvshens\\.com\\/gallery\\/(((\\w)*\\/$)|(\\w*\\/\\d.html$))");
                CanParse = RegexCanParse("^((https|http) ?:\\/\\/)www\\.nvshens\\.com\\/gallery\\/(\\w)*\\/$");
                //Follow = XpathFollow(".");
            }

            public override Task<DataFlowResult> Parse(DataFlowContext context)
            {
                var response = context.GetResponse();
                //context.AddItem("URL", response.Request.Url);
                //context.AddItem("Title", context.GetSelectable().XPath(".//title").GetValue());

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("第一页："+ context.GetSelectable().XPath(".//title").GetValue());
                Console.ForegroundColor = ConsoleColor.White;
                //var subs = context.GetSelectable().XPath("//*[@id=\"post_rank\"]/div[2]/div/div[@class='tag_div']/ul/li/a").Nodes().Count();
                Dictionary<string,string> pageSet = new Dictionary<string,string>();
                var pages = context.GetSelectable().XPath("//*[@id=\"listdiv\"]/div[@class='pagesYY']/div/a[not(@class)]/@href").GetValues();
                var requestList = new List<Request>();
                foreach (var page in pages)
                {
                    if (!pageSet.ContainsKey(page))
                    {
                        var request = new Request();
                        request.Url = page;
                        request.OwnerId = response.Request.OwnerId;
                        requestList.Add(request);

                        pageSet.Add(page, page);
                    }
                }

                if (requestList.Count>0)
                {
                    context.AddTargetRequests(requestList.ToArray());
                }

                return Task.FromResult(DataFlowResult.Success);
            }
            
        }

        class NvshensTagPageDataParser : DataParser
        {
            public NvshensTagPageDataParser()
            {
                CanParse = RegexCanParse("^((https|http) ?:\\/\\/)www\\.nvshens\\.com\\/gallery\\/\\w*\\/\\d.html$");
                //Follow = XpathFollow(".");
            }

            public override Task<DataFlowResult> Parse(DataFlowContext context)
            {
                var response = context.GetResponse();
                //context.AddItem("URL", response.Request.Url);
                //context.AddItem("Title", context.GetSelectable().XPath(".//title").GetValue());

                //var result = CheckType(response.Request.Url);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("page 页:"+ context.GetSelectable().XPath(".//title").GetValue());
                Console.ForegroundColor = ConsoleColor.White;
                //var subs = context.GetSelectable().XPath("*[@id='post_rank']/div[2]/div/div[@class='tag_div']/ul/li/a/@href").GetValues();
                //var subs = context.GetSelectable().XPath("//*[@id=\"post_rank\"]/div[2]/div/div[2]/ul/li/a[1]/@href").GetValue();
                //var subs = context.GetSelectable().XPath("//*[@id=\"post_rank\"]/div[2]/div/div[@class='tag_div']/ul/li/a").Nodes().Count();
                /*var pages = context.GetSelectable().XPath("//*[@id=\"listdiv\"]/div[3]/div/a[not(@class)]/@href").GetValues();
                foreach (var page in pages)
                {
                    var request = new Request();
                    request.Url = page;
                    request.OwnerId = response.Request.OwnerId;
                    context.AddTargetRequests(request);
                }*/

                return Task.FromResult(DataFlowResult.Success);
            }

        }

        class NvshensDetailDataParser : DataParser
        {
            public NvshensDetailDataParser()
            {
                CanParse = RegexCanParse("^((https|http)?:\\/\\/)www\\.nvshens\\.com\\/gallery\\/$");
                //Follow = XpathFollow(".");
            }

            public override Task<DataFlowResult> Parse(DataFlowContext context)
            {
                var response = context.GetResponse();
                context.AddItem("URL", response.Request.Url);
                context.AddItem("Title", context.GetSelectable().XPath(".//title").GetValue());

                //var result = CheckType(response.Request.Url);
                //Console.WriteLine("type:"+result);

                //var subs = context.GetSelectable().XPath("*[@id='post_rank']/div[2]/div/div[@class='tag_div']/ul/li/a/@href").GetValues();
                //var subs = context.GetSelectable().XPath("//*[@id=\"post_rank\"]/div[2]/div/div[2]/ul/li/a[1]/@href").GetValue();
                //var subs = context.GetSelectable().XPath("//*[@id=\"post_rank\"]/div[2]/div/div[@class='tag_div']/ul/li/a").Nodes().Count();
                var subs = context.GetSelectable().XPath("//*[@id=\"post_rank\"]/div[2]/div/div[@class='tag_div']/ul/li/a/@href").GetValues();
                foreach (var sub in subs)
                {
                    CreateFromRequest(response.Request, sub);
                }

                return Task.FromResult(DataFlowResult.Success);
            }

        }

        public static void CreateDirByTag(string tag)
        {
            string rootPath = Environment.CurrentDirectory + "\\Pictures";
            string tagPath = rootPath + "\\" + tag;
            if (!Directory.Exists(tagPath))
            {
                Directory.CreateDirectory(tagPath);
            }
            

        }
    }
}
