using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.AgentCenter.Store;
using DotnetSpider.Extensions;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IMessageQueue = DotnetSpider.MessageQueue.IMessageQueue;

namespace DotnetSpider.AgentCenter;

public class AgentCenterHostService(
    IAgentStore agentStore,
    ILogger<AgentCenterHostService> logger,
    IMessageQueue messageQueue)
    : BackgroundService
{
    private AsyncMessageConsumer<byte[]> _consumer;
    private readonly bool _distributed = !(messageQueue is LocalMQ);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Agent center service is starting");

            await agentStore.EnsureDatabaseAndTableCreatedAsync();

            _consumer = new AsyncMessageConsumer<byte[]>(Topics.AgentCenter);
            _consumer.Received += async bytes =>
            {
                object message;
                try
                {
                    message = await bytes.DeserializeAsync(stoppingToken);
                    if (message == null)
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Deserialize message failed");
                    return;
                }

                switch (message)
                {
                    case Messages.Agent.Register register:
                    {
                        if (_distributed)
                        {
                            logger.LogInformation("Register agent: {AgentId} - {AgentName}", register.AgentId,
                                register.AgentName);
                        }

                        await agentStore.RegisterAsync(new AgentInfo(register.AgentId, register.AgentName,
                            register.ProcessorCount,
                            register.Memory));
                        break;
                    }
                    case Messages.Agent.Heartbeat heartbeat:
                    {
                        if (_distributed)
                        {
                            logger.LogInformation(
                                "Receive heartbeat: {AgentId} - {AgentName}",
                                heartbeat.AgentId, heartbeat.AgentName);
                        }

                        await agentStore.HeartbeatAsync(new AgentHeartbeat(heartbeat.AgentId, heartbeat.AgentName,
                            heartbeat.AvailableMemory, heartbeat.CpuLoad));
                        break;
                    }
                    default:
                    {
                        var msg = JsonSerializer.Serialize(message);
                        logger.LogWarning("Message not supported: {Message}", msg);
                        break;
                    }
                }
            };
            await messageQueue.ConsumeAsync(_consumer);
            logger.LogInformation("Agent center service started");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Agent center service start failed");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Agent center service is stopping");
        _consumer?.Close();

        await base.StopAsync(cancellationToken);
        logger.LogInformation("Agent center service stopped");
    }
}
