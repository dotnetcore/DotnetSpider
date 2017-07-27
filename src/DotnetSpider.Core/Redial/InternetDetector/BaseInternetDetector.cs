using System.Threading;

namespace DotnetSpider.Core.Redial.InternetDetector
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

					if (currentWaitTime > 4)
					{
						return false;
					}

					Thread.Sleep(1500);
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
