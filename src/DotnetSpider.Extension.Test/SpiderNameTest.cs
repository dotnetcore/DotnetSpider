using DotnetSpider.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DotnetSpider.Extension.Test
{
	public class SpiderNameTest
	{
		[SpiderName("HelloSpider")]
		public class MySpider1 : EntitySpider
		{
			public MySpider1()
			{
			}

			protected override void MyInit(params string[] arguments)
			{

			}
		}

		public class MySpider2 : Spider
		{
			public MySpider2()
			{
			}
		}

		public class MySpider3 : CommonSpider
		{
			public MySpider3():base("MySpider3_1")
			{
			}

			protected override void MyInit(params string[] arguments)
			{
			}
		}

		public class MySpider4 : EntitySpider
		{
			public MySpider4() : base("MySpider4_1")
			{
			}

			protected override void MyInit(params string[] arguments)
			{
			}
		}

		[Fact]
		public void SetSpiderNameByAttribute()
		{
			MySpider1 spider = new MySpider1();
			Assert.Equal("HelloSpider", spider.Name);
		}

		[Fact]
		public void SetSpiderNameByClassName()
		{
			MySpider2 spider = new MySpider2();
			Assert.Equal("MySpider2", spider.Name);
		}

		[Fact]
		public void SetSpiderNameByConstructor()
		{
			MySpider3 spider = new MySpider3();
			Assert.Equal("MySpider3_1", spider.Name);

			MySpider4 spider2 = new MySpider4();
			Assert.Equal("MySpider4_1", spider2.Name);
		}
	}
}
