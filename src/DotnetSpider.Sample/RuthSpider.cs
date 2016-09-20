using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Scheduler;

namespace DotnetSpider.Sample
{
	/// <summary>
	/// TSAI网站数据抓取
	/// 测试页面：
	/// 1.列表
	/// http://www.tsia.org.tw/member_list.php?page=1
	/// 2.详情
	/// http://www.tsia.org.tw/member_info.php?ID=1
	/// </summary>
	public class RuthSpider : EntitySpiderBuilder
	{
		protected override EntitySpider GetEntitySpider()
		{
			//Connecting string
			const string connstr = "Data Source=localhost;Initial Catalog=test;User ID=sa;Password=1234";

			EntitySpider context = new EntitySpider(new Site { })
			{
				UserId = "DotnetSpider",
				TaskGroup = "RuthSpider"
			};
			context.SetThreadNum(1);
			context.SetIdentity("RuthSpider " + DateTime.Now.ToString("yyyy_MM_dd_hhmmss"));
			context.AddEntityPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			context.AddStartUrl("http://www.tsia.org.tw/member_list.php?page=1");

			//添加公司列表頁面Entity
			context.AddEntityType(typeof(CompanySummary), new TargetUrlExtractor
			{
				Patterns = new List<string> { @"member_list.php\?page=\d+" }
			});
			//添加公司詳情頁面Entity
			context.AddEntityType(typeof(Company), new TargetUrlExtractor
			{
				Patterns = new List<string> { @"member_info.php\?ID=\d+" }
			});
			//Config Redis
			context.SetScheduler(new RedisScheduler
			{
				Host = "localhost",
				Password = "",
				Port = 6379
			});
			return context;
		}

		/// <summary>
		/// 公司列表頁面
		/// </summary>
		[Schema("test", "RuthSpider_CompanySummary", TableSuffix.Today)]
		[EntitySelector(Type = SelectorType.XPath, Expression = "//td[@class='list_form_bg']/table/tr")]
		public class CompanySummary : ISpiderEntity
		{
			/// <summary>
			/// 數據來源
			/// </summary>
			[StoredAs("Uri", DataType.Text)]
			[PropertySelector(Expression = "url", Type = SelectorType.Enviroment)]
			public String Uri { get; set; }

			/// <summary>
			/// 分類
			/// </summary>
			[StoredAs("Category", DataType.Text)]
			[TrimFormater()]
			[PropertySelector(Expression = ".//td[3]", Option = PropertySelector.ValueOption.PlainText)]
			public String Category { get; set; }

			/// <summary>
			/// 公司名稱
			/// </summary>
			[StoredAs("Name", DataType.Text)]
			[TrimFormater()]
			[PropertySelector(Expression = ".//td[2]/a", Option = PropertySelector.ValueOption.PlainText)]
			public String Name { get; set; }

			/// <summary>
			/// 公司詳情頁面TargetUrl
			/// </summary>
			[TargetUrl(Extras = new[] { "Category" })]
			[StoredAs("TargetUri", DataType.Text)]
			[PropertySelector(Expression = ".//td[2]/a/@href")]
			public String TargetUri { get; set; }
		}

		/// <summary>
		/// 公司詳情頁面
		/// </summary>
		[Schema("test", "RuthSpider_Product", TableSuffix.Today)]
		public class Company : ISpiderEntity
		{
			/// <summary>
			/// 全部html
			/// </summary>
			[StoredAs("Raw", DataType.Text)]
			[TrimFormater()]
			[PropertySelector(Expression = "*", Type = SelectorType.XPath)]
			public String Raw { get; set; }

			/// <summary>
			/// 公司名稱
			/// </summary>
			[StoredAs("CompanyName", DataType.Text)]
			[TrimFormater()]
			[PropertySelector(Expression = "//span[@class='member_info03']", Option = PropertySelector.ValueOption.PlainText)]
			public String CompanyName { get; set; }

			/// <summary>
			/// 負責人
			/// </summary>
			[StoredAs("Director", DataType.Text)]
			[TrimFormater()]
			[PropertySelector(Expression = "//div[@class='inside02Copy']/table/tr[2]/td[2]/span", Option = PropertySelector.ValueOption.PlainText)]
			public String Director { get; set; }

			/// <summary>
			/// 電話
			/// </summary>
			[StoredAs("Tel", DataType.Text)]
			[TrimFormater()]
			[PropertySelector(Expression = "//div[@class='inside02Copy']/table/tr[3]/td[2]/span", Option = PropertySelector.ValueOption.PlainText)]
			public String Tel { get; set; }

			/// <summary>
			/// 傳真
			/// </summary>
			[StoredAs("Fax", DataType.Text)]
			[TrimFormater()]
			[PropertySelector(Expression = "//div[@class='inside02Copy']/table/tr[4]/td[2]/span", Option = PropertySelector.ValueOption.PlainText)]
			public String Fax { get; set; }

			/// <summary>
			/// 公司網址
			/// </summary>
			[StoredAs("Website", DataType.Text)]
			[TrimFormater()]
			[PropertySelector(Expression = "//div[@class='inside02Copy']/table/tr[5]/td[2]/span", Option = PropertySelector.ValueOption.PlainText)]
			public String Website { get; set; }

			/// <summary>
			/// 產品
			/// </summary>
			[StoredAs("Products", DataType.Text)]
			[TrimFormater()]
			[PropertySelector(Expression = "//div[@class='inside02Copy']/table/tr[6]/td[2]/span", Option = PropertySelector.ValueOption.PlainText)]
			public String Products { get; set; }

			/// <summary>
			/// LOGO圖片
			/// </summary>
			[FormatStringFormater(Format = "http://www.tsia.org.tw/{0}")]
			[StoredAs("Logo", DataType.String, 100)]
			[Download]
			[PropertySelector(Expression = "//div[@class='inside02Copy']/table/tr[1]/td[3]/img/@src")]
			public String Logo { get; set; }

			/// <summary>
			/// 分類
			/// </summary>
			[StoredAs("Category", DataType.String, 100)]
			[PropertySelector(Expression = "Category", Type = SelectorType.Enviroment)]
			public String Category { get; set; }

			/// <summary>
			/// 數據來源
			/// </summary>
			[StoredAs("Uri", DataType.String, 100)]
			[PropertySelector(Expression = "url", Type = SelectorType.Enviroment)]
			public String Uri { get; set; }

		}
	}
}
