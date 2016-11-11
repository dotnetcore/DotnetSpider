// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

using System;

namespace DotnetSpider.HtmlAgilityPack
{
    internal class HtmlCmdLine
    {
        #region Static Members

        internal static bool Help;

        #endregion

        #region Constructors

        static HtmlCmdLine()
        {
            Help = false;
            //ParseArgs();
        }

        #endregion

        //#region Internal Methods

        //internal static string GetOption(string name, string def)
        //{
        //    string p = def;
        //    string[] args = Environment.GetCommandLineArgs();
        //    for (int i = 1; i < args.Length; i++)
        //    {
        //        GetStringArg(args[i], name, ref p);
        //    }
        //    return p;
        //}

        //internal static string GetOption(int index, string def)
        //{
        //    string p = def;
        //    string[] args = Environment.GetCommandLineArgs();
        //    int j = 0;
        //    for (int i = 1; i < args.Length; i++)
        //    {
        //        if (GetStringArg(args[i], ref p))
        //        {
        //            if (index == j)
        //                return p;
        //            else
        //                p = def;
        //            j++;
        //        }
        //    }
        //    return p;
        //}

        //internal static bool GetOption(string name, bool def)
        //{
        //    bool p = def;
        //    string[] args = Environment.GetCommandLineArgs();
        //    for (int i = 1; i < args.Length; i++)
        //    {
        //        GetBoolArg(args[i], name, ref p);
        //    }
        //    return p;
        //}

        //internal static int GetOption(string name, int def)
        //{
        //    int p = def;
        //    string[] args = Environment.GetCommandLineArgs();
        //    for (int i = 1; i < args.Length; i++)
        //    {
        //        GetIntArg(args[i], name, ref p);
        //    }
        //    return p;
        //}

        //#endregion

        #region Private Methods

        private static void GetBoolArg(string arg, string name, ref bool argValue)
        {
            if (arg.Length < (name.Length + 1)) // -name is 1 more than name
                return;
            if (('/' != arg[0]) && ('-' != arg[0])) // not a param
                return;
            if (arg.Substring(1, name.Length).ToLower() == name.ToLower())
                argValue = true;
        }

        private static void GetIntArg(string arg, string name, ref int argValue)
        {
            if (arg.Length < (name.Length + 3)) // -name:12 is 3 more than name
                return;
            if (('/' != arg[0]) && ('-' != arg[0])) // not a param
                return;
            if (arg.Substring(1, name.Length).ToLower() == name.ToLower())
            {
                try
                {
                    argValue = Convert.ToInt32(arg.Substring(name.Length + 2, arg.Length - name.Length - 2));
                }
                catch
                {
                }
            }
        }

        private static bool GetStringArg(string arg, ref string argValue)
        {
            if (('/' == arg[0]) || ('-' == arg[0]))
                return false;
            argValue = arg;
            return true;
        }

        private static void GetStringArg(string arg, string name, ref string argValue)
        {
            if (arg.Length < (name.Length + 3)) // -name:x is 3 more than name
                return;
            if (('/' != arg[0]) && ('-' != arg[0])) // not a param
                return;
            if (arg.Substring(1, name.Length).ToLower() == name.ToLower())
                argValue = arg.Substring(name.Length + 2, arg.Length - name.Length - 2);
        }

        //private static void ParseArgs()
        //{
        //    string[] args = Environment.GetCommandLineArgs();
        //    for (int i = 1; i < args.Length; i++)
        //    {
        //        // help
        //        GetBoolArg(args[i], "?", ref Help);
        //        GetBoolArg(args[i], "h", ref Help);
        //        GetBoolArg(args[i], "help", ref Help);
        //    }
        //}

        #endregion
    }
}