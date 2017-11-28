using DotnetSpider.Core;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DotnetSpider.Sample
{
    public class LyProductSpider : EntitySpider
    {
        protected override void MyInit(params string[] arguments)
        {
            Identity = Identity ?? "Ly SAMPLE";
            // storage data to mysql, default is mysql entity pipeline, so you can comment this line. Don't miss sslmode.
            AddPipeline(new MySqlEntityPipeline(Env.DataConnectionStringSettings.ConnectionString));
            string url = "";
            for (int i = 1; i <= 50; i++)
            {
                  url =
                    $"https://www.ly.com/scenery/NewSearchList.aspx?&action=getlist&page={i}&kw=&pid=0&cid=0&cyid=0&sort=&isnow=0&spType=&lbtypes=&IsNJL=0&classify=0&grade=&dctrack=1%CB%871506309369836510%CB%872%CB%878%CB%872118445358272143%CB%870&iid=0.27541947458074256";

                AddStartUrl(url);
            }
  
            AddEntityType<LyProduct>();
        }
    }


    [EntityTable("DotnetSpider", "LyProduct", EntityTable.Today, Indexs = new[] { "Name" }, Uniques = new[]{ "Code" })]
    [EntitySelector(Expression = "//div[@class='s_info']")]
   // [TargetUrlsSelector(XPaths = new[] { "//span[@class=\"page_link\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
    public class LyProduct:SpiderEntity
    {
        [PropertyDefine(Expression = "@sid", Length = 100)]
        public string Code { get; set; }
        [PropertyDefine(Expression = "div/dl/dt/a/@title", Length = 100)]
        public string Name { get; set; }
        [PropertyDefine(Expression = "//[@class='s_level']", IgnoreStore =true,  Length = 100)]
        public string Level { get; set; }

        [PropertyDefine(Expression = "//span[@class='sce_address']/a", Length = 100)]
        public string City { get; set; }
        [PropertyDefine(Expression = "div/dl/dd[2]/p", Length = 100)]
        public string Address { get; set; }
        [PropertyDefine(Expression = "div/dl/dd[3]/p", Length = 100)]
        public string Tese { get; set; }
 
    }
}
