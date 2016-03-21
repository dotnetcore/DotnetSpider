using System;
using System.Collections.Generic;
using System.Linq;

namespace Java2Dotnet.Spider.Common
{
	public class SynchronizedList<T>
	{
		private readonly List<T> _list = new List<T>();

		public void Add(T t)
		{
			lock (this)
			{
				_list.Add(t);
			}
		}

		public void Remove(T t)
		{
			lock (this)
			{
				_list.Remove(t);
			}
		}

		public int Count()
		{
			lock (this)
			{
				return _list.Count;
			}
		}

		public int Count(Func<T, bool> func)
		{
			lock (this)
			{
				return _list.Count(func);
			}
		}

		public List<T> Where(Func<T, bool> func)
		{
			lock (this)
			{
				return _list.Where(func).ToList();
			}
		}

		public List<T> GetAll()
		{
			return _list;
		} 
	}
}