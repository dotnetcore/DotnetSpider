using System.Configuration;

namespace DotnetSpider.Extension.Infrastructure
{
	/// <summary>
	/// 数据库连接设置的更新接口
	/// </summary>
	public interface IConnectionStringSettingsRefresher
	{
		/// <summary>
		/// 获取新的数据库连接设置
		/// </summary>
		/// <returns>数据库连接设置</returns>
		ConnectionStringSettings GetNew();
	}
}
