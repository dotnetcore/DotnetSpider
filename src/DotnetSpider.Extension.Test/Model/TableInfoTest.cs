using DotnetSpider.Extension.Model;
using System;
using System.Collections.Generic;
using Xunit;

namespace DotnetSpider.Extension.Test.Model
{
	public class TableInfoTest
	{
		[Schema]
		public class DuplicateColumnsTable
		{
			[Column]
			public string A { get; set; }
			[Column("A")]
			public string B { get; set; }
		}

		[Schema]
		public class DuplicateColumnsWithDifferenPropertiesTable
		{
			[Column]
			public string A { get; set; }
			[Column("A", Length = 2)]
			public string B { get; set; }
		}

		[Schema]
		public class DonotUseAllPropertiesTable
		{
			[Column]
			public string A { get; set; }
		}

		[Fact(DisplayName = "TableInfo_DuplicateColumns")]
		public void DuplicateColumns()
		{
			var e = Assert.Throws<ArgumentException>(() => { var _ = new TableInfo(typeof(DuplicateColumnsTable)); });
			Assert.Equal("Column names should not be same", e.Message);
		}

		[Fact(DisplayName = "TableInfo_DuplicateColumnsWithDifferenProperties")]
		public void DuplicateColumnsWithDifferenProperties()
		{
			var e = Assert.Throws<ArgumentException>(() => { var _ = new TableInfo(typeof(DuplicateColumnsWithDifferenPropertiesTable)); });
			Assert.Equal("Column names should not be same", e.Message);
		}

		[Fact(DisplayName = "TableInfo_DonotUseAllProperties")]
		public void DonotUseAllProperties()
		{
			var _ = new TableInfo(typeof(DonotUseAllPropertiesTable));
		}
	}
}
