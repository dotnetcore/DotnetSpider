using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Portal
{
	public class QuartzOptions
	{
		private readonly IConfiguration _configuration;

		public QuartzOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public Dictionary<string,string> Properties => _configuration.GetSection("Quartz").Get<Dictionary<string,string>>();
	}
}