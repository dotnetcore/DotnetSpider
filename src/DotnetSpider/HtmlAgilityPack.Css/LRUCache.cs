using System;
using System.Collections.Generic;

namespace DotnetSpider.HtmlAgilityPack.Css
{
	public class LruCache<TInput, TResult> : IDisposable
	{

		private readonly Dictionary<TInput, TResult> _data;
		private readonly IndexedLinkedList<TInput> _lruList = new IndexedLinkedList<TInput>();
		private readonly Func<TInput, TResult> _evalutor;
		private readonly object _rwl = new object();
		private int _capacity;

		public LruCache(Func<TInput, TResult> evalutor, int capacity)
		{
			if (capacity <= 0)
				throw new ArgumentOutOfRangeException();

			_data = new Dictionary<TInput, TResult>(capacity);
			_capacity = capacity;
			_evalutor = evalutor;
		}

		private bool Remove(TInput key)
		{
			bool existed = _data.Remove(key);
			_lruList.Remove(key);
			return existed;
		}


		public TResult GetValue(TInput key)
		{
			TResult value;
			bool found;

			lock (_rwl)
			{
				found = _data.TryGetValue(key, out value);
			}

			if (!found) value = _evalutor(key);

			lock (_rwl)
			{
				if (found)
				{
					_lruList.Remove(key);
					_lruList.Add(key);
				}
				else
				{
					_data[key] = value;
					_lruList.Add(key);

					if (_data.Count > _capacity)
					{
						Remove(_lruList.First);
						_lruList.RemoveFirst();
					}
				}
			}
			return value;
		}

		public int Capacity
		{
			get => _capacity;

			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException();

				lock (_rwl)
				{
					_capacity = value;
					while (_data.Count > _capacity)
					{
						Remove(_lruList.First);
						_lruList.RemoveFirst();
					}
				}
			}
		}




		private class IndexedLinkedList<T>
		{

			private LinkedList<T> _data = new LinkedList<T>();
			private Dictionary<T, LinkedListNode<T>> _index = new Dictionary<T, LinkedListNode<T>>();

			public void Add(T value)
			{
				_index[value] = _data.AddLast(value);
			}

			public void RemoveFirst()
			{
				_index.Remove(_data.First.Value);
				_data.RemoveFirst();
			}

			public void Remove(T value)
			{
				LinkedListNode<T> node;
				if (_index.TryGetValue(value, out node))
				{
					_data.Remove(node);
					_index.Remove(value);
				}
			}

			public void Clear()
			{
				_data.Clear();
				_index.Clear();
			}

			public T First => _data.First.Value;
		}


		public void Dispose()
		{
		}
	}
}
