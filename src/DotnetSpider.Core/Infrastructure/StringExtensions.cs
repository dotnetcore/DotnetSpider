using System;

namespace DotnetSpider.Core.Infrastructure
{
	public static class StringExtensions
	{
		public static string GetRandomString(this string value, int count)
		{
			string checkCode = String.Empty; //存放随机码的字符串   

			Random random = new Random();

			for (int i = 0; i < count; i++) //产生4位校验码   
			{
				var number = random.Next();
				number = number%36;
				if (number < 10)
				{
					number += 48; //数字0-9编码在48-57   
				}
				else
				{
					number += 55; //字母A-Z编码在65-90   
				}

				checkCode += ((char) number).ToString();
			}
			return checkCode;
		}

		public static int CountMatches(string str, string sub)
		{
			if (!String.IsNullOrEmpty(str) && !String.IsNullOrEmpty(sub))
			{
				int count = 0;

				for (int idx = 0; (idx = str.IndexOf(sub, idx, StringComparison.Ordinal)) != -1; idx += sub.Length)
				{
					++count;
				}

				return count;
			}
			return 0;
		}

		public static bool RegionMatches(this string value, bool ignoreCase, int toffset, string other, int ooffset, int len)
		{
			char[] ta = value.ToCharArray();
			int to = toffset;
			char[] pa = other.ToCharArray();
			int po = ooffset;
			// Note: toffset, ooffset, or len might be near -1>>>1.
			if ((ooffset < 0) || (toffset < 0)
			    || (toffset > (long)value.Length - len)
			    || (ooffset > (long)other.Length - len))
			{
				return false;
			}
			while (len-- > 0)
			{
				char c1 = ta[to++];
				char c2 = pa[po++];
				if (c1 == c2)
				{
					continue;
				}
				if (ignoreCase)
				{
					// If characters don't match but case may be ignored,
					// try converting both characters to uppercase.
					// If the results match, then the comparison scan should
					// continue.
					char u1 = char.ToUpper(c1);
					char u2 = char.ToUpper(c2);
					if (u1 == u2)
					{
						continue;
					}
					// Unfortunately, conversion to uppercase does not work properly
					// for the Georgian alphabet, which has strange rules about case
					// conversion.  So we need to make one last check before
					// exiting.
					if (char.ToLower(u1) == char.ToLower(u2))
					{
						continue;
					}
				}
				return false;
			}
			return true;
		}
	}
}
