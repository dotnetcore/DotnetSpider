using System;
using System.IO;
using System.Text;

namespace DotnetSpider.Data.Storage
{
	/// <summary>
	/// 解析结果的文件存储器
	/// </summary>
    public abstract class FileStorageBase : StorageBase
    {
	    /// <summary>
	    /// 文件的写入器
	    /// </summary>
        protected StreamWriter Writer { get; private set; }

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
	    /// <param name="ownerId">任务标识</param>
	    /// <returns></returns>
        protected string GetDataFolder(string ownerId)
        {
            var path = Path.Combine(Folder, ownerId);
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
        protected void CreateFile(string file)
        {
            lock (this)
            {
                if (Writer == null)
                {
                    Writer = new StreamWriter(File.OpenWrite(file), Encoding.UTF8);
                }
            }
        }

        public override void Dispose()
        {
            Writer?.Dispose();
        }
    }
}