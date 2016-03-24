using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ZooKeeperNet.Recipes.Locks
{
    /// <summary>
    /// Roughly following the pattern from curator-recipes for a reentrant mutex that works across processes.
    /// See: https://git-wip-us.apache.org/repos/asf?p=incubator-curator.git;a=blob;f=curator-recipes/src/main/java/org/apache/curator/framework/recipes/locks/InterProcessMutex.java;h=703cc8b03e4de560e73c3b78b5c29392a05eb62e;hb=master
    /// </summary>
    public class InterProcessMutex : IInterProcessLock
    {
        private readonly LockInternals _internals;
        private readonly string _basePath;
        private readonly ConcurrentDictionary<Thread, LockData> _threadData = new ConcurrentDictionary<Thread, LockData>();

        private class LockData
        {
            public readonly Thread OwningThread;
            public readonly string LockPath;

            private int _lockCount = 1;
            public int LockCount { get { return Interlocked.CompareExchange(ref _lockCount, 1, 1); } }
            public int Inc() { return Interlocked.Increment(ref _lockCount); }
            public int Dec() { return Interlocked.Decrement(ref _lockCount); }

            public LockData(Thread owningThread, string lockPath)
            {
                OwningThread = owningThread;
                LockPath = lockPath;
            }
        }

        private const string LOCK_NAME = "lock-";

        public InterProcessMutex(IZooKeeper client, string path)
            : this(client, path, LOCK_NAME, 1, new StandardLockInternalsDriver())
        { }

        public InterProcessMutex(IZooKeeper client, string path, string lockName, int maxLeases,
                                 ILockInternalsDriver driver)
        {
            _basePath = path;
            _internals = new LockInternals(client, driver, path, lockName, maxLeases);
        }

        public bool Acquire(TimeSpan? timeout = null)
        {
            /*
             * Note on concurrency: a given lockData instance
             * can be only acted on by a single thread so locking isn't necessary
             */
            var hasLock = false;

            var lockData = SafeGetLockData();
            if (null != lockData)
            {
                // Re-entering
                lockData.Inc();
                hasLock = true;
            }
            else
            {
                var lockPath = _internals.AttemptLock(timeout, null);
                if (null != lockPath)
                {
                    var newLockData = new LockData(Thread.CurrentThread, lockPath);
                    SetLockData(newLockData);
                    hasLock = true;
                }
            }

            if (!hasLock && !timeout.HasValue)
            {
                throw new IOException("Lost connection while trying to acquire lock: " + _basePath);
            }

            return hasLock;
        }

        private LockData SafeGetLockData()
        {
            LockData theData;
            _threadData.TryGetValue(Thread.CurrentThread, out theData);
            return theData;
        }

        private void SetLockData(LockData theData)
        {
            _threadData[Thread.CurrentThread] = theData;
        }

        public void Release()
        {
            /*
             * Note on concurrency: a given lockData instance
             * can be only acted on by a single thread so locking isn't necessary
             */

            var lockData = SafeGetLockData();
            if (null == lockData)
            {
                throw new InvalidOperationException("You do not own the lock: " + _basePath);
            }

            var newLockCount = lockData.Dec();
            if (newLockCount > 0)
            {
                return;
            }

            if (newLockCount < 0)
            {
                throw new InvalidOperationException("Lock count has gone negative for lock: " + _basePath);
            }

            try
            {
                _internals.ReleaseLock(lockData.LockPath);
                _internals.Dispose();
            }
            finally
            {
                LockData dummy;
                _threadData.TryRemove(Thread.CurrentThread, out dummy);
            }
        }

        public bool IsAcquiredInThisProcess()
        {
            return (_threadData.Count > 0);
        }

        public IEnumerable<string> GetParticipantNodes()
        {
            return _internals.GetParticipantNodes();
        }

        public bool IsOwnedByCurrentThread
        {
            get 
            { 
                var lockData = SafeGetLockData();
                return (null != lockData) && (lockData.LockCount > 0);
            }
        }
    }
}
