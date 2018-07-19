using DotnetSpider.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotnetSpider.Extension.Model
{
	/// <summary>
	/// 对解析的结果进一步加工操作
	/// </summary>
	public interface IDataHandler
	{
		void Handle(ref dynamic data, Page page);
	}
}
