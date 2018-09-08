using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	public interface IWorkerService
	{
		Task<int> AddWorkerAsync(string fullClassName, string connectionId);
		Task RemoveWorkerAsync(string fullClassName, string connectionId);
	}
}
