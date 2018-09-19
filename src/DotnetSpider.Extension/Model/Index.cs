using DotnetSpider.Extension.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.Model
{
	[AttributeUsage(AttributeTargets.Property)]
	public class Index : System.Attribute
	{
		public string Name { get; set; }

		public Index()
		{
		}

		public Index(string name)
		{
			Name = name;
		}
	}
}
