using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using System;

namespace DotnetSpider.Extension.Processor
{
	/// <summary>
	/// We usually extract TargetRequest in <see cref="IPageProcessor"/>, here is for some special case like adding multiple <see cref="ITargetUrlsExtractor"/> etc.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 正常的解析 TargetRequest 是在 Processor 中实现的, 此处是用于一些特别情况如可能想添加多个解析器
	/// </summary>
	public class TargetRequestHandler : IBeforeProcessorHandler
	{
		private readonly ITargetRequestExtractor _targetUrlsExtractor;
		private readonly bool _extractByProcessor;

		/// <summary>
		/// Construct a <see cref="TargetRequestHandler"/> instance.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 构造方法
		/// </summary>
		/// <param name="targetRequestExtractor">目标链接解析器 <see cref="ITargetRequestExtractor"/></param>
		/// <param name="extractByProcessor">Processor是否还需要执行目标链接解析工作(Should <see cref="IPageProcessor"/> continue to execute <see cref="ITargetRequestExtractor"/>)</param>
		public TargetRequestHandler(ITargetRequestExtractor targetRequestExtractor, bool extractByProcessor = false)
		{
			_targetUrlsExtractor = targetRequestExtractor ?? throw new ArgumentNullException(nameof(targetRequestExtractor));
			_extractByProcessor = extractByProcessor;
		}

		/// <summary>
		/// Execute <see cref="ITargetRequestExtractor"/>.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 执行目标链接解析器
		/// </summary>
		/// <param name="page">页面数据 <see cref="Page"/></param>
		public void Handle(ref Page page)
		{
			if (_targetUrlsExtractor == null || page == null)
			{
				return;
			}

			var requests = _targetUrlsExtractor.ExtractRequests(page);
			foreach (var request in requests)
			{
				page.AddTargetRequest(request);
			}

			page.SkipExtractedTargetRequests = !_extractByProcessor;
		}
	}
}
