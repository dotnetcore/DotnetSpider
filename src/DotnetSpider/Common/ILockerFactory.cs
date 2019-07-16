namespace DotnetSpider.Common
{
    public interface ILockerFactory
    {
        ILocker GetLocker();

        ILocker GetLocker(string locker);
    }
}