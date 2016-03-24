using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ZooKeeperNet.Recipes.Locks
{
    /// <summary>
    /// Common lock logic adapted from:
    /// https://git-wip-us.apache.org/repos/asf?p=incubator-curator.git;a=blob;f=curator-recipes/src/main/java/org/apache/curator/framework/recipes/locks/LockInternals.java;h=06a1f6bd7b91c6e7133ab466b062ee25598f3554;hb=master
    /// </summary>
    public class LockInternals : IWatcher, IDisposable
    {
        private static readonly IRetryPolicy DefaultRetryPolicy = new RetryNTimes(5, TimeSpan.FromSeconds(5));

        private readonly IZooKeeper _client;
        private readonly string _path;
        private readonly string _basePath;
        private readonly ILockInternalsDriver _driver;
        private readonly string _lockName;

        // NOTE: Skipping "revokable" support for now

        private IRetryPolicy _retryPolicy = DefaultRetryPolicy;

        private readonly AutoResetEvent _watcherEvent = new AutoResetEvent(false);

        private int _maxLeases;

        public IRetryPolicy RetryPolicy
        {
            get { return _retryPolicy; }
            set { _retryPolicy = value ?? DefaultRetryPolicy; }
        }

        public void Dispose()
        {
            Clean();
            _watcherEvent.Dispose();
        }

        /// <summary>
        /// Attempt to delete the lock node so that sequence numbers get reset.
        /// </summary>
        public void Clean()
        {
            try
            {
                // Don't blindly do delete or things get noisy in the INFO logs on the 
                // server.
                var stat = _client.Exists(_basePath, false);
                if (null != stat && stat.NumChildren < 1)
                {
                    _client.Delete(_basePath, -1);
                }
            }
            catch (KeeperException.BadVersionException)
            {
                // Intentially ignore this - another thread/process got the lock
            }
            catch (KeeperException.NotEmptyException)
            {
                // Intentially ignore this - other threads/processes are waiting
            }
        }

        public LockInternals(IZooKeeper client, ILockInternalsDriver driver, string path, string lockName,
                              int maxLeases)
        {
            _driver = driver;
            _lockName = lockName;
            _maxLeases = maxLeases;
            PathUtils.ValidatePath(path);

            _client = client;
            _basePath = path;
            _path = ZKPaths.MakePath(path, lockName);
        }

        private int MaxLeases
        {
            get { return Interlocked.CompareExchange(ref _maxLeases, 1, 1); }
            set
            {
                Interlocked.Exchange(ref _maxLeases, value);
                NotifyAll();
            }
        }

        private void NotifyAll()
        {
            _watcherEvent.Set();
        }

        public void ReleaseLock(string lockPath)
        {
            DeleteOurPath(lockPath);
        }

        IZooKeeper Client { get { return _client; } }

        public IEnumerable<string> GetParticipantNodes()
        {
            return GetParticipantNodes(Client, _basePath, _lockName, _driver);
        }

        public static IEnumerable<string> GetParticipantNodes(IZooKeeper client, string basePath, string lockName,
                                                              ILockInternalsSorter sorter)
        {
            var names = GetSortedChildren(client, basePath, lockName, sorter);
            var transformed = names.Select(x => ZKPaths.MakePath(basePath, x));
            return transformed;
        }

        public static List<string> GetSortedChildren(IZooKeeper client, string basePath, string lockName,
                                                              ILockInternalsSorter sorter)
        {
            var children = client.GetChildren(basePath, false);
            return GetSortedChildren(lockName, sorter, children);
        }

        public static List<String> GetSortedChildren(string lockName, ILockInternalsSorter sorter, IEnumerable<string> children)
        {
            return new List<string>(children.OrderBy(x => sorter.FixForSorting(x, lockName)));
        }

        List<string> GetSortedChildren()
        {
            return GetSortedChildren(Client, _basePath, _lockName, Driver);
        }

        string LockName { get { return _lockName; } }

        ILockInternalsDriver Driver { get { return _driver; } }

        public string AttemptLock(TimeSpan? timeout, byte[] lockNodeBytes)
        {
            var startTime = DateTime.UtcNow;

            var retryCount = 0;

            string ourPath = null;
            var hasTheLock = false;
            var isDone = false;
            while (!isDone)
            {
                isDone = true;

                try
                {
                    ZKPaths.Mkdirs(Client, _path, false);
                    ourPath = Client.Create(_path, lockNodeBytes, Driver.Acl ?? Ids.OPEN_ACL_UNSAFE, CreateMode.EphemeralSequential);
                    hasTheLock = InternalLockLoop(startTime, timeout, ourPath);
                }
                catch (KeeperException.NoNodeException)
                {
                    // gets thrown by StandardLockInternalsDriver when it can't find the lock node
                    // this can happen when the session expires, etc. So, if the retry allows, just try it all again
                    if (RetryPolicy.AllowRetry(retryCount++, DateTime.UtcNow - startTime))
                    {
                        isDone = false;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return hasTheLock ? ourPath : null;
        }

        private static TimeSpan RemainingTime(DateTime startTime, TimeSpan timeout)
        {
            return timeout - (DateTime.UtcNow - startTime);
        }

        private bool InternalLockLoop(DateTime startTime, TimeSpan? timeToWait, String ourPath)
        {
            var haveTheLock = false;
            var doDelete = false;
            try
            {
                while ((Client.State == ZooKeeper.States.CONNECTED) && !haveTheLock)
                {
                    var children = GetSortedChildren();
                    var sequenceNodeName = ourPath.Substring(_basePath.Length + 1); // +1 to include the slash

                    var predicateResults = Driver.GetsTheLock(Client, children, sequenceNodeName, MaxLeases);
                    if (predicateResults.GetsTheLock)
                    {
                        haveTheLock = true;
                    }
                    else
                    {
                        var previousSequencePath = ZKPaths.MakePath(_basePath, predicateResults.PathToWatch);

                        //                     synchronized(this)
                        {
                            var stat = Client.Exists(previousSequencePath, this);
                            if (stat != null)
                            {
                                if (timeToWait.HasValue)
                                {
                                    var remainingTimeToWait = RemainingTime(startTime, timeToWait.Value);
                                    if (remainingTimeToWait <= TimeSpan.Zero)
                                    {
                                        doDelete = true;    // timed out - delete our node
                                        break;
                                    }

                                    _watcherEvent.WaitOne(remainingTimeToWait);
                                }
                                else
                                {
                                    _watcherEvent.WaitOne(-1);
                                }
                            }
                        }
                        // else it may have been deleted (i.e. lock released). Try to acquire again
                    }
                }
            }
            catch (Exception)
            {
                doDelete = true;
                throw;
            }
            finally
            {
                if (doDelete)
                {
                    DeleteOurPath(ourPath);
                }
            }
            return haveTheLock;
        }

        private void DeleteOurPath(string ourPath)
        {
            try
            {
                Client.Delete(ourPath, -1);
            }
            catch (KeeperException.NoNodeException)
            {
                // ignore - already deleted (possibly expired session, etc.)
            }
        }

        public void Process(WatchedEvent theEvent)
        {
            NotifyAll();
        }
    }
}
