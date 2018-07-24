using DotnetSpider.Core;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction;

namespace DotnetSpider.Extension.Test.Pipeline
{
	public class MySqlEntityFilePipelineTest
	{
		[Fact(DisplayName = "MySqlFileEntityPipeline_InsertSql", Skip = "NEXT")]
		public void MySqlFileEntityPipeline_InsertSql()
		{
			var id = Guid.NewGuid().ToString("N");
			var folder = Path.Combine(Env.BaseDirectory, "mysql", id);
			var path = Path.Combine(folder, "baidu.baidu_search_mysql_file.sql");
			try
			{
				MySqlFileEntityPipelineSpider spider = new MySqlFileEntityPipelineSpider();
				spider.Identity = id;
				spider.Run();

				var lines = File.ReadAllLines(path);
				Assert.Equal(20, lines.Length);
			}
			finally
			{
				if (Directory.Exists(folder))
				{
					Directory.Delete(folder, true);
				}
			}
		}

		private class MySqlFileEntityPipelineSpider : EntitySpider
		{
			public MySqlFileEntityPipelineSpider() : base("MySqlFileEntityPipelineSpider")
			{
			}

			protected override void OnInit(params string[] arguments)
			{
				EmptySleepTime = 1000;
				var word = "可乐|雪碧";
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				AddEntityType<BaiduSearchEntry>();
				AddPipeline(new MySqlEntityFilePipeline(MySqlEntityFilePipeline.FileType.InsertSql));
			}

			[TableInfo("baidu", "baidu_search_mysql_file")]
			[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			class BaiduSearchEntry
			{
				[FieldSelector(Expression = "Keyword", Type = SelectorType.Enviroment)]
				public string Keyword { get; set; }
			}
		}
	}
}
