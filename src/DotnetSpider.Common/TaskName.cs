using System;

namespace DotnetSpider.Common
{
	/// <summary>
	/// 任务名称
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class TaskName : Attribute
	{
		/// <summary>
		/// 任务名称
		/// </summary>
		public string Name
		{
			get;
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="name">任务名称</param>
		public TaskName(string name)
		{
			Name = name;
		}
	}
}
