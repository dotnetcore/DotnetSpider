using System;

namespace DotnetSpider.Extension.Model.Formatter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CharacterCaseFormatter : Formatter
    {
        public override string Name { get; internal set; } = "CharacterCaseFormatter";
        public bool ToUpper { get; set; } = true;
        public override string Formate(string value)
        {
            if (ToUpper)
            {
                return value.ToUpper();
            }
            else
            {
                return value.ToLower();
            }
        }
    }
}
