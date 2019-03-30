using System;

namespace DotnetSpider.Downloader.Entity
{
    public class DownloaderAgent
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int ProcessorCount { get; set; }

        public int TotalMemory { get; set; }

        public DateTime LastModificationTime { get; set; }
    }
}