using DotnetSpider.Extraction.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.Model
{
	public abstract class BaseEntity : IBaseEntity
	{
		[Column]
		[Primary]
		public int Id { get; set; }
	}
}
