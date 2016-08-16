#if !NET_CORE
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Downloader.WebDriver;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Sample
{
    /// <summary>
    /// No pipelines, test for TargetUrl, PropertySelector and UntilCondition.
    /// </summary>
    public class GSXSpider : EntitySpiderBuilder
    {
        protected override EntitySpider GetEntitySpider()
        {
            var cookieThief = new LoginCookieInterceptor()
            {
                Url = "https://i.genshuixue.com/login",
                UserSelector = new Selector
                {
                    Type = SelectorType.XPath,
                    Expression = @"//form[@id=""loginForm""]//input[@name=""username""]"
                },
                User = "someuser",
                PassSelector = new Selector
                {
                    Type = SelectorType.XPath,
                    Expression = @"//form[@id=""loginForm""]/div[@class=""form-group form-password""]//input"
                },
                Pass = "somepwd",
                SubmitSelector = new Selector
                {
                    Type = SelectorType.XPath,
                    Expression = @"//form[@id=""loginForm""]//button[@class=""btn btn-info btn-block""]"
                }
            };

            EntitySpider context = new EntitySpider(new Core.Site
            {
                Headers = new Dictionary<string, string>
                {
                    { "Cache-Control","max-age=0"},
                    { "Host", "i.genshuixue.com" },
                    { "Connection", "keep-alive"},
                    { "Upgrade-Insecure-Requests", "1" },
                    { "Accept-Encoding", "gzip, deflate, sdch"},
                    { "Accept-Language", "zh-CN,zh;q=0.8"}
                },
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36"
            })
            { CookieInterceptor = cookieThief };
            context.SetIdentity("gsx 订单/store test " + DateTime.Now.ToString("yyyy-MM-dd HHmmss"));
            context.AddStartUrl("http://i.genshuixue.com/main");
            context.AddEntityType(typeof(主页)).AddEntityType(typeof(订单列表项)).AddEntityType(typeof(订单));
            context.SetDownloader(new WebDriverDownloader(Browser.Chrome, 300));
            context.SetThreadNum(1);
            context.AddEntityPipeline(new MySqlEntityPipeline("Database='gsx';Data Source=localhost;User ID=root;Password=pass@word1;Port=4040")
            //{ Mode = PipelineMode.Update }
            );
            return context;
        }

        [TargetUrl(UrlPattern = @"http://i\.genshuixue\.com/main[^/]*$", KeepOrigin = true)]
        public class 主页 : ISpiderEntity
        {
            [TargetUrl]
            [PropertySelector(Expression = "//a[@ui-sref=\"jigou.orders\"]")]
            public string 订单页面链接 { get; set; }
        }

        [TargetUrl(UrlPattern = @"http://i\.genshuixue\.com/main(\?tick=[0-9]*)*#/orders//$", KeepOrigin = true)]
        [EntitySelector(Expression = "//div[@id=\"order-wrapper\"]//div[@class=\"orders-table\"]/div[@class=\"table-body ng-scope\"]")]
        public class 订单列表项 : ISpiderEntity
        {
            [PropertySelector(Expression = ".//a[@class=\"teacher-name\"]/@href")]
            [RegexMatchFormatter(Pattern = @"[\d]+")]
            public string 教师ID { get; set; }

            [TargetUrl(OtherPropertiesAsExtras = new[] { "教师ID" })]
            [PropertySelector(Expression = @".//div[@class=""orders-viewdetail""]")]
            public string 详情页面链接 { get; set; }

            public static Func<IWebDriver, IWebElement> UntilCondition()
            {
                return ExpectedConditions.ElementExists(By.XPath("//div[@id=\"order-wrapper\"]//div[@class=\"orders-table\"]/div[@class=\"table-body ng-scope\"]"));
            }
        }

        /// <summary>
        /// Not finished yet.
        /// </summary>
        [TargetUrl(UrlPattern = @"http://i\.genshuixue\.com/teacher/order\.do\?tid=\d+&purchaseId=\d+", KeepOrigin = true)]
        [Schema("GSX", "订单", TableSuffix.Today)]
        [Indexes(Primary = "订单编号")]
        public class 订单 : ISpiderEntity
        {
            [StoredAs("订单编号", DataType.String, 20)]
            [PropertySelector(Expression = @"//div[@id=""main""]//small[1]")]
            [RegexMatchFormatter(Pattern = @"\d{2,}")]
            public string 订单编号 { get; set; }

            [StoredAs("总时长", DataType.String, 5)]
            [PropertySelector(Expression = @"//div[@id=""main""]//td[2]//li[1]")]
            [RegexMatchFormatter(Pattern = @"[\d]+")]
            public int 总时长 { get; set; }

            [StoredAs("已完成", DataType.String, 5)]
            [PropertySelector(Expression = @"//div[@id=""main""]//td[2]//li[2]")]
            [RegexMatchFormatter(Pattern = @"[\d]+")]
            public int 已完成 { get; set; }

            [StoredAs("可约课", DataType.String, 5)]
            [PropertySelector(Expression = @"//div[@id=""main""]//td[2]//li[3]")]
            [RegexMatchFormatter(Pattern = @"[\d]+")]
            public int 可约课 { get; set; }

            [StoredAs("总价", DataType.String, 10)]
            [PropertySelector(Expression = @"//div[@id=""main""]//tr/*[3]//li[2]")]
            [RegexMatchFormatter(Pattern = @"[1-9]\d*\.\d*|0\.\d*[1-9]\d*")]
            public decimal 总价 { get; set; }

            [StoredAs("单价", DataType.String, 10)]
            [PropertySelector(Expression = @"//div[@id=""main""]//td[3]//li[1]")]
            [RegexMatchFormatter(Pattern = @"[1-9]\d*\.\d*|0\.\d*[1-9]\d*")]
            public decimal 单价 { get; set; }

            [StoredAs("折后", DataType.String, 10)]
            [PropertySelector(Expression = @"//div[@id=""main""]//li[@class=""text-success""]")]
            [RegexMatchFormatter(Pattern = @"[1-9]\d*\.\d*|0\.\d*[1-9]\d*")]
            public decimal 折后 { get; set; }

            [StoredAs("订单状态", DataType.String, 20)]
            [PropertySelector(Expression = @"//div[@id=""main""]//td[@class=""order-status""]")]
            public string 订单状态 { get; set; }

            [StoredAs("课程ID", DataType.String, 20)]
            [PropertySelector(Expression = @"//div[@id=""main""]//h4[@class=""course-name""]/a[1]/@href")]
            [RegexMatchFormatter(Pattern = @"\d{2,}")]
            public string 课程ID { get; set; }

            [StoredAs("学生ID", DataType.String, 20)]
            [PropertySelector(Expression = @"//div[@id=""main""]//a[img[@class=""thumbnail""]]/@href")]
            [RegexMatchFormatter(Pattern = @"\d{2,}")]
            public string 学生ID { get; set; }

            [StoredAs("实际收入", DataType.String, 10)]
            [PropertySelector(Expression = @"//div[@id=""main""]//strong[1]")]
            [RegexMatchFormatter(Pattern = @"[1-9]\d*\.\d*|0\.\d*[1-9]\d*")]
            public decimal 实际收入 { get; set; }

        }
    }
}
#endif