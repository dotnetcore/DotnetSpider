using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Cookie 容器接口
	/// </summary>
	public interface IContainCookies
	{
		/// <summary>
		/// Cookie 容器
		/// </summary>
		CookieContainer Container { get; }
	}
}
