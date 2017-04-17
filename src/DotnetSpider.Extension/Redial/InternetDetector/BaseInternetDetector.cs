using System.Threading;

namespace DotnetSpider.Extension.Redial.InternetDetector
{
	public abstract class BaseInternetDetector : IInternetDetector
	{
		public int Timeout { get; set; } = 10;

		public bool Detect()
		{
			int currentWaitTime = 0;
			while (currentWaitTime < Timeout)
			{
				currentWaitTime++;
				try
				{
					if (DoValidate())
					{
						return true;
					}
					Thread.Sleep(2000);
				}
				catch
				{
					// ignored
				}
			}
			return false;
		}

		public abstract bool DoValidate();
	}
}
