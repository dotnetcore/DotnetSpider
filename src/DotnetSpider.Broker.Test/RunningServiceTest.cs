using DotnetSpider.Broker.Services;
using DotnetSpider.Common.Dto;
using DotnetSpider.Common.Entity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;


namespace DotnetSpider.Broker.Test
{
	public class RunningServiceTest : BaseTest
	{
		public RunningServiceTest()
		{
			var options = new BrokerOptions
			{
				ConnectionString = "Server=.\\SQLEXPRESS;Database=DotnetSpider_Dev;Integrated Security = SSPI;",
				StorageType = StorageType.SqlServer,
				Tokens = new HashSet<string> { "aaa" },
				UseToken = false
			};
			Init(options);
		}

		[Fact(DisplayName = "Add")]
		public async Task<Running> Add()
		{
			var service = Services.GetRequiredService<IRunningService>();
			var id = Guid.NewGuid().ToString("N");

			await service.Add(new Running { Identity = id, Priority = 1, BlockTimes = 1 });
			var running = await service.Get(id);

			Assert.Equal(1, running.BlockTimes);
			Assert.Equal(id, running.Identity);
			Assert.Equal(1, running.Priority);
			return running;
		}

		[Fact(DisplayName = "Delete")]
		public async void Delete()
		{
			var running = await Add();
			var service = Services.GetRequiredService<IRunningService>();
			await service.Delete(running.Identity);

			running = await service.Get(running.Identity);

			Assert.Null(running);
		}

		[Fact(DisplayName = "Pop")]
		public async void Pop()
		{

			var runningService = Services.GetRequiredService<IRunningService>();

			var id1 = Guid.NewGuid().ToString("N");
			var id2 = Guid.NewGuid().ToString("N");
			var id3 = Guid.NewGuid().ToString("N");
			var id4 = Guid.NewGuid().ToString("N");

			await runningService.Add(new Running { Identity = id1, Priority = 1, BlockTimes = 1 });
			await runningService.Add(new Running { Identity = id2, Priority = 1, BlockTimes = 1 });
			await runningService.Add(new Running { Identity = id3, Priority = 1, BlockTimes = 1 });
			await runningService.Add(new Running { Identity = id4, Priority = 1, BlockTimes = 1 });
			using (var conn = CreateDbConnection())
			{
				var t = conn.BeginTransaction();
				var running = await runningService.Pop(conn, t, new string[] { });
				Assert.NotNull(running);
			}
		}
	}
}
