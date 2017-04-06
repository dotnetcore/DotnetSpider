using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Infrastructure
{
	public class PriorityBlockingQueue<T>
	{
		readonly IComparer<T> _comparer;
		T[] _heap;

		public int Count { get; private set; }

		public PriorityBlockingQueue() : this(null) { }
		public PriorityBlockingQueue(int capacity) : this(capacity, null) { }
		public PriorityBlockingQueue(IComparer<T> comparer) : this(16, comparer) { }

		private readonly int _capacity;

		public PriorityBlockingQueue(int capacity, IComparer<T> comparer)
		{
			_comparer = comparer ?? Comparer<T>.Default;
			_capacity = capacity;
			_heap = new T[_capacity];
		}

		public void Clear()
		{
			_heap = new T[_capacity];
			Count = 0;
		}

		public void Push(T v)
		{
			if (Count >= _heap.Length) Array.Resize(ref _heap, Count * 2);
			_heap[Count] = v;
			SiftUp(Count++);
		}

		public T Pop()
		{
			var v = Top();
			_heap[0] = _heap[--Count];
			if (Count > 0) SiftDown(0);
			return v;
		}

		public T Top()
		{
			if (Count > 0) return _heap[0];
			throw new InvalidOperationException("优先队列为空");
		}

		void SiftUp(int n)
		{
			var v = _heap[n];
			for (var n2 = n / 2; n > 0 && _comparer.Compare(v, _heap[n2]) > 0; n = n2, n2 /= 2) _heap[n] = _heap[n2];
			_heap[n] = v;
		}

		void SiftDown(int n)
		{
			var v = _heap[n];
			for (var n2 = n * 2; n2 < Count; n = n2, n2 *= 2)
			{
				if (n2 + 1 < Count && _comparer.Compare(_heap[n2 + 1], _heap[n2]) > 0) n2++;
				if (_comparer.Compare(v, _heap[n2]) >= 0) break;
				_heap[n] = _heap[n2];
			}
			_heap[n] = v;
		}
	}
}
