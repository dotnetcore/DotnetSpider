//using System.Collections.Generic;
//using DotnetSpider.Core;
//using DotnetSpider.Core.Pipeline;
//using DotnetSpider.Core.Scheduler;
//using Newtonsoft.Json.Linq;
//using DotnetSpider.Extension.Model;

//namespace DotnetSpider.Extension.Pipeline
//{
//	public class LinkSpiderPipeline : CachedPipeline
//	{
//		public IScheduler NextSpiderScheduler { get; }
//		public ISpider NextSpider { get; }
//		private readonly LinkSpiderPrepareStartUrls _prepareStartUrls;
//		private readonly string _entityName;

//		public LinkSpiderPipeline(string entityName, IScheduler nextSpiderScheduler, ISpider nextSpider, LinkSpiderPrepareStartUrls prepareStartUrls)
//		{
//			NextSpiderScheduler = nextSpiderScheduler;
//			NextSpider = nextSpider;
//			_prepareStartUrls = prepareStartUrls;
//			_entityName = entityName;
//		}

//		private void Process(List<JObject> datas)
//		{
//			_prepareStartUrls.Build((Spider)Spider, datas);

//			foreach (var startRequest in Spider.Site.StartRequests)
//			{
//				NextSpiderScheduler.Push(startRequest);
//			}
//		}

//		protected override void Process(List<ResultItems> resultItemsList)
//		{
//			if (resultItemsList == null || resultItemsList.Count == 0)
//			{
//				return;
//			}

//			List<JObject> list = new List<JObject>();
//			foreach (var resultItems in resultItemsList)
//			{
//				dynamic data = resultItems.GetResultItem(_entityName);

//				if (data != null)
//				{
//					if (data is JObject)
//					{
//						list.Add(data);
//					}
//					else
//					{
//						list.AddRange(data);
//					}
//				}
//			}
//			Process(list);
//		}
//	}
//}
