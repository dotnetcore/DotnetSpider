namespace DotnetSpider.Core.Test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			EnvironmentTest test = new EnvironmentTest();
			test.DefaultConfig();
			test.InsideConfig();
			test.OutsideConfig();
		}
	}
}
