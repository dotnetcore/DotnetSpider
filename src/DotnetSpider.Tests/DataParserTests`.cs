using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Http;
using DotnetSpider.Selector;
using Xunit;

namespace DotnetSpider.Tests
{
	public partial class DataParserTests
	{
		/// <summary>
		/// 检测在实体类上面添加 GlobalValueSelectors 是否生效
		/// 1. 实体类上面添加 ValueSelector
		/// 2. 属性上使用 Environment 查询共享值
		/// </summary>
		[Fact]
		public void GlobalValueSelectors()
		{
			//TODO
		}

		/// <summary>
		/// 测试实体类的解析是否正确
		/// 1. GlobalValue
		/// 2. FollowSelector
		/// 3. Environment
		/// 4. ValueSelector
		/// </summary>
		[Fact]
		public async Task ParseEntity()
		{
			var request = new Request("https://list.jd.com/list.html?cat=9987,653,655",
				new Dictionary<string, string> {{"cat", "手机"}, {"cat3", "110"}});
			var dataContext = new DataContext(null, new SpiderOptions(), request,
				new Response {Content = new ResponseContent {Data = File.ReadAllBytes("Jd.html")}});

			var parser = new DataParser<Product>();
			parser.SetHtmlSelectableBuilder();
			await parser.HandleAsync(dataContext);

			var results = (List<Product>)dataContext.GetData(typeof(Product));
			Assert.Equal(60, results.Count);
			Assert.Contains("手机商品筛选", results[0].Title);
			Assert.Contains("手机商品筛选", results[1].Title);
			Assert.Contains("手机商品筛选", results[2].Title);
			Assert.Equal("手机", results[0].CategoryName);
			Assert.Equal(110, results[0].CategoryId);
			Assert.Equal("https://item.jd.com/3031737.html", results[0].Url);
			Assert.Equal("3031737", results[0].Sku);
			Assert.Equal("荣耀官方旗舰店", results[0].ShopName);
			Assert.Equal("荣耀 NOTE 8 4GB+32GB 全网通版 冰河银", results[0].Name);
			Assert.Equal("1000000904", results[0].VenderId);
			Assert.Equal("1000000904", results[0].JdzyShopId);
			Assert.Equal(DateTimeOffset.Now.ToString("yyyy-MM-dd"), results[0].RunId.ToString("yyyy-MM-dd"));

			var requests = dataContext.FollowRequests;
			Assert.Equal(7, requests.Count);
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
		[Fact]
		public async Task SingleEntitySelector()
		{
			var request = new Request("http://abcd.com");
			var dataContext =
				new DataContext(null, new SpiderOptions(), request,
					new Response {Content = new ResponseContent {Data = Encoding.UTF8.GetBytes(Html)}});

			var parser = new DataParser<N>();
			parser.SetHtmlSelectableBuilder();

			await parser.HandleAsync(dataContext);

			var results = (List<N>)dataContext.GetData(typeof(N));
			Assert.Equal("i am title", results[0].title);
			Assert.Equal("i am dotnetspider", results[0].dotnetspider);
		}

		/// <summary>
		/// 测试页面与数据对象 1:n 解析是否正确
		/// </summary>
		[Fact]
		public async Task MultiEntitySelector()
		{
			var request = new Request("http://abcd.com");
			var dataContext =
				new DataContext(null, new SpiderOptions(), request,
					new Response {Content = new ResponseContent {Data = Encoding.UTF8.GetBytes(Html)}});

			var parser = new DataParser<E>();

			await parser.HandleAsync(dataContext);

			var results = (List<E>)dataContext.GetData(typeof(E));

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
		[FollowRequestSelector(Expressions = new[] {".//div[@class='page clearfix']"},
			Patterns = new[] {"list\\.html"})]
		[GlobalValueSelector(Name = "Title", Expression = ".//div[@class='s-title']", Type = SelectorType.XPath)]
		[GlobalValueSelector(Name = "Title2", Expression = ".//div[@class='s-title']", Type = SelectorType.XPath)]
		private class Product : EntityBase<Product>
		{
			[ValueSelector(Expression = "Title", Type = SelectorType.Environment)]
			public string Title { get; set; }

#pragma warning disable 649
			public string AAA;
#pragma warning restore 649

#pragma warning disable 169
			private string _bb;
#pragma warning restore 169

			[ValueSelector(Expression = "cat", Type = SelectorType.Environment)]
			public string CategoryName { get; set; }

			[ValueSelector(Expression = "cat3", Type = SelectorType.Environment)]
			public int CategoryId { get; set; }

			[ValueSelector(Expression = "./div[1]/div[1]/a/@href")]
			public string Url { get; set; }

			[ValueSelector(Expression = "./div[1]/@data-sku")]
			public string Sku { get; set; }

			[ValueSelector(Expression = "./div[1]/div[5]/strong/a")]
			public long CommentsCount { get; set; }

			[ValueSelector(Expression = ".//div[@class='p-shop']/@data-shop_name")]
			public string ShopName { get; set; }

			[ValueSelector(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[ValueSelector(Expression = "./div[1]/@venderid")]
			public string VenderId { get; set; }

			[ValueSelector(Expression = "./div[1]/@jdzy_shop_id")]
			public string JdzyShopId { get; set; }

			[ValueSelector(Expression = "TODAY", Type = SelectorType.Environment)]
			public DateTime RunId { get; set; }
		}
	}
}
