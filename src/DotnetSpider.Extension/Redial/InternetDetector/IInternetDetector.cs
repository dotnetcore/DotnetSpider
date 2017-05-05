namespace DotnetSpider.Extension.Redial.InternetDetector
{
	public interface IInternetDetector
	{
		int Timeout { get; set; }
		bool Detect();
	}
}
