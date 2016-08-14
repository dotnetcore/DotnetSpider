using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.ORM;
using Newtonsoft.Json;
using DotnetSpider.Redial;
using StackExchange.Redis;
using NLog;
using DotnetSpider.Core.Common;
using System.Net;
using DotnetSpider.Core.Monitor;
using System.Threading;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Extension.Downloader;
using System.Text.RegularExpressions;
#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Extension
{
    public class EntitySpider : Spider
    {
        private const string InitStatusSetName = "init-status";
        private const string ValidateStatusName = "validate-status";

        protected ConnectionMultiplexer Redis;
        protected IDatabase Db;

        [JsonIgnore]
        public Action TaskFinished { get; set; } = () => { };
        [JsonIgnore]
        public Action VerifyCollectedData { get; set; }
        public List<EntityMetadata> Entities { get; internal set; } = new List<EntityMetadata>();
        public RedialExecutor RedialExecutor { get; set; }
        public List<PrepareStartUrls> PrepareStartUrls { get; set; } = new List<PrepareStartUrls>();
        public TargetUrlsHandler TargetUrlsHandler { get; set; }
        public List<TargetUrlExtractor> TargetUrlExtractors { get; set; } = new List<TargetUrlExtractor>();
        public List<GlobalValueSelector> GlobalValues { get; set; } = new List<GlobalValueSelector>();
        public CookieInterceptor CookieInterceptor { get; set; }
        public List<BaseEntityPipeline> EntityPipelines { get; set; } = new List<BaseEntityPipeline>();
        public int CachedSize { get; set; }
        /// <summary>
        /// Key: Url patterns. Value: Until condition generators used by webdriverdownloaders.
        /// </summary>
        public Dictionary<string, MethodInfo> UntilConditionMethods { get; set; } = new Dictionary<string, MethodInfo>();

        public EntitySpider(Site site)
        {
            if (site == null)
            {
                throw new SpiderException("Site should not be null.");
            }
            Site = site;
        }

        public override void Run(params string[] arguments)
        {
            InitEnvoriment();

            try
            {
#if !NET_CORE
				if (CookieInterceptor != null)
				{
					Logger.Log(LogInfo.Create("尝试获取 Cookie...", Logger.Name, this, LogLevel.Info));
					var cookie = CookieInterceptor.GetCookie();
					if (string.IsNullOrEmpty(cookie.Item1))
					{
						Logger.Log(LogInfo.Create("获取 Cookie 失败, 爬虫无法继续.", Logger.Name, this, LogLevel.Error));
						return;
					}
					else
					{
						Site.Cookie = cookie.Item1;
                        Site.Cookies = cookie.Item2;
					}
				}
#endif

                Logger.Log(LogInfo.Create("创建爬虫...", Logger.Name, this, LogLevel.Info));

                EntityProcessor processor = new EntityProcessor(this);
                foreach (var t in TargetUrlExtractors)
                {
                    processor.AddTargetUrlExtractor(t);
                }
                foreach (var entity in Entities)
                {
                    processor.AddEntity(entity);
                }
                if (TargetUrlsHandler != null)
                {
                    processor.TargetUrlsHandler = TargetUrlsHandler;
                }
                PageProcessor = processor;
                foreach (var entity in Entities)
                {
                    string entiyName = entity.Entity.Name;

                    foreach (var pipeline in EntityPipelines)
                    {
                        pipeline.InitiEntity(entity);
                    }

                    Pipelines.Add(new EntityPipeline(entiyName, EntityPipelines));
                }

                CheckIfSettingsCorrect();

                bool needInitStartRequest = true;
                string key = "locker-" + Identity;
                if (Db != null)
                {
                    while (!Db.LockTake(key, "0", TimeSpan.FromMinutes(10)))
                    {
                        Thread.Sleep(1000);
                    }
                    var lockerValue = Db.HashGet(InitStatusSetName, Identity);
                    needInitStartRequest = lockerValue != "init finished";
                }

                if (arguments.Contains("rerun"))
                {
                    Scheduler.Clear();
                    needInitStartRequest = true;
                }

                Logger.Log(LogInfo.Create("构建内部模块、准备爬虫数据...", Logger.Name, this, LogLevel.Info));
                if (needInitStartRequest)
                {
                    if (PrepareStartUrls != null)
                    {
                        foreach (var prepareStartUrl in PrepareStartUrls)
                        {
                            prepareStartUrl.Build(this, null);
                        }
                    }
                }
                InitComponent();
                SpiderMonitor.Default.Register(this);

                Db?.LockRelease(key, 0);

                RegisterControl(this);

                base.Run();

                TaskFinished();

                HandleVerifyCollectData();
            }
            finally
            {
                Dispose();
                SpiderMonitor.Default.Dispose();
            }
        }

        public EntitySpider AddEntityType(Type type)
        {
            CheckIfRunning();

            if (typeof(ISpiderEntity).IsAssignableFrom(type))
            {
                Entities.Add(ParseMyEntityMetaData(type.GetTypeInfoCrossPlatform()));
                GlobalValues = type.GetTypeInfo().GetCustomAttributes<GlobalValueSelector>().Select(e => new GlobalValueSelector
                {
                    Name = e.Name,
                    Expression = e.Expression,
                    Type = e.Type
                }).ToList();
            }
            else
            {
                throw new SpiderException($"Type: {type.FullName} is not a ISpiderEntity.");
            }

            return this;
        }

        public EntitySpider AddTargetUrlExtractor(TargetUrlExtractor targetUrlExtractor)
        {
            CheckIfRunning();
            TargetUrlExtractors.Add(targetUrlExtractor);
            return this;
        }

        public EntitySpider AddEntityPipeline(BaseEntityPipeline pipeline)
        {
            CheckIfRunning();
            EntityPipelines.Add(pipeline);
            return this;
        }

        public void SetCachedSize(int count)
        {
            CheckIfRunning();
            foreach (var pipeline in Pipelines)
            {
                ((CachedPipeline)pipeline).CachedSize = count;
            }
        }

        public override Spider AddPipeline(IPipeline pipeline)
        {
            throw new SpiderException("EntitySpider only support AddEntityPipeline.");
        }

        public override Spider AddPipelines(IList<IPipeline> pipelines)
        {
            throw new SpiderException("EntitySpider only support AddEntityPipeline.");
        }

        public ISpider ToDefaultSpider()
        {
            return new DefaultSpider("", new Site());
        }

        private static string GetEntityName(Type type)
        {
            return type.FullName;
        }

        private void HandleVerifyCollectData()
        {
            if (VerifyCollectedData == null)
            {
                return;
            }
            string key = "locker-validate-" + Identity;

            try
            {
                bool needInitStartRequest = true;
                if (Redis != null)
                {
                    while (!Db.LockTake(key, "0", TimeSpan.FromMinutes(10)))
                    {
                        Thread.Sleep(1000);
                    }

                    var lockerValue = Db.HashGet(ValidateStatusName, Identity);
                    needInitStartRequest = lockerValue != "verify finished";
                }
                if (needInitStartRequest)
                {
                    Logger.Log(LogInfo.Create("开始执行数据验证...", Logger.Name, this, LogLevel.Info));

                    VerifyCollectedData();
                }

                Logger.Log(LogInfo.Create("数据验证已完成.", Logger.Name, this, LogLevel.Info));

                if (needInitStartRequest && Redis != null)
                {
                    Db.HashSet(ValidateStatusName, Identity, "verify finished");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
            finally
            {
                if (Redis != null)
                {
                    Db.LockRelease(key, 0);
                }
            }
        }

        private void RegisterControl(Spider spider)
        {
            if (Redis != null)
            {
                try
                {
                    Redis.GetSubscriber().Subscribe($"{spider.Identity}", (c, m) =>
                    {
                        switch (m)
                        {
                            case "STOP":
                                {
                                    spider.Stop();
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
                catch
                {
                    // ignored
                }
            }
        }

        private void InitEnvoriment()
        {
            if (RedialExecutor != null)
            {
                NetworkCenter.Current.Executor = RedialExecutor;
            }

            if (!string.IsNullOrEmpty(ConfigurationManager.Get("redisHost")) && string.IsNullOrWhiteSpace(ConfigurationManager.Get("redisHost")))
            {
                var host = ConfigurationManager.Get("redisHost");

                var confiruation = new ConfigurationOptions()
                {
                    ServiceName = "DotnetSpider",
                    Password = ConfigurationManager.Get("redisPassword"),
                    ConnectTimeout = 65530,
                    KeepAlive = 8,
                    ConnectRetry = 20,
                    SyncTimeout = 65530,
                    ResponseTimeout = 65530
                };
#if NET_CORE
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Lewis: This is a Workaround for .NET CORE can't use EndPoint to create Socket.
                    var address = Dns.GetHostAddressesAsync(host).Result.FirstOrDefault();
                    if (address == null)
                    {
                        throw new SpiderException("Can't resovle your host: " + host);
                    }
                    confiruation.EndPoints.Add(new IPEndPoint(address, 6379));
                }
                else
                {
                    confiruation.EndPoints.Add(new DnsEndPoint(host, 6379));
                }
#else
				confiruation.EndPoints.Add(new DnsEndPoint(host, 6379));
#endif
                Redis = ConnectionMultiplexer.Connect(confiruation);
                Db = Redis.GetDatabase(1);
            }
        }

        /// <summary>
        /// Instance method, use "this" as context.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public EntityMetadata ParseMyEntityMetaData(
#if !NET_CORE
            Type entityType
#else
            TypeInfo entityType
#endif
        )
        {
            return paserEntityMetaData(entityType, this);
        }

        public static EntityMetadata PaserEntityMetaData(
#if !NET_CORE
            Type entityType
#else
            TypeInfo entityType
#endif

        )
        {
            return paserEntityMetaData(entityType, null);
        }

        private static EntityMetadata paserEntityMetaData(
#if !NET_CORE
            Type entityType, 
#else
            TypeInfo entityType,
#endif
            EntitySpider es)
        {

            EntityMetadata entityMetadata = new EntityMetadata();

            entityMetadata.Schema = entityType.GetCustomAttribute<Schema>();
            var indexes = entityType.GetCustomAttribute<Indexes>();
            if (indexes != null)
            {
                entityMetadata.Indexes = indexes.Index?.Select(i => i.Split(',')).ToList();
                entityMetadata.Uniques = indexes.Unique?.Select(i => i.Split(',')).ToList();
                entityMetadata.Primary = indexes.Primary?.Split(',');
                entityMetadata.AutoIncrement = indexes.AutoIncrement;
            }
            var updates = entityType.GetCustomAttribute<UpdateColumns>();
            if (updates != null)
            {
                entityMetadata.Updates = updates.Columns;
            }

            var targetUrls = entityType.GetCustomAttributes<TargetUrl>();

            var conditionFunc = entityType.GetMethod("UntilCondition", BindingFlags.Static | BindingFlags.Public);
            //If a class definition have TargetUrl and UntilCondition, then add it to es.UntilConditionMethods, TargetUrl as the key.
            if (targetUrls != null)
            {
                entityMetadata.TargetUrls = new string[targetUrls.Count()];
                int i = 0;
                foreach (var att in targetUrls)
                {
                    var url = att.UrlPattern;
                    url = att.KeepOrigin ? att.UrlPattern : string.Format("({0})", url.Replace(".", "\\.").Replace("*", "[^\"'#]*"));
                    entityMetadata.TargetUrls[i] = url;
                    if (conditionFunc != null && es != null)
                    {
                        es.UntilConditionMethods.Add(url, conditionFunc);
                    }
                    i++;
                }
            }
            //if a class definition have only UntilCondition, then add it to es.UntilConditionMethods with string.Empty as the key.
            else if (conditionFunc != null && es != null)
            {
                if (!es.UntilConditionMethods.ContainsKey(string.Empty))
                {
                    es.UntilConditionMethods.Add(string.Empty, conditionFunc);
                }
            }

            Entity entity = ParseEntity(entityType);
            entityMetadata.Entity = entity;
            EntitySelector extractByAttribute = entityType.GetCustomAttribute<EntitySelector>();
            if (extractByAttribute != null)
            {
                entityMetadata.Entity.Selector = new BaseSelector { Expression = extractByAttribute.Expression, Type = extractByAttribute.Type };
            }

            return entityMetadata;
        }

        private static Entity ParseEntity(
#if !NET_CORE
            Type entityType
#else
            TypeInfo entityType
#endif
        )
        {
            Entity entity = new Entity
            {
                Name = GetEntityName(entityType.GetTypeCrossPlatform())
            };
            var properties = entityType.GetProperties();
            foreach (var propertyInfo in properties)
            {
                var type = propertyInfo.PropertyType;

                if (typeof(ISpiderEntity).IsAssignableFrom(type) || typeof(List<ISpiderEntity>).IsAssignableFrom(type))
                {
                    Entity token = new Entity();
                    if (typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        token.Multi = true;
                    }
                    else
                    {
                        token.Multi = false;
                    }
                    token.Name = GetEntityName(propertyInfo.PropertyType);
                    EntitySelector extractByAttribute = entityType.GetCustomAttribute<EntitySelector>();
                    if (extractByAttribute != null)
                    {
                        token.Selector = new BaseSelector { Expression = extractByAttribute.Expression, Type = extractByAttribute.Type };
                    }
                    var extractBy = propertyInfo.GetCustomAttribute<PropertySelector>();
                    if (extractBy != null)
                    {
                        token.Selector = new BaseSelector()
                        {
                            Expression = extractBy.Expression,
                            Type = extractBy.Type,
                            Argument = extractBy.Argument
                        };
                    }

                    token.Fields.Add(ParseEntity(propertyInfo.PropertyType.GetTypeInfoCrossPlatform()));
                }
                else
                {
                    Field token = new Field();

                    var extractBy = propertyInfo.GetCustomAttribute<PropertySelector>();
                    var storeAs = propertyInfo.GetCustomAttribute<StoredAs>();

                    if (typeof(IList).IsAssignableFrom(type))
                    {
                        token.Multi = true;
                    }
                    else
                    {
                        token.Multi = false;
                    }

                    if (extractBy != null)
                    {
                        token.Option = extractBy.Option;
                        token.Selector = new BaseSelector()
                        {
                            Expression = extractBy.Expression,
                            Type = extractBy.Type,
                            Argument = extractBy.Argument
                        };
                        token.Pattern = extractBy.Pattern;
                        token.ReplaceString = extractBy.ReplaceString;

                    }

                    if (storeAs != null)
                    {
                        token.Name = storeAs.Name;
                        token.DataType = ParseDataType(storeAs);
                    }
                    else
                    {
                        token.Name = propertyInfo.Name;
                    }

                    foreach (var formatter in propertyInfo.GetCustomAttributes<Formatter>(true))
                    {
                        token.Formatters.Add(formatter);
                    }

                    TargetUrl toAddUrl = propertyInfo.GetCustomAttribute<TargetUrl>();
                    if (toAddUrl != null)
                    {
                        var urlExtra = new TargetUrlExtractor();
                        urlExtra.Region = token.Selector;
                        if (!string.IsNullOrEmpty(toAddUrl.UrlPattern))
                        {
                            urlExtra.Patterns.Add(toAddUrl.UrlPattern);
                        }
                        entity.UrlExtras.Add(urlExtra, toAddUrl.OtherPropertiesAsExtras?.ToList());
                    }
                    else
                        entity.Fields.Add(token);
                }
            }
            return entity;
        }

        public MethodInfo GetConditionMethodByUrl(string url)
        {
            foreach (var item in UntilConditionMethods)
            {
                if (Regex.IsMatch(url, item.Key))
                {
                    return item.Value;
                }
            }
            return null;
        }
        private static string ParseDataType(StoredAs storedAs)
        {
            string reslut = "";

            switch (storedAs.Type)
            {
                case DataType.Bool:
                    {
                        reslut = "BOOL";
                        break;
                    }
                case DataType.Date:
                    {
                        reslut = "DATE";
                        break;
                    }
                case DataType.Time:
                    {
                        reslut = "TIME";
                        break;
                    }
                case DataType.Text:
                    {
                        reslut = "TEXT";
                        break;
                    }

                case DataType.String:
                    {
                        reslut = $"STRING,{storedAs.Lenth}";
                        break;
                    }
            }

            return reslut;
        }
    }
}
