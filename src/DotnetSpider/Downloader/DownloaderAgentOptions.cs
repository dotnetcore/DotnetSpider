using System;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Downloader
{
    public interface IDownloaderAgentOptions
    {
        bool SupportAdsl { get; }

        bool IgnoreRedialForTest { get; }

        int RedialIntervalLimit { get; }

        string AgentId { get; }

        string Name { get; }

        string AdslInterface { get; }

        string AdslAccount { get; }

        string AdslPassword { get; }

        string ProxySupplyUrl { get; }
    }

    public class DownloaderAgentOptions : IDownloaderAgentOptions
    {
        private readonly IConfiguration _configuration;
        private readonly string _defaultAgentId = Guid.NewGuid().ToString("N");

        public DownloaderAgentOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool SupportAdsl => !string.IsNullOrWhiteSpace(AdslAccount);

        public bool IgnoreRedialForTest => !string.IsNullOrWhiteSpace(_configuration["IgnoreRedialForTest"]) &&
                                           bool.Parse(_configuration["IgnoreRedialForTest"]);

        public int RedialIntervalLimit => string.IsNullOrWhiteSpace(_configuration["RedialIntervalLimit"])
            ? 2
            : int.Parse(_configuration["RedialIntervalLimit"]);

        public string AgentId => string.IsNullOrWhiteSpace(_configuration["AgentId"])
            ? _defaultAgentId
            : _configuration["AgentId"];

        public string Name => string.IsNullOrWhiteSpace(_configuration["AgentName"])
            ? "DownloadAgent"
            : _configuration["AgentName"];

        public string AdslInterface => _configuration["AdslInterface"];

        public string AdslAccount => _configuration["AdslAccount"];

        public string AdslPassword => _configuration["AdslPassword"];

        public string ProxySupplyUrl => _configuration["ProxySupplyUrl"];
    }
}