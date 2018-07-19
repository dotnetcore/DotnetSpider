namespace DotnetSpider.Common
{
	public interface ILogger
	{
		void Information(string message);
		void Information(string message, object propertyValue1);
		void Warning(string message);
		void Warning(string message, object propertyValue1);
		void Error(string message);
		void Error(string message, object propertyValue1);
		void Verbose(string message);
	}
}
