using System;
using System.Security.Cryptography;
using System.Text;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// 类名：HashEncrypt
	/// 作用：对传入的字符串进行Hash运算，返回通过Hash算法加密过的字串。
	/// 属性：［无］
	/// 构造函数额参数：
	/// IsReturnNum:是否返回为加密后字符的Byte代码
	/// IsCaseSensitive：是否区分大小写。
	/// 方法：此类提供MD5，SHA1，SHA256，SHA512等四种算法，加密字串的长度依次增大。
	/// </summary>
	public static class Encrypt
	{
//		public static string Md5Encrypt(string myString)
//		{
//#if !NET_CORE
//			MD5 md5 = new MD5CryptoServiceProvider();
//#else
//			MD5 md5 = MD5.Create();
//#endif
//			byte[] fromData = Encoding.UTF8.GetBytes(myString);
//			byte[] targetData = md5.ComputeHash(fromData);

//			return targetData.Aggregate<byte, string>(null, (current, t) => current + t.ToString("x"));
//		}

		public static string Md5Encrypt(string myString)
		{
#if !NET_CORE
			MD5 md5 = new MD5CryptoServiceProvider();
#else
			MD5 md5 = MD5.Create();
#endif
			byte[] fromData = Encoding.UTF8.GetBytes(myString);
			byte[] targetData = md5.ComputeHash(fromData);

			return BitConverter.ToString(targetData).Replace("-", "").Substring(8, 16).ToLower();
		}
	}
}
