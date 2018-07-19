using DotnetSpider.Downloader.Redial;
using System;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 网络中心, 当一台机器中有多个爬虫在跑时, 下载器、数据管道、URL队列都有可能会用到网络, 需要一个网络中心
	/// 统筹网络通讯, 如在拨号前要完成所有的网络请求->停止所有网络相关活动->拨号->执行网络请求
	/// </summary>
	public class NetworkCenter
	{
		private static NetworkCenter _instance = new NetworkCenter();

		/// <summary>
		/// 网络中心单例对象
		/// </summary>
		public static NetworkCenter Current => _instance;

		/// <summary>
		/// 拨号器+网络通讯器
		/// </summary>
		public IRedialExecutor Executor { get; set; }

		private NetworkCenter()
		{
		}

		/// <summary>
		/// 执行网络请求
		/// </summary>
		/// <param name="name">网络请求名称, 仅用于标识作用</param>
		/// <param name="action">网络请求的具体操作</param>
		public void Execute(string name, Action action)
		{
			if (Executor != null)
			{
				Executor.Execute(name, action);
			}
			else
			{
				action();
			}
		}

		/// <summary>
		/// 执行网络请求
		/// </summary>
		/// <param name="name">网络请求名称, 仅用于标识作用</param>
		/// <param name="action">网络请求的具体操作</param>
		/// <param name="obj">网络请求需要的参数对象</param>
		public void Execute(string name, Action<object> action, object obj)
		{
			if (Executor != null)
			{
				Executor.Execute(name, action, obj);
			}
			else
			{
				action(obj);
			}
		}

		/// <summary>
		/// 带返回数据的网络请求
		/// </summary>
		/// <typeparam name="T">返回数据</typeparam>
		/// <param name="name">网络请求名称, 仅用于标识作用</param>
		/// <param name="func">网络请求的具体操作</param>
		/// <returns>返回数据</returns>
		public T Execute<T>(string name, Func<T> func)
		{
			if (Executor != null)
			{
				return Executor.Execute(name, func);
			}
			else
			{
				return func();
			}
		}

		/// <summary>
		/// 带返回数据的网络请求
		/// </summary>
		/// <typeparam name="T">返回数据</typeparam>
		/// <param name="name">网络请求名称, 仅用于标识作用</param>
		/// <param name="func">网络请求的具体操作</param>
		/// <param name="obj">网络请求需要的参数对象</param>
		/// <returns>返回数据</returns>
		public T Execute<T>(string name, Func<object, T> func, object obj)
		{
			if (Executor != null)
			{
				return Executor.Execute(name, func, obj);
			}
			else
			{
				return func(obj);
			}
		}
	}
}
