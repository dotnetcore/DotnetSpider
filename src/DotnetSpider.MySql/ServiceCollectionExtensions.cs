using DotnetSpider.DataFlow;
using DotnetSpider.DownloadAgentRegisterCenter;
using DotnetSpider.MySql.DownloadAgentRegisterCenter.Store;
using DotnetSpider.Statistics;
using DotnetSpider.Statistics.Store;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.MySql
{
	public static class ServiceCollectionExtensions
	{
		public static DownloadAgentRegisterCenterBuilder UseMySql(
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
