using System;

namespace Eastday
{
    class Program
    {
        static void Main(string[] args)
        {
            EastdaySpider.CrawlerPagesTraversal();
            Console.WriteLine("Press any key to continue...");
            Console.Read();
        }
    }
}
