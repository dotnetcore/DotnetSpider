using DotnetSpider.Common;
using DotnetSpider.Core;
using Xunit;

namespace DotnetSpider.Extension.Test
{
	public class SpiderNameTest
	{
		[TaskName("HelloSpider")]
		public class MySpider1 : EntitySpider
		{
			public MySpider1()
			{
			}

			protected override void OnInit(params string[] arguments)
			{

			}
		}

		public class MySpider2 : Spider
		{
			public MySpider2()
			{
			}
		}

		public class MySpider3 : Spider
		{
			public MySpider3()
			{
				Name = "MySpider3_1";
			}

			protected override void OnInit(params string[] arguments)
			{
			}
		}

		public class MySpider4 : EntitySpider
		{
			public MySpider4()
			{
				Name = "MySpider4_1";
			}

			protected override void OnInit(params string[] arguments)
			{
			}
		}

		[Fact(DisplayName = "SetSpiderNameByAttribute")]
		public void SetSpiderNameByAttribute()
		{
			MySpider1 spider = new MySpider1();
			Assert.Equal("HelloSpider", spider.Name);
		}

		[Fact(DisplayName = "SetSpiderNameByClassName")]
		public void SetSpiderNameByClassName()
		{
			MySpider2 spider = new MySpider2();
			Assert.Equal("MySpider2", spider.Name);
		}

		[Fact(DisplayName = "SetSpiderNameByConstructor")]
		public void SetSpiderNameByConstructor()
		{
			MySpider3 spider = new MySpider3();
			Assert.Equal("MySpider3_1", spider.Name);

			MySpider4 spider2 = new MySpider4();
			Assert.Equal("MySpider4_1", spider2.Name);
		}
	}
}
