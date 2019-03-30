using System;
using System.IO;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Downloader;
using DotnetSpider.Network.InternetDetector;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Network
{
    public class NetworkCenter
    {
        private readonly string _sessionsFolder;
        private readonly ILockerFactory _lockerFactory;
        private readonly IDownloaderAgentOptions _options;
        private readonly ILogger _logger;
        private readonly IAdslRedialer _redialer;
        private readonly IInternetDetector _internetDetector;
        private const string RedialLocker = "REDIAL";

        public bool SupportAdsl => _options.SupportAdsl;

        public NetworkCenter(
            IAdslRedialer redialer,
            IInternetDetector internetDetector,
            ILockerFactory lockerFactory,
            IDownloaderAgentOptions options,
            ILogger<NetworkCenter> logger)
        {
            _redialer = redialer;
            _internetDetector = internetDetector;
            _lockerFactory = lockerFactory;
            _options = options;
            _logger = logger;

            _sessionsFolder = Path.Combine(Framework.GlobalDirectory, "sessions");
            if (!Directory.Exists(_sessionsFolder))
            {
                Directory.CreateDirectory(_sessionsFolder);
            }
        }

        public void Redial()
        {
            if (!_options.SupportAdsl)
            {
                throw new SpiderException("下载代理器不支持 ADSL 拨号");
            }

            ILocker locker = null;
            try
            {
                locker = _lockerFactory.GetLocker(RedialLocker);

                var interval = double.MaxValue;
                if (DateTime.TryParse(locker.Information, out DateTime latestRedialTime))
                {
                    interval = (DateTime.Now - latestRedialTime).TotalSeconds;
                }

                if (interval < _options.RedialIntervalLimit * 60)
                {
                    _logger.LogInformation($"在间隔时间内 {_options.RedialIntervalLimit} 已经拨号");
                }
                else
                {
                    WaitForAllSessionsExit();

                    bool success = false;
                    for (int i = 0; i < 10; ++i)
                    {
                        try
                        {
                            if (!_options.IgnoreRedialForTest)
                            {
                                _logger.LogInformation($"尝试拨号 [{i}]");
                                _redialer.Redial();
                            }

                            if (_internetDetector.Detect())
                            {
                                _logger.LogInformation("拨号成功");
                                success = true;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation($"拨号失败 [{i}]: {ex}");
                        }
                    }

                    if (!success)
                    {
                        throw new SpiderException("拨号失败");
                    }
                }
            }
            finally
            {
                locker?.Dispose();
            }
        }

        public void Execute(Action action)
        {
            if (!SupportAdsl)
            {
                action();
            }
            else
            {
                ILocker locker = null;
                ILocker redialLocker = null;
                try
                {
                    redialLocker = _lockerFactory.GetLocker(RedialLocker);
                    locker = _lockerFactory.GetLocker();
                    redialLocker.Dispose();
                    action();
                }
                finally
                {
                    redialLocker?.Dispose();
                    locker?.Dispose();
                }
            }
        }

        public T Execute<T>(Func<T> func)
        {
            if (!SupportAdsl)
            {
                return func();
            }

            ILocker locker = null;
            ILocker redialLocker = null;
            try
            {
                redialLocker = _lockerFactory.GetLocker(RedialLocker);
                locker = _lockerFactory.GetLocker();
                redialLocker.Dispose();
                return func();
            }
            finally
            {
                redialLocker?.Dispose();
                locker?.Dispose();
            }
        }

        private void WaitForAllSessionsExit()
        {
            while (true)
            {
                try
                {
                    var files = Directory.GetFiles(_sessionsFolder);
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    break;
                }
                catch
                {
                    _logger.LogDebug("等待其它下载完成");
                    Thread.Sleep(100);
                }
            }
        }
    }
}