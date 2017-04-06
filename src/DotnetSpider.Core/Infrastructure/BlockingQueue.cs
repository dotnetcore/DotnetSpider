using System;
using System.Collections;
using System.Threading;

namespace DotnetSpider.Core.Infrastructure
{
	public sealed class BlockingQueue<T> : ICollection
	{
		#region Fields
		// Buffer used to store queue objects with max "Size".
		private readonly T[] _buffer;
		// Current number of elements in the queue.
		private int _count;
		// Max number of elements queue can hold without blocking.
		private readonly int _size;
		// Index of slot for object to remove on next Dequeue.
		private int _head;
		// Index of slot for next Enqueue object.
		private int _tail;
		// Object used to synchronize the queue.
		private readonly object _syncRoot;
		#endregion

		#region Constructors
		/// <summary>
		/// Create instance of Queue with Bounded number of elements.
		/// After that many elements are used, another Enqueue
		/// operation will "block" or wait until a Consumer calls
		/// Dequeue to free a slot.  Likewise, if the queue
		/// is empty, a call to Dequeue will block until
		/// another thread calls Enqueue.
		/// </summary>
		/// <param name="size"></param>
		public BlockingQueue(int size)
		{
			if (size < 1)
				throw new ArgumentOutOfRangeException("size must"
									 + " be greater then zero.");
			_syncRoot = new object();
			_size = size;
			_buffer = new T[size];
			_count = 0;
			_head = 0;
			_tail = 0;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets the object values currently in the queue.
		/// If queue is empty, this will return a zero length array.
		/// The returned array length can be 0 to Size.
		/// This method does not modify the queue,
		/// but returns a shallow copy
		/// of the queue buffer containing the objects
		/// contained in the queue.
		/// </summary>
		public object[] Values
		{
			get
			{
				// Copy used elements to a new array
				// of "count" size.  Note a simple
				// Buffer copy will not work as head
				// could be anywhere and we want
				// a zero based array.
				object[] values;
				lock (_syncRoot)
				{
					values = new object[_count];
					int pos = _head;
					for (int i = 0; i < _count; i++)
					{
						values[i] = _buffer[pos];
						pos = (pos + 1) % _size;
					}
				}
				return values;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds an object to the end of the queue.
		/// If queue is full, this method will
		/// block until another thread calls one
		/// of the Dequeue methods.  This method will wait
		/// "Timeout.Infinite" until queue has a free slot.
		/// </summary>
		/// <param name="value"></param>
		public void Enqueue(T value)
		{
			Enqueue(value, Timeout.Infinite);
		}

		/// <summary>
		/// Adds an object to the end of the queue.
		/// If queue is full, this method will
		/// block until another thread calls one
		/// of the Dequeue methods or millisecondsTimeout
		/// expires.  If timeout, method will throw QueueTimeoutException.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="millisecondsTimeout"></param>
		public void Enqueue(T value, int millisecondsTimeout)
		{
			lock (_syncRoot)
			{
				while (_count == _size)
				{
					try
					{
						if (!System.Threading.Monitor.Wait(_syncRoot, millisecondsTimeout))
							throw new QueueTimeoutException();
					}
					catch
					{
						// Monitor exited with exception.
						// Could be owner thread of monitor
						// object was terminated or timeout
						// on wait. Pulse any/all waiting
						// threads to ensure we don't get
						// any "live locked" producers.
						System.Threading.Monitor.PulseAll(_syncRoot);
						throw;
					}
				}
				_buffer[_tail] = value;
				_tail = (_tail + 1) % _size;
				_count++;
				if (_count == 1) // Could have blocking Dequeue thread(s).
					System.Threading.Monitor.PulseAll(_syncRoot);
			}
		}

		/// <summary>
		/// Non-blocking version of Enqueue().
		/// If Enqueue is successfull, this will
		/// return true; otherwise false if queue is full.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>true if successfull,
		/// otherwise false.</returns>
		public bool TryEnqueue(T value)
		{
			lock (_syncRoot)
			{
				if (_count == _size)
					return false;
				_buffer[_tail] = value;
				_tail = (_tail + 1) % _size;
				_count++;
				if (_count == 1)
					// Could have blocking Dequeue thread(s).
					System.Threading.Monitor.PulseAll(_syncRoot);
			}
			return true;
		}

		/// <summary>
		/// Removes and returns the object
		/// at the beginning of the Queue.
		/// If queue is empty, method will block until
		/// another thread calls one of
		/// the Enqueue methods. This method will wait
		/// "Timeout.Infinite" until another
		/// thread Enqueues and object.
		/// </summary>
		/// <returns></returns>
		public T Dequeue()
		{
			return Dequeue(Timeout.Infinite);
		}

		/// <summary>
		/// Removes and returns the object
		/// at the beginning of the Queue.
		/// If queue is empty, method will block until
		/// another thread calls one of
		/// the Enqueue methods or millisecondsTimeout expires.
		/// If timeout, method will throw QueueTimeoutException.
		/// </summary>
		/// <returns>The object that is removed from
		/// the beginning of the Queue.</returns>
		public T Dequeue(int millisecondsTimeout)
		{
			T value;
			lock (_syncRoot)
			{
				while (_count == 0)
				{
					try
					{
						if (!System.Threading.Monitor.Wait(_syncRoot, millisecondsTimeout))
							throw new QueueTimeoutException();
					}
					catch
					{
						System.Threading.Monitor.PulseAll(_syncRoot);
						throw;
					}
				}
				value = _buffer[_head];
				_buffer[_head] = default(T);
				_head = (_head + 1) % _size;
				_count--;
				if (_count == (_size - 1))
					// Could have blocking Enqueue thread(s).
					System.Threading.Monitor.PulseAll(_syncRoot);
			}
			return value;
		}

		/// <summary>
		/// Non-blocking version of Dequeue.
		/// Will return false if queue is empty and set
		/// value to null, otherwise will return true
		/// and set value to the dequeued object.
		/// </summary>
		/// <param name="value">The object that is removed from
		///     the beginning of the Queue or null if empty.</param>
		/// <returns>true if successfull, otherwise false.</returns>
		public bool TryDequeue(out T value)
		{
			lock (_syncRoot)
			{
				if (_count == 0)
				{
					value = default(T);
					return false;
				}
				value = _buffer[_head];
				_buffer[_head] = default(T);
				_head = (_head + 1) % _size;
				_count--;
				if (_count == (_size - 1))
					// Could have blocking Enqueue thread(s).
					System.Threading.Monitor.PulseAll(_syncRoot);
			}
			return true;
		}

		/// <summary>
		/// Returns the object at the beginning
		/// of the queue without removing it.
		/// </summary>
		/// <returns>The object at the beginning
		/// of the queue.</returns>
		/// <remarks>
		/// This method is similar to the Dequeue method,
		/// but Peek does not modify the queue. 
		/// A null reference can be added to the Queue as a value. 
		/// To distinguish between a null value and the end of the queue,
		/// check the Count property or
		/// catch the InvalidOperationException,
		/// which is thrown when the Queue is empty.
		/// </remarks>
		/// <exception>
		///                The queue is empty.
		///     <cref>InvalidOpertionException</cref>
		/// </exception>
		public T Peek()
		{
			lock (_syncRoot)
			{
				if (_count == 0)
					throw new InvalidOperationException("The Queue is empty.");

				T value = _buffer[_head];
				return value;
			}
		}

		/// <summary>
		/// Returns the object at the beginning
		/// of the Queue without removing it.
		/// Similar to the Peek method, however this method
		/// will not throw exception if
		/// queue is empty, but instead will return false.
		/// </summary>
		/// <param name="value">The object at the beginning
		///          of the Queue or null if empty.</param>
		/// <returns>The object at the beginning of the Queue.</returns>
		public bool TryPeek(out T value)
		{
			lock (_syncRoot)
			{
				if (_count == 0)
				{
					value = default(T);
					return false;
				}
				value = _buffer[_head];
			}
			return true;
		}

		/// <summary>
		/// Removes all objects from the Queue.
		/// </summary>
		/// <remarks>
		/// Count is set to zero. Size does not change.
		/// </remarks>
		public void Clear()
		{
			lock (_syncRoot)
			{
				_count = 0;
				_head = 0;
				_tail = 0;
				for (int i = 0; i < _buffer.Length; i++)
				{
					_buffer[i] = default(T);
				}
			}
		}

		#endregion

		#region ICollection Members
		/// <summary>
		/// Gets a value indicating whether access
		/// to the Queue is synchronized (thread-safe).
		/// </summary>
		public bool IsSynchronized => true;

		/// <summary>
		/// Returns the max elements allowed
		/// in the queue before blocking Enqueue
		/// operations.  This is the size set in the constructor.
		/// </summary>
		public int Size => _size;

		/// <summary>
		/// Gets the number of elements contained in the Queue.
		/// </summary>
		public int Count
		{
			get { lock (_syncRoot) { return _count; } }
		}

		/// <summary>
		/// Copies the Queue elements to an existing one-dimensional Array,
		/// starting at the specified array index.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional Array that is the destination
		/// of the elements copied from Queue.
		/// The Array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index
		///     in array at which copying begins.</param>
		public void CopyTo(Array array, int index)
		{
			object[] tmpArray = Values;
			tmpArray.CopyTo(array, index);
		}

		/// <summary>
		/// Gets an object that can be used to synchronize
		/// access to the Queue.
		/// </summary>
		public object SyncRoot => _syncRoot;

		#endregion

		#region IEnumerable Members
		/// <summary>
		/// GetEnumerator not implemented. You can't enumerate
		/// the active queue as you would an array as it is dynamic
		/// with active gets and puts. You could
		/// if you locked it first and unlocked after
		/// enumeration, but that does not work well for GetEnumerator.
		/// The recommended method is to Get Values
		/// and enumerate the returned array copy.
		/// That way the queue is locked for
		/// only a short time and a copy returned
		/// so that can be safely enumerated using
		/// the array's enumerator. You could also
		/// create a custom enumerator that would
		/// dequeue the objects until empty queue,
		/// but that is a custom need. 
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException("Not Implemented.");
		}
		#endregion
	} // End BlockingQueue

	public class QueueTimeoutException : SpiderException
	{
		public QueueTimeoutException() : base("Queue method timed out on wait.")
		{
		}
	}
}

