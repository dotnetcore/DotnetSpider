using System;
using NLog;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using System.Collections.Generic;
using OpenQA.Selenium.Support.UI;
#if !NET_CORE
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using DotnetSpider.Core.Common;
using DotnetSpider.Extension.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
#endif

namespace DotnetSpider.Extension.Downloader
{
    public abstract class CookieInterceptor : Named
    {
        protected readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public abstract Tuple<string, Dictionary<string, string>> GetCookie();
    }

#if !NET_CORE
    public abstract class WebDriverCookieInterceptor : CookieInterceptor
    {
        protected IWebElement FindElement(RemoteWebDriver webDriver, Selector element, int waitTime = 0)
        {
            if (element.Type != SelectorType.XPath && element.Type != SelectorType.Css)
                throw new SpiderException("Unsport findy: " + element.Type);

            if (waitTime > 0)
            {
                waitElements(webDriver, element, waitTime);
            }
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
            return null;
        }

        private void waitElements(RemoteWebDriver webDriver, Selector element, int waitTime)
        {
            bool canWait = false;
            canWait = element != null && element.Type == SelectorType.XPath || element.Type == SelectorType.Css;
            WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
            if (canWait)
            {
                if (element.Type == SelectorType.XPath)
                    wait.Until(ExpectedConditions.ElementExists(By.XPath(element.Expression)));
                else if (element.Type == SelectorType.Css)
                    wait.Until(ExpectedConditions.ElementExists(By.CssSelector(element.Expression)));
            }
            else
                Thread.Sleep(waitTime);
        }

        protected RemoteWebDriver GetWebDriver()
        {
            ChromeDriverService cds = ChromeDriverService.CreateDefaultService();
            cds.HideCommandPromptWindow = true;
            ChromeOptions opt = new ChromeOptions();
            opt.AddUserProfilePreference("profile", new { default_content_setting_values = new { images = 2 } });
            RemoteWebDriver webDriver = new ChromeDriver(cds, opt);

            webDriver.Manage().Window.Maximize();
            return webDriver;
        }
    }

    public class CommonCookieInterceptor : WebDriverCookieInterceptor
    {
        public string Url { get; set; }

        public Selector InputSelector { get; set; }

        public string InputString { get; set; }

        public Selector GotoSelector { get; set; }

        public override Tuple<string, Dictionary<string, string>> GetCookie()
        {
            string cookie = string.Empty;
            var cookies = new Dictionary<string, string>();
            while (string.IsNullOrEmpty(cookie))
            {
                var webDriver = GetWebDriver();
                try
                {
                    webDriver.Navigate().GoToUrl(Url);
                    Thread.Sleep(5000);

                    if (InputSelector != null)
                    {
                        var input = FindElement(webDriver, InputSelector);
                        if (!string.IsNullOrEmpty(InputString))
                        {
                            input.SendKeys(InputString);
                        }
                    }

                    if (GotoSelector != null)
                    {
                        var gotoButton = FindElement(webDriver, GotoSelector);
                        gotoButton.Click();
                        Thread.Sleep(2000);
                    }

                    var cookieList = webDriver.Manage().Cookies.AllCookies.ToList();

                    if (cookieList.Count > 0)
                    {
                        foreach (var cookieItem in cookieList)
                        {
                            cookie += cookieItem.Name + "=" + cookieItem.Value + "; ";
                            cookies.Add(cookieItem.Name, cookieItem.Value);
                        }
                    }

                    webDriver.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Get cookie failed.");
                    webDriver.Dispose();
                    cookie = null;
                }
            }

            return new Tuple<string, Dictionary<string, string>>(cookie, cookies);
        }
    }

    public class FiddlerCookieInterceptor : CommonCookieInterceptor
    {
        public int ProxyPort { get; set; } = 30000;
        public string Pattern { get; set; }

        public override Tuple<string, Dictionary<string, string>> GetCookie()
        {
            if (string.IsNullOrEmpty(Pattern))
            {
                throw new Exception("Fiddler CookieTrapper: Pattern cannot be null!");
            }

            string cookie = string.Empty;
            using (FiddlerClient fiddlerWrapper = new FiddlerClient(ProxyPort, Pattern))
            {
                fiddlerWrapper.StartCapture(true);
                try
                {
                    base.GetCookie();
                    var header = fiddlerWrapper.Headers;
                    const string cookiesPattern = @"Cookie: (.*?)\r\n";
                    cookie = Regex.Match(header, cookiesPattern).Groups[1].Value;
                }
                catch
                {
                    // ignored
                }
                fiddlerWrapper.StopCapture();
            }
            //Item2 not implemented
            return new Tuple<string, Dictionary<string, string>>(cookie, null);
        }
    }

    public class FiddlerLoginCookieInterceptor : LoginCookieInterceptor
    {
        public int ProxyPort { get; set; } = 30000;
        public string Pattern { get; set; }

        public override Tuple<string, Dictionary<string, string>> GetCookie()
        {
            if (string.IsNullOrEmpty(Pattern))
            {
                throw new Exception("Fiddler CookieTrapper: Pattern cannot be null!");
            }

            string cookie = string.Empty;
            using (FiddlerClient fiddlerWrapper = new FiddlerClient(ProxyPort, Pattern))
            {
                fiddlerWrapper.StartCapture(true);
                try
                {
                    base.GetCookie();
                    var header = fiddlerWrapper.Headers;
                    const string cookiesPattern = @"Cookie: (.*?)\r\n";
                    cookie = Regex.Match(header, cookiesPattern).Groups[1].Value;
                }
                catch
                {
                    // ignored
                }
                fiddlerWrapper.StopCapture();
            }
            //Item2 not implemented
            return new Tuple<string, Dictionary<string, string>>(cookie, null);
        }
    }

    public class LoginCookieInterceptor : WebDriverCookieInterceptor
    {
        public string Url { get; set; }

        public string AfterLoginUrl { get; set; }

        public Selector UserSelector { get; set; }

        public string User { get; set; }

        public Selector PassSelector { get; set; }

        public string Pass { get; set; }

        public Selector SubmitSelector { get; set; }

        public Selector LoginAreaSelector { get; set; }

        public override Tuple<string, Dictionary<string, string>> GetCookie()
        {
            string cookie = string.Empty;
            var cookies = new Dictionary<string, string>();
            while (string.IsNullOrEmpty(cookie))
            {
                var webDriver = GetWebDriver();
                try
                {
                    webDriver.Navigate().GoToUrl(Url);

                    if (LoginAreaSelector != null)
                    {
                        IWebElement loginArea = FindElement(webDriver, LoginAreaSelector, 10000);
                        loginArea.Click();
                    }

                    var user = FindElement(webDriver, UserSelector, 1000);

                    user.Clear();
                    user.SendKeys(User);
                    var pass = FindElement(webDriver, PassSelector,1500);
                    pass.SendKeys(Pass);
                    var submit = FindElement(webDriver, SubmitSelector,1500);
                    
                    submit.Click();

                    if (!string.IsNullOrEmpty(AfterLoginUrl))
                    {
                        webDriver.Navigate().GoToUrl(AfterLoginUrl);
                        Thread.Sleep(10000);
                    }

                    var cookieList = webDriver.Manage().Cookies.AllCookies.ToList();

                    if (cookieList.Count > 0)
                    {
                        foreach (var cookieItem in cookieList)
                        {
                            cookie += cookieItem.Name + "=" + cookieItem.Value + "; ";
                            cookies.Add(cookieItem.Name, cookieItem.Value);
                        }
                    }

                    webDriver.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Get cookie failed.");
                    webDriver.Dispose();
                    cookie = null;
                }
            }

            return new Tuple<string, Dictionary<string, string>>(cookie, cookies);
        }
    }
#endif
}
