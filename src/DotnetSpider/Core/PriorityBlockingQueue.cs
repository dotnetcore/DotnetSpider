using System;
using System.Collections.Generic;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 优先级的线程安全队列
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class PriorityBlockingQueue<T>
	{
		private readonly IComparer<T> _comparer;
		private readonly int _capacity;

		private T[] _heap;

		/// <summary>
		/// 当前队列中的元素个数
		/// </summary>
		public int Count { get; private set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		public PriorityBlockingQueue() : this(null)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="capacity">The number of elements that the new list can initially store.</param>
		public PriorityBlockingQueue(int capacity) : this(capacity, null)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="comparer">优先级比较器</param>
		public PriorityBlockingQueue(IComparer<T> comparer) : this(16, comparer)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="capacity">The number of elements that the new list can initially store.</param>
		/// <param name="comparer">优先级比较器</param>
		public PriorityBlockingQueue(int capacity, IComparer<T> comparer)
		{
			_comparer = comparer ?? Comparer<T>.Default;
			_capacity = capacity;
			_heap = new T[_capacity];
		}

		/// <summary>
		/// 清空队列
		/// </summary>
		public void Clear()
		{
			_heap = new T[_capacity];
			Count = 0;
		}

		/// <summary>
		/// 把元素入队
		/// </summary>
		/// <param name="v">元素</param>
		public void Push(T v)
		{
			if (Count >= _heap.Length) Array.Resize(ref _heap, Count * 2);
			_heap[Count] = v;
			SiftUp(Count++);
		}

		/// <summary>
		/// 元素出队
		/// </summary>
		/// <returns>元素</returns>
		public T Pop()
		{
			var v = Top();
			_heap[0] = _heap[--Count];
			if (Count > 0) SiftDown(0);
			return v;
		}

		/// <summary>
		/// 队列第一个元素
		/// </summary>
		/// <returns>元素</returns>
		public T Top()
		{
			if (Count > 0) return _heap[0];
			throw new InvalidOperationException("优先队列为空");
		}

		private void SiftUp(int n)
		{
			var v = _heap[n];
			for (var n2 = n / 2; n > 0 && _comparer.Compare(v, _heap[n2]) > 0; n = n2, n2 /= 2) _heap[n] = _heap[n2];
			_heap[n] = v;
		}

		private void SiftDown(int n)
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