using DotnetSpider.Core;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver.Core.Events;
using DotnetSpider.Core.Selector;

namespace DotnetSpider.Sample
{
    public class ZhiPinSpider : EntitySpider
    {
        protected override void MyInit(params string[] arguments)
        {
            Identity = Identity ?? "bosszhipin";
            // storage data to mysql, default is mysql entity pipeline, so you can comment this line. Don't miss sslmode.
            AddPipeline(new MySqlEntityPipeline(Env.DataConnectionStringSettings.ConnectionString));
            AddStartUrl("https://www.zhipin.com/job_detail/?query=.net&page=1", new Dictionary<string, object> { { "query", ".NET" } });
            AddStartUrl("https://www.zhipin.com/job_detail/?query=java&page=1", new Dictionary<string, object> { { "query", "JAVA" } });
            AddStartUrl("https://www.zhipin.com/job_detail/?query=Python&page=1", new Dictionary<string, object> { { "query", "PYTHON" } });
          

            AddEntityType<Work>();
        }
    }



    [EntityTable("DotnetSpider", "BOSS", EntityTable.Monday, Indexs = new[] { "JobId" })]
    [EntitySelector(Expression = "//div[@class='job-list']/ul/li")]
    [TargetUrlsSelector(XPaths = new[] { "//div[@class=\"page\"]" }, Patterns = new[] { @"page=[0-9]+" })]
    public class Work : SpiderEntity
    {
 
        [PropertyDefine(Expression = "div/div[@class='info-company']/div/h3/a", Length = 100)]
        public string Company { get; set; }
        [PropertyDefine(Expression = "div/div[@class='info-primary']/h3/a", Length = 100)]
        [SubStringFormatter(FindFirstIndex = " ")]
        public string Job { get; set; }
        [PropertyDefine(Expression = "div/div[@class='info-primary']/h3/a/@data-jobid", Length = 100)]
        public string JobId { get; set; }
        [PropertyDefine(Expression = "div/div[@class='info-primary']/h3/a/span", Length = 100)]
        [SubStringFormatter(FindFirstIndex = "-")]
        public string Offer1 { get; set; }
        [PropertyDefine(Expression = "div/div[@class='info-primary']/h3/a/span", Length = 100)]
        [SubStringFormatter(Start =1,  FindLastIndex = "-")]
        public string Offer2 { get; set; }
        [PropertyDefine(Expression = "div/div[@class='info-primary']/p", Length = 100)]
        [ReplaceFormatter(NewValue = "", OldValue = "<em class=\"vline\"></em>")]
        [SubStringFormatter(Start = 2, FindFirstIndex = "年")]
        public string YearRange { get; set; }
        [PropertyDefine(Expression = "div/div[@class='info-primary']/p", Length = 100)]
        [ReplaceFormatter(NewValue = "", OldValue = "<em class=\"vline\"></em>")]
        [SubStringFormatter(Length = 2)]
        public string City { get; set; }
        [PropertyDefine(Expression = "div/div[@class='info-primary']/p", Length = 100)]
        [ReplaceFormatter(NewValue = "", OldValue = "<em class=\"vline\"></em>")]
        [SubStringFormatter(Start = 1, FindLastIndex = "年")]
        public string Xueli { get; set; }
        [PropertyDefine(Expression = "div[@class='job-tags']/span", Length = 100)]
        public string Tips { get; set; }
        [PropertyDefine(Expression = "div/div/div[@class='company-text']/p", Length = 100)]
        [ReplaceFormatter(NewValue = "", OldValue = "<em class=\"vline\"></em>")]
        public string Level { get; set; }

        [PropertyDefine(Expression = "query", Type = SelectorType.Enviroment)]
        public string Type { get; set; }
    }


}
