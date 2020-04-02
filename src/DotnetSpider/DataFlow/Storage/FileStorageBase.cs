using System;
using System.IO;
using System.Text;

namespace DotnetSpider.DataFlow.Storage
{
    /// <summary>
    /// 解析结果的文件存储器
    /// </summary>
    public abstract class FileStorageBase : StorageBase
    {
        private readonly object _locker = new object();

        /// <summary>
        /// 存储的根文件夹
        /// </summary>
        protected string Folder { get; }

        /// <summary>
        /// 构造方法
        /// </summary>
        protected FileStorageBase()
        {
            Folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files");
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }
        }

        /// <summary>
        /// 获取存储文件夹
        /// </summary>
        /// <param name="owner">任务标识</param>
        /// <returns></returns>
        protected string GetDataFolder(string owner)
        {
            var path = Path.Combine(Folder, owner);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// 创建文件写入器
        /// </summary>
        /// <param name="file"></param>
        protected StreamWriter OpenWrite(string file)
        {
            lock (_locker)
            {
                return new StreamWriter(File.OpenWrite(file), Encoding.UTF8);
            }
        }
    }
}