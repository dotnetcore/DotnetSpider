using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Downloader;
using DotnetSpider.Extensions;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.MessageQueue;
using DotnetSpider.Statistic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetSpider.Agent;

public class AgentHostService : BackgroundService
{
    private readonly List<AsyncMessageConsumer<byte[]>> _consumers = new();
    private readonly IOptions<AgentOptions> _options;
    private readonly ILogger _logger;
    private readonly IMessageQueue _messageQueue;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IStatisticService _statisticService;
    private readonly IServiceProvider _serviceProvider;

    public AgentHostService(IMessageQueue messageQueue,
        IOptions<AgentOptions> options,
        IHostApplicationLifetime applicationLifetime,
        ILoggerFactory loggerFactory,
        IStatisticService statisticService, IServiceProvider serviceProvider)
    {
        _messageQueue = messageQueue;
        _applicationLifetime = applicationLifetime;
        _statisticService = statisticService;
        _options = options;
        _logger = loggerFactory.CreateLogger(GetType());
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_messageQueue.IsDistributed)
        {
            _logger.LogDebug("Agent {AgentId}, {AgentName} is starting", _options.Value.AgentId,
                _options.Value.AgentName);
        }
        else
        {
            _logger.LogDebug("Agent is starting");
        }

        await _statisticService.RegisterAgentAsync(_options.Value.AgentId, _options.Value.AgentName);

        // 分布式才需要注册
        if (_messageQueue.IsDistributed)
        {
            await _messageQueue.PublishAsBytesAsync(Topics.AgentCenter,
                new Messages.Agent.Register
                {
                    AgentId = _options.Value.AgentId,
                    AgentName = _options.Value.AgentName,
                    Memory = MachineInfo.Current.Memory,
                    ProcessorCount = Environment.ProcessorCount
                });
        }

        // 同类型下载器注册于相同的 topic， 用于负载均衡
        await RegisterAgentAsync("Agent");

        if (_messageQueue.IsDistributed)
        {
            // 注册 agent_{id} 用于固定节点下载
            await RegisterAgentAsync(string.Format(Topics.Spider, _options.Value.AgentId));

            // 分布式才需要发送心跳
            Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await HeartbeatAsync();
                    await Task.Delay(5000, stoppingToken);
                }
            }, stoppingToken).ConfigureAwait(false).GetAwaiter();
        }

        if (_messageQueue.IsDistributed)
        {
            _logger.LogDebug("Agent {AgentId}, {AgentName} started", _options.Value.AgentId,
                _options.Value.AgentName);
        }
        else
        {
            _logger.LogDebug("Agent started");
        }
    }

    private async Task RegisterAgentAsync(string topic)
    {
        var consumer = new AsyncMessageConsumer<byte[]>(topic);
        consumer.Received += HandleMessageAsync;
        await _messageQueue.ConsumeAsync(consumer);
        _consumers.Add(consumer);
    }

    private async Task HandleMessageAsync(byte[] bytes)
    {
        object message;
        try
        {
            message = await bytes.DeserializeAsync();
            if (message == null)
            {
                return;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Deserialize message failed");
            return;
        }

        switch (message)
        {
            case Messages.Agent.Exit exit:
            {
                if (exit.AgentId == _options.Value.AgentId)
                {
                    _applicationLifetime.StopApplication();
                }

                break;
            }
            case Request request:
                Task.Run(async () =>
                {
                    var downloader = _serviceProvider.GetKeyedService<IDownloader>(request.Downloader);
                    var response = await downloader.DownloadAsync(request);
                    if (response == null)
                    {
                        return;
                    }

                    response.Agent = _options.Value.AgentId;

                    var topic = string.Format(Topics.Spider, request.Owner);
                    await _messageQueue.PublishAsBytesAsync(topic, response);

                    if (_messageQueue.IsDistributed)
                    {
                        _logger.LogInformation(
                            "Agent {AgentId} - {AgentName}, spider {Owner} download {RequestUri}, {Hash} completed",
                            _options.Value.AgentId, _options.Value.AgentName,
                            request.Owner, request.RequestUri, request.Hash);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Spider {Owner} download {RequestUri}, {Hash} completed",
                            request.Owner, request.RequestUri, request.Hash);
                    }
                }).ConfigureAwait(false).GetAwaiter();
                break;
            default:
            {
                var msg = JsonSerializer.Serialize(message);
                _logger.LogWarning("Message not supported: {Message}", msg);
                break;
            }
        }
    }

    private async Task HeartbeatAsync()
    {
        _logger.LogDebug("I am alive {AgentId}, {AgentName}", _options.Value.AgentId, _options.Value.AgentName);

        await _messageQueue.PublishAsBytesAsync(Topics.AgentCenter,
            new Messages.Agent.Heartbeat
            {
                AgentId = _options.Value.AgentId,
                AgentName = _options.Value.AgentName,
                AvailableMemory = MachineInfo.Current.AvailableMemory,
                CpuLoad = 0
            });
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_messageQueue.IsDistributed)
        {
            _logger.LogInformation("Agent {AgentId} is stopping", _options.Value.AgentId);
        }
        else
        {
            _logger.LogInformation("Agent is stopping");
        }

        foreach (var consumer in _consumers)
        {
            consumer.Close();
        }

        await base.StopAsync(cancellationToken);

        if (_messageQueue.IsDistributed)
        {
            _logger.LogInformation("Agent {AgentId} stopped", _options.Value.AgentId);
        }
        else
        {
            _logger.LogInformation("Agent stopped");
        }
    }
}
