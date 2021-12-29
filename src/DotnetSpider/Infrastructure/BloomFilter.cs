using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DotnetSpider.Infrastructure
{
	/// <summary>
	/// BloomFilter
	/// </summary>
	public class BloomFilter
	{
		private readonly BitArray _bitSet;
		private readonly int _bitSetSize;
		private readonly double _bitsPerElement;
		private readonly int _expectedNumberOfFilterElements; // expected (maximum) number of elements to be added
		private int _numberOfAddedElements; // number of elements actually added to the Bloom filter
		private readonly int _k; // number of hash functions
		private static readonly Encoding _charset = Encoding.UTF8; // encoding used for storing hash values as strings

		/// <summary>
		/// The digest method is reused between instances
		/// </summary>
		/// <remarks>MD5 gives good enough accuracy in most circumstances. Change to SHA1 if it's needed</remarks>
		private static readonly KeyedHashAlgorithm _keyedHashAlgorithm = new HMACSHA1();

		/// <summary>
		/// Constructs an empty Bloom filter. The total length of the Bloom filter will be
		/// c*n.
		/// </summary>
		/// <param name="c">is the number of bits used per element.</param>
		/// <param name="n">is the expected number of elements the filter will contain.</param>
		/// <param name="k">is the number of hash functions used.</param>
		public BloomFilter(double c, int n, int k)
		{
			_expectedNumberOfFilterElements = n;
			_k = k;
			_bitsPerElement = c;
			_bitSetSize = (int)Math.Ceiling(c * n);
			_numberOfAddedElements = 0;
			_bitSet = new BitArray(_bitSetSize);
		}

		/// <summary>
		/// Constructs an empty Bloom filter. The optimal number of hash functions (k) is estimated from the total size of the Bloom
		/// and the number of expected elements.
		/// </summary>
		/// <param name="bitSetSize">defines how many bits should be used in total for the filter.</param>
		/// <param name="expectedNumberOElements">defines the maximum number of elements the filter is expected to contain.</param>
		public BloomFilter(int bitSetSize, int expectedNumberOElements)
			: this(
				bitSetSize / (double)expectedNumberOElements,
				expectedNumberOElements,
				(int)Math.Round(bitSetSize / (double)expectedNumberOElements * Math.Log(2.0))
			)
		{
		}

		/// <summary>
		/// Constructs an empty Bloom filter with a given false positive probability. The number of bits per
		/// element and the number of hash functions is estimated
		/// to match the false positive probability.
		/// </summary>
		/// <param name="falsePositiveProbability">is the desired false positive probability.</param>
		/// <param name="expectedNumberOfElements">is the expected number of elements in the Bloom filter.</param>
		public BloomFilter(double falsePositiveProbability, int expectedNumberOfElements)
			: this(
				Math.Ceiling(-(Math.Log(falsePositiveProbability) / Math.Log(2))) / Math.Log(2), // c = k / ln(2)
				expectedNumberOfElements,
				(int)Math.Ceiling(-(Math.Log(falsePositiveProbability) / Math.Log(2))) // k = ceil(-log_2(false prob.))
			)
		{
		}

		/// <summary>
		/// Construct a new Bloom filter based on existing Bloom filter data.
		/// </summary>
		/// <param name="bitSetSize">defines how many bits should be used for the filter.</param>
		/// <param name="expectedNumberOfFilterElements">defines the maximum number of elements the filter is expected to contain.</param>
		/// <param name="actualNumberOfFilterElements">specifies how many elements have been inserted into the <code>filterData</code> BitArray.</param>
		/// <param name="filterData">a BitArray representing an existing Bloom filter.</param>
		public BloomFilter(int bitSetSize, int expectedNumberOfFilterElements, int actualNumberOfFilterElements,
			BitArray filterData)
			: this(bitSetSize, expectedNumberOfFilterElements)
		{
			_bitSet = filterData;
			_numberOfAddedElements = actualNumberOfFilterElements;
		}

		/// <summary>
		/// Generates a digest based on the contents of a string.
		/// </summary>
		/// <param name="val">specifies the input data.</param>
		/// <param name="charset">specifies the encoding of the input data.</param>
		/// <returns>digest as long.</returns>
		public static int CreateHash(string val, Encoding charset)
		{
			return CreateHash(charset.GetBytes(val));
		}

		/// <summary>
		/// Generates a digest based on the contents of a string.
		/// </summary>
		/// <param name="val">specifies the input data. The encoding is expected to be UTF-8.</param>
		/// <returns>digest as long.</returns>
		public static int CreateHash(string val)
		{
			return CreateHash(val, _charset);
		}

		/// <summary>
		/// Generates a digest based on the contents of an array of bytes.
		/// </summary>
		/// <param name="data">specifies input data.</param>
		/// <returns>digest as long.</returns>
		public static int CreateHash(byte[] data)
		{
			return CreateHashes(data, 1)[0];
		}

		/// <summary>
		/// Generates digests based on the contents of an array of bytes and splits the result into 4-byte int's and store them in an array. The
		/// digest function is called until the required number of int's are produced. For each call to digest a salt
		/// is prepended to the data. The salt is increased by 1 for each call.
		/// </summary>
		/// <param name="data">specifies input data</param>
		/// <param name="hashes">number of hashes/int's to produce</param>
		/// <returns>array of int-sized hashes</returns>
		public static int[] CreateHashes(byte[] data, int hashes)
		{
			var result = new int[hashes];

			var k = 0;
			var salt = new byte[1];
			while (k < hashes)
			{
				byte[] digest;
				lock (_keyedHashAlgorithm)
				{
					_keyedHashAlgorithm.Key = salt;
					salt[0]++;
					digest = _keyedHashAlgorithm.ComputeHash(data);
				}

				for (var i = 0; i < digest.Length / 4 && k < hashes; i++)
				{
					var h = 0;
					for (var j = i * 4; j < i * 4 + 4; j++)
					{
						h <<= 8;
						h |= digest[j] & 0xFF;
					}

					result[k] = h;
					k++;
				}
			}

			return result;
		}

		/// <summary>
		/// Compares the contents of two instances to see if they are equal.
		/// </summary>
		/// <param name="obj">is the object to compare to.</param>
		/// <returns>True if the contents of the objects are equal.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (GetType() != obj.GetType())
			{
				return false;
			}

			var other = (BloomFilter)obj;
			if (_expectedNumberOfFilterElements != other._expectedNumberOfFilterElements)
			{
				return false;
			}

			if (_k != other._k)
			{
				return false;
			}

			if (_bitSetSize != other._bitSetSize)
			{
				return false;
			}

			if (_bitSet != other._bitSet && (_bitSet == null || !Equals(_bitSet, other._bitSet)))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Calculates a hash code for this class.
		/// <remarks>performance concerns : note that we read all the bits of bitset to compute the hash</remarks>
		/// <returns>hash code representing the contents of an instance of this class.</returns>
		/// </summary>
		public override int GetHashCode()
		{
			var hash = 7;
			hash = 61 * hash + (_bitSet != null ? HashBytes(_bitSet) : 0);
			hash = 61 * hash + _expectedNumberOfFilterElements;
			hash = 61 * hash + _bitSetSize;
			hash = 61 * hash + _k;
			return hash;
		}


		/// <summary>
		/// Calculates the expected probability of false positives based on
		/// the number of expected filter elements and the size of the Bloom filter.
		/// <br /><br />
		/// The value returned by this method is the <i>expected</i> rate of false
		/// positives, assuming the number of inserted elements equals the number of
		/// expected elements. If the number of elements in the Bloom filter is less
		/// than the expected value, the true probability of false positives will be lower.
		/// </summary>
		/// <returns>expected probability of false positives.</returns>
		public double ExpectedFalsePositiveProbability()
		{
			return GetFalsePositiveProbability(_expectedNumberOfFilterElements);
		}

		/// <summary>
		/// Calculate the probability of a false positive given the specified
		/// number of inserted elements.
		/// </summary>
		/// <param name="numberOfElements">number of inserted elements.</param>
		/// <returns>probability of a false positive.</returns>
		public double GetFalsePositiveProbability(double numberOfElements)
		{
			// (1 - e^(-k * n / m)) ^ k
			return Math.Pow(1 - Math.Exp(-_k * numberOfElements
			                             / _bitSetSize), _k);
		}

		/// <summary>
		/// Get the current probability of a false positive. The probability is calculated from
		/// the size of the Bloom filter and the current number of elements added to it.
		/// </summary>
		/// <returns>probability of false positives.</returns>
		public double GetFalsePositiveProbability()
		{
			return GetFalsePositiveProbability(_numberOfAddedElements);
		}


		/// <summary>
		/// Returns the value chosen for K.<br />
		/// <br />
		/// K is the optimal number of hash functions based on the size
		/// of the Bloom filter and the expected number of inserted elements.
		/// </summary>
		/// <returns>optimal k.</returns>
		public int K => _k;

		/// <summary>
		/// Sets all bits to false in the Bloom filter.
		/// </summary>
		public void Clear()
		{
			_bitSet.SetAll(false);
			_numberOfAddedElements = 0;
		}

		/// <summary>
		/// Adds an object to the Bloom filter. The output from the object's
		/// ToString() method is used as input to the hash functions.
		/// </summary>
		/// <param name="element">is an element to register in the Bloom filter.</param>
		public void Add(object element)
		{
			Add(_charset.GetBytes(element.ToString()));
		}

		/// <summary>
		/// Adds an array of bytes to the Bloom filter.
		/// </summary>
		/// <param name="bytes">array of bytes to add to the Bloom filter.</param>
		public void Add(byte[] bytes)
		{
			var hashes = CreateHashes(bytes, _k);
			foreach (var hash in hashes)
			{
				_bitSet.Set(Math.Abs(hash % _bitSetSize), true);
			}

			_numberOfAddedElements++;
		}

		/// <summary>
		/// Adds all elements from a Collection to the Bloom filter.
		/// </summary>
		/// <param name="c">Collection of elements.</param>
		public void AddAll(IEnumerable<object> c)
		{
			foreach (var element in c)
			{
				Add(element);
			}
		}

		/// <summary>
		/// Adds all elements from a Collection to the Bloom filter.
		/// </summary>
		/// <param name="c">Collection of elements.</param>
		public void AddAll(IEnumerable<byte[]> c)
		{
			foreach (var byteArray in c)
			{
				Add(byteArray);
			}
		}

		/// <summary>
		/// Returns true if the element could have been inserted into the Bloom filter.
		/// Use getFalsePositiveProbability() to calculate the probability of this
		/// being correct.
		/// </summary>
		/// <param name="element">element to check.</param>
		/// <returns>true if the element could have been inserted into the Bloom filter.</returns>
		public bool Contains(object element)
		{
			return Contains(_charset.GetBytes(element.ToString()));
		}

		/// <summary>
		/// Returns true if the array of bytes could have been inserted into the Bloom filter.
		/// Use getFalsePositiveProbability() to calculate the probability of this
		/// being correct.
		/// </summary>
		/// <param name="bytes">array of bytes to check.</param>
		/// <returns>true if the array could have been inserted into the Bloom filter.</returns>
		public bool Contains(byte[] bytes)
		{
			var hashes = CreateHashes(bytes, _k);
			return hashes.All(hash => _bitSet.Get(Math.Abs(hash % _bitSetSize)));
		}

		/// <summary>
		/// Returns true if all the elements of a Collection could have been inserted
		/// into the Bloom filter. Use getFalsePositiveProbability() to calculate the
		/// probability of this being correct.
		/// </summary>
		/// <param name="c">elements to check.</param>
		/// <returns>true if all the elements in c could have been inserted into the Bloom filter.</returns>
		public bool ContainsAll(IEnumerable<object> c)
		{
			return c.All(Contains);
		}

		/// <summary>
		/// Read a single bit from the Bloom filter.
		/// </summary>
		/// <param name="bit">the bit to read.</param>
		/// <returns>true if the bit is set, false if it is not.</returns>
		public bool GetBit(int bit)
		{
			return _bitSet.Get(bit);
		}

		/// <summary>
		/// Set a single bit in the Bloom filter.
		/// </summary>
		/// <param name="bit">is the bit to set.</param>
		/// <param name="value">If true, the bit is set. If false, the bit is cleared.</param>
		public void SetBit(int bit, bool value)
		{
			_bitSet.Set(bit, value);
		}

		/// <summary>
		/// Return the bit set used to store the Bloom filter.
		/// </summary>
		/// <returns>bit set representing the Bloom filter.</returns>
		public BitArray GetBitSet()
		{
			return _bitSet;
		}

		/// <summary>
		/// Returns the number of bits in the Bloom filter. Use count() to retrieve
		/// the number of inserted elements.
		/// </summary>
		/// <returns>the size of the bitset used by the Bloom filter.</returns>
		public int Size()
		{
			return _bitSetSize;
		}

		/// <summary>
		/// Returns the number of elements added to the Bloom filter after it
		/// was constructed or after clear() was called.
		/// </summary>
		/// <returns>number of elements added to the Bloom filter.</returns>
		public int Count()
		{
			return _numberOfAddedElements;
		}

		/// <summary>
		/// Returns the expected number of elements to be inserted into the filter.
		/// This value is the same value as the one passed to the constructor.
		/// </summary>
		/// <returns>expected number of elements.</returns>
		public int GetExpectedNumberOfElements()
		{
			return _expectedNumberOfFilterElements;
		}

		/// <summary>
		/// Get expected number of bits per element when the Bloom filter is full. This value is set by the constructor
		/// when the Bloom filter is created. See also getBitsPerElement().
		/// </summary>
		/// <returns>expected number of bits per element.</returns>
		public double GetExpectedBitsPerElement()
		{
			return _bitsPerElement;
		}

		/// <summary>
		/// Get actual number of bits per element based on the number of elements that have currently been inserted and the length
		/// of the Bloom filter. See also getExpectedBitsPerElement().
		/// </summary>
		/// <returns>number of bits per element.</returns>
		public double GetBitsPerElement()
		{
			return _bitSetSize / (double)_numberOfAddedElements;
		}

		/// <summary>
		/// Generate a hash value from an array of bits
		/// </summary>
		/// <remarks>voir http://blog.roblevine.co.uk for comparison of hash algorithm implementations</remarks>
		/// <param name="data">array of bits to hash</param>
		/// <returns></returns>
		public static int HashBytes(BitArray data)
		{
			// convert bit array to integer array
			var intArray = new int[(data.Length + 31) / 32];
			data.CopyTo(intArray, 0);
			// compute the hash from integer array values
			unchecked
			{
				return intArray.Aggregate(23, (current, n) => current * 37 + n);
			}
		}

		/// <summary>
		/// Check if two arrays of bits are equals
		/// Returns true if every bit of this first array is equal to the corresponding bit of the second, false otherwise
		/// </summary>
		public static bool Equals(BitArray a, BitArray b)
		{
			if (a.Length != b.Length) return false;

			var enumA = a.GetEnumerator();
			var enumB = b.GetEnumerator();

			while (enumA.MoveNext() && enumB.MoveNext())
			{
				if (enumB.Current != null && enumA.Current != null && (bool)enumA.Current != (bool)enumB.Current)
					return false;
			}

			return true;
		}
	}
}
