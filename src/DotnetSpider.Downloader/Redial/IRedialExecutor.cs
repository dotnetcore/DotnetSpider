using System;

namespace DotnetSpider.Downloader.Redial
{
	/// <summary>
	/// 拨号器+网络通讯器
	/// </summary>
	public interface IRedialExecutor : IDisposable
	{
		/// <summary>
		/// 执行拨号
		/// </summary>
		/// <param name="action">执行完拨号后回调方法</param>
		/// <returns>拨号结果</returns>
		RedialResult Redial(Action<object> action = null);

		/// <summary>
		/// 执行网络请求
		/// </summary>
		/// <param name="name">网络请求名称, 仅用于标识作用</param>
		/// <param name="action">网络请求的具体操作</param>
		void Execute(string name, Action action);

		/// <summary>
		/// 执行网络请求
		/// </summary>
		/// <param name="name">网络请求名称, 仅用于标识作用</param>
		/// <param name="action">网络请求的具体操作</param>
		/// <param name="obj">网络请求需要的参数对象</param>
		void Execute(string name, Action<object> action, object obj);

		/// <summary>
		/// 带返回数据的网络请求
		/// </summary>
		/// <typeparam name="T">返回数据</typeparam>
		/// <param name="name">网络请求名称, 仅用于标识作用</param>
		/// <param name="func">网络请求的具体操作</param>
		/// <param name="obj">网络请求需要的参数对象</param>
		/// <returns>返回数据</returns>
		T Execute<T>(string name, Func<object, T> func, object obj);

		/// <summary>
		/// 带返回数据的网络请求
		/// </summary>
		/// <typeparam name="T">返回数据</typeparam>
		/// <param name="name">网络请求名称, 仅用于标识作用</param>
		/// <param name="func">网络请求的具体操作</param>
		/// <returns>返回数据</returns>
		T Execute<T>(string name, Func<T> func);
	}
}
