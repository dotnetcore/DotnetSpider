using System;
using DotnetSpider.Downloader;

namespace DotnetSpider.Scheduler.Component
{
    public interface IDuplicateRemover : IDisposable
    {
        /// <summary>
        /// Check whether the request is duplicate.
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Whether the request is duplicate.</returns>
        bool IsDuplicate(Request request);
        
        int Total { get; }
        
        /// <summary>
        /// Reset duplicate check.
        /// </summary>
        void ResetDuplicateCheck();
    }
}