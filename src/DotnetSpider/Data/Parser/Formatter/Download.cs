using System;
using System.IO;
using System.Net;

namespace DotnetSpider.Data.Parser.Formatter
{
	/// <summary>
	/// 下载内容
	/// </summary>
	public class Download : Formatter
	{
		private readonly WebClient _client = new WebClient();
		/// <summary>
		/// 执行下载操作
		/// </summary>
		/// <param name="value">下载的链接</param>
		/// <returns>下载完成后的文件名</returns>
		protected override string FormatValue(string value)
		{
			var filePath = value;
			var name = Path.GetFileName(filePath);
			string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", name);
			_client.DownloadFile(file, filePath);
			return file;
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
		}
	}
}
