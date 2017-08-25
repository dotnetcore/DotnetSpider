using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Processor
{
	public class NullPageProcessor : BasePageProcessor
	{
		protected override void Handle(Page page)
		{
			Console.WriteLine("You used a null processor.");
		}
	}
}
