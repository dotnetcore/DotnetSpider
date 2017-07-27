using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Linq;

namespace DotnetSpider.Extension.Test
{
	[TestClass]
	public class CustomSpiderTest
	{
		public class CustomSpider1 : CustomSpider
		{
			public CustomSpider1() : base("CustomSpiderTEST")
			{
				Identity = Guid.NewGuid().ToString();
			}

			protected override void ImplementAction(params string[] arguments)
			{
				Console.WriteLine("hello");
			}
		}

		[TestMethod]
		public void LogTest()
		{
			CustomSpider1 spider = new CustomSpider1();
			spider.Run();

			using (MySqlConnection conn = new MySqlConnection(Core.Infrastructure.Config.ConnectString))
			{
				var c1 = conn.Query<CountResult>($"SELECT COUNT(*) as Count FROM dotnetspider.log where identity='{spider.Identity}'").First().Count;
				var c2 = conn.Query<CountResult>($"SELECT COUNT(*) as Count FROM dotnetspider.status where identity='{spider.Identity}'").First().Count;
				Assert.AreEqual(2, c1);
				Assert.AreEqual(1, c2);
			}
		}
	}
}
