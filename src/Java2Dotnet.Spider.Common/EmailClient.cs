using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Java2Dotnet.Spider.Common
{
	public class EmaillMessage
	{
		public string Subject { get; set; }
		public string Body { get; set; }
		public string Recipient { get; set; }
		public string Sender { get; set; }
		public string Timestamp { get; set; }
		public bool IsHtml { get; set; }
		public Encoding Encoding { get; set; }

		public EmaillMessage(string subject, string body, string recipient, string sender = null)
		{
			Subject = subject;
			Body = body;
			Recipient = recipient;
			Sender = sender;
			Timestamp = DateTime.Now.ToString();
			Encoding = Encoding.UTF8;
		}
	}

	public class EmailClient : IDisposable
	{
		private byte[] data;
		private TcpClient tcpClient;
		private NetworkStream stream;

		public string Host { get; }
		public int Port { get; }
		public string User { get; }
		public string Password { get; }

		public EmailClient(string host, string user, string password, int port = 25)
		{
			Host = host;
			Port = port;
			User = user;
			Password = password;
		}

		public EmaillMessage[] GetMail()
		{
			lock (this)
			{
				try
				{
					bool b;
					Connect(Host, Port);
					//Connect("localhost", 110);
					b = POPReceive();
					Send("USER " + User + "\n");
					b = POPReceive();
					Send("PASS " + Password + "\n");
					b = POPReceive();
					Send("STAT\n");
					int mLength = Int32.Parse("" + Receive()[4]);
					EmaillMessage[] m = new EmaillMessage[mLength];
					for (int x = mLength; x > 0; x--)
					{
						Send("RETR " + x + "\n");
						b = POPReceive(); //known possible bug?
						bool loopTester = true;
						string receivedMsg = "";
						do
						{
							string currentReception = Receive();
							receivedMsg = receivedMsg + currentReception;
							string[] s = Parser.SplitToLines(currentReception);
							foreach (string S in s)
							{
								if (S == ".")
								{
									loopTester = false;
								}
								else
								{

								}
							}
						}
						while (loopTester);
						m[x - 1] = Parser.Parse(receivedMsg);
					}
					Send("QUIT\n");
					return m;
				}
				catch
				{
					return null;
				}
			}
		}

		public void SendMail(EmaillMessage m)
		{
			lock (this)
			{
				try
				{
					Connect(Host, Port);
					string result = Receive();
					Send($"HELO {Host}\n");
					result = Receive();
					Send("AUTH LOGIN\n");
					result = Receive();
					Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(User)) + "\n");
					result = Receive();
					Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(Password)) + "\n");
					result = Receive();
					Send($"MAIL FROM: <{(string.IsNullOrEmpty(m.Sender) ? User : m.Sender)}>\n");
					result = Receive();
					Send($"RCPT TO: <{m.Recipient}>\n");
					result = Receive();
					Send("DATA\n");
					result = Receive();
					Send(String.Format(@"From: {0}
To: {1}
Date: {2}
Subject: {3}
Content-Type: {4};

{5}
.
", (string.IsNullOrEmpty(m.Sender) ? User : m.Sender), m.Recipient, m.Timestamp, m.Subject, m.IsHtml ? "text/html" : "text/plain", m.Body));
					result = Receive();
					Send("QUIT\n");

					result = Receive();
				}
				catch (Exception e)
				{
				}
			}
		}

		private void Connect(string host, int port)
		{
			tcpClient = new TcpClient();
            tcpClient.ConnectAsync(host,port);
			stream = tcpClient.GetStream();
		}

		private bool POPReceive()
		{
			string s = Receive();
			try
			{

				if (s.Substring(0, 3).ToUpper() == "+OK")
				{
					return true;
				}
				else if (s.Substring(0, 3).ToUpper() == "-ER")
				{
					//Server returned following error: s
					return false;
				}
				else
				{
					//"Server message unexpected, attempting to continue anyways.\n" + s 
					return false;
				}
			}
			catch (ArgumentOutOfRangeException)
			{
				//"Server message unexpected, attempting to continue anyways.\n" + s
				return false;
			}
		}

		private string Receive()
		{
			try
			{
				data = new Byte[2048];
				String responseData = String.Empty;
				int bytes = 0;
				bytes = stream.Read(data, 0, data.Length);
				responseData = Encoding.UTF8.GetString(data, 0, bytes);
				return responseData;
			}
			catch { return null; }
		}

		private void Send(string s)
		{
			data = System.Text.Encoding.UTF8.GetBytes(s);
			stream.Write(data, 0, data.Length);
		}

		public void Dispose()
		{
			try
			{
#if !NET_CORE
				tcpClient.Close();
				stream.Close();
#else
				tcpClient.Dispose();
				stream.Dispose();
#endif
			}
			catch
			{
			}
		}
	}

	public class Parser
	{
		private delegate void Del(string s, EmaillMessage m);
		private delegate string Del2(string s, EmaillMessage m);
		private static bool mimeHandled, multiHandled;

		public static EmaillMessage Parse(string s)
		{
			Del subjectHandler = AssignSubject;
			Del senderHandler = AssignSender;
			Del recipientHandler = AssignRecipent;
			Del dateHandler = AssignDate;
			Del mimeHandler = AssignMime;
			Del encodingHandler = AssignEncoding;

			Dictionary<string, Delegate> d = new Dictionary<string, Delegate>();
			d.Add("Date: ", dateHandler);
			d.Add("Mime-Version: ", mimeHandler);//most parsing occurs therein
			d.Add("Subject: ", subjectHandler);
			d.Add("From: ", senderHandler);
			d.Add("To: ", recipientHandler);
			d.Add("charset=", encodingHandler);

			mimeHandled = false;
			multiHandled = false;
			EmaillMessage m = new EmaillMessage("", s, "", "");

			foreach (string headerCheck in SplitToLines(ReturnHeader(s)))
			{
				foreach (KeyValuePair<string, Delegate> k in d)
				{
					if (headerCheck.StartsWith(k.Key))
					{
						k.Value.DynamicInvoke(headerCheck.Substring(k.Key.Length), m);
					}
				}
			}
			if (!mimeHandled) { ParsePlainText(m.Body, m); }
			if (multiHandled) { m.Body = RemoveHeader(m.Body); }
			Console.WriteLine("returning m");
			return m;
		}

		public static string DecodeQuotedPrintable(string input, EmaillMessage message)
		{
			Console.WriteLine("decoding QP");
			string originalS = input;
			var occurences = new Regex(@"(=[0-9A-Z][0-9A-Z])+", RegexOptions.Multiline);
			var matches = occurences.Matches(input);
			foreach (Match m in matches)
			{
				byte[] bytes = new byte[m.Value.Length / 3];
				for (int i = 0; i < bytes.Length; i++)
				{
					string hex = m.Value.Substring(i * 3 + 1, 2);
					int iHex = Convert.ToInt32(hex, 16);
					bytes[i] = Convert.ToByte(iHex);
				}
				input = input.Replace(m.Value, message.Encoding.GetString(bytes));
			}
			input = input.Replace("=\r\n", "\r\n");
			input = input.Replace("=\n", "\n");
			input = input.Replace("=\r", "\r");
			return input;
		}

		public static string DecodeBase64(string input, EmaillMessage message)
		{
			try
			{
				string originalS = input;
				input = message.Encoding.GetString(Convert.FromBase64String(input));
				return input;
			}
			catch
			{
				return input;
			}
		}

		public static void ParsePlainText(string s, EmaillMessage m)
		{
			string originalS = s;
			s = DecodeTransferEncoding(s, m);
			s = RemoveHeader(s);
			s = s.Replace(Environment.NewLine, "<br>");
			s = s.Replace("\r\n", "<br>");
			s = s.Replace("\r", "<br>");
			s = s.Replace("\n", "<br>");
			m.Body = m.Body.Replace(originalS, s);
		}

		public static void ParseHtmlText(string s, EmaillMessage m)
		{
			string originalS = s;
			s = DecodeTransferEncoding(s, m);

			s = RemoveHeader(s);
			m.Body = m.Body.Replace(originalS, s);
		}

		static string DecodeTransferEncoding(string s, EmaillMessage m)
		{
			Dictionary<string, Delegate> d = new Dictionary<string, Delegate>();
			Del2 quotedPrintableHandler = DecodeQuotedPrintable;
			Del2 base64Handler = DecodeBase64;
			d.Add("base64", base64Handler);
			d.Add("quoted-printable", quotedPrintableHandler);

			string[] keywords = new string[] { "Content-Type: ", "\tcharset=", "Content-Transfer-Encoding: " };

			foreach (string headerCheck in SplitToLines(ReturnHeader(s)))
			{
				foreach (string keyword in keywords)
				{
					if (headerCheck.StartsWith(keyword))
					{
						foreach (KeyValuePair<string, Delegate> k in d)
						{
							if (headerCheck.Substring(keyword.Length) == k.Key)
							{
								return (string)k.Value.DynamicInvoke(s, m);
							}
						}
					}
				}
			}
			return s;
		}

		static void ParseMultiMix(string s, EmaillMessage m)
		{
			multiHandled = true;
			string[] postParse = null;
			string boundary = null;
			foreach (string headerCheck in SplitToLines(ReturnHeader(s)))
			{
				if (headerCheck.StartsWith("\tboundary=\""))
				{
					boundary = "--" + headerCheck.Substring("boundary=".Length + 2, headerCheck.Length - "boundary=\"".Length - 2);
					postParse = s.Split(new[] { boundary }, StringSplitOptions.None);
					Console.WriteLine();
				}
			}
			try
			{
				m.Body = m.Body.Replace(boundary, "");
				for (int x = 1; x < postParse.Length; x++)
				{
					AssignMime("", m, postParse[x]);
				}
			}
			catch { }
		}

		private static string ReturnHeader(string s)
		{
			string[] preParse = s.Split(new[] { "\r\n\r\n", "\r\r", "\n\n" }, StringSplitOptions.None);
			return preParse[0];
		}

		private static string RemoveHeader(string s)
		{
			string[] preParse = s.Split(new[] { "\r\n\r\n", "\r\r", "\n\n", Environment.NewLine + Environment.NewLine }, StringSplitOptions.None);

			string returnString = "";
			for (int x = 1; x < preParse.Length; x++)
			{
				returnString = returnString + preParse[x];
			}
			return returnString;
		}

		public static string[] SplitToLines(string s)
		{
			return s.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		}

		private static void AssignMime(string s, EmaillMessage m, string optionalMimeSubPart)//not using newer optional param support because delegates
		{
			mimeHandled = true;
			Dictionary<string, Delegate> d = new Dictionary<string, Delegate>();

			Del plainTextHandler = ParsePlainText;
			Del htmlHandler = ParseHtmlText;
			Del multiAltHandler = ParseMultiMix;
			Del multiMixHandler = ParseMultiMix;

			d.Add("multipart/alternative;", multiAltHandler);
			d.Add("multipart/mixed;", multiMixHandler);
			d.Add("text/plain;", plainTextHandler);
			d.Add("text/html;", htmlHandler);


			string[] keywords = new string[] { "Content-Type: ", "\tcharset=" };
			foreach (string headerCheck in SplitToLines(ReturnHeader(optionalMimeSubPart)))
			{
				foreach (string keyword in keywords)
				{
					if (headerCheck.StartsWith(keyword))
					{
						Console.WriteLine(keyword + " noted in " + headerCheck);
						foreach (KeyValuePair<string, Delegate> k in d)
						{
							if (headerCheck.Substring(keyword.Length) == k.Key)
							{
								k.Value.DynamicInvoke(optionalMimeSubPart, m);
							}
						}
					}
				}
			}

		}
		private static void AssignSubject(string s, EmaillMessage m)
		{
			m.Subject = s;
		}

		private static void AssignSender(string s, EmaillMessage m)
		{
			m.Sender = s;
		}

		private static void AssignRecipent(string s, EmaillMessage m)
		{
			m.Recipient = s;
		}

		private static void AssignMime(string s, EmaillMessage m)
		{
			AssignMime(s, m, m.Body);
		}

		private static void AssignDate(string s, EmaillMessage m)
		{
			m.Timestamp = s;
		}

		private static void AssignEncoding(string s, EmaillMessage m)
		{
			m.Encoding = Encoding.GetEncoding(s);
		}

		public static EmaillMessage Dotstuff(EmaillMessage m)
		{
			return m;
		}

		public static EmaillMessage UnDotstuff(EmaillMessage m)
		{
			return m;
		}
	}
}