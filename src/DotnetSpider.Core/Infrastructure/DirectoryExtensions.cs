using System.Collections.Generic;
using System.IO;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// 文件夹扩展
	/// </summary>
	public static class DirectoryExtension
	{
		/// <summary>
		/// 如果文件夹没有创建, 则帮助创建
		/// </summary>
		/// <param name="fullName">文件路径</param>
		/// <returns>文件夹路径</returns>
		public static string CheckAndMakeParentDirectory(string fullName)
		{
			var path = Path.GetDirectoryName(fullName);

			if (path == null) return null;
			var directory = new DirectoryInfo(path);
			if (!directory.Exists)
			{
				directory.Create();
			}
			return path;
		}

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
			copyToPath = copyToPath.LastIndexOf(Path.DirectorySeparatorChar) == copyToPath.Length - 1 ? copyToPath : (copyToPath + Path.DirectorySeparatorChar) + Path.GetFileName(sourcePath);
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
