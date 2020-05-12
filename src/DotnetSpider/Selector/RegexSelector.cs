using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Selector
{
    /// <summary>
    /// 正则查询器
    /// </summary>
    public class RegexSelector : ISelector
    {
        private readonly Regex _regex;
        private readonly string _replacement;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="pattern">正则表达式</param>
        /// <param name="options"></param>
        /// <param name="replacement"></param>
        public RegexSelector(string pattern, RegexOptions options = RegexOptions.None, string replacement = "$0")
        {
            pattern.NotNullOrWhiteSpace(nameof(pattern));
            _regex = new Regex(pattern, options);
            _replacement = replacement;
        }

        /// <summary>
        /// 从文本中查询单个结果
        /// 如果符合条件的结果有多个, 仅返回第一个
        /// </summary>
        /// <param name="text">需要查询的文本</param>
        /// <returns>查询结果</returns>
        public ISelectable Select(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var match = _regex.Match(text);
            if (match.Success)
            {
				return new TextSelectable(match.Result(_replacement));
            }

            return null;
        }

        /// <summary>
        /// 从文本中查询所有结果
        /// </summary>
        /// <param name="text">需要查询的文本</param>
        /// <returns>查询结果</returns>
        public IEnumerable<ISelectable> SelectList(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var matches = _regex.Matches(text);

            var results = new List<string>();
            foreach (Match match in matches)
            {
				var value = match.Result(_replacement);
				if (!string.IsNullOrWhiteSpace(value))
				{
					results.Add(value);
				}
            }

            return results.Select(x => new TextSelectable(x));
        }
    }
}
