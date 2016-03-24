using System;

namespace ZooKeeperNet.Recipes.Locks
{
    /// <summary>
    /// Roughly following the pattern from curator-recipes
    /// See: http://curator.incubator.apache.org/curator-recipes/index.html
    /// </summary>
    public interface IInterProcessLock
    {
        /// <summary>
        /// Acquire the mutex, blocking until it's available or the timeout passes.  Each call 
        /// to Acquire() must be balanced by a call to Release.
        /// 
        /// NOTE: The same thread can call acquire re-entrantly.
        /// </summary>
        /// <param name="timeout">time to wait or null to wait indefinitely</param>
        /// <returns>true if the mutex was acquired, false otherwise.</returns>
        bool Acquire(TimeSpan? timeout = null);

        /// <summary>
        /// Perform a single release of the mutex.  If the thread had made multiple calls
        /// to acquire, the mutex will continue to be held until all such calls are balanced
        /// with corresponding Release() calls.
        /// </summary>
        void Release();

        /// <summary>
        /// Returns true if the mutex is acquired by a thread in this process.
        /// </summary>
        /// <returns></returns>
        bool IsAcquiredInThisProcess();
    }
}
