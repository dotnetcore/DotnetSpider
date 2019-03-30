using System;
using System.IO;
using System.Text;

namespace DotnetSpider.Data.Storage
{
    public abstract class FileStorageBase : StorageBase
    {
        protected StreamWriter Writer { get; private set; }

        protected string Folder { get; }

        protected FileStorageBase()
        {
            Folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files");
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }
        }

        protected string GetDataFolder(string ownerId)
        {
            var path = Path.Combine(Folder, ownerId);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

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