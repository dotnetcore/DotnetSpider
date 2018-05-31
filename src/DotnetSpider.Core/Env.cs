﻿#if !NET45
using System.Runtime.InteropServices;
#endif
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DotnetSpider.Core
{
    /// <summary>
    /// 爬虫配置
    /// </summary>
    public static class Env
    {
        /// <summary>
        /// TODO: 原则上此版本号与Nuget包版本号同步, 但是不知道有什么好的自动化更新方法
        /// </summary>
        public const string Version = "2.0.21";

        /// <summary>
        /// 从配置文件中读取默认Redis连接字符串的关键字
        /// </summary>
        private const string RedisConnectStringKey = "redisConnectString";
        private const string EmailHostKey = "emailHost";
        private const string EmailPortKey = "emailPort";
        private const string EmailAccountKey = "emailAccount";
        private const string EmailPasswordKey = "emailPassword";
        private const string EmailDisplayNameKey = "emailDisplayName";
        private const string HubServiceUrlKey = "serviceUrl";
        private const string HubServiceTokenKey = "serviceToken";
        private const string SystemConnectionStringKey = "SystemConnection";
        private const string DataConnectionStringKey = "DataConnection";
        private const string SqlEncryptCodeKey = "sqlEncryptCode";

        /// <summary>
        /// 开启企业服务(HTTP), 默认打开, 测试的时候开关
        /// </summary>
        public static bool HubService = true;

        /// <summary>
        /// 定义数据主键的名称
        /// 使用实体定义爬虫解析时, 自动插入数据必须使用自增主键, 在自动构造插入数据的SQL语句时会忽略主键
        /// </summary>
        public static string[] IdColumns = { "Id", "__Id", "id", "__id" };

        /// <summary>
        /// 定义数据采集的时间
        /// 使用实体定义爬虫解析时, 会自动添加CDate数据列
        /// </summary>
        public static string CDateColumn = "cdate";

        /// <summary>
        /// 爬虫系统使用的数据库连接配置
        /// </summary>
        public static ConnectionStringSettings SystemConnectionStringSettings { get; private set; }

        /// <summary>
        /// 数据管道的数据库连接配置
        /// </summary>
        public static ConnectionStringSettings DataConnectionStringSettings { get; private set; }

        /// <summary>
        /// 当前操作系统的HostName
        /// </summary>
        public static string HostName { get; set; }

        /// <summary>
        /// 当前操作系统的IP地址
        /// IP地址应考虑的问题: 内网IP+ ADSL拨号时应该能准确取到内网IP地址
        /// </summary>
        public static string Ip { get; set; }

        /// <summary>
        /// 当前环境的节点编号
        /// </summary>
        public static string NodeId { get; set; }

        /// <summary>
        /// 从配置文件中读取的Redis连接字符串
        /// </summary>
        public static string RedisConnectString { get; private set; }

        /// <summary>
        /// 从配置文件中读取的邮件服务器地址
        /// </summary>
        public static string EmailHost { get; private set; }

        /// <summary>
        /// 从配置文件中读取的邮件服务器端口
        /// </summary>
        public static string EmailPort { get; private set; }

        /// <summary>
        /// 从配置文件中读取的邮件服务帐号
        /// </summary>
        public static string EmailAccount { get; private set; }

        /// <summary>
        /// 从配置文件中读取的邮件服务密码
        /// </summary>
        public static string EmailPassword { get; private set; }

        /// <summary>
        /// 从配置文件中读取的邮件服务发送时显示的名称
        /// </summary>
        public static string EmailDisplayName { get; private set; }

        /// <summary>
        /// 框架使用的全局共享目录, 不是必需要的配置, 某些情况下还会没有权限(把程序部署到租凭的IIS空间中)
        /// </summary>
        public static string GlobalDirectory { get; private set; }

        /// <summary>
        /// 全局配置文件的路径, 当在一台机器配置多个程序, 数据库的帐号密码更新时只需要更新全局配置文件就可以保证所有程序正常运行
        /// </summary>
        internal static string DefaultGlobalAppConfigPath { get; private set; }

        /// <summary>
        /// 程序运行的工作目录
        /// </summary>
        public static string BaseDirectory { get; private set; }

        /// <summary>
        /// 路径分隔符, 因操作系统不同而不同
        /// </summary>
        public static string PathSeperator { get; private set; }

        /// <summary>
        /// 是否启用企业服务日志, 默认值是判断配置文件中是否配置了HubServiceUrl
        /// </summary>
        public static bool HubServiceLog { get; set; }

        /// <summary>
        /// 从配置文件中读取的企业服务地址
        /// </summary>
        public static string HubServiceUrl { get; private set; }

        /// <summary>
        /// 企业服务HTTP日志的地址
        /// </summary>
        public static string HubServiceLogUrl { get; private set; }

        /// <summary>
        /// 企业服务HTTP数据管道的地址
        /// </summary>
        public static string HubServicePipelineUrl { get; private set; }

        /// <summary>
        /// 企业服务HTTP爬虫状态的上传地址
        /// </summary>
        public static string HubServiceStatusApiUrl { get; private set; }

        /// <summary>
        /// 向企业服务添加运行记录的地址
        /// </summary>
        public static string HubServiceTaskApiUrl { get; internal set; }

        /// <summary>
        /// 访问企业服务时使用的凭证
        /// </summary>
        public static string HubServiceToken { get; private set; }

        /// <summary>
        /// 爬虫系统使用的数据库连接字符串
        /// </summary>
        public static string SystemConnectionString => SystemConnectionStringSettings?.ConnectionString;

        /// <summary>
        /// 数据管道默认使用的数据库连接字符串
        /// </summary>
        public static string DataConnectionString => DataConnectionStringSettings?.ConnectionString;

        /// <summary>
        /// 配置PageProcessor是否对深度为1的链接进行正则筛选
        /// </summary>
        public static bool ProcessorFilterDefaultRequest = true;

        /// <summary>
        /// 使用HTTP数据管道时, 对数据进行对称加密的密钥
        /// </summary>
        public static string SqlEncryptCode { get; private set; }

        /// <summary>
        /// 任务唯一标识的最大长度限制
        /// </summary>
        public static int IdentityMaxLength { get; set; } = 120;

        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static string ConfigurationFilePath { get; set; }

        /// <summary>
        /// 当前操作系统是否Windows
        /// </summary>
        public static readonly bool IsWindows;

        static Env()
        {
#if NET45
			IsWindows = true;
#else
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif

            Reload();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <param name="path">配置文件路径</param>
        public static void LoadConfiguration(string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                // 默认的App.config会编译成 {程序集名称}.exe.config 或者 {程序集名称}.dll.config
#if NET45
				path = Path.Combine(BaseDirectory, $"{AppDomain.CurrentDomain.FriendlyName}.config");
#else
                path = Path.Combine(BaseDirectory, $"{AppDomain.CurrentDomain.FriendlyName}.dll.config");
#endif
                // WORKAROUND: 测试框架的入口配置文件会导至读取配置文件错误
                if (string.IsNullOrWhiteSpace(path) || "testhost" == AppDomain.CurrentDomain.FriendlyName)
                {
                    path = Path.Combine(BaseDirectory, "app.config");
                }
            }

            if (path.ToLower().StartsWith("%global%"))
            {
                var fileName = path.Substring(8, path.Length - 8);
                path = string.IsNullOrWhiteSpace(fileName) ? Path.Combine(GlobalDirectory, "app.config") : Path.Combine(GlobalDirectory, fileName);
            }

            if (!File.Exists(path))
            {
                // 配置文件不是必要的运行条件, 可以在代码里自行处理连接字符串等
                return;
            }

            ConfigurationFilePath = path;

            var configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
            {
                ExeConfigFilename = path
            }, ConfigurationUserLevel.None);

            RedisConnectString = configuration.AppSettings.Settings[RedisConnectStringKey]?.Value?.Trim();
            EmailHost = configuration.AppSettings.Settings[EmailHostKey]?.Value?.Trim();
            EmailPort = configuration.AppSettings.Settings[EmailPortKey]?.Value?.Trim();
            EmailAccount = configuration.AppSettings.Settings[EmailAccountKey]?.Value?.Trim();
            EmailPassword = configuration.AppSettings.Settings[EmailPasswordKey]?.Value?.Trim();
            EmailDisplayName = configuration.AppSettings.Settings[EmailDisplayNameKey]?.Value?.Trim();
            HubServiceUrl = configuration.AppSettings.Settings[HubServiceUrlKey]?.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(HubServiceUrl))
            {
                HubServiceLogUrl = $"{HubServiceUrl}{(HubServiceUrl.EndsWith("/") ? "" : "/")}api/v1.0/log";
                HubServiceToken = configuration.AppSettings.Settings[HubServiceTokenKey]?.Value?.Trim();
                HubServiceStatusApiUrl = $"{HubServiceUrl}{(HubServiceUrl.EndsWith("/") ? "" : "/")}api/v1.0/taskstatus";
                HubServiceTaskApiUrl = $"{HubServiceUrl}{(HubServiceUrl.EndsWith("/") ? "" : "/")}api/v1.0/task";
                HubServicePipelineUrl = $"{HubServiceUrl}{(HubServiceUrl.EndsWith("/") ? "" : "/")}api/v1.0/process";
            }
            HubServiceLog = !string.IsNullOrWhiteSpace(HubServiceLogUrl);
            SystemConnectionStringSettings = configuration.ConnectionStrings.ConnectionStrings[SystemConnectionStringKey];
            DataConnectionStringSettings = configuration.ConnectionStrings.ConnectionStrings[DataConnectionStringKey];
            SqlEncryptCode = configuration.AppSettings.Settings[SqlEncryptCodeKey]?.Value?.Trim();
        }

        /// <summary>
        /// 重新加载配置文件
        /// </summary>
        internal static void Reload()
        {
            BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

#if NET45
			PathSeperator = "\\";
#else
            PathSeperator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/";
#endif

            GlobalDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dotnetspider");
            DefaultGlobalAppConfigPath = Path.Combine(GlobalDirectory, "app.config");

            try
            {
                DirectoryInfo di = new DirectoryInfo(GlobalDirectory);
                if (!di.Exists)
                {
                    di.Create();
                }
            }
            catch
            {
                // 某些情况下没有Global文件夹权限, 直接忽略。Global文件夹不是必要的运行环境
            }

            HostName = Dns.GetHostName();

            var interf = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(i => (i.NetworkInterfaceType == NetworkInterfaceType.Ethernet || i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) && i.OperationalStatus == OperationalStatus.Up);

            if (interf != null)
            {
                var unicastAddresses = interf.GetIPProperties().UnicastAddresses;
                Ip = unicastAddresses.FirstOrDefault(a => a.IPv4Mask?.ToString() != "255.255.255.255" && a.Address.AddressFamily == AddressFamily.InterNetwork)?.Address.ToString();
            }

            NodeId = Ip;
            LoadConfiguration(ConfigurationFilePath);
        }
    }
}
