// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Text.RegularExpressions;
// using DotnetSpider.Downloader;
// using DotnetSpider.Http;
//
// namespace DotnetSpider.DataFlow.Parser
// {
//     /// <summary>
//     /// 数据解析相关的帮助文件
//     /// </summary>
//     public static class DataParserHelper
//     {
//         /// <summary>
//         /// 通过 XPATH 查找链接
//         /// </summary>
//         /// <param name="xPaths"></param>
//         /// <returns></returns>
//         public static Func<DataContext, List<string>> QueryFollowRequestsByXPath(params string[] xPaths)
//         {
//             return context =>
//             {
//                 var urls = new List<string>();
//                 foreach (var xpath in xPaths)
//                 {
//                     var links = context.Selectable.XPath(xpath).Links().GetValues();
//                     foreach (var link in links)
//                     {
// #if !NETSTANDARD
//                         urls.Add(System.Web.HttpUtility.HtmlDecode(System.Web.HttpUtility.UrlDecode(link)));
// #else
// 						urls.Add(WebUtility.HtmlDecode(WebUtility.UrlDecode(link)));
// #endif
//                     }
//                 }
//
//                 return urls;
//             };
//         }
//
//         /// <summary>
//         /// 通过正则判断是否可以解析
//         /// </summary>
//         /// <param name="patterns">正则表达式</param>
//         /// <returns></returns>
//         public static Func<Request, bool> CheckIfRequiredByRegex(params string[] patterns)
//         {
//             return request =>
//             {
//                 foreach (var pattern in patterns)
//                 {
//                     if (Regex.IsMatch(request.RequestUri.ToString(), pattern))
//                     {
//                         return true;
//                     }
//                 }
//
//                 return false;
//             };
//         }
//     }
// }