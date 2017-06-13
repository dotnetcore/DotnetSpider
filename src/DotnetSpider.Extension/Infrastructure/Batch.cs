using DotnetSpider.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.Infrastructure
{
	public class Batch
	{
		public static string Now = DateTime.Now.ToString("yyyy_MM_dd_hhmmss");
		public static string Daily = DateTimeUtils.RunIdOfToday;
		public static string Weekly = DateTimeUtils.RunIdOfMonday;
		public static string Monthly = DateTimeUtils.RunIdOfMonthly;
	}
}
