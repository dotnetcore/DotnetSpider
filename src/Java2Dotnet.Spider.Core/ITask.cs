using System;
using System.Collections.Generic;

namespace Java2Dotnet.Spider.Core
{
	public interface ITask
	{
		string UserId { get; }

		string TaskGroup { get; }
	}
}