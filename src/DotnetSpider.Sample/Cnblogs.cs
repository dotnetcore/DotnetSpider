using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using DotnetSpider.Core.Infrastructure.Database;
using Dapper;
using DotnetSpider.Extension.Model;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Sample
{
    public class Cnblogs
    {
        public static void Run()
        {
            // 定义要采集的 Site 对象, 可以设置 Header、Cookie、代理等
            var site = new Site { EncodingName = "UTF-8" };
            for (int i = 1; i < 5; ++i)
            {
                // 添加初始采集链接
                site.AddStartUrl("http://www.cnblogs.com");

            }

            // 使用内存Scheduler、自定义PageProcessor、自定义Pipeline创建爬虫
            Spider spider = Spider.Create(site,
                new QueueDuplicateRemovedScheduler(),
                new BlogSumaryProcessor()).
                AddPipeline(new MyPipeline());
            spider.ThreadNum = 2;

            // 启动爬虫
            spider.Run();
        }

        private class MyPipeline : BasePipeline
        {
            private static long blogSumaryCount = 0;
            private static long newsCount = 0;

            public MyPipeline()
                
            {

                using (var conn = new MySqlConnection(Env.DataConnectionString))
                {
                    conn.Execute($"CREATE SCHEMA IF NOT EXISTS `DotnetSpider` DEFAULT CHARACTER SET utf8mb4 ;");
                    conn.Execute($"CREATE TABLE IF NOT EXISTS `DotnetSpider`.`BlogSumary` ( `Name` varchar(300) DEFAULT NULL,`Type` varchar(50) DEFAULT NULL, `Author` varchar(300) DEFAULT NULL, `Url` varchar(300) DEFAULT NULL, `PublishTime` timestamp NULL DEFAULT CURRENT_TIMESTAMP) DEFAULT CHARSET=utf8mb4;");
                  //  conn.Execute($"CREATE TABLE IF NOT EXISTS `DotnetSpider`.`News` ( `Name` varchar(300) DEFAULT NULL,  `Url` varchar(300) DEFAULT NULL, `PublishTime` timestamp NULL DEFAULT CURRENT_TIMESTAMP) DEFAULT CHARSET=utf8mb4;");


                }

            }

            public override void Process(params ResultItems[] resultItems)
            {

                var blogs = new List<BlogSumary>();
             
                foreach (var resultItem in resultItems)
                {
              
                        foreach (BlogSumary entry in resultItem.GetResultItem("BlogSumary"))
                        {
                            blogSumaryCount++;
                            Console.WriteLine($"BlogSumary [{blogSumaryCount}] {entry}");
                            blogs.Add(entry);

                        }
             

                   
                }
                // 可以自由实现插入数据库或保存到文件
                using (var conn = new MySqlConnection(Env.DataConnectionString))
                {
                
                    var sql1 =
                        $"INSERT IGNORE `DotnetSpider`.`BlogSumary` (`Name`, `Author`,`Type`, `PublishTime`,`Url`) VALUES (@Name, @Author,@Type, @PublishTime,@Url);";

                    SqlMapper.Execute(conn, sql1, blogs);
                    
                }

             
            }
        }

        private class BlogSumaryProcessor : BasePageProcessor
        {
            public BlogSumaryProcessor()
            {
                // 定义目标页的筛选
                AddTargetUrlExtractor(".", "^http://www\\.cnblogs\\.com/$", "http://www\\.cnblogs\\.com/sitehome/p/\\d+", "^http://www\\.cnblogs\\.com/news/$", "www\\.cnblogs\\.com/news/\\d+");
            }

            protected override void Handle(Page page)
            {
                // 利用 Selectable 查询并构造自己想要的数据对象
                var blogSummaryElements = page.Selectable.SelectList(Selectors.XPath("//div[@class='post_item']")).Nodes();
                List<BlogSumary> results = new List<BlogSumary>();
                foreach (var blogSummary in blogSummaryElements)
                {
                    var video = new BlogSumary();
                    video.Name = blogSummary.Select(Selectors.XPath(".//a[@class='titlelnk']")).GetValue();
                    video.Url = blogSummary.Select(Selectors.XPath(".//a[@class='titlelnk']/@href")).GetValue();
                    video.Author = blogSummary.Select(Selectors.XPath(".//div[@class='post_item_foot']/a[1]")).GetValue();
                    video.PublishTime = blogSummary.Select(Selectors.XPath(".//div[@class='post_item_foot']/text()")).GetValue();
                    video.Type = page.Url.Contains("news")==true?"News":"Blogs";

                    results.Add(video);
                }

                // 以自定义KEY存入page对象中供Pipeline调用
                page.AddResultItem("BlogSumary", results);
            }
        }
 
        [EntityTable("DotnetSpider", "BlogSumary")]
        class BlogSumary
        {
            public string Name { get; set; }
            public string Author { get; set; }
            public string PublishTime { get; set; }
            public string Url { get; set; }

            public string Type { get; set; }

            public override string ToString()
            {
                return $"{Name}|{Author}|{PublishTime}|{Url}";
            }
        }
     
    }
}
