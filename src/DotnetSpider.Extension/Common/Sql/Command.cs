using System.Collections.Generic;

namespace DotnetSpider.Extension.Common.Sql
{
	/// <summary>
	/// 表示 SQL 命令的类。其中包含 SQL 语句和参数两个部分。参数的值要和 SQL 语句中的问号一一对应。 
	/// </summary>
	public class Command
	{
		private string _statement;
		private List<string> _params;
		private List<string> _paramTypes;

		/// <summary>
		/// 缺省构造函数
		/// </summary>
		public Command()
		{
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="statement">SQL 语句</param>
		/// <param name="params">参数</param>
		public Command(string statement, List<string> @params)
		{
			_statement = statement;
			_params = @params;
		}

		public Command(string statement, List<string> @params, List<string> paramTypes)
		{
			_statement = statement;
			_params = @params;
			_paramTypes = paramTypes;
		}

		public List<string> GetParamTypes()
		{
			return _paramTypes;
		}

		public void SetParamTypes(List<string> paramTypes)
		{
			_paramTypes = paramTypes;
		}

		/// <summary>
		/// 获得 SQL 语句
		/// </summary>
		/// <returns>SQL 语句</returns>
		public string GetStatement()
		{
			return _statement;
		}

		/// <summary>
		/// 设置 SQL 语句
		/// </summary>
		/// <param name="statement">SQL 语句</param>
		public void SetStatement(string statement)
		{
			_statement = statement;
		}

		/// <summary>
		/// 获得参数
		/// </summary>
		/// <returns>参数</returns>
		public List<string> GetParams()
		{
			return _params;
		}

		/// <summary>
		/// 设置参数
		/// </summary>
		/// <param name="params">参数</param>
		public void SetParams(List<string> @params)
		{
			_params = @params;
		}

		public override string ToString()
		{
			return "Command{" +
					"statement='" + _statement + '\'' +
					", params=" + _params +
					'}';
		}
	}
}

