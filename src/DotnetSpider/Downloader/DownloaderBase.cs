using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DotnetSpider.Common;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 下载器
	/// </summary>
    public abstract class DownloaderBase : IDownloader
    {
        private readonly string _downloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "downloads");

        /// <summary>
        /// 日志接口
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// 下载器代理标识
        /// </summary>
        public string AgentId { get; set; }

        /// <summary>
        /// 最后一次使用时间
        /// </summary>
        public DateTime LastUsedTime { get; set; }

        /// <summary>
        /// 是否下载文件
        /// </summary>
        public bool DownloadFile { get; set; }

        /// <summary>
        /// What mediatype should not be treated as file to download.
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 定义哪些类型的内容不需要当成文件下载
        /// </summary>
        public List<string> ExcludeMediaTypes { get; set; } =
            new List<string>
            {
                "",
                "text/html",
                "text/plain",
                "text/richtext",
                "text/xml",
                "text/XML",
                "text/json",
                "text/javascript",
                "application/soap+xml",
                "application/xml",
                "application/json",
                "application/x-javascript",
                "application/javascript",
                "application/x-www-form-urlencoded"
            };

        /// <summary>
        /// 代理池
        /// </summary>
        public IHttpProxyPool HttpProxyPool { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        protected DownloaderBase()
        {
            LastUsedTime = DateTime.Now;
        }

        protected abstract Task<Response> ImplDownloadAsync(Request request);

        protected string CreateFilePath(Request request)
        {
	        var isUri = Uri.TryCreate(request.Url, UriKind.RelativeOrAbsolute, out var uri);
            if (isUri)
            {
                var intervalPath = Path.Combine(request.OwnerId, (uri.Host + uri.LocalPath).Replace("//", "/"));
                var filePath = $"{_downloadFolder}/{intervalPath}";
                return filePath;
            }

            var fileName = Path.GetFileName(request.Url);
            if (fileName != null)
            {
                var intervalPath = Path.Combine(request.OwnerId, fileName);
                var filePath = $"{_downloadFolder}/{intervalPath}";
                return filePath;
            }

            return null;
        }

        protected void StorageFile(Request request, byte[] bytes)
        {
            var filePath = CreateFilePath(request);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Logger?.LogError($"任务 {request.OwnerId} 文件名无法解析 {request.Url}");
                return;
            }

            if (!File.Exists(filePath))
            {
                try
                {
                    var folder = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(folder))
                    {
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        File.WriteAllBytes(filePath, bytes);
                        Logger?.LogInformation($"任务 {request.OwnerId} 保存文件 {request.Url} 成功");
                    }
                }
                catch (Exception e)
                {
                    Logger?.LogError($"任务 {request.OwnerId} 保存文件 {request.Url} 失败: {e.Message}");
                }
            }
            else
            {
                Logger?.LogInformation($"任务 {request.OwnerId} 文件 {request.Url} 已经存在");
            }
        }

        public async Task<Response> DownloadAsync(Request request)
        {
            LastUsedTime = DateTime.Now;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Response response;
            try
            {
                response = await ImplDownloadAsync(request);
            }
            catch (Exception e)
            {
                response = new Response
                {
                    AgentId = AgentId,
                    Request = request,
                    Exception = e.Message
                };
            }

            stopwatch.Stop();

            response.AgentId = AgentId;
            response.Request.AgentId = AgentId;
            response.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            return response;
        }

        public virtual void AddCookies(params Cookie[] cookies)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}