using System.Reflection;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	public abstract class AppBase : INamed, IRunable, IIdentity, ITask
	{
		public string Identity { get; set; }

		public string Name { get; set; }

		public string TaskId { get; set; }

		protected AppBase()
		{
			var type = GetType();
			var nameAttribute = type.GetCustomAttribute<TaskName>();
			if (nameAttribute != null)
			{
				Name = nameAttribute.Name;
			}
			else
			{
				Name = type.Name;
			}
		}

		protected AppBase(string name) : this()
		{
			Name = name;
		}

		public Task RunAsync(params string[] arguments)
		{
			return Task.Factory.StartNew(() =>
			{
				Run(arguments);
			});
		}

		public abstract void Run(params string[] arguments);
	}
}
