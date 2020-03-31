using System.IO;

namespace DotnetSpider.Tests
{
    public abstract class TestBase
    {
        protected bool IsCI()
        {
            return Directory.Exists("/home/vsts/work");
        }
    }
}