using System;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// 任务的描述
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class Description : Attribute
	{
		/// <summary>
		/// 任务所有者
		/// </summary>
		public string Owner { get; set; }

		/// <summary>
		/// 程序的开发者
		/// </summary>
		public string Developer{ get; set; }

		/// <summary>
		/// 程序的开发时间
		/// </summary>
		public string Date{ get; set; }

		/// <summary>
		/// 任务主题
		/// </summary>
		public string Subject{ get; set; }

		/// <summary>
		/// 联系邮箱
		/// </summary>
		public string Email{ get; set; }
	}
}