using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	public interface IControllable : IRunable, IContiunable, IExitable, IPausable
	{
	}
}
