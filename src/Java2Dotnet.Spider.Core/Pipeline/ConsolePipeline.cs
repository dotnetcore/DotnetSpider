namespace Java2Dotnet.Spider.Core.Pipeline
{
	/// <summary>
	/// Write results in console.
	/// Usually used in test.
	/// </summary>
	public class ConsolePipeline : IPipeline
	{
		public void Process(ResultItems resultItems, ISpider spider)
		{
			foreach (var entry in resultItems.Results)
			{
				System.Console.WriteLine(entry.Key + ":\t" + entry.Value);
			}
		}

		public void Dispose()
		{
		}
	}
}
