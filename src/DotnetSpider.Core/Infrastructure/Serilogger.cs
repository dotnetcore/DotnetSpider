using DotnetSpider.Common;

namespace DotnetSpider.Core.Infrastructure
{
	public class Serilogger : ILogger
	{
		private readonly Serilog.ILogger _logger;

		public Serilogger(Serilog.ILogger logger)
		{
			_logger = logger;
		}

		public void Error(string message)
		{
			_logger.Error(message);
		}

		public void Error(string message, object propertyValue1)
		{
			_logger.Error(message, propertyValue1);
		}

		public void Information(string message)
		{
			_logger.Information(message);
		}

		public void Information(string message, object propertyValue1)
		{
			_logger.Information(message, propertyValue1);
		}

		public void Verbose(string message)
		{
			_logger.Verbose(message);
		}

		public void Warning(string message)
		{
			_logger.Warning(message);
		}

		public void Warning(string message, object propertyValue1)
		{
			_logger.Warning(message, propertyValue1);
		}
	}
}
