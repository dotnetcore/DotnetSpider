using System;
using CLRConsole = System.Console;

namespace DotnetSpider.Core.Infrastructure
{
    public static class ConsoleHelper
    {
        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.Red, ConsoleColor colorAfter = ConsoleColor.White)
        {
            WriteLine(message, 0, color, colorAfter);
        }

        public static void WriteLine(string message, int blankLineCount, ConsoleColor color = ConsoleColor.Red, ConsoleColor colorAfter = ConsoleColor.White)
        {
            CLRConsole.ForegroundColor = color;
            AppendBlankLines(blankLineCount);
            CLRConsole.WriteLine(message);
            AppendBlankLines(blankLineCount);
            CLRConsole.ForegroundColor = colorAfter;
        }

        private static void AppendBlankLines(int blankLineCount)
        {
            if (blankLineCount <= 0) return;

            for (int i = 0; i < blankLineCount; i++)
            {
                CLRConsole.WriteLine();
            }
        }
    }
}
