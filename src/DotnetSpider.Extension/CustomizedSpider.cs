using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Redial;

namespace DotnetSpider.Extension
{
	public abstract class CustomizedSpider : Spider
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public CustomizedSpider() : this(new Site())
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="site">目标站点信息</param>
		public CustomizedSpider(Site site) : base(site)
		{
		}

		/// <summary>
		/// 自定义的初始化
		/// </summary>
		/// <param name="arguments">运行参数</param>
		protected abstract void MyInit(params string[] arguments);

		/// <summary>
		/// 运行爬虫
		/// </summary>
		/// <param name="arguments">运行参数</param>
		protected override void Execute(params string[] arguments)
		{
			PrintInfo.Print();

			if (arguments.Any(t => t?.ToLower() == SpiderArguments.Report))
			{
				VerifyDataOrGenerateReport(arguments);
			}
			else
			{
				Logger.Information("Init custom component...");

				NetworkCenter.Current.Execute("myInit", () =>
				{
					MyInit(arguments);
				});

				base.Execute(arguments);
			}
		}
	}
}
