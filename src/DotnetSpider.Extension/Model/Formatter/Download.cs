using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Model.Formatter
{
	/// <summary>
	/// 下载内容
	/// </summary>
	public class Download : Formatter
	{
		/// <summary>
		/// 执行下载操作
		/// </summary>
		/// <param name="value">下载的链接</param>
		/// <returns>下载完成后的文件名</returns>
		protected override object FormateValue(object value)
		{
			var filePath = value.ToString();
			var name = Path.GetFileName(filePath);
			Task<byte[]> task = HttpSender.Client.GetByteArrayAsync(filePath);
			task.ContinueWith(t =>
			{
				if (t.Exception != null)
				{
					throw t.Exception;
				}
				var fileData = t.Result;
				string file = Path.Combine(Core.Env.GlobalDirectory, "images", name);
				if (!File.Exists(file))
				{
					var stream = FileUtil.PrepareFile(file).OpenWrite();
					foreach (var b in fileData)
					{
						stream.WriteByte(b);
					}
					stream.Flush();
					stream.Dispose();
				}
			});

			return name;
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
		}
	}
}