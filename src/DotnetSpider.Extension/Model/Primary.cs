using DotnetSpider.Extension.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.Model
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class Primary : System.Attribute
	{
	}
}
