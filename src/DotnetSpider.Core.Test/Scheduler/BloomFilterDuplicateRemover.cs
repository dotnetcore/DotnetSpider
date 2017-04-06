//using System;
//using System.Diagnostics;
//using System.Runtime.InteropServices;
//using DotnetSpider.Core.Common;
//using DotnetSpider.Core;
//using DotnetSpider.Core.Scheduler.Component;

//namespace DotnetSpider.Test.Scheduler
//{
//	public struct Memorystatus1 //这个结构用于获得系统信息
//	{
//		internal uint DwLength;
//		internal uint DwMemoryLoad;
//		internal uint DwTotalPhys;
//		internal uint DwAvailPhys;
//		internal uint DwTotalPageFile;
//		internal uint DwAvailPageFile;
//		internal uint DwTotalVirtual;
//		internal uint DwAvailVirtual;
//	}

//	
//	public class BloomFilterDuplicateRemoverTest
//	{
//		[DllImport("kernel32.dll ")]//调用系统DLL
//		public static extern void GlobalMemoryStatus(ref   Memorystatus1 lpBuffer); //获得系统DLL里的函数

//		[TestMethod]
//		public void TestRemove()
//		{
//			BloomFilterDuplicateRemover bloomFilterDuplicateRemover = new BloomFilterDuplicateRemover(10);
//			bool isDuplicate = bloomFilterDuplicateRemover.IsDuplicate(new Request("a", 1, null));

//			Assert.False(isDuplicate);
//			isDuplicate = bloomFilterDuplicateRemover.IsDuplicate(new Request("a", 1, null));
//			Assert.IsTrue(isDuplicate);
//			isDuplicate = bloomFilterDuplicateRemover.IsDuplicate(new Request("b", 1, null));
//			Assert.False(isDuplicate);
//			isDuplicate = bloomFilterDuplicateRemover.IsDuplicate(new Request("b", 1, null));
//			Assert.IsTrue(isDuplicate);
//		}

//		public long GetProcessUsedMemory()
//		{
//			double usedMemory = 0;
//			usedMemory = Process.GetCurrentProcess().WorkingSet64;
//			return (long)usedMemory;
//		}

//		[TestMethod]
//		public void TestMemory()
//		{
//			int times = 5000000;
//			IDuplicateRemover duplicateRemover = new BloomFilterDuplicateRemover(times, 0.005);
//			long used = GetProcessUsedMemory();

//			double time = DateTimeUtils.GetCurrentTimeStamp();
//			for (int i = 0; i < times; i++)
//			{
//				duplicateRemover.IsDuplicate(new Request(i.ToString(), 1, null));
//			}
//			Console.WriteLine("Time used by bloomfilter:" + (DateTimeUtils.GetCurrentTimeStamp() - time));
//			Console.WriteLine("Memory used by bloomfilter:" + (GetProcessUsedMemory() - used));

//			duplicateRemover = new HashSetDuplicateRemover();
//			GC.Collect();
//			used = GetProcessUsedMemory();
//			time = DateTimeUtils.GetCurrentTimeStamp();
//			for (int i = 0; i < times; i++)
//			{
//				duplicateRemover.IsDuplicate(new Request(i.ToString(), 1, null));
//			}
//			Console.WriteLine("Time used by hashset:" + (DateTimeUtils.GetCurrentTimeStamp() - time));
//			Console.WriteLine("Memory used by hashset:" + (GetProcessUsedMemory() - used));
//		}

//		[TestMethod]
//		public void TestMissHit()
//		{
//			int times = 5000000;
//			IDuplicateRemover duplicateRemover = new BloomFilterDuplicateRemover(times, 0.01);
//			int right = 0;
//			int wrong = 0;
//			int missCheck = 0;
//			for (int i = 0; i < times; i++)
//			{
//				bool duplicate = duplicateRemover.IsDuplicate(new Request(i.ToString(), 1, null));
//				if (duplicate)
//				{
//					wrong++;
//				}
//				else
//				{
//					right++;
//				}
//				duplicate = duplicateRemover.IsDuplicate(new Request(i.ToString(), 1, null));
//				if (!duplicate)
//				{
//					missCheck++;
//				}
//			}
//			double missRate = wrong / (double)right;
//			Assert.IsTrue(missRate < 0.01);

//			Console.WriteLine("Right count: " + right + " Wrong count: " + wrong + " Miss check: " + missCheck);
//		}
//	}
//}
