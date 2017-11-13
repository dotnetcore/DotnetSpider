using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	public interface IExecuteRecord
	{
		ISpider Spider { get; }
		bool Add();
		void Remove();
	}
}
