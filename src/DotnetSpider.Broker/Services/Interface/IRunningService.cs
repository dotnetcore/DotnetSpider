using DotnetSpider.Common.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	public interface IRunningService
	{
		Task Add(Running history);
		Task Delete(string identity);
		Task<List<Running>> GetAll();
		Task<Running> Get(string identity);
		Task<Running> Pop(IDbConnection conn, IDbTransaction transaction, string[] runnings);
	}
}
