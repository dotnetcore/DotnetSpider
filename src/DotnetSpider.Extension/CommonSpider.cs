using System;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using System.Threading;
using DotnetSpider.Extension.Infrastructure;
using NLog;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Redial;

namespace DotnetSpider.Extension
{
    public abstract class CommonSpider : Spider
    {
        private const string InitFinishedValue = "init complete";
        internal const string InitStatusSetKey = "dotnetspider:init-stats";
        internal string InitLockKey => $"dotnetspider:initLocker:{Identity}";

        protected abstract void MyInit(params string[] arguments);

        public Action DataVerificationAndReport;

        protected CommonSpider(Site site) : base(site)
        {
        }

        public CommonSpider(string name, Site site) : base(site)
        {
            Name = name;
        }

        public CommonSpider(string name) : base(new Site())
        {
            Name = name;
        }

        protected override void Execute(params string[] arguments)
        {
            PrintInfo.Print();

            Logger.AllLog(Identity, "Build custom component...", LogLevel.Info);

            NetworkCenter.Current.Execute("myInit", () =>
            {
                MyInit(arguments);
            });

            if (arguments.Contains("skip"))
            {
                EmptySleepTime = 1000;

                if (Pipelines == null || Pipelines.Count == 0)
                {
                    AddPipeline(new NullPipeline());
                }
                if (PageProcessors == null || PageProcessors.Count == 0)
                {
                    AddPageProcessor(new NullPageProcessor());
                }
            }

            CheckIfSettingsCorrect();

            RegisterControl(this);

            base.Execute(arguments);

            if (IsComplete && DataVerificationAndReport != null)
            {
                NetworkCenter.Current.Execute("verifyAndReport", () =>
                {
                    BaseVerification.ProcessVerifidation(Identity, DataVerificationAndReport);
                });
            }
        }

        protected override void InitScheduler(params string[] arguments)
        {
            base.InitScheduler(arguments);

            if (arguments.Contains("rerun"))
            {
                Scheduler.Clear();
                Scheduler.Dispose();
                BaseVerification.RemoveVerifidationLock(Identity);
            }
        }

        /// <summary>
        /// 分布式任务时使用, 只需要调用一次
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        protected override bool IfRequireInitStartRequests(string[] arguments)
        {
            if (RedisConnection.Default != null)
            {
                if (arguments.Contains("rerun"))
                {
                    RedisConnection.Default.Database.HashDelete(InitStatusSetKey, Identity);
                    RedisConnection.Default.Database.LockRelease(InitLockKey, "0");
                    return true;
                }
                else
                {
                    while (!RedisConnection.Default.Database.LockTake(InitLockKey, "0", TimeSpan.FromMinutes(30)))
                    {
                        Thread.Sleep(1500);
                    }
                    var lockerValue = RedisConnection.Default.Database.HashGet(InitStatusSetKey, Identity);
                    return lockerValue != InitFinishedValue;
                }
            }
            else
            {
                return true;
            }
        }

        protected override void InitStartRequestsFinished()
        {
            if (RedisConnection.Default != null)
            {
                RedisConnection.Default.Database.HashSet(InitStatusSetKey, Identity, InitFinishedValue);
                RedisConnection.Default.Database.LockRelease(InitLockKey, 0);
            }
        }

        protected void RegisterControl(ISpider spider)
        {
            if (RedisConnection.Default != null)
            {
                try
                {
                    RedisConnection.Default.Subscriber.Subscribe($"{spider.Identity}", (c, m) =>
                    {
                        switch (m)
                        {
                            case "PAUSE":
                                {
                                    spider.Pause();
                                    break;
                                }
                            case "CONTINUE":
                                {
                                    spider.Contiune();
                                    break;
                                }
                            case "RUNASYNC":
                                {
                                    spider.RunAsync();
                                    break;
                                }
                            case "EXIT":
                                {
                                    spider.Exit();
                                    break;
                                }
                        }
                    });
                }
                catch (Exception e)
                {
                    Logger.AllLog(Identity, "Register contol failed.", LogLevel.Error, e);
                }
            }
        }

        public ISpider ToDefaultSpider()
        {
            return new DefaultSpider("", new Site());
        }
    }
}
