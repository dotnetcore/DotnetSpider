namespace DotnetSpider.Agent
{
    public class AgentOptions
    {
        /// <summary>
        /// 节点类型
        /// ADSL 和普通型不能混合部署
        /// </summary>
        public string ADSLAccount { get; set; }
        public string ADSLPassword { get; set; }
        /// <summary>
        /// ADSL 网络接口
        /// </summary>
        public string ADSLInterface { get; set; }
        public string AgentId { get; set; }
        public string AgentName { get; set; }
        /// <summary>
        /// 是否支持 Puppeteer 节点
        /// </summary>
        public bool SupportPuppeteer { get; set; }
    }
}