#if !NET_CORE
using System;
using System.Net.Mail;

namespace Java2Dotnet.Spider.Common
{
	public class EmailUtil
	{
		static readonly string User;
		static readonly string Pass;
		static readonly string Server;

		static EmailUtil()
		{
			User = System.Configuration.ConfigurationManager.AppSettings["emailUser"];
			Pass = System.Configuration.ConfigurationManager.AppSettings["emailPassword"];
			Server = System.Configuration.ConfigurationManager.AppSettings["emailServer"];
		}

		public static void Send(string subject, string emailto, string mailBody, string attached)
		{
			MailAddress addressFrom = new MailAddress(User, "Auto Data Center"); //邮件的发件人
			MailMessage mail = new MailMessage();

			//设置邮件的标题
			mail.Subject = subject;
			//设置邮件的发件人
			//Pass:如果不想显示自己的邮箱地址，这里可以填符合mail格式的任意名称，真正发mail的用户不在这里设定，这个仅仅只做显示用
			mail.From = addressFrom;
			//设置邮件的内容
			mail.Body = mailBody;
			//设置邮件的格式
			mail.BodyEncoding = System.Text.Encoding.UTF8;
			mail.IsBodyHtml = true;
			//设置邮件的发送级别
			mail.Priority = MailPriority.Normal;

			string[] sendTos = emailto.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var sendTo in sendTos)
			{
				mail.To.Add(new MailAddress(sendTo));
			}

			//设置邮件的附件，将在客户端选择的附件先上传到服务器保存一个，然后加入到mail中
			if (!string.IsNullOrEmpty(attached))
			{
				mail.Attachments.Add(new Attachment(attached));
			}

			mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;

			using (SmtpClient client = new SmtpClient())
			{
				//设置用于 SMTP 事务的主机的名称，填IP地址也可以了
				client.Host = Server;
				//设置用于 SMTP 事务的端口，默认的是 25
				client.UseDefaultCredentials = false;
				client.Timeout = 30000000;
				client.EnableSsl = false;
				//这里才是真正的邮箱登陆名和密码，比如我的邮箱地址是 hbgx@hotmail， 我的用户名为 hbgx ，我的密码是 xgbh
				client.Credentials = new System.Net.NetworkCredential(User, Pass);
				client.DeliveryMethod = SmtpDeliveryMethod.Network;
				//都定义完了，正式发送了，很是简单吧！
				client.Send(mail);
			}
		}

		public static string Send(string subject, string emailto, string body)
		{
			try
			{
				Send(subject, emailto, body, null);

				return "发送成功";
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
	}
}
#endif