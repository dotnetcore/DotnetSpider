using DotnetSpider.Extension.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.Model
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class Unique : System.Attribute
	{
		public string Name { get; set; }

		public Unique()
		{
		}

		public Unique(string name)
		{
			Name = name;
		}
	}
}
