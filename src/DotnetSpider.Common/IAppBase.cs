using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotnetSpider.Common
{
	/// <summary>
	/// 标准任务接口
	/// </summary>
	public interface IAppBase : IRunable, IIdentity, ITask, INamed
	{
	}
}
