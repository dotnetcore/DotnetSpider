using System.Web;
using System.Web.Mvc;

namespace Java2Dotnet.Spider.Portal
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}
