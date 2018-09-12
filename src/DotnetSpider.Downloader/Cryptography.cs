using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 加、解密帮助类
	/// </summary>
	internal static class Cryptography
	{
		/// <summary>
		/// 计算32位MD5
		/// </summary>
		/// <param name="str">需要计算的字符串</param>
		/// <returns>32位的MD5值</returns>
		public static string ToMd5(this string str)
		{
#if !NETSTANDARD
			MD5 md5 = new MD5CryptoServiceProvider();
#else
			MD5 md5 = MD5.Create();
#endif
			byte[] fromData = Encoding.UTF8.GetBytes(str);
			byte[] targetData = md5.ComputeHash(fromData);

			return BitConverter.ToString(targetData).Replace("-", "").ToLower();
		}
	}
}
