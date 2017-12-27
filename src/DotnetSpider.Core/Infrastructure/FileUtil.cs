using System.IO;

namespace DotnetSpider.Core.Infrastructure
{
	public static class FileUtil
	{
		public static FileInfo PrepareFile(string fullName)
		{
			DirectoryUtil.CheckAndMakeParentDirecotry(fullName);
			return new FileInfo(fullName);
		}
	}
}
