namespace Java2Dotnet.Spider.Core.Pipeline
{
#if NET_CORE
	using Java2Dotnet.Spider.Log;
#endif

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
