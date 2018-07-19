using System;
using System.Security.Cryptography;
using System.Text;

namespace DotnetSpider.Common
{
	public static class Md5Util
	{
		/// <summary>
		/// 计算32位MD5
		/// </summary>
		/// <param name="str">需要计算的字符串</param>
		/// <returns>32位的MD5值</returns>
		public static string Md5Encrypt32(string str)
		{
#if !NETSTANDARD20
			MD5 md5 = new MD5CryptoServiceProvider();
#else
			MD5 md5 = MD5.Create();
#endif
			byte[] fromData = Encoding.UTF8.GetBytes(str);
			byte[] targetData = md5.ComputeHash(fromData);

			return BitConverter.ToString(targetData).Replace("-", "").ToLower();
		}

		/// <summary>
		/// 计算8位MD5
		/// </summary>
		/// <param name="str">需要计算的字符串</param>
		/// <returns>8位的MD5值</returns>
		public static string Md5Encrypt(string str)
		{
			return Md5Encrypt32(str).Substring(8, 16).ToLower();
		}
	}
}