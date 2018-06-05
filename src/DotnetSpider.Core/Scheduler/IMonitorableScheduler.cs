namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// The scheduler whose requests can be counted for monitor.
	/// </summary>
	public interface IMonitorable
	{
		bool IsExited { get; set; }
        /// <summary>
        /// ʣ��������
        /// </summary>
        long LeftRequestsCount { get; }

        /// <summary>
        /// �ܵ�������
        /// </summary>
        long TotalRequestsCount { get; }

        /// <summary>
        /// �ɼ��ɹ���������
        /// </summary>
        long SuccessRequestsCount { get; }

        /// <summary>
        /// �ɼ�ʧ�ܵĴ���, ����������, ���һ�����Ӳɼ���ζ�ʧ�ܻ��¼���
        /// </summary>
        long ErrorRequestsCount { get; }

        /// <summary>
        /// �ɼ��ɹ����������� 1
        /// </summary>
        void IncreaseSuccessCount();

        /// <summary>
        /// �ɼ�ʧ�ܵĴ����� 1
        /// </summary>
        void IncreaseErrorCount();
    }
}