using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider;
using DotnetSpider.Common;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Parser.Attribute;
using DotnetSpider.DataFlow.Storage.Model;
using DotnetSpider.Downloader;
using DotnetSpider.EventBus;
using DotnetSpider.Selector;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Ocsp;

namespace Pamirs.Spiders.Speiyou
{
	public class CourseSpider : Spider
	{
		public CourseSpider(IEventBus eventBus, IStatisticsService statisticsService, SpiderOptions options,
			ILogger<Spider> logger, IServiceProvider services) : base(eventBus, statisticsService, options, logger,
			services)
		{
		}

		protected override void Initialize()
		{
			var token = Environment.GetEnvironmentVariable("DOTNET_SPIDER_SPEIYOU_TOKEN");
			if (string.IsNullOrWhiteSpace(token))
			{
				var msg = "未配置学而思 Token";
				Logger.LogError(msg);
				throw new SpiderException(msg);
			}

			Speed = 1;

			var grades = new List<Grade>
			{
				new Grade
				{
					GradeId = "-6",
					CityId = "010"
				}
			};
			foreach (var grade in grades)
			{
				var body =
					"{\"timeType\":\"0\",\"levelIds\":\"\",\"gradeId\":\"" + grade.GradeId +
					"\",\"groups\":\"time,address,teacher,tutor\",\"subjectIds\":\"\",\"token\":\"" + token +
					"\",\"claCourseType\":\"0\",\"isHiddenFull\":\"0\",\"term\":\"\"}";
				AddRequests(new Request
				{
					Url =
						$"https://xesapi.speiyou.cn/v1/py/course/list?v=6.12.1&client_type=2&distinct_id=ba8b1dbae5b645a43bf30e2b64a8f035&token={token}&lat=31.247221&lng=121.492479",
					Method = "POST",
					Body = body,
					Headers = new Dictionary<string, string>
					{
						{"X-Tingyun-Id", "zAj7Vu-0QAI;c=2;r=1739976003;"},
						{"Authorization", $"TouristBearer {token}"},
						{"Accept-Language", "zh-CN"},
						{"gradeId", grade.GradeId},
						{"area", grade.CityId},
						{"v", "6.12.1"},
						{"devid", "ffffffff-d11c-bcbc-5c73-513d2afd1e08"},
						{"sv", "5.1.1"},
						{"client_type", "2"},
						{"Accept-Encoding", "gzip"},
						{"Content-Type", "application/json; charset=UTF-8"},
						{"User-Agent", "okhttp/3.6.0"}
					},
					Properties = new Dictionary<string, string>
					{
						{"city_id", grade.CityId},
						{"grade_id", grade.GradeId}
					}
				});
			}

			AddDataFlow(new CourseParser());
		}

		class CourseParser : DataParser<Course>
		{
			public CourseParser()
			{
				FollowRequestQuerier = context =>
				{
					var selectable = context.GetSelectable();
					var total = int.Parse(selectable.Select(Selectors.JsonPath("$.data.queryCount")).GetValue());
					var pageCount = total / 10 + total % 10 == 1 ? 1 : 0;
					var list = new List<Request>();
					if (total > 10)
					{
						for (int i = 1; i < pageCount; ++i)
						{
							var request = CreateFromRequest(context.Response.Request, context.Response.Request.Url);
							request.Body = request.Body.Replace("\"page\":\"1\"", "\"page\":\"" + (i + 1) + "\"");
							list.Add(request);
						}
					}

					return list;
				};
			}

			public override Task<DataFlowResult> HandleAsync(DataFlowContext context)
			{
				var a = base.HandleAsync(context);
				return a;
			}
		}

		[Schema("speiyou", "course")]
		[EntitySelector(Expression = "$.data.queryData[*]", Type = SelectorType.JsonPath)]
		class Course : EntityBase<Course>
		{
			protected override void Configure()
			{
				HasIndex(x => new {x.ClassCourseId, x.CityId, x.GradeId, x.ClassId, x.BatchId}, true);
				HasIndex(x => x.BatchId);
			}

			[StringLength(100)]
			[ValueSelector(Expression = "city_id", Type = SelectorType.Enviroment)]
			public string CityId { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "grade_id", Type = SelectorType.Enviroment)]
			public string GradeId { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_id", Type = SelectorType.JsonPath)]
			public string ClassId { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_feetype", Type = SelectorType.JsonPath)]
			public string ClassFeeType { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_end_date", Type = SelectorType.JsonPath)]
			public string ClassEndDate { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "clc_regist_count", Type = SelectorType.JsonPath)]
			public string CourseRegistCount { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$cla_course_id", Type = SelectorType.JsonPath)]
			public string ClassCourseId { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.d_name", Type = SelectorType.JsonPath)]
			public string DName { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_tutor_real_name", Type = SelectorType.JsonPath)]
			public string ClassTutorRealName { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_max_persons", Type = SelectorType.JsonPath)]
			public string ClassMaxPersons { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.tea_teacher_name", Type = SelectorType.JsonPath)]
			public string TeacherName { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_classtime_names", Type = SelectorType.JsonPath)]
			public string ClassTimeNames { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_start_date", Type = SelectorType.JsonPath)]
			public string ClassStartDate { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_class_type", Type = SelectorType.JsonPath)]
			public string ClassType { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_classroom_name", Type = SelectorType.JsonPath)]
			public string ClassRoomName { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_venue_name", Type = SelectorType.JsonPath)]
			public string ClassVenueName { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_area_name", Type = SelectorType.JsonPath)]
			public string ClassAreaName { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_price", Type = SelectorType.JsonPath)]
			public string ClassPrice { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.dept_name", Type = SelectorType.JsonPath)]
			public string DeptName { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_surplus_persons", Type = SelectorType.JsonPath)]
			public string ClassSurplusPersons { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_quota_num", Type = SelectorType.JsonPath)]
			public string ClassQuotaNum { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.class_regist_timestate", Type = SelectorType.JsonPath)]
			public string ClassRegistTimeState { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$class_course_num", Type = SelectorType.JsonPath)]
			public string ClassCourseNum { get; set; }

			[StringLength(100)]
			[ValueSelector(Expression = "$.cla_biz_type", Type = SelectorType.JsonPath)]
			public string ClassBizType { get; set; }

			[ValueSelector(Expression = "NOW", Type = SelectorType.Enviroment)]
			public DateTime CreationTime { get; set; }

			[ValueSelector(Expression = "TODAY", Type = SelectorType.Enviroment)]
			public DateTime BatchId { get; set; }
		}

		class Grade
		{
			public string CityId { get; set; }

			public string GradeId { get; set; }
		}
	}
}