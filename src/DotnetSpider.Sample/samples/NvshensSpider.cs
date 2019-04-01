using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Data;
using DotnetSpider.Data.Parser;
using DotnetSpider.Downloader;

namespace DotnetSpider.Sample.samples
{
    public class NvshensSpider
    {
        public static void Run()
        {
            ImageDownloader.GetInstance().Start();

            var builder = new SpiderBuilder();
            builder.AddSerilog();
            builder.ConfigureAppConfiguration();
            builder.UseStandalone();
            builder.AddSpider<EntitySpider>();
            var provider = builder.Build();
            var spider = provider.Create<Spider>();

            spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
            spider.Name = "宅男女神图片采集"; // 设置任务名称
            spider.Speed = 2; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
            spider.Depth = 5; // 设置采集深度
            spider.DownloaderSettings.Type = DownloaderType.HttpClient; // 使用普通下载器, 无关 Cookie, 干净的 HttpClient
            //spider.AddDataFlow(new NvshensTagIndexDataParser());
            spider.AddDataFlow(new NvshensFirstPageTagDataParser());
            spider.AddDataFlow(new NvshensPageTagDataParser());
            spider.AddDataFlow(new NvshensFirstPageDetailDataParser());
            spider.AddDataFlow(new NvshensPageDetailDataParser());
            //spider.AddRequests("https://www.nvshens.com/gallery/"); // 设置起始链接
            spider.AddRequests("https://www.nvshens.com/gallery/luoli/"); // 设置起始链接
            spider.RunAsync(); // 启动
        }

        /// <summary>
        /// 爬取Tag索引页的所有tag
        /// </summary>
        class NvshensTagIndexDataParser : DataParser
        {
            public NvshensTagIndexDataParser()
            {
                CanParse = DataParserHelper.CanParseByRegex("^((https|http)?:\\/\\/)www\\.nvshens\\.com\\/gallery\\/$");
                //Follow = XpathFollow(".");
            }

            protected override Task<DataFlowResult> Parse(DataFlowContext context)
            {
                context.AddItem("URL", context.Response.Request.Url);
                context.AddItem("Title", context.GetSelectable().XPath(".//title").GetValue());

                Dictionary<string, string> tags = new Dictionary<string, string>();
                var tagNodes = context.GetSelectable()
                    .XPath("//*[@id=\"post_rank\"]/div[2]/div/div[@class='tag_div']/ul/li/a").Nodes();
                foreach (var node in tagNodes)
                {
                    var url = node.XPath("./@href").GetValue();
                    var name = node.GetValue();
                    tags.Add(url, name);
                    Console.WriteLine("url:" + url + " - name:" + name);
                }

                var requests = new List<Request>();
                foreach (var tag in tags)
                {
                    var request = new Request
                    {
                        Url = tag.Key,
                        OwnerId = context.Response.Request.OwnerId
                    };
                    request.AddProperty("tag", tag.Value);

                    requests.Add(request);
                }

                context.FollowRequests.AddRange(requests.ToArray());

                return Task.FromResult(DataFlowResult.Success);
            }
        }

        /// <summary>
        /// Tag页，解析Tag下第一页的所有画册subject，同时解析出其它分页
        /// </summary>
        class NvshensFirstPageTagDataParser : DataParser
        {
            public NvshensFirstPageTagDataParser()
            {
                //CanParse = RegexCanParse("^((https|http) ?:\\/\\/)www\\.nvshens\\.com\\/gallery\\/(((\\w)*\\/$)|(\\w*\\/\\d.html$))");
                CanParse = DataParserHelper.CanParseByRegex("^((https|http) ?:\\/\\/)www\\.nvshens\\.com\\/gallery\\/(\\w)*\\/$");
            }

            protected override Task<DataFlowResult> Parse(DataFlowContext context)
            {
                //context.AddItem("URL", response.Request.Url);
                //context.AddItem("Title", context.GetSelectable().XPath(".//title").GetValue());

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("第一页：" + context.GetSelectable().XPath(".//title").GetValue());
                Console.ForegroundColor = ConsoleColor.White;

                GetSubjectPageUrl(context);
                GetSubjectUrl(context);

                return Task.FromResult(DataFlowResult.Success);
            }
        }

        /// <summary>
        /// 处理Tag除了第一页之外的分页页面
        /// </summary>
        class NvshensPageTagDataParser : DataParser
        {
            public NvshensPageTagDataParser()
            {
                CanParse = DataParserHelper.CanParseByRegex("^((https|http) ?:\\/\\/)www\\.nvshens\\.com\\/gallery\\/\\w*\\/\\d+.html$");
                //Follow = XpathFollow(".");
            }

            protected override Task<DataFlowResult> Parse(DataFlowContext context)
            {
                //context.AddItem("URL", response.Request.Url);
                //context.AddItem("Title", context.GetSelectable().XPath(".//title").GetValue());

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("page 页:" + context.GetSelectable().XPath(".//title").GetValue());
                Console.ForegroundColor = ConsoleColor.White;

                GetSubjectUrl(context);

                return Task.FromResult(DataFlowResult.Success);
            }
        }

        /// <summary>
        /// 第一页明细
        /// </summary>
        class NvshensFirstPageDetailDataParser : DataParser
        {
            public NvshensFirstPageDetailDataParser()
            {
                CanParse = DataParserHelper.CanParseByRegex("^((https|http)?:\\/\\/)www\\.nvshens\\.com\\/\\w+\\/\\d*\\/$");
                //Follow = XpathFollow(".");
            }

            protected override Task<DataFlowResult> Parse(DataFlowContext context)
            {
                GetDetailPictureUrl(context);
                GetDetailPageUrl(context);

                return Task.FromResult(DataFlowResult.Success);
            }
        }

        /// <summary>
        /// 图片浏览页除第一页的分页
        /// </summary>
        class NvshensPageDetailDataParser : DataParser
        {
            public NvshensPageDetailDataParser()
            {
                CanParse = DataParserHelper.CanParseByRegex("^((https|http)?:\\/\\/)www\\.nvshens\\.com\\/\\w\\/\\d*\\/\\d+.html$");
                //Follow = XpathFollow(".");
            }

            protected override Task<DataFlowResult> Parse(DataFlowContext context)
            {
                GetDetailPictureUrl(context);

                return Task.FromResult(DataFlowResult.Success);
            }
        }

        /// <summary>
        /// 解析画册的分页
        /// </summary>
        /// <param name="context"></param>
        public static void GetSubjectPageUrl(DataFlowContext context)
        {
            Dictionary<string, string> pageSet = new Dictionary<string, string>();
            var pages = context.GetSelectable()
                .XPath("//*[@id=\"listdiv\"]/div[@class='pagesYY']/div/a[not(@class)]/@href").GetValues();
            var requestList = new List<Request>();
            foreach (var page in pages)
            {
                if (!pageSet.ContainsKey(page))
                {
                    try
                    {
                        var request = new Request
                        {
                            Url = page,
                            OwnerId = context.Response.Request.OwnerId
                        };
                        //request.Properties.Add("tag", response.Request.Properties["tag"]); 
                        request.AddProperty("tag", "萝莉");
                        requestList.Add(request);

                        pageSet.Add(page, page);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            if (requestList.Count > 0)
            {
                context.FollowRequests.AddRange(requestList.ToArray());
            }
        }

        /// <summary>
        /// 获取主题的地址
        /// </summary>
        /// <param name="context"></param>
        public static void GetSubjectUrl(DataFlowContext context)
        {
            var pages = context.GetSelectable()
                .XPath("//*[@id=\"listdiv\"]/ul/li/div[@class='galleryli_title']/a/@href").GetValues();
            var requestList = new List<Request>();
            foreach (var page in pages)
            {
                var request = new Request
                {
                    Url = page,
                    OwnerId = context.Response.Request.OwnerId
                };
                request.AddProperty("tag", context.Response.Request.GetProperty("tag"));
                request.AddProperty("referer", context.Response.Request.Url);
                requestList.Add(request);
            }

            if (requestList.Count > 0)
            {
                context.FollowRequests.AddRange(requestList.ToArray());
            }
        }

        /// <summary>
        /// 取得详细图片查看的分页url
        /// </summary>
        /// <param name="context"></param>
        public static void GetDetailPageUrl(DataFlowContext context)
        {
            Dictionary<string, string> pageSet = new Dictionary<string, string>();
            var pages = context.GetSelectable().XPath("//*[@id=\"pages\"]/a[not(@class)]/@href").GetValues();
            var requestList = new List<Request>();
            foreach (var page in pages)
            {
                if (!pageSet.ContainsKey(page))
                {
                    var request = new Request
                    {
                        Url = page,
                        OwnerId = context.Response.Request.OwnerId
                    };
                    request.AddProperty("tag", context.Response.Request.GetProperty("tag"));
                    request.AddProperty("referer", context.Response.Request.GetProperty("referer"));
                    requestList.Add(request);

                    pageSet.Add(page, page);
                }
            }

            if (requestList.Count > 0)
            {
                context.FollowRequests.AddRange(requestList.ToArray());
            }
        }

        /// <summary>
        /// 获取图片浏览页里抽图片地址
        /// </summary>
        /// <param name="context"></param>
        public static void GetDetailPictureUrl(DataFlowContext context)
        {
            context.AddItem("URL", context.Response.Request.Url);
            context.AddItem("Title", context.GetSelectable().XPath(".//title").GetValue());

            var images = context.GetSelectable().XPath("//*[@id=\"hgallery\"]/img/@src").GetValues();
            foreach (var image in images)
            {
                //处理图片URL下载
                var request = new Request
                {
                    Url = image,
                    OwnerId = context.Response.Request.OwnerId
                };
                request.AddProperty("tag", context.Response.Request.GetProperty("tag"));
                request.AddProperty("referer", context.Response.Request.GetProperty("referer"));
                request.AddProperty("subject", context.GetSelectable().XPath(".//title").GetValue());
                ImageDownloader.GetInstance().AddRequest(request);
            }
        }
    }
}