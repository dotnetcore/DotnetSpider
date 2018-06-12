using System.Collections.Generic;

namespace DotnetSpider.Core.Selector
{
    /// <summary>
    /// ��ѯ��
    /// </summary>
    public interface ISelector
    {
        /// <summary>
        /// ���ı��в�ѯ�������
        /// ������������Ľ���ж��, �����ص�һ��
        /// </summary>
        /// <param name="text">��Ҫ��ѯ���ı�</param>
        /// <returns>��ѯ���</returns>
        dynamic Select(dynamic text);

        /// <summary>
        /// ���ı��в�ѯ���н��
        /// </summary>
        /// <param name="text">��Ҫ��ѯ���ı�</param>
        /// <returns>��ѯ���</returns>
        IEnumerable<dynamic> SelectList(dynamic text);
    }
}
