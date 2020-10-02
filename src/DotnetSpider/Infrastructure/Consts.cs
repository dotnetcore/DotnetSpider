namespace DotnetSpider.Infrastructure
{
	public static class Const
	{
		public const string ProxyPrefix = "DotnetSpider_Proxy_";
		public const string IgnoreSslError = "DotnetSpider_Ignore_SSL_Error";
		public const string DefaultDownloader = "DotnetSpider_Default_Downloader";
		public const string PPPoEPattern = "DotnetSpconider_PPPOE_Pattern";

		public static class Downloader
		{
			public const string HttpClient = "DotnetSpider_HttpClient_Downloader";
			public const string ProxyHttpClient = "DotnetSpider_Proxy_HttpClient_Downloader";
			public const string FakeHttpClient = "DotnetSpider_Fake_HttpClient_Downloader";
			public const string FakeProxyHttpClient = "DotnetSpider_Fake_Proxy_HttpClient_Downloader";

			public const string PPPoEHttpClient = "DotnetSpider_PPPoE_HttpClient_Downloader";
			public const string Puppeteer = "DotnetSpider_Puppeteer_Downloader";
			public const string PPPoEPuppeteer = "DotnetSpider_PPPoE_Puppeteer_Downloader";
			public const string File = "DotnetSpider_File_Downloader";
			public const string Empty = "DotnetSpider_Empty_Downloader";
		}

		public static class Topic
		{
			public const string AgentCenter = "DotnetSpider_Agent_Center";
			public const string Statistics = "DotnetSpider_Statistics_Center";
			public const string Spider = "DotnetSpider_{0}";
		}

		public static class EnvironmentNames
		{
			public const string EntityIndex = "ENTITY_INDEX";
			public const string Guid = "GUID";
			public const string Date = "DATE";
			public const string Today = "TODAY";
			public const string Datetime = "DATETIME";
			public const string Now = "NOW";
			public const string Month = "MONTH";
			public const string Monday = "MONDAY";
			public const string SpiderId = "SPIDER_ID";
			public const string RequestHash = "REQUEST_HASH";
		}
	}
}
