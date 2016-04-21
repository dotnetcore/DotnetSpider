using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Java2Dotnet.Spider.Common
{
	public static class EmailUtil2
	{
		public static void Send(string subject, string emailto, string body)
		{
			try
			{
				string emailApiHost = ConfigurationManager.Get("emailApiHost");
				byte[] bytes = Encoding.UTF8.GetBytes(body);
				StringBuilder builder = new StringBuilder();
				foreach (var b in bytes)
				{
					builder.Append(b).Append("|");
				}
				Dictionary<string, string> data = new Dictionary<string, string>
				{
					{"EmailTo", emailto},
					{"Body", builder.ToString()},
					{"Subject", subject}
				};

				HttpClient client = new HttpClient();
				var result = client.PostAsync(emailApiHost, new FormUrlEncodedContent(data)).Result;
			}
			catch (Exception e)
			{
			}
		}
	}
}