using System.Collections.Generic;
using System.IO;

namespace DotnetSpider.Common
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
		public static string CheckAndMakeParentDirecotry(string fullName)
		{
			string path = Path.GetDirectoryName(fullName);

			if (path != null)
			{
				DirectoryInfo directory = new DirectoryInfo(path);
				if (!directory.Exists)
				{
					directory.Create();
				}
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
			string sourcepath = source.FullName;
			Queue<FileSystemInfo> copyfolders = new Queue<FileSystemInfo>(new DirectoryInfo(sourcepath).GetFileSystemInfos());
			string copytopath = destination;
			copytopath = copytopath.LastIndexOf(Path.DirectorySeparatorChar) == copytopath.Length - 1 ? copytopath : (copytopath + Path.DirectorySeparatorChar) + Path.GetFileName(sourcepath);
			Directory.CreateDirectory(copytopath);
			while (copyfolders.Count > 0)
			{
				FileSystemInfo filsSystemInfo = copyfolders.Dequeue();
				if (!(filsSystemInfo is FileInfo file))
				{
					DirectoryInfo directory = (DirectoryInfo) filsSystemInfo;
					Directory.CreateDirectory(directory.FullName.Replace(sourcepath, copytopath));
					foreach (FileSystemInfo fi in directory.GetFileSystemInfos())
					{
						copyfolders.Enqueue(fi);
					}
				}
				else
				{
					file.CopyTo(file.FullName.Replace(sourcepath, copytopath));
				}
			}
		}
	}
}
