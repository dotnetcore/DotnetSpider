using System;
using System.Collections.Generic;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// Print page model in console
	/// Usually used in test.
	/// </summary>
	public class ConsoleEntityPipeline : BaseEntityPipeline
	{
		public override void AddEntity(EntityDefine metadata)
		{
		}

		public override int Process(string entityName, List<DataObject> datas)
		{
			foreach (var data in datas)
			{
				Console.WriteLine($"{entityName}: {data}");
			}
			return datas.Count;
		}
	}
}
