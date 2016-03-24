using System;
using System.Collections.Generic;
using System.Linq;
using ZooKeeperNet.Generate;

namespace ZooKeeperNet.Recipes.Locks
{
    public class PredicateResults
    {
        public readonly bool GetsTheLock;
        public readonly string PathToWatch;

        public PredicateResults(string pathToWatch, bool getsTheLock)
        {
            PathToWatch = pathToWatch;
            GetsTheLock = getsTheLock;
        }
    }

    public interface ILockInternalsSorter
    {
        string FixForSorting(string str, string lockName);
    }

    public interface ILockInternalsDriver : ILockInternalsSorter
    {
        IEnumerable<ACL> Acl { get; } 
        PredicateResults GetsTheLock(IZooKeeper client, IEnumerable<string> children, string sequenceNodeName, int maxLeases);
    }

    public class StandardLockInternalsDriver : ILockInternalsDriver
    {
        private readonly IEnumerable<ACL> _acl;

        public StandardLockInternalsDriver(IEnumerable<ACL> acl = null)
        {
            _acl = acl ?? Ids.OPEN_ACL_UNSAFE;
        }

        public IEnumerable<ACL> Acl { get { return _acl; } } 

        public PredicateResults GetsTheLock(IZooKeeper client, IEnumerable<string> children, string sequenceNodeName, int maxLeases)
        {
            var scannableChildren = children as string[] ?? children.ToArray();
            var ourIndex = Array.IndexOf(scannableChildren, sequenceNodeName);
            ValidateOurIndex(sequenceNodeName, ourIndex);

            var getsTheLock = ourIndex < maxLeases;
            String pathToWatch = getsTheLock ? null : scannableChildren[ourIndex - maxLeases];

            return new PredicateResults(pathToWatch, getsTheLock);
        }

        public string FixForSorting(string str, string lockName)
        {
            return StandardFixForSorting(str, lockName);
        }

        public static string StandardFixForSorting(string str, string lockName)
        {
            var index = str.LastIndexOf(lockName, StringComparison.InvariantCulture);
            if (index >= 0)
            {
                index += lockName.Length;
                return index <= str.Length ? str.Substring(index) : string.Empty;
            }
            return str;
        }

        static void ValidateOurIndex(String sequenceNodeName, int ourIndex)
        {
            if (ourIndex < 0)
            {
                throw new KeeperException.NoNodeException("Sequential path not found: " + sequenceNodeName);
            }
        }
    }
}
