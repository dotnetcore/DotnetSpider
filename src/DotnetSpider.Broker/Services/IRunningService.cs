using DotnetSpider.Common.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	public interface IRunningService
	{
		Task Add(Running history);
		Task Delete(string identity);
		Task<List<Running>> GetAll();
		Task<List<Running>> Get(string[] runnings);
	}
}
