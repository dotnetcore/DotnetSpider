using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace DotnetSpider.Core.Selector
{
    /// <summary>
    /// HTML�ļ���ѯ�ĳ���
    /// </summary>
    public abstract class BaseHtmlSelector : ISelector
    {
        /// <summary>
        /// �жϲ�ѯ�Ƿ��������
        /// </summary>
        /// <returns>������� True, ��˵���ǲ�ѯԪ�ص�����ֵ</returns>
        public abstract bool HasAttribute { get; }

        /// <summary>
        /// �Խڵ���в�ѯ, ��ѯ���Ϊ��һ�����ϲ�ѯ������Ԫ��
        /// </summary>
        /// <param name="element">HTMLԪ��</param>
        /// <returns>��ѯ���</returns>
        public abstract dynamic Select(HtmlNode element);

        /// <summary>
        /// �Խڵ���в�ѯ, ��ѯ���Ϊ���з��ϲ�ѯ������Ԫ��
        /// </summary>
        /// <param name="element">HTMLԪ��</param>
        /// <returns>��ѯ���</returns>
        public abstract IEnumerable<dynamic> SelectList(HtmlNode element);

        /// <summary>
        /// ��Html�ı����в�ѯ, ��ѯ���Ϊ��һ�����ϲ�ѯ������Ԫ��
        /// </summary>
        /// <param name="text">Html�ı�</param>
        /// <returns>��ѯ���</returns>
        public virtual dynamic Select(dynamic text)
        {
            if (text != null)
            {
                if (text is string)
                {
                    HtmlDocument document = new HtmlDocument { OptionAutoCloseOnEnd = true };
                    document.LoadHtml(text);
                    return Select(document.DocumentNode);
                }
                else
                {
                    return Select(text as HtmlNode);
                }
            }
            return null;
        }

        /// <summary>
        /// ��Html�ı����в�ѯ, ��ѯ���Ϊ���з��ϲ�ѯ������Ԫ��
        /// </summary>
        /// <param name="text">Html�ı�</param>
        /// <returns>��ѯ���</returns>
        public virtual IEnumerable<dynamic> SelectList(dynamic text)
        {
            if (text != null)
            {
                if (text is HtmlNode htmlNode)
                {
                    return SelectList(htmlNode);
                }
                else
                {
                    HtmlDocument document = new HtmlDocument { OptionAutoCloseOnEnd = true };
                    document.LoadHtml(text);
                    return SelectList(document.DocumentNode);
                }
            }
            else
            {
                return Enumerable.Empty<dynamic>();
            }
        }
    }
}