
namespace DotnetSpider.Core
{
    /// <summary>
    /// 名称接口定义
    /// </summary>
    public interface INamed
    {
        string Name { get; set; }
    }

    public abstract class Named : INamed
    {
        public string Name { get; set; }
    }
}
