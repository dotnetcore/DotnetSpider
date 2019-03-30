using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotnetSpider.Core
{
    /// <summary>
    /// 加、解密帮助类
    /// </summary>
    public static class Cryptography
    {
        /// <summary>
        /// 计算32位MD5
        /// </summary>
        /// <param name="str">需要计算的字符串</param>
        /// <returns>32位的MD5值</returns>
        public static string ToMd5(this string str)
        {
#if !NETSTANDARD
            var md5 = new MD5CryptoServiceProvider();
#else
			var md5 = MD5.Create();
#endif
            var fromData = Encoding.UTF8.GetBytes(str);
            var targetData = md5.ComputeHash(fromData);

            return BitConverter.ToString(targetData).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 计算8位MD5
        /// </summary>
        /// <param name="str">需要计算的字符串</param>
        /// <returns>8位的MD5值</returns>
        public static string ToShortMd5(this string str)
        {
            return ToMd5(str).Substring(8, 16).ToLower();
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="key">秘钥</param>
        /// <param name="str">需要加密的字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string ToDes(string key, string str)
        {
            var cryptoProvider = new DESCryptoServiceProvider();
            var bytes = Encoding.ASCII.GetBytes(key);
            var crypto = cryptoProvider.CreateEncryptor(bytes, bytes);

            var ms = new MemoryStream();
            var cst = new CryptoStream(ms, crypto, CryptoStreamMode.Write);
            var sw = new StreamWriter(cst);
            sw.Write(str);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();

            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }
    }
}