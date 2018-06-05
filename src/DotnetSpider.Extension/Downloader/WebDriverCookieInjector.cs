using OpenQA.Selenium.Firefox;
using System;
using DotnetSpider.Core.Selector;
using System.Linq;
using System.Threading;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core;
using System.IO;
using System.Collections.Generic;
using System.Net;

namespace DotnetSpider.Extension.Downloader
{

    /// <summary>
    /// WebDriver ��Cookieע����
    /// </summary>
	public class WebDriverCookieInjector : CookieInjector
	{
        /// <summary>
        /// ��½������
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// ��½�ɹ�����Ҫ�ٴε�����������
        /// </summary>
        public string AfterLoginUrl { get; set; }

        /// <summary>
        /// �û�������ҳ�е�Ԫ��ѡ����
        /// </summary>
        public SelectorAttribute UserSelector { get; set; }

        /// <summary>
        /// �û���
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// ��������ҳ�е�Ԫ��ѡ����
        /// </summary>
        public SelectorAttribute PasswordSelector { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// ��½��ť��Ԫ��ѡ����
        /// </summary>
        public SelectorAttribute SubmitSelector { get; set; }

        /// <summary>
        /// �����
        /// </summary>
        public Browser Browser { get; set; } = Browser.Chrome;

        /// <summary>
        /// �������û���Ϣǰִ�е�һЩ׼������
        /// </summary>
        /// <param name="webDriver">WebDriver</param>
        protected virtual void BeforeInput(RemoteWebDriver webDriver) { }

        /// <summary>
        /// ��ɵ�½��ִ�е�һЩ׼������
        /// </summary>
        /// <param name="webDriver"></param>
        protected virtual void AfterLogin(RemoteWebDriver webDriver) { }

        /// <summary>
        /// ����Ԫ��
        /// </summary>
        /// <param name="webDriver">WebDriver</param>
        /// <param name="element">ҳ��Ԫ��ѡ����</param>
        /// <returns>ҳ��Ԫ��</returns>
        protected IWebElement FindElement(RemoteWebDriver webDriver, SelectorAttribute element)
        {
            switch (element.Type)
            {

                case SelectorType.XPath:
                    {
                        return webDriver.FindElementByXPath(element.Expression);
                    }
                case SelectorType.Css:
                    {
                        return webDriver.FindElementByCssSelector(element.Expression);
                    }
            }
            throw new SpiderException("Unsport findy: " + element.Type);
        }

        /// <summary>
        /// ȡ�� Cookie
        /// </summary>
        /// <param name="spider">����</param>
        /// <returns>Cookie</returns>
        protected override CookieCollection GetCookies(ISpider spider)
        {
            if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Password) || UserSelector == null || PasswordSelector == null)
            {
                throw new SpiderException("Arguments of WebDriverCookieInjector are incorrect");
            }
            var cookies = new Dictionary<string, string>();

            var webDriver = CreateWebDriver();
            var result = new CookieCollection();
            try
            {
                webDriver.Navigate().GoToUrl(Url);
                Thread.Sleep(10000);

                BeforeInput(webDriver);

                if (UserSelector != null)
                {
                    var user = FindElement(webDriver, UserSelector);
                    user.Clear();
                    user.SendKeys(User);
                    Thread.Sleep(1500);
                }

                if (PasswordSelector != null)
                {
                    var pass = FindElement(webDriver, PasswordSelector);
                    pass.Clear();
                    pass.SendKeys(Password);
                    Thread.Sleep(1500);
                }

                var submit = FindElement(webDriver, SubmitSelector);
                submit.Click();
                Thread.Sleep(10000);

                AfterLogin(webDriver);

                var cookieList = webDriver.Manage().Cookies.AllCookies.ToList();
                if (cookieList.Count > 0)
                {
                    foreach (var cookieItem in cookieList)
                    {
                        result.Add(new System.Net.Cookie(cookieItem.Name, cookieItem.Value, cookieItem.Path, cookieItem.Domain));
                    }
                }

                webDriver.Dispose();
            }
            catch
            {
                
                Logger.Error("Get cookie failed.");
                webDriver.Dispose();
                return null;
            }

            return result;
        }

        /// <summary>
        /// ����WebDriver����
        /// </summary>
        /// <returns>WebDriver����</returns>
        protected RemoteWebDriver CreateWebDriver()
        {
            RemoteWebDriver webDriver;
            switch (Browser)
            {
                case Browser.Chrome:
                    {
                        ChromeDriverService cds = ChromeDriverService.CreateDefaultService();
                        cds.HideCommandPromptWindow = true;
                        ChromeOptions opt = new ChromeOptions();
                        opt.AddUserProfilePreference("profile", new { default_content_setting_values = new { images = 2 } });
                        webDriver = new ChromeDriver(cds, opt);
                        break;
                    }
                case Browser.Firefox:
                    {
                        string path = Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\Mozilla\Firefox\Profiles\";
                        string[] pathsToProfiles = Directory.GetDirectories(path, "*.webdriver", SearchOption.TopDirectoryOnly);
                        if (pathsToProfiles.Length == 1)
                        {
                            FirefoxProfile profile = new FirefoxProfile(pathsToProfiles[0], false) { AlwaysLoadNoFocusLibrary = true };
                            FirefoxOptions options = new FirefoxOptions();
                            options.Profile = profile;
                            webDriver = new FirefoxDriver(options);
                        }
                        else
                        {
                            throw new Exception("No Firefox profiles: webdriver.");
                        }
                        break;
                    }
                default:
                    {
                        throw new Exception("Unsupported browser!");
                    }
            }

            webDriver.Manage().Window.Maximize();
            return webDriver;
        }
    }
}
