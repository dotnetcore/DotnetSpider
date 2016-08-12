using DotnetSpider.Extension;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Downloader.WebDriver;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Sample
{
    public class GSXSpider : SpiderBuilder
    {
        protected override EntitySpider GetSpiderContext()
        {
            var cookieThief = new LoginCookieInterceptor()
            {
                Url = "https://i.genshuixue.com/login",
                UserSelector = new Selector
                {
                    Type = ExtractType.XPath,
                    Expression = @"//form[@id=""loginForm""]//input[@name=""username""]"
                },
                User = "someuser",
                PassSelector = new Selector
                {
                    Type = ExtractType.XPath,
                    Expression = @"//form[@id=""loginForm""]/div[@class=""form-group form-password""]//input"
                },
                Pass = "somepwd",
                SubmitSelector = new Selector
                {
                    Type = ExtractType.XPath,
                    Expression = @"//form[@id=""loginForm""]//button[@class=""btn btn-info btn-block""]"
                }
            };
            //var cookie = cookieThief.GetCookie();

            EntitySpider context = new EntitySpider(new Core.Site
            {
                //Cookie = cookie.Item1,
                //Cookies = cookie.Item2,
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
            }) { CookieInterceptor = cookieThief };
            context.SetIdentity("gsx 订单/store test " + DateTime.Now.ToString("yyyy-MM-dd HHmmss"));
            //context.AddPipeline(new MysqlPipeline
            //{
            //    ConnectString = "Database='GSX';Data Source=localhost;User ID=root;Password=pass@word1;Port=4040"
            //});
            context.AddStartUrl("http://i.genshuixue.com/main");
            context.AddEntityType(typeof(主页)).AddEntityType(typeof(订单列表项));
            context.SetDownloader(new WebDriverDownloader(Browser.Chrome, 300));
            context.SetThreadNum(4);
            return context;
        }

        [TargetUrl(UrlPattern = @"http://i\.genshuixue\.com/main[^/]*$", KeepOrigin = true)]
        public class 主页 : ISpiderEntity
        {
            [TargetUrl]
            //[PropertyExtractBy(Expression = @"//div[@id=""sidebar""]//li[3]/ul/li[3]")]
            [PropertyExtractBy(Expression = "//a[@ui-sref=\"jigou.orders\"]")]
            public string 订单页面链接 { get; set; }
        }

        [TargetUrl(UrlPattern = @"http://i\.genshuixue\.com/main(\?tick=[0-9]*)*#/orders//$", KeepOrigin = true)]
        [TypeExtractBy(Expression = "//div[@id=\"order-wrapper\"]//div[@class=\"orders-table\"]/div[@class=\"table-body ng-scope\"]", Multi = true)]
        public class 订单列表项 : ISpiderEntity
        {
            [PropertyExtractBy(Expression = ".//a[@class=\"teacher-name\"]/@href", Pattern = "^.*/([0-9]*)", ReplaceString = "$1")]
            public string 教师ID { get; set; }

            [TargetUrl(OtherPropertiesAsExtras = new[] { "教师ID" })]
            [PropertyExtractBy(Expression = @".//div[@class=""orders-viewdetail""]")]
            public string 详情页面链接 { get; set; }

            public static Func<IWebDriver, IWebElement> UntilCondition()
            {
                return ExpectedConditions.ElementExists(By.XPath("//div[@id=\"order-wrapper\"]//div[@class=\"orders-table\"]/div[@class=\"table-body ng-scope\"]"));
            }
        }

        [TargetUrl(UrlPattern = "http://www.genshuixue.com/order/teacherOrderDetail?purchase_id=*")]
        [Schema("GSX", "订单", TableSuffix.Today)]
        public class 订单 : ISpiderEntity
        {
            public string 订单编号 { get; set; }

            public int 总时长 { get; set; }

            public int 已完成 { get; set; }

            public int 可约课 { get; set; }

            public decimal 总价 { get; set; }

            public decimal 单价 { get; set; }


        }
    }
}
