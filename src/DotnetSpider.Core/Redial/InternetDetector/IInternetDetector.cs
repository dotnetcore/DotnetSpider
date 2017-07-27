namespace DotnetSpider.Core.Redial.InternetDetector
{
	public interface IInternetDetector
	{
		int Timeout { get; set; }
		bool Detect();
	}
}
