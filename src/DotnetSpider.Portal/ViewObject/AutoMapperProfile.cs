using AutoMapper;
using DotnetSpider.AgentCenter.Store;
using DotnetSpider.Portal.Data;

namespace DotnetSpider.Portal.ViewObject
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<AgentInfo, AgentViewObject>()
				.ForMember(x => x.CreationTime, opt =>
					opt.MapFrom(d => d.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")))
				.ForMember(x => x.LastModificationTime, opt =>
					opt.MapFrom(d => d.LastModificationTime.ToString("yyyy-MM-dd HH:mm:ss")));


			CreateMap<AgentHeartbeat, AgentHeartbeatViewObject>()
				.ForMember(x => x.CreationTime, opt =>
					opt.MapFrom(d => d.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")));

			CreateMap<Data.Spider, ListSpiderViewObject>()
				.ForMember(x => x.CreationTime, opt =>
					opt.MapFrom(d => d.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")))
				.ForMember(x => x.LastModificationTime, opt =>
					opt.MapFrom(d => d.LastModificationTime.ToString("yyyy-MM-dd HH:mm:ss")));

			CreateMap<SpiderViewObject, Data.Spider>();

			CreateMap<SpiderHistory, SpiderHistoryViewObject>();
		}
	}
}
