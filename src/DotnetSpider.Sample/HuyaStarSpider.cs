using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Scheduler;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DotnetSpider.Sample
{
	[TaskName("HuyaStar")]
	public class HuyaStarSpider : EntitySpider
	{
		protected override void MyInit(params string[] arguments)
		{
			Scheduler = new MyScheduler(arguments.Contains("reset"));
			Downloader.AddBeforeDownloadHandler(new HuyaStarBeforeDownloadHandler());
			AddEntityType<HuyaStarInfo>();
		}
	}

	class MyScheduler : PagingQueueDuplicateRemovedScheduler
	{
		public MyScheduler(bool reset) : base(100, reset, "虎牙直播贡献值采集")
		{
		}

		protected override IEnumerable<Request> GenerateRequest(IDbConnection conn, int page)
		{
			var requests = new List<Request>();
			var rooms = conn.Query($"SELECT * FROM yy.new_huya_cateroom_2018_06_01 where runid='2018-06-22' order by id desc limit @start,@length",
				new { start = (page - 1) * _size, length = _size });

			foreach (var room in rooms)
			{
				var dic = room as IDictionary<string, object>;
				var uid = dic["uid"];
				var privateHost = dic["privatehost"];

				var request = new Request
				{
					Url = $"http://www.huya.com/cache5min.php?m=WeekRank&do=getItemsByPid&pid={uid}",
					Extras = new Dictionary<string, dynamic>()
					{
						{ "refer", privateHost},
						{ "uid", uid}
					}
				};
				requests.Add(request);
			}
			return requests;
		}

		protected override long GetTotalCount(IDbConnection conn)
		{
			return conn.QuerySingle<long>($"SELECT COUNT(*) FROM yy.new_huya_cateroom_2018_06_01 WHERE runid='2018-06-22';");
		}
	}

	internal class HuyaStarBeforeDownloadHandler : IBeforeDownloadHandler
	{
		public void Handle(ref Request request, IDownloader downloader, ISpider spider)
		{
			string refer = request.GetExtra("refer");
			request.Referer = $"http://www.huya.com/{refer}";
		}
	}

	[TableInfo("yy", "new_huya_room_star_test", TableNamePostfix.FirstDayOfTheMonth)]
	[EntitySelector(Expression = "$.data.vWeekRankItem[*]", Type = SelectorType.JsonPath)]
	internal class HuyaStarInfo : BaseEntity
	{
		[Field(Expression = "$.lUid", Length = 100, Type = SelectorType.JsonPath)]
		public string LUid { get; set; }

		[Field(Expression = "uid", Length = 100, Type = SelectorType.Enviroment)]
		public string Uid { get; set; }

		[Field(Expression = "$.sNickName", Length = 100, Type = SelectorType.JsonPath)]
		public string SNickName { get; set; }

		[Field(Expression = "$.iScore", Length = 100, Type = SelectorType.JsonPath)]
		public string IScore { get; set; }

		[Field(Expression = "$.iGuardLevel", Length = 100, Type = SelectorType.JsonPath)]
		public string IGuardLevel { get; set; }

		[Field(Expression = "$.iNobleLevel", Length = 100, Type = SelectorType.JsonPath)]
		public string INobleLevel { get; set; }

		[Field(Expression = "$.sLogo", Length = 100, Type = SelectorType.JsonPath)]
		public string SLogo { get; set; }

		[Field(Expression = "$.iUserLevel", Length = 100, Type = SelectorType.JsonPath)]
		public string IUserLevel { get; set; }

		[Field(Expression = "today", Type = SelectorType.Enviroment)]
		public DateTime RunId { get; set; }
	}
}
