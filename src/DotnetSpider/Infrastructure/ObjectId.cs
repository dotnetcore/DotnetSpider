using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;

namespace DotnetSpider.Infrastructure
{
#if NET452
    [Serializable]
#endif
	public struct ObjectId : IComparable<ObjectId>, IEquatable<ObjectId>, IConvertible
	{
		// private static fields
		private static readonly ObjectId EmptyInstance = default;
		private static readonly long Random = CalculateRandomValue();
		private static int _staticIncrement = new Random().Next();
		private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		// private fields
		private readonly int _a;
		private readonly int _b;
		private readonly int _c;

		/// <summary>
		/// System.Text.Json.JsonSerializer 需要注册类型
		/// </summary>
		internal static void AddTypeDescriptor()
		{
			TypeDescriptor.AddAttributes(typeof(ObjectId), new TypeConverterAttribute(typeof(StringConverter)));
		}

		// constructors
		/// <summary>
		/// Initializes a new instance of the ObjectId class.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		public ObjectId(byte[] bytes)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException(nameof(bytes));
			}

			if (bytes.Length != 12)
			{
				throw new ArgumentException("Byte array must be 12 bytes long", nameof(bytes));
			}

			FromByteArray(bytes, 0, out _a, out _b, out _c);
		}

		public ObjectId(ReadOnlySpan<byte> bytes)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException(nameof(bytes));
			}

			if (bytes.Length != 12)
			{
				throw new ArgumentException("Byte array must be 12 bytes long", nameof(bytes));
			}

			FromByteArray(bytes, 0, out _a, out _b, out _c);
		}

		/// <summary>
		/// Initializes a new instance of the ObjectId class.
		/// </summary>
		/// <param name="value">The value.</param>
		public ObjectId(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			var bytes = ParseHexString(value);
			FromByteArray(bytes, 0, out _a, out _b, out _c);
		}

		private ObjectId(int a, int b, int c)
		{
			_a = a;
			_b = b;
			_c = c;
		}

		// public static properties
		/// <summary>
		/// Gets an instance of ObjectId where the value is empty.
		/// </summary>
		public static ObjectId Empty => EmptyInstance;

		// public properties
		/// <summary>
		/// Gets the timestamp.
		/// </summary>
		public int Timestamp => _a;

		/// <summary>
		/// Gets the machine.
		/// </summary>
		public int Machine => (_b >> 8) & 0xffffff;

		/// <summary>
		/// Gets the PID.
		/// </summary>
		public short Pid => (short)(((_b << 8) & 0xff00) | ((_c >> 24) & 0x00ff));

		/// <summary>
		/// Gets the increment.
		/// </summary>
		public int Increment => _c & 0xffffff;

		/// <summary>
		/// Gets the creation time (derived from the timestamp).
		/// </summary>
		public DateTime CreationTime => UnixEpoch.AddSeconds((uint)Timestamp);

		// public operators
		/// <summary>
		/// Compares two ObjectIds.
		/// </summary>
		/// <param name="lhs">The first ObjectId.</param>
		/// <param name="rhs">The other ObjectId</param>
		/// <returns>True if the first ObjectId is less than the second ObjectId.</returns>
		public static bool operator <(ObjectId lhs, ObjectId rhs)
		{
			return lhs.CompareTo(rhs) < 0;
		}

		/// <summary>
		/// Compares two ObjectIds.
		/// </summary>
		/// <param name="lhs">The first ObjectId.</param>
		/// <param name="rhs">The other ObjectId</param>
		/// <returns>True if the first ObjectId is less than or equal to the second ObjectId.</returns>
		public static bool operator <=(ObjectId lhs, ObjectId rhs)
		{
			return lhs.CompareTo(rhs) <= 0;
		}

		/// <summary>
		/// Compares two ObjectIds.
		/// </summary>
		/// <param name="lhs">The first ObjectId.</param>
		/// <param name="rhs">The other ObjectId.</param>
		/// <returns>True if the two ObjectIds are equal.</returns>
		public static bool operator ==(ObjectId lhs, ObjectId rhs)
		{
			return lhs.Equals(rhs);
		}

		/// <summary>
		/// Compares two ObjectIds.
		/// </summary>
		/// <param name="lhs">The first ObjectId.</param>
		/// <param name="rhs">The other ObjectId.</param>
		/// <returns>True if the two ObjectIds are not equal.</returns>
		public static bool operator !=(ObjectId lhs, ObjectId rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// Compares two ObjectIds.
		/// </summary>
		/// <param name="lhs">The first ObjectId.</param>
		/// <param name="rhs">The other ObjectId</param>
		/// <returns>True if the first ObjectId is greather than or equal to the second ObjectId.</returns>
		public static bool operator >=(ObjectId lhs, ObjectId rhs)
		{
			return lhs.CompareTo(rhs) >= 0;
		}

		/// <summary>
		/// Compares two ObjectIds.
		/// </summary>
		/// <param name="lhs">The first ObjectId.</param>
		/// <param name="rhs">The other ObjectId</param>
		/// <returns>True if the first ObjectId is greather than the second ObjectId.</returns>
		public static bool operator >(ObjectId lhs, ObjectId rhs)
		{
			return lhs.CompareTo(rhs) > 0;
		}

		// public static methods
		/// <summary>
		/// Generates a new ObjectId with a unique value.
		/// </summary>
		/// <returns>An ObjectId.</returns>
		public static ObjectId CreateId()
		{
			return CreateId(GetTimestampFromDateTime(DateTime.UtcNow));
		}

		/// <summary>
		/// Generates a new ObjectId with a unique value (with the timestamp component based on a given DateTime).
		/// </summary>
		/// <param name="timestamp">The timestamp component (expressed as a DateTime).</param>
		/// <returns>An ObjectId.</returns>
		public static ObjectId CreateId(DateTime timestamp)
		{
			return CreateId(GetTimestampFromDateTime(timestamp));
		}

		/// <summary>
		/// Generates a new ObjectId with a unique value (with the given timestamp).
		/// </summary>
		/// <param name="timestamp">The timestamp component.</param>
		/// <returns>An ObjectId.</returns>
		public static ObjectId CreateId(int timestamp)
		{
			int increment = Interlocked.Increment(ref _staticIncrement) & 0x00ffffff; // only use low order 3 bytes
			return Create(timestamp, Random, increment);
		}

		/// <summary>
		/// Parses a string and creates a new ObjectId.
		/// </summary>
		/// <param name="s">The string value.</param>
		/// <returns>A ObjectId.</returns>
		public static ObjectId Parse(string s)
		{
			if (s == null)
			{
				throw new ArgumentNullException(nameof(s));
			}

			if (TryParse(s, out var objectId))
			{
				return objectId;
			}
			else
			{
				var message = $"'{s}' is not a valid 24 digit hex string.";
				throw new FormatException(message);
			}
		}

		/// <summary>
		/// Tries to parse a string and create a new ObjectId.
		/// </summary>
		/// <param name="s">The string value.</param>
		/// <param name="objectId">The new ObjectId.</param>
		/// <returns>True if the string was parsed successfully.</returns>
		public static bool TryParse(string s, out ObjectId objectId)
		{
			// don't throw ArgumentNullException if s is null
			if (s != null && s.Length == 24)
			{
				if (TryParseHexString(s, out var bytes))
				{
					objectId = new ObjectId(bytes);
					return true;
				}
			}

			objectId = default;
			return false;
		}

		/// <summary>
		/// Unpacks a byte array into the components of an ObjectId.
		/// </summary>
		/// <param name="bytes">A byte array.</param>
		/// <param name="timestamp">The timestamp.</param>
		/// <param name="machine">The machine hash.</param>
		/// <param name="pid">The PID.</param>
		/// <param name="increment">The increment.</param>
		[Obsolete("This method will be removed in a later release.")]
		public static void Unpack(byte[] bytes, out int timestamp, out int machine, out short pid, out int increment)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}

			if (bytes.Length != 12)
			{
				throw new ArgumentOutOfRangeException("bytes", "Byte array must be 12 bytes long.");
			}

			timestamp = (bytes[0] << 24) + (bytes[1] << 16) + (bytes[2] << 8) + bytes[3];
			machine = (bytes[4] << 16) + (bytes[5] << 8) + bytes[6];
			pid = (short)((bytes[7] << 8) + bytes[8]);
			increment = (bytes[9] << 16) + (bytes[10] << 8) + bytes[11];
		}

		// private static methods
		private static long CalculateRandomValue()
		{
			var seed = (int)DateTime.UtcNow.Ticks ^ GetMachineHash() ^ GetPid();
			var random = new Random(seed);
			var high = random.Next();
			var low = random.Next();
			var combined = (long)((ulong)(uint)high << 32 | (uint)low);
			return combined & 0xffffffffff;
		}

		private static ObjectId Create(int timestamp, long random, int increment)
		{
			if (random < 0 || random > 0xffffffffff)
			{
				throw new ArgumentOutOfRangeException(nameof(random),
					"The random value must be between 0 and 1099511627775 (it must fit in 5 bytes).");
			}

			if (increment < 0 || increment > 0xffffff)
			{
				throw new ArgumentOutOfRangeException(nameof(increment),
					"The increment value must be between 0 and 16777215 (it must fit in 3 bytes).");
			}

			var a = timestamp;
			var b = (int)(random >> 8); // first 4 bytes of random
			var c = (int)(random << 24) | increment; // 5th byte of random and 3 byte increment
			return new ObjectId(a, b, c);
		}

		/// <summary>
		/// Gets the current process id.  This method exists because of how CAS operates on the call stack, checking
		/// for permissions before executing the method.  Hence, if we inlined this call, the calling method would not execute
		/// before throwing an exception requiring the try/catch at an even higher level that we don't necessarily control.
		/// </summary>
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int GetCurrentProcessId()
		{
			return Process.GetCurrentProcess().Id;
		}

		private static int GetMachineHash()
		{
			// use instead of Dns.HostName so it will work offline
			var machineName = GetMachineName();
			return 0x00ffffff & machineName.GetHashCode(); // use first 3 bytes of hash
		}

		private static string GetMachineName()
		{
			return Environment.MachineName;
		}

		private static short GetPid()
		{
			try
			{
				return (short)GetCurrentProcessId(); // use low order two bytes only
			}
			catch (SecurityException)
			{
				return 0;
			}
		}

		private static int GetTimestampFromDateTime(DateTime timestamp)
		{
			var secondsSinceEpoch =
				(long)Math.Floor((ToUniversalTime(timestamp) - UnixEpoch).TotalSeconds);
			if (secondsSinceEpoch < uint.MinValue || secondsSinceEpoch > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(timestamp));
			}

			return (int)(uint)secondsSinceEpoch;
		}

		/// <summary>
		/// Converts a DateTime to UTC (with special handling for MinValue and MaxValue).
		/// </summary>
		/// <param name="dateTime">A DateTime.</param>
		/// <returns>The DateTime in UTC.</returns>
		private static DateTime ToUniversalTime(DateTime dateTime)
		{
			if (dateTime == DateTime.MinValue)
			{
				return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
			}
			else if (dateTime == DateTime.MaxValue)
			{
				return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
			}
			else
			{
				return dateTime.ToUniversalTime();
			}
		}

		private static void FromByteArray(byte[] bytes, int offset, out int a, out int b, out int c)
		{
			a = (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
			b = (bytes[offset + 4] << 24) | (bytes[offset + 5] << 16) | (bytes[offset + 6] << 8) | bytes[offset + 7];
			c = (bytes[offset + 8] << 24) | (bytes[offset + 9] << 16) | (bytes[offset + 10] << 8) | bytes[offset + 11];
		}

		private static void FromByteArray(ReadOnlySpan<byte> bytes, int offset, out int a, out int b, out int c)
		{
			a = (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
			b = (bytes[offset + 4] << 24) | (bytes[offset + 5] << 16) | (bytes[offset + 6] << 8) | bytes[offset + 7];
			c = (bytes[offset + 8] << 24) | (bytes[offset + 9] << 16) | (bytes[offset + 10] << 8) | bytes[offset + 11];
		}

		// public methods
		/// <summary>
		/// Compares this ObjectId to another ObjectId.
		/// </summary>
		/// <param name="other">The other ObjectId.</param>
		/// <returns>A 32-bit signed integer that indicates whether this ObjectId is less than, equal to, or greather than the other.</returns>
		public int CompareTo(ObjectId other)
		{
			int result = ((uint)_a).CompareTo((uint)other._a);
			if (result != 0)
			{
				return result;
			}

			result = ((uint)_b).CompareTo((uint)other._b);
			if (result != 0)
			{
				return result;
			}

			return ((uint)_c).CompareTo((uint)other._c);
		}

		/// <summary>
		/// Compares this ObjectId to another ObjectId.
		/// </summary>
		/// <param name="rhs">The other ObjectId.</param>
		/// <returns>True if the two ObjectIds are equal.</returns>
		public bool Equals(ObjectId rhs)
		{
			return
				_a == rhs._a &&
				_b == rhs._b &&
				_c == rhs._c;
		}

		/// <summary>
		/// Compares this ObjectId to another object.
		/// </summary>
		/// <param name="obj">The other object.</param>
		/// <returns>True if the other object is an ObjectId and equal to this one.</returns>
		public override bool Equals(object obj)
		{
			if (obj is ObjectId id)
			{
				return Equals(id);
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the hash code.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			int hash = 17;
			hash = 37 * hash + _a.GetHashCode();
			hash = 37 * hash + _b.GetHashCode();
			hash = 37 * hash + _c.GetHashCode();
			return hash;
		}

		/// <summary>
		/// Converts the ObjectId to a byte array.
		/// </summary>
		/// <returns>A byte array.</returns>
		public byte[] ToByteArray()
		{
			var bytes = new byte[12];
			ToByteArray(bytes, 0);
			return bytes;
		}

		public static byte[] ParseHexString(string s)
		{
			if (s == null)
			{
				throw new ArgumentNullException(nameof(s));
			}

			if (!TryParseHexString(s, out var bytes))
			{
				throw new FormatException("String should contain only hexadecimal digits.");
			}

			return bytes;
		}

		/// <summary>
		/// Tries to parse a hex string to a byte array.
		/// </summary>
		/// <param name="s">The hex string.</param>
		/// <param name="bytes">A byte array.</param>
		/// <returns>True if the hex string was successfully parsed.</returns>
		private static bool TryParseHexString(string s, out byte[] bytes)
		{
			bytes = null;

			if (s == null)
			{
				return false;
			}

			var buffer = new byte[(s.Length + 1) / 2];

			var i = 0;
			var j = 0;

			if (s.Length % 2 == 1)
			{
				// if s has an odd length assume an implied leading "0"
				if (!TryParseHexChar(s[i++], out var y))
				{
					return false;
				}

				buffer[j++] = (byte)y;
			}

			while (i < s.Length)
			{
				if (!TryParseHexChar(s[i++], out var x))
				{
					return false;
				}

				if (!TryParseHexChar(s[i++], out var y))
				{
					return false;
				}

				buffer[j++] = (byte)((x << 4) | y);
			}

			bytes = buffer;
			return true;
		}

		// private static methods
		private static bool TryParseHexChar(char c, out int value)
		{
			if (c >= '0' && c <= '9')
			{
				value = c - '0';
				return true;
			}

			if (c >= 'a' && c <= 'f')
			{
				value = 10 + (c - 'a');
				return true;
			}

			if (c >= 'A' && c <= 'F')
			{
				value = 10 + (c - 'A');
				return true;
			}

			value = 0;
			return false;
		}

		/// <summary>
		/// Converts the ObjectId to a byte array.
		/// </summary>
		/// <param name="destination">The destination.</param>
		/// <param name="offset">The offset.</param>
		public void ToByteArray(byte[] destination, int offset)
		{
			if (destination == null)
			{
				throw new ArgumentNullException(nameof(destination));
			}

			if (offset + 12 > destination.Length)
			{
				throw new ArgumentException("Not enough room in destination buffer.", nameof(offset));
			}

			destination[offset + 0] = (byte)(_a >> 24);
			destination[offset + 1] = (byte)(_a >> 16);
			destination[offset + 2] = (byte)(_a >> 8);
			destination[offset + 3] = (byte)_a;
			destination[offset + 4] = (byte)(_b >> 24);
			destination[offset + 5] = (byte)(_b >> 16);
			destination[offset + 6] = (byte)(_b >> 8);
			destination[offset + 7] = (byte)_b;
			destination[offset + 8] = (byte)(_c >> 24);
			destination[offset + 9] = (byte)(_c >> 16);
			destination[offset + 10] = (byte)(_c >> 8);
			destination[offset + 11] = (byte)_c;
		}

		/// <summary>
		/// Returns a string representation of the value.
		/// </summary>
		/// <returns>A string representation of the value.</returns>
		public override string ToString()
		{
			var c = new char[24];
			c[0] = ToHexChar((_a >> 28) & 0x0f);
			c[1] = ToHexChar((_a >> 24) & 0x0f);
			c[2] = ToHexChar((_a >> 20) & 0x0f);
			c[3] = ToHexChar((_a >> 16) & 0x0f);
			c[4] = ToHexChar((_a >> 12) & 0x0f);
			c[5] = ToHexChar((_a >> 8) & 0x0f);
			c[6] = ToHexChar((_a >> 4) & 0x0f);
			c[7] = ToHexChar(_a & 0x0f);
			c[8] = ToHexChar((_b >> 28) & 0x0f);
			c[9] = ToHexChar((_b >> 24) & 0x0f);
			c[10] = ToHexChar((_b >> 20) & 0x0f);
			c[11] = ToHexChar((_b >> 16) & 0x0f);
			c[12] = ToHexChar((_b >> 12) & 0x0f);
			c[13] = ToHexChar((_b >> 8) & 0x0f);
			c[14] = ToHexChar((_b >> 4) & 0x0f);
			c[15] = ToHexChar(_b & 0x0f);
			c[16] = ToHexChar((_c >> 28) & 0x0f);
			c[17] = ToHexChar((_c >> 24) & 0x0f);
			c[18] = ToHexChar((_c >> 20) & 0x0f);
			c[19] = ToHexChar((_c >> 16) & 0x0f);
			c[20] = ToHexChar((_c >> 12) & 0x0f);
			c[21] = ToHexChar((_c >> 8) & 0x0f);
			c[22] = ToHexChar((_c >> 4) & 0x0f);
			c[23] = ToHexChar(_c & 0x0f);
			return new string(c);
		}

		/// <summary>
		/// Converts a value to a hex character.
		/// </summary>
		/// <param name="value">The value (assumed to be between 0 and 15).</param>
		/// <returns>The hex character.</returns>
		private static char ToHexChar(int value)
		{
			return (char)(value + (value < 10 ? '0' : 'a' - 10));
		}

		// explicit IConvertible implementation
		TypeCode IConvertible.GetTypeCode()
		{
			return TypeCode.Object;
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		string IConvertible.ToString(IFormatProvider provider)
		{
			return ToString();
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			switch (Type.GetTypeCode(conversionType))
			{
				case TypeCode.String:
					return ((IConvertible)this).ToString(provider);
				case TypeCode.Object:
					if (conversionType == typeof(object) || conversionType == typeof(ObjectId))
					{
						return this;
					}

					break;
			}

			throw new InvalidCastException();
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
	}
}
