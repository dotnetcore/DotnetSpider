//using System.Diagnostics;
//using Antlr4.Runtime;
//using Java2Dotnet.Spider.Extension.Grammar;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Java2Dotnet.Spider.Extension.Test
//{
//	[TestClass]
//	public class PerformanceTest
//	{
//		[TestMethod]
//		public void NewObject()
//		{
//			Stopwatch watch = new Stopwatch();
//			watch.Start();
//			AntlrInputStream input = new AntlrInputStream("regex('//',1)");
//			ModifyScriptLexer lexer = new ModifyScriptLexer(input);
//			CommonTokenStream tokens = new CommonTokenStream(lexer);

//			for (int i = 0; i < 10000; ++i)
//			{
//				lexer.Reset();
//				tokens.Reset();

//				ModifyScriptVisitor tableColumnVisitor = new ModifyScriptVisitor("", null);
//				ModifyScriptParser parser = new ModifyScriptParser(tokens);
//				tableColumnVisitor.Visit(parser.expr());
//			}

//			watch.Stop();
//			long time1 = watch.ElapsedMilliseconds;

//			Stopwatch watch1 = new Stopwatch();
//			watch.Start();


//			for (int i = 0; i < 10000; ++i)
//			{
//				AntlrInputStream input1 = new AntlrInputStream("regex('//',1)");
//				ModifyScriptLexer lexer1 = new ModifyScriptLexer(input1);
//				CommonTokenStream tokens1 = new CommonTokenStream(lexer1);

//				ModifyScriptVisitor tableColumnVisitor = new ModifyScriptVisitor("", null);
//				ModifyScriptParser parser1 = new ModifyScriptParser(tokens1);
//				tableColumnVisitor.Visit(parser1.expr());
//			}

//			watch1.Stop();
//			long time2 = watch.ElapsedMilliseconds;

//			Assert.IsTrue(time1 < time2);
//		}
//	}
//}
