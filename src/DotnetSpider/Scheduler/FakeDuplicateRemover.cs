using DotnetSpider.Data;
using DotnetSpider.Downloader;
using DotnetSpider.Scheduler.Component;

namespace DotnetSpider.Scheduler
{
    internal class FakeDuplicateRemover : IDuplicateRemover
    {
        public void Dispose()
        {
        }

        public bool IsDuplicate(Request request)
        {
            Check.NotNull(request.OwnerId, nameof(request.OwnerId));
            return false;
        }

        public int Total => 0;

        public void ResetDuplicateCheck()
        {
        }
    }
}