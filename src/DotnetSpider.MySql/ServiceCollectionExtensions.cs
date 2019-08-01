using DotnetSpider.DataFlow;
using DotnetSpider.DownloadAgentRegisterCenter;
using DotnetSpider.DownloadAgentRegisterCenter.Internal;
using DotnetSpider.Statistics;
using DotnetSpider.Statistics.Store;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider
{
	public static class ServiceCollectionExtensions
	{
		public static DownloadAgentRegisterCenterBuilder UseMySqlDownloaderAgentStore(
			this DownloadAgentRegisterCenterBuilder builder)
		{
			builder.Services.AddSingleton<IDownloaderAgentStore, MySqlDownloaderAgentStore>();
			return builder;
		}

		public static StatisticsBuilder UseMySql(this StatisticsBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));
			builder.Services.AddSingleton<IStatisticsStore, MySqlStatisticsStore>();
			return builder;
		}
	}
}
