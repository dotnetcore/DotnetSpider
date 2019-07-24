using System.Collections.Generic;
using System.Linq;
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

		public Dictionary<string, string> Properties
		{
			get
			{
				var properties = _configuration.GetChildren().Where(x => x.Key.StartsWith("quartz."))
					.ToDictionary(x => x.Key, x => x.Value);
				return properties;
			}
		}
	}
}