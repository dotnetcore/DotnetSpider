//using System.Collections.Generic;
//using DotnetSpider.Core.Scheduler;
//using DotnetSpider.Extension.Configuration;
//using DotnetSpider.Extension.Pipeline;
//using System.Linq;
//using System.Threading.Tasks;

//namespace DotnetSpider.Extension
//{
//	public class LinkContextSpider : ModelSpider
//	{
//		public LinkContextSpider(SpiderContext spiderContext) : base(spiderContext)
//		{
//			var nextSpider = spiderContext as LinkSpiderContext;
//			if (nextSpider != null)
//			{
//				NextSpiders = nextSpider.NextSpiderContexts;
//			}
//		}

//		protected override Core.Spider GenerateSpider(IScheduler scheduler)
//		{
//			var spider = base.GenerateSpider(scheduler);

//			foreach (var entity in SpiderContext.Entities)
//			{
//				string name = entity.Name;
//				if (NextSpiders.Keys.Contains(name))
//				{
//					var nextScheduler = NextSpiders[name].Scheduler.GetScheduler();
//					foreach (var prepare in NextSpiders[name].PrepareStartUrls)
//					{
//						var linkSpiderPrepareStartUrls = prepare as LinkSpiderPrepareStartUrls;
//						if (linkSpiderPrepareStartUrls != null)
//						{
//							spider.AddPipeline(new LinkSpiderPipeline(name, nextScheduler, NextSpiders[name].ToDefaultSpider(), linkSpiderPrepareStartUrls));
//						}
//					}
//				}
//			}

//			return spider;
//		}

//		public override void Run(params string[] args)
//		{
//			Task parentTask = Task.Run(() =>
//			{
//				base.Run((args));
//			});
//			List<Task> tasks = NextSpiders.Select(spiderContext => Task.Run(() =>
//			{
//				LinkContextSpider spider = new LinkContextSpider(spiderContext.Value);
//				spider.Run(args);
//			})).ToList();

//			tasks.Add(parentTask);
//			Task.WaitAll(tasks.ToArray());

//		}

//		public Dictionary<string, SpiderContext> NextSpiders { get; }
//	}
//}
