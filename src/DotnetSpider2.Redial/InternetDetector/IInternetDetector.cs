namespace DotnetSpider.Redial.InternetDetector
{
	public interface IInternetDetector
	{
		int Timeout { get; set; }
		bool Detect();
	}
}
