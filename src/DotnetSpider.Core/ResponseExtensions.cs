using DotnetSpider.Common;
using DotnetSpider.Downloader;
using DotnetSpider.Extraction;

namespace DotnetSpider.Core
{
	public static class ResponseExtensions
	{
		public static Selectable Selectable(this Response response, bool removeOutboundLinks = false)
		{
			if (!(response.Content is string))
			{
				throw new SpiderException("Only support text response.");
			}
			response.Delivery = response.Delivery != null && response.Delivery is Selectable ? response.Delivery :
				response.ContentType == ContentType.Json ? new Selectable(response.Content.ToString())
				: new Selectable(response.Content.ToString(), response.Request.Url.ToString(), removeOutboundLinks);
			response.Delivery.Properties = response.Request.Properties;
			return response.Delivery;
		}

		public static Page ToPage(this Response response)
		{
			var page = new Page(response.Request)
			{
				Content = response.Content,
				ContentType = response.ContentType,
				Delivery = response.Delivery,
				StatusCode = response.StatusCode,
				TargetUrl = response.TargetUrl
			};
			return page;
		}
	}
}
