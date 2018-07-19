//#if !NETCOREAPP
//using System;
//using System.Drawing;
//using System.IO;
//using Tesseract;

//namespace DotnetSpider.Sample
//{
//	public class OcrDemo
//	{
//		public static void Process()
//		{
//			TesseractEngine ocrEngine = new TesseractEngine(Path.Combine(Core.Env.BaseDirectory, "tessdata"), "eng", EngineMode.Default);
//			ocrEngine.SetVariable("tessedit_char_whitelist", "0123456789");

//			var image = Image.FromFile("929c331e8319a761773125efe3f11f20.png");
//			using (var ocrPage = ocrEngine.Process(new Bitmap(image)))
//			{
//				var numbers = ocrPage.GetText();
//				numbers.Replace("\n", "").Replace(" ", "");
//				Console.WriteLine($"Recognize: {numbers}");
//			}
//		}
//	}
//}
//#endif