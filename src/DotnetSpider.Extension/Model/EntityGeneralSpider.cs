using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;

namespace DotnetSpider.Extension.Model
{
	public class EntityGeneralSpider : BaseModelSpider
	{
		public EntityGeneralSpider(Site site, string identify, string userid, string taskGroup, IPageProcessor pageProcessor, IScheduler scheduler) : base(site, identify, userid, taskGroup, pageProcessor, scheduler)
		{
		}
	}
}
