using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Java2Dotnet.Spider.Common
{
    public class EmailClient
    {
        static byte[] data;
        static TcpClient tcpClient;
        static NetworkStream stream;
        
        public static Message[] GetMail()
        {
            try
            {
                bool b;
                Connect(SettingsForm.pOPHost, SettingsForm.pOPPort);
                //Connect("localhost", 110);
                b = POPReceive();
                Send("USER " + SettingsForm.pOPName + "\n");
                b = POPReceive();
                Send("PASS " + SettingsForm.pOPPass + "\n");
                b = POPReceive();
                Send("STAT\n");
                int mLength = Int32.Parse("" + Receive()[4]);
                Message[] m = new Message[mLength];
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
                        string[] s = Parser.splitToLines(currentReception);
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
       
        public static void SendMail(Message m)
        {
            try {
                Connect(SettingsForm.sMTPHost, SettingsForm.sMTPPort);
                Console.WriteLine(Receive()[0]);
                Send("HELO "+SettingsForm.sMTPHost+"\n");
                Console.WriteLine(Receive()[0]);
                Send("AUTH LOGIN\n");
                Console.WriteLine(Receive()[0]);
                Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(SettingsForm.sMTPName)) + "\n");
                Console.WriteLine(Receive()[0]);
                Send(Convert.ToBase64String(Encoding.UTF8.GetBytes(SettingsForm.sMTPPass)) + "\n");
                Console.WriteLine(Receive()[0]);
                Send("MAIL FROM: <"+SettingsForm.sMTPName+">\n");
                Console.WriteLine(Receive()[0]);
                Send("RCPT TO: <"+m.recipient+">\n");
                Console.WriteLine(Receive()[0]);
                Send("DATA\n");
                Console.WriteLine(Receive()[0]);//354
                Send(String.Format(@"From: {0}
To: {1}
Date: {2}
Subject: {3}

{4}
.
", m.sender, m.recipient, m.timestamp, m.subject, m.body));
                Console.WriteLine(Receive()[0]);
                Send("QUIT\n");
                Console.WriteLine(Receive()[0]);
            }
            catch { }
        }
        
        static void Connect(string h,int p)
        {
            try
            {
                tcpClient = new TcpClient(h,p);
                stream = tcpClient.GetStream();
            }
            catch (SocketException)
            {
                MessageBox.Show(String.Format("Couldnt connect to {0} at port {1}.", h,p));
            }
        }
        static bool POPReceive()
        {
            string s = Receive();
            try
            {
                
                if (s.Substring(0, 3).ToUpper() == "+OK")
                {
                    return true;
                }
                else if (s.Substring(0, 3).ToUpper()== "-ER")
                {
                    MessageBox.Show("server returned following error: " + s);
                    return false;
                }
                else
                {
                    MessageBox.Show("Server message unexpected, attempting to continue anyways.\n"+s);
                    return false;
                }
            } 
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Server message unexpected, attempting to continue anyways.\n" + s);
                return false;
            }
        }
        
        static string Receive()
        {
            try {
                data = new Byte[2048];
                String responseData = String.Empty;
                int bytes = 0;
                bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                //foreach (byte b in data){ Console.WriteLine(b); }
                return responseData;
            }
            catch { return null; }
        }
        
        static void Send(string s)
        {
            data = System.Text.Encoding.UTF8.GetBytes(s);
            stream.Write(data, 0, data.Length);
        }
    }
}