using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace HtmlAgilityPack.Css
{
    public class LRUCache<TInput, TResult> : IDisposable
    {

        private readonly Dictionary<TInput, TResult> data;
        private readonly IndexedLinkedList<TInput> lruList = new IndexedLinkedList<TInput>();
        private readonly Func<TInput, TResult> evalutor;
        private ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();
        private int capacity;

        public LRUCache(Func<TInput, TResult> evalutor, int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException();

            this.data = new Dictionary<TInput, TResult>(capacity);
            this.capacity = capacity;
            this.evalutor = evalutor;
        }

        private bool Remove(TInput key)
        {
            bool existed = data.Remove(key);
            lruList.Remove(key);
            return existed;
        }


        public TResult GetValue(TInput key)
        {
            TResult value;
            bool found;

            rwl.EnterReadLock();
            try
            {
                found = data.TryGetValue(key, out value);
            }
            finally
            {
                rwl.ExitReadLock();
            }


            if (!found) value = evalutor(key);

            rwl.EnterWriteLock();
            try
            {
                if (found)
                {
                    lruList.Remove(key);
                    lruList.Add(key);
                }
                else
                {
                    data[key] = value;
                    lruList.Add(key);

                    if (data.Count > capacity)
                    {
                        Remove(lruList.First);
                        lruList.RemoveFirst();
                    }
                }

            }
            finally
            {
                rwl.ExitWriteLock();
            }


            return value;
        }

        public int Capacity
        {
            get
            {
                return capacity;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();

                rwl.EnterWriteLock();
                try
                {
                    capacity = value;
                    while (data.Count > capacity)
                    {
                        Remove(lruList.First);
                        lruList.RemoveFirst();
                    }
                }
                finally
                {
                    rwl.ExitWriteLock();
                }

            }
        }




        private class IndexedLinkedList<T>
        {

            private LinkedList<T> data = new LinkedList<T>();
            private Dictionary<T, LinkedListNode<T>> index = new Dictionary<T, LinkedListNode<T>>();

            public void Add(T value)
            {
                index[value] = data.AddLast(value);
            }

            public void RemoveFirst()
            {
                index.Remove(data.First.Value);
                data.RemoveFirst();
            }

            public void Remove(T value)
            {
                LinkedListNode<T> node;
                if (index.TryGetValue(value, out node))
                {
                    data.Remove(node);
                    index.Remove(value);
                }
            }

            public void Clear()
            {
                data.Clear();
                index.Clear();
            }

            public T First
            {
                get
                {
                    return data.First.Value;
                }
            }
        }


        public void Dispose()
        {
            if (rwl == null) return;
            try
            {
                rwl.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // It should ignore duplicate calls to Dispose(), but it doesn't.
            }
            rwl = null;
        }
    }



}
