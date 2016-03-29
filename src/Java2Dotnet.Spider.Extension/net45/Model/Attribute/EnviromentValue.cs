using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EnviromentExtractBy : BaseExtractBy
	{
		public string Name { get; set; }
	}
}
