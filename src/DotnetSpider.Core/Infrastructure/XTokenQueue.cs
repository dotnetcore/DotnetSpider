using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Infrastructure
{
	public class XTokenQueue
	{
		private string _queue;
		private int _pos;
		private static readonly string[] Quotes = { "\"", "\'" };

		public XTokenQueue(string data)
		{
			_queue = data ?? throw new ArgumentException("Object must not be null");
		}

		public bool IsEmpty => RemainingLength() == 0;

		private int RemainingLength()
		{
			return _queue.Length - _pos;
		}

		public char Peek()
		{
			return IsEmpty ? '\u0000' : _queue[_pos];
		}

		public void AddFirst(char c)
		{
			AddFirst(c.ToString());
		}

		public void AddFirst(string seq)
		{
			_queue = seq + _queue.Substring(_pos);
			_pos = 0;
		}

		public bool Matches(string seq)
		{
			return _queue.RegionMatches(true, _pos, seq, 0, seq.Length);
		}

		public bool MatchesRegex(string seq)
		{
			return Regex.IsMatch(_queue.Substring(_pos), seq, RegexOptions.IgnoreCase);
		}

		public bool MatchesCs(string seq)
		{
			return _queue.IndexOf(seq, _pos, StringComparison.Ordinal) > -1;
		}

		public bool MatchesAny(params string[] seq)
		{
			string[] arr = seq;
			int len = seq.Length;

			for (int i = 0; i < len; ++i)
			{
				string s = arr[i];
				if (Matches(s))
				{
					return true;
				}
			}

			return false;
		}

		public bool MatchesAny(params char[] seq)
		{
			if (IsEmpty)
			{
				return false;
			}
			else
			{
				char[] arr = seq;
				int len = seq.Length;

				for (int i = 0; i < len; ++i)
				{
					char c = arr[i];
					if (_queue[_pos] == c)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool MatchesStartTag()
		{
			return RemainingLength() >= 2 && _queue[_pos] == 60 && char.IsLetter(_queue[_pos + 1]);
		}

		public bool MatchChomp(string seq)
		{
			if (Matches(seq))
			{
				_pos += seq.Length;
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool MatchesWhitespace()
		{
			return !IsEmpty && char.IsWhiteSpace(_queue[_pos]);
		}

		public bool MatchesWord()
		{
			return !IsEmpty && (char.IsLetter(_queue[_pos]) || char.IsDigit(_queue[_pos]));
		}

		public void Advance()
		{
			if (!IsEmpty)
			{
				++_pos;
			}
		}

		public char Consume()
		{
			return _queue[_pos++];
		}

		public void Consume(string seq)
		{
			if (!Matches(seq))
			{
				throw new SpiderException("Queue did not match expected sequence");
			}

			int len = seq.Length;
			if (len > RemainingLength())
			{
				throw new SpiderException("Queue not long enough to consume sequence");
			}

			_pos += len;
		}

		public string ConsumeTo(string seq)
		{
			int offset = _queue.IndexOf(seq, _pos, StringComparison.Ordinal);
			if (offset != -1)
			{
				string consumed = _queue.Substring(_pos, offset);
				_pos += consumed.Length;
				return consumed;
			}
			else
			{
				return Remainder();
			}
		}

		public string ConsumeToIgnoreCase(string seq)
		{
			int start = _pos;
			string first = seq.Substring(0, 1);
			bool canScan = first.ToLower().Equals(first.ToUpper());

			while (!IsEmpty && !Matches(seq))
			{
				if (canScan)
				{
					int data = _queue.IndexOf(first, _pos, StringComparison.Ordinal) - _pos;
					if (data == 0)
					{
						++_pos;
					}
					else if (data < 0)
					{
						_pos = _queue.Length;
					}
					else
					{
						_pos += data;
					}
				}
				else
				{
					++_pos;
				}
			}

			string var6 = _queue.Substring(start, _pos);
			return var6;
		}

		public string ConsumeToAny(params string[] seq)
		{
			int start;
			for (start = _pos; !IsEmpty && !MatchesAny(seq); ++_pos)
			{
			}

			string data = _queue.Substring(start, _pos);
			return data;
		}

		public string ConsumeAny(params string[] seq)
		{
			string[] arr = seq;
			int len = seq.Length;

			for (int i = 0; i < len; ++i)
			{
				string s = arr[i];
				if (Matches(s))
				{
					_pos += s.Length;
					return s;
				}
			}

			return "";
		}

		public string ChompTo(string seq)
		{
			string data = ConsumeTo(seq);
			MatchChomp(seq);
			return data;
		}

		public string ChompToIgnoreCase(string seq)
		{
			string data = ConsumeToIgnoreCase(seq);
			MatchChomp(seq);
			return data;
		}

		public string ChompBalancedQuotes()
		{
			string quote = ConsumeAny(Quotes);
			if (quote.Length == 0)
			{
				return "";
			}
			else
			{
				StringBuilder accum = new StringBuilder(quote);
				accum.Append(ConsumeToUnescaped(quote));
				accum.Append(Consume());
				return accum.ToString();
			}
		}

		public string ChompBalancedNotInQuotes(char open, char close)
		{
			StringBuilder accum = new StringBuilder();
			int depth = 0;
			char last = char.MinValue;
			bool inQuotes = false;
			char quote = char.MinValue;

			while (!IsEmpty)
			{
				char c = Consume();
				if (last == 0 || last != 92)
				{
					if (!inQuotes)
					{
						if (!c.Equals('\'') && !c.Equals('\"'))
						{
							if (c.Equals(open))
							{
								++depth;
							}
							else if (c.Equals(close))
							{
								--depth;
							}
						}
						else
						{
							inQuotes = true;
							quote = c;
						}
					}
					else if (c.Equals(quote))
					{
						inQuotes = false;
					}
				}

				if (depth > 0 && last != 0)
				{
					accum.Append(c);
				}

				last = c;
				if (depth <= 0)
				{
					break;
				}
			}

			return accum.ToString();
		}

		public string ChompBalanced(char open, char close)
		{
			StringBuilder accum = new StringBuilder();
			int depth = 0;
			char last = (char)0;

			while (!IsEmpty)
			{
				char c = Consume();
				if (last == 0 || last != 92)
				{
					if (c.Equals(open))
					{
						++depth;
					}
					else if (c.Equals(close))
					{
						--depth;
					}
				}

				if (depth > 0 && last != 0)
				{
					accum.Append(c);
				}

				last = c;
				if (depth <= 0)
				{
					break;
				}
			}

			return accum.ToString();
		}

		public static string Unescape(string str)
		{
			StringBuilder builder = new StringBuilder();
			char last = (char)0;
			char[] arr = str.ToCharArray();
			int len = arr.Length;

			for (int i = 0; i < len; ++i)
			{
				char c = arr[i];
				if (c == 92)
				{
					if (last != 0 && last == 92)
					{
						builder.Append(c);
					}
				}
				else
				{
					builder.Append(c);
				}

				last = c;
			}

			return builder.ToString();
		}

		public bool ConsumeWhitespace()
		{
			bool seen;
			for (seen = false; MatchesWhitespace(); seen = true)
			{
				++_pos;
			}

			return seen;
		}

		public string ConsumeWord()
		{
			int start;
			for (start = _pos; MatchesWord(); ++_pos)
			{
			}

			return _queue.Substring(start, _pos);
		}

		public string ConsumeTagName()
		{
			int start;
			for (start = _pos; !IsEmpty && (MatchesWord() || MatchesAny(':', '_', '-')); ++_pos)
			{
			}

			return _queue.Substring(start, _pos);
		}

		public string ConsumeElementSelector()
		{
			int start;
			for (start = _pos; !IsEmpty && (MatchesWord() || MatchesAny('|', '_', '-')); ++_pos)
			{
			}

			return _queue.Substring(start, _pos);
		}

		public void UnConsume(int length)
		{
			IsTrue(length <= _pos, "length " + length + " is larger than consumed chars " + _pos);
			_pos -= length;
		}

		public void UnConsume(string word)
		{
			UnConsume(word.Length);
		}

		public string ConsumeCssIdentifier()
		{
			int start;
			for (start = _pos; !IsEmpty && (MatchesWord() || MatchesAny('-', '_')); ++_pos)
			{
			}

			return _queue.Substring(start, _pos);
		}

		public string ConsumeAttributeKey()
		{
			int start;
			for (start = _pos; !IsEmpty && (MatchesWord() || MatchesAny('-', '_', ':')); ++_pos)
			{
			}

			return _queue.Substring(start, _pos);
		}

		public string Remainder()
		{
			StringBuilder accum = new StringBuilder();

			while (!IsEmpty)
			{
				accum.Append(Consume());
			}

			return accum.ToString();
		}

		public override string ToString()
		{
			return _queue.Substring(_pos);
		}

		public bool ContainsAny(params string[] seq)
		{
			string[] arr = seq;
			int len = seq.Length;

			for (int i = 0; i < len; ++i)
			{
				string s = arr[i];
				if (_queue.Contains(s))
				{
					return true;
				}
			}

			return false;
		}

		public static string TrimQuotes(string str)
		{
			IsTrue(!string.IsNullOrEmpty(str));

			// ReSharper disable once PossibleNullReferenceException
			string quote = str.Substring(0, 1);
			if (In(quote, "\"", "\'"))
			{
				IsTrue(str.EndsWith(quote), "Quote for " + str + " is incomplete!");
				str = str.Substring(1, str.Length - 1);
			}

			return str;
		}

		public static bool In(string needle, params string[] haystack)
		{
			string[] arr = haystack;
			int len = haystack.Length;

			for (int i = 0; i < len; ++i)
			{
				string hay = arr[i];
				if (hay.Equals(needle))
				{
					return true;
				}
			}

			return false;
		}

		public static List<string> TrimQuotes(List<string> strs)
		{
			IsTrue(strs != null);

			// ReSharper disable once AssignNullToNotNullAttribute
			return strs.Select(TrimQuotes).ToList();
		}

		public string ConsumeToUnescaped(string str)
		{
			string s = ConsumeToAny(str);
			if (s.Length > 0 && s[s.Length - 1] == 92)
			{
				s = s + Consume();
				s = s + ConsumeToUnescaped(str);
			}

			IsTrue(_pos < _queue.Length, "Unclosed quotes! " + _queue);
			return s;
		}

		public static void IsTrue(bool val)
		{
			if (!val)
			{
				throw new ArgumentException("Must be true");
			}
		}

		public static void IsTrue(bool val, string msg)
		{
			if (!val)
			{
				throw new ArgumentException(msg);
			}
		}

		public List<string> ParseFuncionParams()
		{
			List<string> list = new List<string>();
			StringBuilder accum = new StringBuilder();

			while (!IsEmpty)
			{
				ConsumeWhitespace();
				if (MatchChomp(","))
				{
					list.Add(accum.ToString());
					accum = new StringBuilder();
				}
				else if (MatchesAny(Quotes))
				{
					string quoteUsed = ConsumeAny(Quotes);
					accum.Append(quoteUsed);
					accum.Append(ConsumeToUnescaped(quoteUsed));
					accum.Append(Consume());
				}
				else
				{
					accum.Append(ConsumeToAny("\"", "\'", ","));
				}
			}

			if (accum.Length > 0)
			{
				list.Add(accum.ToString());
			}

			return list;
		}

		public static List<string> ParseFuncionParams(string paramStr)
		{
			XTokenQueue tq = new XTokenQueue(paramStr);
			return tq.ParseFuncionParams();
		}
	}
}
