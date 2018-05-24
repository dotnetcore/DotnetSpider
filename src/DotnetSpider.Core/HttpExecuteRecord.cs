using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Redial;
using Newtonsoft.Json;
using NLog;
using Polly;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace DotnetSpider.Core
{
    public class HttpExecuteRecord : IExecuteRecord
    {
        private static readonly ILogger Logger = LogCenter.GetLogger();

        public bool Add(string taskId, string name, string identity)
        {
            if (string.IsNullOrEmpty(taskId) || string.IsNullOrWhiteSpace(taskId))
            {
                return true;
            }
            var json = JsonConvert.SerializeObject(new
            {
                TaskId = taskId
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var retryTimesPolicy = Policy.Handle<Exception>().Retry(10, (ex, count) =>
                {
                    Logger.Error($"Try to add execute record failed [{count}]: {ex}");
                    Thread.Sleep(5000);
                });
                retryTimesPolicy.Execute(() =>
                {
                    NetworkCenter.Current.Execute("executeRecord", () =>
                    {
                        var response = HttpSender.Client.PostAsync(Env.HttpIncreaseRunningUrl, content).Result;
                        response.EnsureSuccessStatusCode();
                    });
                });
                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"Add execute record failed: {e}");
                return false;
            }
        }

        public void Remove(string taskId, string name, string identity)
        {
            if (string.IsNullOrEmpty(taskId) || string.IsNullOrWhiteSpace(taskId))
            {
                return;
            }

            var json = JsonConvert.SerializeObject(new
            {
                TaskId = taskId
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var retryTimesPolicy = Policy.Handle<Exception>().Retry(10, (ex, count) =>
                {
                    Logger.Error($"Try to remove execute record failed [{count}]: {ex}");
                    Thread.Sleep(5000);
                });
                retryTimesPolicy.Execute(() =>
                {
                    NetworkCenter.Current.Execute("executeRecord", () =>
                    {
                        var response = HttpSender.Client.PostAsync(Env.HttpReduceRunningUrl, content).Result;
                        response.EnsureSuccessStatusCode();
                    });
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Remove execute record failed: {e}");
            }
        }
    }
}
