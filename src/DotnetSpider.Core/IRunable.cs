using System;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	public interface IRunable
	{
		Task RunAsync(params string[] arguments);
		void Run(params string[] arguments);
	}
}
