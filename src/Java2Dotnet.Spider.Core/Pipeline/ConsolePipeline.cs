namespace Java2Dotnet.Spider.Core.Pipeline
{
#if NET_CORE
	using Java2Dotnet.Spider.JLog;
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
#if NET_CORE
				Log.WriteLine(entry.Key + ":\t" + entry.Value);
#else
				System.Console.WriteLine(entry.Key + ":\t" + entry.Value);
#endif
			}
		}

		public void Dispose()
		{
		}
	}
}
