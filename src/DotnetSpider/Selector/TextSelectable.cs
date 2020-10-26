using System;
using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Selector
{
	/// <summary>
	/// 查询接口
	/// </summary>
	public class TextSelectable : Selectable
	{
		private readonly string _text;

		public override SelectableType Type => SelectableType.Text;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="text">内容</param>
		public TextSelectable(string text)
		{
			_text = text;
		}

		/// <summary>
		/// 取得查询器里所有的结果
		/// </summary>
		/// <returns>查询接口</returns>
		public override IEnumerable<ISelectable> Nodes()
		{
			return new[] {new TextSelectable(_text)};
		}

		/// <summary>
		/// 查找所有的链接
		/// </summary>
		/// <returns>查询接口</returns>
		public override IEnumerable<string> Links()
		{
			// todo: re-impl with regex
			var links = SelectList(Selectors.XPath("./descendant-or-self::*/@href")).Select(x => x.Value);
			var sourceLinks = SelectList(Selectors.XPath("./descendant-or-self::*/@src"))
				.Select(x => x.Value);

			var results = new HashSet<string>();
			foreach (var link in links)
			{
				if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out _))
				{
					results.Add(link);
				}
			}

			foreach (var link in sourceLinks)
			{
				if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out _))
				{
					results.Add(link);
				}
			}

			return results;
		}


		public override string Value => _text;

		/// <summary>
		/// 通过查询器查找结果
		/// </summary>
		/// <param name="selector">查询器</param>
		/// <returns>查询接口</returns>
		public override ISelectable Select(ISelector selector)
		{
			selector.NotNull(nameof(selector));
			return selector.Select(_text);
		}

		/// <summary>
		/// 通过查询器查找结果
		/// </summary>
		/// <param name="selector">查询器</param>
		/// <returns>查询接口</returns>
		public override IEnumerable<ISelectable> SelectList(ISelector selector)
		{
			selector.NotNull(nameof(selector));
			return selector.SelectList(_text);
		}
	}
}
