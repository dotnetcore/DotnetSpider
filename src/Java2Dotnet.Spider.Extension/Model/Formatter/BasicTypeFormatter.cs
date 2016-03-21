using System;
using HtmlAgilityPack;

namespace Java2Dotnet.Spider.Extension.Model.Formatter
{
	public abstract class BasicTypeFormatter<T> : IObjectFormatter
	{
		public IObjectFormatter NextFormatter { get; set; }

		public virtual void InitParam(string[] extra)
		{
		}

		public virtual dynamic Format(string raw)
		{
			if (raw == null)
			{
				return default(T);
			}

			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(raw);

			return FormatTrimmed(document.DocumentNode.InnerText);
		}

		protected abstract dynamic FormatTrimmed(string raw);
	}

	public class StringFormatter : BasicTypeFormatter<string>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			if (string.IsNullOrEmpty(raw))
			{
				return null;
			}
			return raw.Trim();
		}
	}

	public class IntegerFormatter : BasicTypeFormatter<int>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			return int.Parse(raw);
		}
	}

	public class NullableIntegerFormatter : BasicTypeFormatter<int?>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			if (string.IsNullOrEmpty(raw))
			{
				return null;
			}
			return int.Parse(raw);
		}
	}

	public class NullableLongFormatter : BasicTypeFormatter<long?>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			if (string.IsNullOrEmpty(raw))
			{
				return null;
			}
			return long.Parse(raw);
		}
	}

	public class LongFormatter : BasicTypeFormatter<long>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			return long.Parse(raw);
		}
	}

	public class DoubleFormatter : BasicTypeFormatter<double>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			return double.Parse(raw);
		}
	}

	public class NullableDoubleFormatter : BasicTypeFormatter<double?>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			if (string.IsNullOrEmpty(raw))
			{
				return null;
			}
			return double.Parse(raw);
		}
	}

	public class FloatFormatter : BasicTypeFormatter<float>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			return float.Parse(raw);
		}
	}

	public class NullableFloatFormatter : BasicTypeFormatter<float?>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			if (string.IsNullOrEmpty(raw))
			{
				return null;
			}
			return float.Parse(raw);
		}
	}

	public class ShortFormatter : BasicTypeFormatter<short>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			return short.Parse(raw);
		}
	}

	public class NullableShortFormatter : BasicTypeFormatter<short?>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			if (string.IsNullOrEmpty(raw))
			{
				return null;
			}
			return short.Parse(raw);
		}
	}

	public class CharactorFormatter : BasicTypeFormatter<char>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			if (string.IsNullOrEmpty(raw))
			{
				if (raw != null) return raw[0];
			}
			return char.MinValue;
		}
	}

	public class ByteFormatter : BasicTypeFormatter<Byte>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			byte b;
			if (byte.TryParse(raw, out b))
			{
				return b;
			}
			return Byte.MinValue;
		}
	}

	public class DatetimeFormatter : BasicTypeFormatter<DateTime>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			DateTime b;
			if (DateTime.TryParse(raw, out b))
			{
				return b;
			}
			return b;
		}
	}

	public class BooleanFormatter : BasicTypeFormatter<bool>
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			bool b;
			if (bool.TryParse(raw, out b))
			{
				return b;
			}
			return false;
		}
	}
}
