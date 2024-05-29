using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace DotnetSpider.Proxy;

public class FiddlerProxySupplier(IOptions<ProxyOptions> options) : IProxySupplier
{
    private Uri[] _proxies = [new(options.Value.ProxyTestUrl)];

    public Task<IEnumerable<Uri>> GetProxiesAsync()
    {
        if (_proxies.Length <= 0)
        {
            return Task.FromResult(Enumerable.Empty<Uri>());
        }

        var result = _proxies.Clone() as IEnumerable<Uri>;
        _proxies = [];
        return Task.FromResult(result);
    }
}