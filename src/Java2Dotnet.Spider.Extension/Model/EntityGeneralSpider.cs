using Java2Dotnet.Spider.Core.Processor;
using Java2Dotnet.Spider.Core.Scheduler;

namespace Java2Dotnet.Spider.Extension.Model
{
	public class EntityGeneralSpider : BaseModelSpider
	{
		public EntityGeneralSpider(string identify, string userid, string taskGroup, IPageProcessor pageProcessor, IScheduler scheduler) : base(identify, userid, taskGroup, pageProcessor, scheduler)
		{
		}
	}
}
