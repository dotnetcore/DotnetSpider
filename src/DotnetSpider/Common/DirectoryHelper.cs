using System.Collections.Generic;
using System.IO;

namespace DotnetSpider.Common
{
    public static class DirectoryHelper
    {
        /// <summary>
        /// 复制文件夹
        /// </summary>
        /// <param name="source">被复制的文件夹</param>
        /// <param name="destination">目标文件夹</param>
        /// <returns></returns>
        public static void CopyTo(this DirectoryInfo source, string destination)
        {
            var sourcePath = source.FullName;
            var copyFolders = new Queue<FileSystemInfo>(new DirectoryInfo(sourcePath).GetFileSystemInfos());
            var copyToPath = destination;
            copyToPath = copyToPath.LastIndexOf(Path.DirectorySeparatorChar) == copyToPath.Length - 1 ? copyToPath : copyToPath + Path.DirectorySeparatorChar + Path.GetFileName(sourcePath);
            Directory.CreateDirectory(copyToPath);
            while (copyFolders.Count > 0)
            {
                var fileSystemInfo = copyFolders.Dequeue();
                if (!(fileSystemInfo is FileInfo file))
                {
                    var directory = (DirectoryInfo)fileSystemInfo;
                    Directory.CreateDirectory(directory.FullName.Replace(sourcePath, copyToPath));
                    foreach (var fi in directory.GetFileSystemInfos())
                    {
                        copyFolders.Enqueue(fi);
                    }
                }
                else
                {
                    file.CopyTo(file.FullName.Replace(sourcePath, copyToPath));
                }
            }
        }
    }
}