using System;
using System.Collections;

namespace DotnetSpider.Core.Infrastructure
{
	public static class BitArrayExtensions
	{
		private const int BitsPerInt32 = 32;
		private const int BitsPerByte = 8;

		public static void CopyTo(this BitArray bits, Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_NeedNonNegNum");

			if (array.Rank != 1)
				throw new ArgumentException("Arg_RankMultiDimNotSupported");

			int[] tmp = new int[bits.Length];
			int ii = 0;
			foreach (var bit in bits)
			{
				tmp[ii] = (int)bit;
				++ii;
			}
			if (array is int[])
			{
				Array.Copy(tmp, 0, array, index, GetArrayLength(bits.Length, BitsPerInt32));
			}
			else if (array is byte[])
			{
				int arrayLength = GetArrayLength(bits.Length, BitsPerByte);
				if ((array.Length - index) < arrayLength)
					throw new ArgumentException("Argument_InvalidOffLen");

				byte[] b = (byte[])array;
				for (int i = 0; i < arrayLength; i++)
					b[index + i] = (byte)((tmp[i / 4] >> ((i % 4) * 8)) & 0x000000FF); // Shift to bring the required byte to LSB, then mask
			}
			else if (array is bool[])
			{
				if (array.Length - index < bits.Length)
					throw new ArgumentException("Argument_InvalidOffLen");

				bool[] b = (bool[])array;
				for (int i = 0; i < bits.Length; i++)
					b[index + i] = ((tmp[i / 32] >> (i % 32)) & 0x00000001) != 0;
			}
			else
				throw new ArgumentException("Arg_BitArrayTypeUnsupported");
		}

		private static int GetArrayLength(int n, int div)
		{
			if (div <= 0)
			{
				throw new ArgumentException("GetArrayLength: div arg must be greater than 0");
			}
			return n > 0 ? (((n - 1) / div) + 1) : 0;
		}
	}
}