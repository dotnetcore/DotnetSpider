using System;

namespace DotnetSpider.Core
{
    /// <summary>
    /// Console的帮助类
    /// </summary>
    public static class ConsoleHelper
    {
        /// <summary>
        /// 打印一行信息
        /// </summary>
        /// <param name="message">需要打印的信息</param>
        /// <param name="color">打印的颜色</param>
        /// <param name="colorAfter">设置打印完成Console的字体颜色</param>
        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.Red, ConsoleColor colorAfter = ConsoleColor.White)
        {
            WriteLine(message, 0, color, colorAfter);
        }

        /// <summary>
        /// 打印一行信息
        /// </summary>
        /// <param name="message">需要打印的信息</param>
        /// <param name="blankLineCount">打印空行数</param>
        /// <param name="color">打印的颜色</param>
        /// <param name="colorAfter">设置打印完成Console的字体颜色</param>
        public static void WriteLine(string message, int blankLineCount, ConsoleColor color = ConsoleColor.Red, ConsoleColor colorAfter = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            AppendBlankLines(blankLineCount);
            Console.WriteLine(message);
            AppendBlankLines(blankLineCount);
            Console.ForegroundColor = colorAfter;
        }

        private static void AppendBlankLines(int blankLineCount)
        {
            if (blankLineCount <= 0) return;

            for (int i = 0; i < blankLineCount; i++)
            {
                Console.WriteLine();
            }
        }
    }
}