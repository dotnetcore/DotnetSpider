using System.Collections.Generic;
using System.IO;

namespace DotnetSpider.Core.Infrastructure
{
	public static class DirectoryExtension
	{
		public static DirectoryInfo CopyTo(this DirectoryInfo directoryInfo, string target)
		{
			string sourcepath = directoryInfo.FullName;
			Queue<FileSystemInfo> copyfolders = new Queue<FileSystemInfo>(new DirectoryInfo(sourcepath).GetFileSystemInfos());
			string copytopath = target;
			copytopath = (copytopath.LastIndexOf(Path.DirectorySeparatorChar) == copytopath.Length - 1) ? copytopath : (copytopath + Path.DirectorySeparatorChar) + Path.GetFileName(sourcepath);
			Directory.CreateDirectory(copytopath);
			while (copyfolders.Count > 0)
			{
				FileSystemInfo atom = copyfolders.Dequeue();
				FileInfo file = atom as FileInfo;
				if (file == null)
				{
					DirectoryInfo directory = (DirectoryInfo)atom;
					Directory.CreateDirectory(directory.FullName.Replace(sourcepath, copytopath));
					foreach (FileSystemInfo fi in directory.GetFileSystemInfos())
						copyfolders.Enqueue(fi);
				}
				else
					file.CopyTo(file.FullName.Replace(sourcepath, copytopath));
			}

			return new DirectoryInfo(target);
		}
	}
}
