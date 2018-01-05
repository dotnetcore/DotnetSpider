using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// 加、解密帮助类
	/// </summary>
	public static class CryptoUtil
	{
		/// <summary>
		/// 计算32位MD5
		/// </summary>
		/// <param name="str">需要计算的字符串</param>
		/// <returns>32位的MD5值</returns>
		public static string Md5Encrypt32(string str)
		{
#if !NET_CORE
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

		/// <summary>
		/// DES加密
		/// </summary>
		/// <param name="key">秘钥</param>
		/// <param name="str">需要加密的字符串</param>
		/// <returns>加密后的字符串</returns>
		public static string DesEncrypt(string key, string str)
		{
			DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
			var bytes = Encoding.ASCII.GetBytes(key);
			var des = DES.Create();
			var encryptor = cryptoProvider.CreateEncryptor(bytes, bytes);

			var ms = new MemoryStream();
			CryptoStream cst = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
			StreamWriter sw = new StreamWriter(cst);
			sw.Write(str);
			sw.Flush();
			cst.FlushFinalBlock();
			sw.Flush();

			return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
		}
	}
}
