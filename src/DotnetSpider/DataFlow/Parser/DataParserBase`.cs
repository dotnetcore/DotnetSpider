using System.Threading.Tasks;
using DotnetSpider.DataFlow.Storage.Model;

namespace DotnetSpider.DataFlow.Parser
{
	public abstract class DataParserBase<T> : DataParserBase where T : EntityBase<T>, new()
	{
		public override string Name => $"DataParser<{typeof(T).Name}>";

		protected readonly Model<T> Model;
		protected readonly TableMetadata TableMetadata;

		/// <summary>
		/// 构造方法
		/// </summary>
		protected DataParserBase()
		{
			Model = new Model<T>();
			TableMetadata = new T().GetTableMetadata();
		}

		protected override  Task<DataFlowResult> Parse(DataFlowContext context)
		{
			if (!context.Contains(Model.TypeName))
			{
				context.Add(Model.TypeName, TableMetadata);
			}
			return Task.FromResult(DataFlowResult.Success);
		}

		protected virtual void AddParseResult(DataFlowContext context, ParseResult<T> result)
		{
			if (result.Count > 0)
			{
				var items = context.GetParseData(Model.TypeName);
				if (items == null)
				{
					context.AddParseData(Model.TypeName, result);
				}
				else
				{
					((ParseResult<T>) items).AddRange(result);
				}
			}
		}
	}
}