using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Parser.Attribute;
using DotnetSpider.DataFlow.Storage.Model;
using DotnetSpider.Downloader;
using DotnetSpider.Selector;
using Xunit;

namespace DotnetSpider.Tests.Data.Parser
{
	public partial class DataParserTests
	{
		/// <summary>
		/// 检测在实体类上面添加 ShareValueSelectors 是否生效
		/// 1. 实体类上面添加 ValueSelector
		/// 2. 属性上使用 Environment 查询共享值
		/// </summary>
		[Fact(DisplayName = "ShareValueSelectors")]
		public void ShareValueSelectors()
		{
			//TODO
		}


		/// <summary>
		/// 检测在实体类上面添加 FollowSelector 是否生效
		/// </summary>
		[Fact(DisplayName = "FollowSelector")]
		public void FollowSelector()
		{
			//TODO
		}

		/// <summary>
		/// 检测环境变量查询是否正确
		/// 1. Request 中的 Properties 可以查询
		/// 2. 特殊类型如 当天时间、元素索引、当天日期、当月等
		/// </summary>
		[Fact(DisplayName = "EnvironmentSelector")]
		public void EnvironmentSelector()
		{
			//TODO
		}

		/// <summary>
		/// 检测配置在属性上的 Formatter 是否有被调用
		/// 1. 单个
		/// 2. 多个
		/// </summary>
		[Fact(DisplayName = "Formatters")]
		public void Formatters()
		{
			//TODO
		}

		/// <summary>
		/// 测试实体类的解析是否正确
		/// </summary>
		[Fact(DisplayName = "ParseEntity")]
		public void ParseEntity()
		{
			var service = LocalSpiderProvider.Value.CreateScopeServiceProvider();
			var dataContext = new DataFlowContext(new Response
			{
				Request = new Request("https://list.jd.com/list.html?cat=9987,653,655",
					new Dictionary<string, string> {{"cat", "手机"}, {"cat3", "110"}}),
				Content = File.ReadAllBytes("Jd.html"),
				CharSet = "UTF-8"
			}, service);

			DataParser<Product> extractor = new DataParser<Product>();


			extractor.HandleAsync(dataContext).GetAwaiter().GetResult();

			var results = (ParseResult<Product>)dataContext.GetParseData(typeof(Product).FullName);
			Assert.Equal(60, results.Count);
			Assert.Equal("手机", results[0].CategoryName);
			Assert.Equal(110, results[0].CategoryId);
			Assert.Equal("https://item.jd.com/3031737.html", results[0].Url);
			Assert.Equal("3031737", results[0].Sku);
			Assert.Equal("荣耀官方旗舰店", results[0].ShopName);
			Assert.Equal("荣耀 NOTE 8 4GB+32GB 全网通版 冰河银", results[0].Name);
			Assert.Equal("1000000904", results[0].VenderId);
			Assert.Equal("1000000904", results[0].JdzyShopId);
			Assert.Equal(DateTimeOffset.Now.ToString("yyyy-MM-dd"), results[0].RunId.ToString("yyyy-MM-dd"));
		}

		private string Html = @"
<div class='title'>i am title</div>
<div class='dotnetspider'>i am dotnetspider</div>
<div>
	<div class='aaaa'>a</div>
	<div class='aaaa'>b</div>
</div>
";

		/// <summary>
		/// 测试页面与数据对象 1:1 解析是否正确
		/// </summary>
		[Fact(DisplayName = "SingleEntitySelector")]
		public void SingleEntitySelector()
		{
			var service = LocalSpiderProvider.Value.CreateScopeServiceProvider();
			var dataContext =
				new DataFlowContext(
					new Response
					{
						Request = new Request("http://abcd.com"),
						Content = Encoding.UTF8.GetBytes(Html),
						CharSet = "UTF-8"
					}, service);

			var parser = new DataParser<N>();


			parser.HandleAsync(dataContext).GetAwaiter().GetResult();

			var results = (ParseResult<N>)dataContext.GetParseData(typeof(N).FullName);

			Assert.Equal("i am title", results[0].title);
			Assert.Equal("i am dotnetspider", results[0].dotnetspider);
		}

		/// <summary>
		/// 测试页面与数据对象 1:n 解析是否正确
		/// </summary>
		[Fact(DisplayName = "MultiEntitySelector")]
		public void MultiEntitySelector()
		{
			var service = LocalSpiderProvider.Value.CreateScopeServiceProvider();
			var dataContext =
				new DataFlowContext(
					new Response
					{
						Request = new Request("http://abcd.com"),
						Content = Encoding.UTF8.GetBytes(Html),
						CharSet = "UTF-8"
					}, service);

			var parser = new DataParser<E>();

			parser.HandleAsync(dataContext).GetAwaiter().GetResult();

			var results = (ParseResult<E>)dataContext.GetParseData(typeof(E).FullName);

			Assert.Equal("a", results[0].title);
			Assert.Equal("b", results[1].title);
		}

		private class N : EntityBase<N>
		{
			[ValueSelector(Expression = "./div[@class='title']")]
			public string title { get; set; }

			[ValueSelector(Expression = "./div[@class='dotnetspider']")]
			public string dotnetspider { get; set; }
		}

		[EntitySelector(Expression = "//div[@class='aaaa']")]
		private class E : EntityBase<E>
		{
			[ValueSelector(Expression = ".")] public string title { get; set; }
		}

		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		private class Product : EntityBase<Product>
		{
#pragma warning disable 649
			public string AAA;
#pragma warning restore 649

#pragma warning disable 169
			private string _bb;
#pragma warning restore 169

			[ValueSelector(Expression = "cat", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[ValueSelector(Expression = "cat3", Type = SelectorType.Enviroment)]
			public int CategoryId { get; set; }

			[ValueSelector(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[ValueSelector(Expression = "./@data-sku")]
			public string Sku { get; set; }

			[ValueSelector(Expression = "./div[5]/strong/a")]
			public long CommentsCount { get; set; }

			[ValueSelector(Expression = ".//div[@class='p-shop']/@data-shop_name")]
			public string ShopName { get; set; }

			[ValueSelector(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[ValueSelector(Expression = "./@venderid")]
			public string VenderId { get; set; }

			[ValueSelector(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }

			[ValueSelector(Expression = "TODAY", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }
		}
	}
}
