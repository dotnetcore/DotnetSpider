using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Configuration.Json;
using Newtonsoft.Json;

namespace Java2Dotnet.Spider.ScriptsConsole
{
	public class Program
	{
		private static string TaskJson = "{\"Entities\":[{\"Multi\":true,\"Selector\":{\"Type\":0,\"Expression\":\"//li[@class='gl-item']/div[contains(@class,'j-sku-item')]\"},\"Schema\":{\"Database\":\"spider_node_test\",\"TableName\":\"HOST_NAME\",\"Suffix\":1,\"TypeId\":\"Java2Dotnet.Spider.Extension.ORM.Schema,Java2Dotnet.Spider.Extension,Version=1.0.0.0,Culture=neutral,PublicKeyToken=null\"},\"Identity\":\"Java2Dotnet.Spider.Test.Example.JdSkuSampleSpider+Product\",\"Indexes\":[[\"category\"]],\"Uniques\":[[\"category\",\"sku\"],[\"sku\"]],\"AutoIncrement\":null,\"Primary\":null,\"Fields\":[{\"DataType\":\"string(20)\",\"Selector\":{\"Type\":4,\"Expression\":\"name\"},\"Multi\":false,\"Name\":\"category\",\"Formatters\":[]},{\"DataType\":\"string(20)\",\"Selector\":{\"Type\":4,\"Expression\":\"cat3\"},\"Multi\":false,\"Name\":\"cat3\",\"Formatters\":[]},{\"DataType\":\"text\",\"Selector\":{\"Type\":0,\"Expression\":\"./div[1]/a/@href\"},\"Multi\":false,\"Name\":\"url\",\"Formatters\":[]},{\"DataType\":\"string(25)\",\"Selector\":{\"Type\":0,\"Expression\":\"./@data-sku\"},\"Multi\":false,\"Name\":\"sku\",\"Formatters\":[]},{\"DataType\":\"string(32)\",\"Selector\":{\"Type\":0,\"Expression\":\"./div[5]/strong/a\"},\"Multi\":false,\"Name\":\"commentscount\",\"Formatters\":[]},{\"DataType\":\"string(100)\",\"Selector\":{\"Type\":0,\"Expression\":\".//div[@class='p-shop']/@data-shop_name\"},\"Multi\":false,\"Name\":\"shopname\",\"Formatters\":[]},{\"DataType\":\"string(50)\",\"Selector\":{\"Type\":0,\"Expression\":\".//div[@class='p-name']/a/em\"},\"Multi\":false,\"Name\":\"name\",\"Formatters\":[]},{\"DataType\":\"string(25)\",\"Selector\":{\"Type\":0,\"Expression\":\"./@venderid\"},\"Multi\":false,\"Name\":\"venderid\",\"Formatters\":[]},{\"DataType\":\"string(25)\",\"Selector\":{\"Type\":0,\"Expression\":\"./@jdzy_shop_id\"},\"Multi\":false,\"Name\":\"jdzy_shop_id\",\"Formatters\":[]},{\"DataType\":\"date\",\"Selector\":{\"Type\":4,\"Expression\":\"Monday\"},\"Multi\":false,\"Name\":\"run_id\",\"Formatters\":[]},{\"DataType\":\"time\",\"Selector\":{\"Type\":4,\"Expression\":\"Now\"},\"Multi\":false,\"Name\":\"cdate\",\"Formatters\":[]}],\"Stopping\":null}],\"SpiderName\":\"USER_ID JDsku/storetest{MACROS_TODAY}\",\"ThreadNum\":1,\"Deep\":2147483647,\"EmptySleepTime\":15000,\"CachedSize\":1,\"Scheduler\":{\"Host\":\"redis_primary\",\"Port\":6379,\"Password\":\"#frAiI^MtFxh3Ks&swrnVyzAtRTq%w\",\"Type\":1},\"Downloader\":null,\"Site\":{\"Headers\":{},\"ContentType\":0,\"UserAgent\":null,\"Accept\":null,\"Domain\":\"list.jd.com\",\"EncodingName\":\"UTF-8\",\"Encoding\":null,\"Timeout\":5000,\"AcceptStatCode\":[200],\"StartRequests\":[{\"Depth\":1,\"NextDepth\":2,\"Referer\":null,\"Origin\":null,\"Priority\":0,\"Extras\":{\"name\":\"手机\",\"cat3\":\"655\"},\"Method\":null,\"PostBody\":null,\"Url\":\"http://list.jd.com/list.html?cat=9987,653,655&page=1&ext=57050::1943^^&go=0&JL=6_0_0\",\"Identity\":\"eacb7577e2b1d7ec60caffce1a9cb83\"},{\"Depth\":1,\"NextDepth\":2,\"Referer\":null,\"Origin\":null,\"Priority\":0,\"Extras\":{\"name\":\"手机\",\"cat3\":\"655\"},\"Method\":null,\"PostBody\":null,\"Url\":\"http://list.jd.com/list.html?cat=9987,653,655&page=2&ext=57050::1943^^&go=0&JL=6_0_0\",\"Identity\":\"25e974b5ba736be361e33b345ccbec1\"},{\"Depth\":1,\"NextDepth\":2,\"Referer\":null,\"Origin\":null,\"Priority\":0,\"Extras\":{\"name\":\"手机\",\"cat3\":\"655\"},\"Method\":null,\"PostBody\":null,\"Url\":\"http://list.jd.com/list.html?cat=9987,653,655&page=3&ext=57050::1943^^&go=0&JL=6_0_0\",\"Identity\":\"4f17dc46e8571ef4ae15a441af4ad44\"}],\"SleepTime\":500,\"RetryTimes\":5,\"CycleRetryTimes\":20,\"Cookie\":null,\"HttpProxy\":null,\"IsUseGzip\":false,\"HttpProxyPoolEnable\":false},\"NetworkValidater\":null,\"Redialer\":null,\"PrepareStartUrls\":null,\"StartUrls\":{\"http://list.jd.com/list.html?cat=9987,653,655&page=1&ext=57050::1943^^&go=0&JL=6_0_0\":{\"name\":\"手机\",\"cat3\":\"655\"},\"http://list.jd.com/list.html?cat=9987,653,655&page=2&ext=57050::1943^^&go=0&JL=6_0_0\":{\"name\":\"手机\",\"cat3\":\"655\"},\"http://list.jd.com/list.html?cat=9987,653,655&page=3&ext=57050::1943^^&go=0&JL=6_0_0\":{\"name\":\"手机\",\"cat3\":\"655\"}},\"Pipeline\":{\"Type\":1,\"ConnectString\":\"Database='mysql';DataSource=office.86research.cn;UserID=root;Password=1qazZAQ!;Port=3306\"},\"Corporation\":null,\"ValidationReportTo\":null,\"CustomizePage\":null,\"CustomizeTargetUrls\":null,\"EnviromentValues\":[]}";

		public static void Main(string[] args)
		{
			string testUserId = Guid.NewGuid().ToString("N");
			Core.Spider.PrintInfo();
			string hostName = Dns.GetHostName();
			Console.WriteLine($"HostName: {hostName} UserId: {testUserId} Time: {DateTime.Now}");
			Console.WriteLine($"Start SpiderNode: {hostName} ...");
			SpiderNode node = new SpiderNode();
			node.Run();
			Console.WriteLine($"Start SpiderNode: {hostName} finished.");

			// Test
			TaskManager manager = new TaskManager();
			manager.AddTestTask(testUserId, TaskJson.Replace("USER_ID", testUserId).Replace("HOST_NAME",hostName));
			manager.TriggerTask(hostName, testUserId, TaskManager.TestTaskId);

			while (true)
			{
			   Thread.Sleep(1000);
			}
		}
	}
}
