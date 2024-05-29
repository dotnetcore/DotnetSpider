using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DataFlow;

public class FlowBuilder
{
    private readonly List<Func<ResponseDelegate, ResponseDelegate>> _components = [];
    private readonly List<Func<IDataFlow>> _flowFactories = [];

    public async Task<(string, ResponseDelegate)> BuildAsync()
    {
        var info = await InitializeAsync();

        var requestDelegate = (ResponseDelegate)(context =>
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<FlowBuilder>>();
            logger.LogDebug("Start data flow");
            return Task.CompletedTask;
        });

        for (var index = _components.Count - 1; index >= 0; --index)
        {
            var middleware = _components[index];
            requestDelegate = middleware(requestDelegate);
        }

        return (info, requestDelegate);
    }

    public void AddFlow<T>() where T : IDataFlow
    {
        var factory = () => (IDataFlow)Activator.CreateInstance<T>();
        _flowFactories.Add(factory);
        _components.Add((@delegate => CreateMiddleware(factory, @delegate)));
    }

    public void AddFlow(Func<IDataFlow> factory)
    {
        _flowFactories.Add(factory);
        _components.Add((@delegate => CreateMiddleware(factory, @delegate)));
    }

    private ResponseDelegate CreateMiddleware(Func<IDataFlow> factory, ResponseDelegate next)
    {
        return async context =>
        {
            var middleware = factory();
            middleware.SetLogger(context.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger(middleware.GetType()));
            await middleware.InitializeAsync();
            await middleware.HandleAsync(context, next);
        };
    }

    private Task<string> InitializeAsync()
    {
        if (_flowFactories.Count == 0)
        {
            // Logger.LogWarning($"{SpiderId} there is no any dataFlow");
            return Task.FromResult("Empty data flow chain");
        }

        // var dataFlowInfo = string.Join(" -> ", _flowFactories.Select(x => x.Name));
        var pre = "DataFlow full chain: ";
        var flowInfo = new StringBuilder(pre);
        //Logger.LogInformation($"{SpiderId} DataFlows: {dataFlowInfo}");
        foreach (var factory in _flowFactories)
        {
            var dataFlow = factory();
            if (dataFlow == null)
            {
                continue;
            }

            if (flowInfo.Length == pre.Length)
            {
                flowInfo.Append(dataFlow.GetType().Name);
            }
            else
            {
                flowInfo.Append(" -> ").Append(dataFlow.GetType().Name);
            }

            // await dataFlow.InitializeAsync();

            // if (dataFlow is DataParser dataParser)
            // {
            //     _dataParsers.Add(dataParser);
            // }
        }

        return Task.FromResult(flowInfo.ToString());
    }
}
