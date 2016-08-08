using System;
using System.Collections.Generic;

namespace DotnetSpider.Core
{
	public interface ITask
	{
		string UserId { get; }

		string TaskGroup { get; }
	}
}