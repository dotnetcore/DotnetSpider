namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// Write results in console.
	/// Usually used in test.
	/// </summary>
	public class ConsolePipeline : BasePipeline
	{
		public override void Process(ResultItems resultItems)
		{
			foreach (var entry in resultItems.Results)
			{
				System.Console.WriteLine(entry.Key + ":\t" + entry.Value);
			}
		}
	}
}
