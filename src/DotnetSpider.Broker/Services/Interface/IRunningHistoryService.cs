using DotnetSpider.Common.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	public interface IRunningHistoryService
	{
		Task Add(RunningHistory history);
		Task Update(RunningHistory history);
		Task Delete(string identity);
	}
}
