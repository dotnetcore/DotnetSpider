using System;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Model.Attribute
{
    /// <summary>
    /// Define the url patterns for class.
    /// All urls matching the pattern will be crawled and extracted for new objects.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class TargetUrl : System.Attribute
    {
        /// <summary>
        /// The url patterns for class.
        /// Use regex expression with some changes:
        ///      "." stand for literal character "." instead of "any character".
        ///      "*" stand for any legal character for url in 0-n length ([^"'#]*) instead of "any length".
        /// </summary>
        public string UrlPattern { get; set; }
        /// <summary>
        /// The properties' name from which the extras of the new reuqest with get from.
        /// </summary>
        public string[] OtherPropertiesAsExtras { get; set; }
        /// <summary>
        /// Use the UrlPattern as pure regex without any changes(no "." and "*" transformation).
        /// </summary>
        public bool KeepOrigin { get; set; }
    }
}
