//using System;
//using System.Threading;
//using Java2Dotnet.Spider.Core;
//using Java2Dotnet.Spider.Extension.Configuration;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Remote;

//namespace Java2Dotnet.Spider.Extension.Utils
//{

//	public class CommonLoginUtil
//	{
//		private readonly LoginArguments _loginArguments;

//		public CommonLoginUtil(LoginArguments loginArguments)
//		{
//			_loginArguments = loginArguments;
//		}


//		public bool Login(RemoteWebDriver webDriver)
//		{
//			try
//			{
//				webDriver.Navigate().GoToUrl(_loginArguments.Url);
//				var user = FindElement(webDriver, _loginArguments.User);

//				user.Clear();
//				user.SendKeys(_loginArguments.User.Value);
//				Thread.Sleep(1500);
//				var pass = FindElement(webDriver, _loginArguments.Pass);
//				pass.SendKeys(_loginArguments.Pass.Value);
//				Thread.Sleep(1500);
//				var submit = FindElement(webDriver, _loginArguments.Submit);
//				submit.Click();
//				Thread.Sleep(5000);
//				return true;
//			}
//			catch (Exception)
//			{
//				return false;
//			}
//		}

		
//	}
//}
