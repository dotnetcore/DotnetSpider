using System.Diagnostics;

namespace DotnetSpider.Core
{
    /// <summary>
    /// 通过Process调用rasdial.exe拨号
    /// </summary>
    public class Rasdial
    {
        private readonly string _interfaceName;
        private readonly string _username;
        private readonly string _password;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="interfaceName">网络名称</param>
        /// <param name="username">账号</param>
        /// <param name="password">密码</param>
        public Rasdial(string interfaceName, string username = null, string password = null)
        {
            _interfaceName = interfaceName;
            _username = username;
            _password = password;
        }

        /// <summary>
        /// 拨号
        /// </summary>
        /// <returns>返回拨号进程的返回值</returns>
        public int Connect()
        {
            Process process = new Process
            {
                StartInfo =
                {
                    FileName = "rasdial.exe",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = @"C:\Windows\System32",
                    Arguments = _interfaceName + " " + _username + " " + _password
                }
            };
            process.Start();
            process.WaitForExit(10000);
            return process.ExitCode;
        }

        /// <summary>
        /// 断开现有拨号
        /// </summary>
        public void Disconnect()
        {
            Process process = new Process
            {
                StartInfo =
                {
                    FileName = "rasdial.exe",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = @"C:\Windows\System32",
                    Arguments = _interfaceName + @" /DISCONNECT"
                }
            };
            process.Start();
            process.WaitForExit(10000);
        }
    }
}