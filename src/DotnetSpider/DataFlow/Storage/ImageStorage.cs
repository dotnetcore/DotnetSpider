using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.DataFlow.Storage;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace DotnetSpider.DataFlow
{
	public class ImageStorage : FileStorageBase
	{
		private HashSet<string> _imageExtensions;
		private static readonly object _locker = new();

		protected override bool IsNullOrEmpty(DataFlowContext context)
		{
			return context.Response == null || context.Response.Content.Bytes.Length == 0;
		}

		public string[] ImageExtensions { get; set; } = {"jpeg", "gif", "jpg", "bmp", "png", "ico", "svg"};

		public override Task InitializeAsync()
		{
			base.InitializeAsync();

			_imageExtensions = new HashSet<string>(ImageExtensions);
			return Task.CompletedTask;
		}

		public override Task HandleAsync(DataFlowContext context)
		{
			if (IsNullOrEmpty(context))
			{
				Logger.LogWarning("数据流上下文不包含解析结果");
				return Task.CompletedTask;
			}

			var fileName = context.Request.RequestUri.AbsolutePath;
			if (!_imageExtensions.Any(x => fileName.EndsWith(x)))
			{
				return Task.CompletedTask;
			}

			var path = Path.Combine(GetDataFolder(context.Request.Owner), "images");
			path = $"{path}{fileName}";
			var folder = Path.GetDirectoryName(path);
			if (string.IsNullOrWhiteSpace(folder))
			{
				return Task.CompletedTask;
			}

			lock (_locker)
			{
				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}
			}

			File.WriteAllBytes(path, context.Response.Content.Bytes);

			return Task.CompletedTask;
		}
	}
}
