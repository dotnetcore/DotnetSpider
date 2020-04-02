using System.Net;
using System.Threading.Tasks;

namespace DotnetSpider.DataFlow.Parser
{
    /// <summary>
    /// 默认数据解析器
    /// </summary>
    public class DefaultDataParser : DataParser
    {
        protected override Task Parse(DataContext context)
        {
            if (context.Response != null)
            {
                context.AddData("RequestUri", context.Request.RequestUri.ToString());
                context.AddData("Content", context.Response.ReadAsString());
                context.AddData("TargetUri", context.Response.TargetUri);
                context.AddData("Success", context.Response.StatusCode == HttpStatusCode.OK);
                context.AddData("ElapsedMilliseconds", context.Response.ElapsedMilliseconds);
            }

            return Task.CompletedTask;
        }

        public override Task InitAsync()
        {
            return Task.CompletedTask;
        }
    }
}