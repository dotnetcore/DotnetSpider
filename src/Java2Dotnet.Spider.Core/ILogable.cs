using Java2Dotnet.Spider.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Core
{
	public interface ILogable
	{
		ILogService Logger { get; }
	}
}
