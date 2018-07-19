using DotnetSpider.Common;
using System;

namespace DotnetSpider.Downloader.AfterDownloadCompleteHandlers
{
	/// <summary>
	/// Handler that cutout
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 截取下载内容的处理器
	/// </summary>
	public class CutoutHandler : AfterDownloadCompleteHandler
	{
		private readonly string _startPart;
		private readonly string _endPart;
		private readonly int _startOffset;
		private readonly int _endOffset;

		/// <summary>
		/// Construct a CutoutHandler instance, it will cutout 
		/// with <paramref name="startOffset"/> to index of <paramref name="endPart"/> with <paramref name="endOffset"/>.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 构造方法
		/// </summary>
		/// <param name="startPart">起始部分的内容</param>
		/// <param name="endPart">结束部分的内容</param>
		/// <param name="startOffset">开始截取的偏移</param>
		/// <param name="endOffset">结束截取的偏移</param>
		public CutoutHandler(string startPart, string endPart, int startOffset = 0, int endOffset = 0)
		{
			_startPart = startPart;
			_endOffset = endOffset;
			_endPart = endPart;
			_startOffset = startOffset;
		}

		/// <summary>
		/// Cutout
		/// </summary>
		/// <summary>
		/// 截取下载内容
		/// </summary>
		/// <param name="response">页面数据 <see cref="Response"/></param>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		public override void Handle(ref Response response, IDownloader downloader)
		{
			if (response == null || string.IsNullOrWhiteSpace(response.Content))
			{
				return;
			}

			string rawText = response.Content;

			int begin = rawText.IndexOf(_startPart, StringComparison.Ordinal);

			if (begin < 0)
			{
				throw new DownloaderException($"Cutout failed, can not find begin string: {_startPart}");
			}

			int end = rawText.IndexOf(_endPart, begin, StringComparison.Ordinal);
			int length = end - begin;

			begin += _startOffset;
			length -= _startOffset;
			length -= _endOffset;
			length += _endPart.Length;

			if (begin < 0 || length < 0)
			{
				throw new DownloaderException("Cutout failed. Please check your settings");
			}

			string newRawText = rawText.Substring(begin, length).Trim();
			response.Content = newRawText;
		}
	}
}
