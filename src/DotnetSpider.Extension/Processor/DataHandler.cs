namespace DotnetSpider.Extension.Processor
{
	public abstract class DataHandler
	{
		public enum ResultType
		{
			MissTargetUrls,
			Ok
		}

		public abstract ResultType Handle(dynamic data);
	}
}
