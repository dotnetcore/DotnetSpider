using Dapper;
using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Core.Scheduler;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DotnetSpider.Extension.Scheduler
{
	public abstract class PagingQueueDuplicateRemovedScheduler : QueueDuplicateRemovedScheduler
	{
		private readonly string _description;
		private readonly bool _reset;

		private readonly string _pagingRecordTableName = Env.DefaultDatabase + ".`paging_record`";
		private readonly string _pagingTableName = Env.DefaultDatabase + ".`paging`";
		private readonly string _pagingRunningTableName = Env.DefaultDatabase + ".`paging_running`";
		private readonly string _taskName;
		private readonly string _identity;
		private int _currentPage;
		private bool _inited;

		protected readonly int Size;

		private readonly RetryPolicy _retryPolicy = Policy.Handle<Exception>().Retry(10000, (ex, count) =>
		{
			Log.Logger.Error($"PushRequests failed [{count}]: {ex}");
		});


		public PagingQueueDuplicateRemovedScheduler(string taskName, string identity, int size, bool reset, string description = null)
		{
			_taskName = taskName;
			_identity = identity;
			Size = size;
			_description = description;
			_reset = reset;
		}

		protected virtual IDbConnection CreateDbConnection()
		{
			return Env.DataConnectionStringSettings.CreateDbConnection();
		}

		protected abstract long GetTotalCount(IDbConnection conn);

		protected abstract IEnumerable<Request> GenerateRequest(IDbConnection conn, int page);

		public override Request Poll()
		{
			var request = base.Poll();

			if (request == null)
			{
				lock (this)
				{
					if (!_inited)
					{
						using (var conn = CreateDbConnection())
						{
							conn.Execute($"create database if not exists {Env.DefaultDatabase}");
							conn.Execute($"create table if not exists {_pagingRecordTableName}(`identity` varchar(50) NOT NULL,`description` varchar(50) DEFAULT NULL, `creation_date` timestamp DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(`identity`))");
							conn.Execute($"create table if not exists {_pagingTableName}(page int(11) NOT null, `task_name` varchar(60) NOT null, `creation_date` timestamp DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(`page`,`task_name`))");
							conn.Execute($"create table if not exists {_pagingRunningTableName}(page int(11) NOT null, `task_name` varchar(60) NOT null, `creation_date` timestamp DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(`page`,`task_name`))");

							var exist = conn.QueryFirst<int>($"SELECT count(*) from {_pagingRecordTableName} where `identity` = @Identity", new { _identity });
							if (exist == 0)
							{
								var affected = conn.Execute($"INSERT INTO {_pagingRecordTableName}(`identity`,`description`) VALUES (@Identity, @Description)", new { _identity, Description = _description });
								if (affected > 0 && _reset)
								{
									conn.Execute($"delete from {_pagingTableName} where `task_name` = @t", new { t = _taskName });
									conn.Execute($"delete from {_pagingRunningTableName} where `task_name` = @t", new { t = _taskName });

									var totalCount = GetTotalCount(conn);
									var pageCount = totalCount / Size + (totalCount % Size > 0 ? 1 : 0);
									var pages = new List<dynamic>();
									for (var page = 1; page <= pageCount; page++)
									{
										pages.Add(new { p = page, t = _taskName });
										if (pages.Count >= 1000 || page >= pageCount)
										{
											conn.Execute($"INSERT INTO {_pagingTableName}(page,`task_name`) values (@p,@t)", pages);
											pages.Clear();
										}
									}
								}
							}
						}

						_inited = true;
					}
				}
				LoadRequests();
			}
			return request;
		}

		private void LoadRequests()
		{
			if (_currentPage > 0)
			{
				Log.Logger.Information($"Paging: {_currentPage}.");
			}

			_retryPolicy.Execute(() =>
			{
				using (var conn = CreateDbConnection())
				{
					if (_currentPage > 0)
					{
						if (conn.Execute($"DELETE FROM {_pagingRunningTableName} where page = @p and `task_name`=@t;", new { p = _currentPage, t = _taskName }) > 0)
						{
							_currentPage = 0;
						}
					}

					//获取分页
					var page = conn.QueryFirstOrDefault<int>($"select page from {_pagingTableName} where `task_name` = @t limit 1", new { t = _taskName });
					if (page > 0)
					{
						var tablePage = new { p = page, t = _taskName };

						var affected = conn.Execute($"DELETE FROM {_pagingTableName} where page = @p and `task_name`=@t;", tablePage);
						if (affected > 0)
						{
							conn.Execute($"INSERT IGNORE INTO {_pagingRunningTableName} (page,`task_name`) values(@p,@t)", tablePage);

							var requests = GenerateRequest(conn, page).ToList();

							if (!requests.Any())
							{
								conn.Execute($"DELETE FROM {_pagingTableName} where page = @p and `task_name`=@t;", tablePage);
							}
							else
							{
								_currentPage = page;

								foreach (var request in requests)
								{
									Push(request, null);
								}
							}
						}
					}
				}
			});
		}
	}
}
