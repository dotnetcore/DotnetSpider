using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.DataFlow.Storage
{
	public class ImageStorage : FileStorageBase
	{
		private HashSet<string> _imageSuffixes;
		private static readonly object _locker = new object();

		protected override bool IsContextEmpty(DataFlowContext context)
		{
			return context.Response == null || context.Response.Content.Bytes.Length == 0;
		}

		public string[] ImageSuffixes { get; set; } = {"jpeg", "gif", "jpg", "bmp", "png", "ico"};

		public override Task InitAsync()
		{
			_imageSuffixes = new HashSet<string>(ImageSuffixes);
			return Task.CompletedTask;
		}

		protected override Task StoreAsync(DataFlowContext context)
		{
			var fileName = context.Request.RequestUri.AbsolutePath;
			if (_imageSuffixes.Any(x => fileName.EndsWith(x)))
			{
				var path = Path.Combine(AppContext.BaseDirectory, "data", context.Request.Owner, "image");
				path = $"{path}{fileName}";
				var folder = Path.GetDirectoryName(path);
				if (!string.IsNullOrWhiteSpace(folder))
				{
					lock (_locker)
					{
						if (!Directory.Exists(folder))
						{
							Directory.CreateDirectory(folder);
						}
					}

					File.WriteAllBytes(path, context.Response.Content.Bytes);
				}
			}

			return Task.CompletedTask;
		}
	}
}
