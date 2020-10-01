// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net.Http;
// using System.Threading;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Http;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
//
// namespace DotnetSpider.Proxy
// {
// 	internal class HttpClientFactory : IHttpClientFactory, IHttpMessageHandlerFactory
// 	{
// 		private static readonly TimerCallback _cleanupCallback =
// 			(TimerCallback)(s => ((HttpClientFactory)s).CleanupTimer_Tick());
//
// 		private readonly ILogger _logger;
// 		private readonly IServiceProvider _services;
// 		private readonly IServiceScopeFactory _scopeFactory;
// 		private readonly IOptionsMonitor<HttpClientFactoryOptions> _optionsMonitor;
// 		private readonly IHttpMessageHandlerBuilderFilter[] _filters;
// 		private readonly Func<string, Lazy<ActiveHandlerTrackingEntry>> _entryFactory;
// 		private readonly TimeSpan DefaultCleanupInterval = TimeSpan.FromSeconds(10.0);
// 		private Timer _cleanupTimer;
// 		private readonly object _cleanupTimerLock;
// 		private readonly object _cleanupActiveLock;
// 		internal readonly ConcurrentDictionary<string, Lazy<ActiveHandlerTrackingEntry>> _activeHandlers;
// 		internal readonly ConcurrentQueue<ExpiredHandlerTrackingEntry> _expiredHandlers;
// 		private readonly TimerCallback _expiryCallback;
//
// 		public HttpClientFactory(
// 			IServiceProvider services,
// 			IServiceScopeFactory scopeFactory,
// 			ILoggerFactory loggerFactory,
// 			IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor,
// 			IEnumerable<IHttpMessageHandlerBuilderFilter> filters)
// 		{
// 			if (services == null)
// 				throw new ArgumentNullException(nameof(services));
// 			if (scopeFactory == null)
// 				throw new ArgumentNullException(nameof(scopeFactory));
// 			if (loggerFactory == null)
// 				throw new ArgumentNullException(nameof(loggerFactory));
// 			if (optionsMonitor == null)
// 				throw new ArgumentNullException(nameof(optionsMonitor));
// 			if (filters == null)
// 				throw new ArgumentNullException(nameof(filters));
// 			this._services = services;
// 			this._scopeFactory = scopeFactory;
// 			this._optionsMonitor = optionsMonitor;
// 			this._filters = filters.ToArray<IHttpMessageHandlerBuilderFilter>();
// 			this._logger = (ILogger)loggerFactory.CreateLogger<HttpClientFactory>();
// 			this._activeHandlers =
// 				new ConcurrentDictionary<string, Lazy<ActiveHandlerTrackingEntry>>(
// 					(IEqualityComparer<string>)StringComparer.Ordinal);
// 			this._entryFactory = (Func<string, Lazy<ActiveHandlerTrackingEntry>>)(name =>
// 				new Lazy<ActiveHandlerTrackingEntry>(
// 					(Func<ActiveHandlerTrackingEntry>)(() => this.CreateHandlerEntry(name)),
// 					LazyThreadSafetyMode.ExecutionAndPublication));
// 			this._expiredHandlers = new ConcurrentQueue<ExpiredHandlerTrackingEntry>();
// 			this._expiryCallback = new TimerCallback(this.ExpiryTimer_Tick);
// 			this._cleanupTimerLock = new object();
// 			this._cleanupActiveLock = new object();
// 		}
//
// 		public HttpClient CreateClient(string name)
// 		{
// 			HttpClient httpClient = name != null
// 				? new HttpClient(this.CreateHandler(name), false)
// 				: throw new ArgumentNullException(nameof(name));
// 			HttpClientFactoryOptions clientFactoryOptions = this._optionsMonitor.Get(name);
// 			for (int index = 0; index < clientFactoryOptions.HttpClientActions.Count; ++index)
// 				clientFactoryOptions.HttpClientActions[index](httpClient);
// 			return httpClient;
// 		}
//
// 		public HttpMessageHandler CreateHandler(string name)
// 		{
// 			if (name == null)
// 				throw new ArgumentNullException(nameof(name));
// 			ActiveHandlerTrackingEntry entry = this._activeHandlers.GetOrAdd(name, this._entryFactory).Value;
// 			this.StartHandlerEntryTimer(entry);
// 			return (HttpMessageHandler)entry.Handler;
// 		}
//
// 		internal ActiveHandlerTrackingEntry CreateHandlerEntry(string name)
// 		{
// 			IServiceProvider provider = this._services;
// 			IServiceScope scope = (IServiceScope)null;
// 			HttpClientFactoryOptions options = this._optionsMonitor.Get(name);
// 			if (!options.SuppressHandlerScope)
// 			{
// 				scope = this._scopeFactory.CreateScope();
// 				provider = scope.ServiceProvider;
// 			}
//
// 			try
// 			{
// 				HttpMessageHandlerBuilder requiredService = provider.GetRequiredService<HttpMessageHandlerBuilder>();
// 				requiredService.Name = name;
// 				Action<HttpMessageHandlerBuilder> next = new Action<HttpMessageHandlerBuilder>(Configure);
// 				for (int index = this._filters.Length - 1; index >= 0; --index)
// 					next = this._filters[index].Configure(next);
// 				next(requiredService);
// 				LifetimeTrackingHttpMessageHandler handler =
// 					new LifetimeTrackingHttpMessageHandler(requiredService.Build());
// 				return new ActiveHandlerTrackingEntry(name, handler, scope, options.HandlerLifetime);
// 			}
// 			catch
// 			{
// 				scope?.Dispose();
// 				throw;
// 			}
//
// 			void Configure(HttpMessageHandlerBuilder b)
// 			{
// 				for (int index = 0; index < options.HttpMessageHandlerBuilderActions.Count; ++index)
// 					options.HttpMessageHandlerBuilderActions[index](b);
// 			}
// 		}
//
// 		internal void ExpiryTimer_Tick(object state)
// 		{
// 			ActiveHandlerTrackingEntry other = (ActiveHandlerTrackingEntry)state;
// 			this._activeHandlers.TryRemove(other.Name, out Lazy<ActiveHandlerTrackingEntry> _);
// 			this._expiredHandlers.Enqueue(new ExpiredHandlerTrackingEntry(other));
// 			HttpClientFactory.Log.HandlerExpired(this._logger, other.Name, other.Lifetime);
// 			this.StartCleanupTimer();
// 		}
//
// 		internal virtual void StartHandlerEntryTimer(ActiveHandlerTrackingEntry entry) =>
// 			entry.StartExpiryTimer(this._expiryCallback);
//
// 		internal virtual void StartCleanupTimer()
// 		{
// 			lock (this._cleanupTimerLock)
// 			{
// 				if (this._cleanupTimer != null)
// 					return;
// 				this._cleanupTimer = NonCapturingTimer.Create(HttpClientFactory._cleanupCallback, (object)this,
// 					this.DefaultCleanupInterval, Timeout.InfiniteTimeSpan);
// 			}
// 		}
//
// 		internal virtual void StopCleanupTimer()
// 		{
// 			lock (this._cleanupTimerLock)
// 			{
// 				this._cleanupTimer.Dispose();
// 				this._cleanupTimer = (Timer)null;
// 			}
// 		}
//
// 		internal void CleanupTimer_Tick()
// 		{
// 			this.StopCleanupTimer();
// 			if (!Monitor.TryEnter(this._cleanupActiveLock))
// 			{
// 				this.StartCleanupTimer();
// 			}
// 			else
// 			{
// 				try
// 				{
// 					int count = this._expiredHandlers.Count;
// 					HttpClientFactory.Log.CleanupCycleStart(this._logger, count);
// 					ValueStopwatch valueStopwatch = ValueStopwatch.StartNew();
// 					int disposedCount = 0;
// 					for (int index = 0; index < count; ++index)
// 					{
// 						ExpiredHandlerTrackingEntry result;
// 						this._expiredHandlers.TryDequeue(out result);
// 						if (result.CanDispose)
// 						{
// 							try
// 							{
// 								result.InnerHandler.Dispose();
// 								result.Scope?.Dispose();
// 								++disposedCount;
// 							}
// 							catch (Exception ex)
// 							{
// 								HttpClientFactory.Log.CleanupItemFailed(this._logger, result.Name, ex);
// 							}
// 						}
// 						else
// 							this._expiredHandlers.Enqueue(result);
// 					}
//
// 					HttpClientFactory.Log.CleanupCycleEnd(this._logger, valueStopwatch.GetElapsedTime(),
// 						disposedCount, this._expiredHandlers.Count);
// 				}
// 				finally
// 				{
// 					Monitor.Exit(this._cleanupActiveLock);
// 				}
//
// 				if (this._expiredHandlers.Count <= 0)
// 					return;
// 				this.StartCleanupTimer();
// 			}
// 		}
//
// 		private static class Log
// 		{
// 			private static readonly Action<ILogger, int, Exception> _cleanupCycleStart =
// 				LoggerMessage.Define<int>(LogLevel.Debug, HttpClientFactory.Log.EventIds.CleanupCycleStart,
// 					"Starting HttpMessageHandler cleanup cycle with {InitialCount} items");
//
// 			private static readonly Action<ILogger, double, int, int, Exception> _cleanupCycleEnd =
// 				LoggerMessage.Define<double, int, int>(LogLevel.Debug,
// 					HttpClientFactory.Log.EventIds.CleanupCycleEnd,
// 					"Ending HttpMessageHandler cleanup cycle after {ElapsedMilliseconds}ms - processed: {DisposedCount} items - remaining: {RemainingItems} items");
//
// 			private static readonly Action<ILogger, string, Exception> _cleanupItemFailed =
// 				LoggerMessage.Define<string>(LogLevel.Error, HttpClientFactory.Log.EventIds.CleanupItemFailed,
// 					"HttpMessageHandler.Dispose() threw and unhandled exception for client: '{ClientName}'");
//
// 			private static readonly Action<ILogger, double, string, Exception> _handlerExpired =
// 				LoggerMessage.Define<double, string>(LogLevel.Debug,
// 					HttpClientFactory.Log.EventIds.HandlerExpired,
// 					"HttpMessageHandler expired after {HandlerLifetime}ms for client '{ClientName}'");
//
// 			public static void CleanupCycleStart(ILogger logger, int initialCount) =>
// 				HttpClientFactory.Log._cleanupCycleStart(logger, initialCount, (Exception)null);
//
// 			public static void CleanupCycleEnd(
// 				ILogger logger,
// 				TimeSpan duration,
// 				int disposedCount,
// 				int finalCount)
// 			{
// 				HttpClientFactory.Log._cleanupCycleEnd(logger, duration.TotalMilliseconds, disposedCount,
// 					finalCount, (Exception)null);
// 			}
//
// 			public static void CleanupItemFailed(ILogger logger, string clientName, Exception exception) =>
// 				HttpClientFactory.Log._cleanupItemFailed(logger, clientName, exception);
//
// 			public static void HandlerExpired(ILogger logger, string clientName, TimeSpan lifetime) =>
// 				HttpClientFactory.Log._handlerExpired(logger, lifetime.TotalMilliseconds, clientName,
// 					(Exception)null);
//
// 			public static class EventIds
// 			{
// 				public static readonly EventId CleanupCycleStart = new EventId(100, nameof(CleanupCycleStart));
// 				public static readonly EventId CleanupCycleEnd = new EventId(101, nameof(CleanupCycleEnd));
// 				public static readonly EventId CleanupItemFailed = new EventId(102, nameof(CleanupItemFailed));
// 				public static readonly EventId HandlerExpired = new EventId(103, nameof(HandlerExpired));
// 			}
// 		}
// 	}
// }
