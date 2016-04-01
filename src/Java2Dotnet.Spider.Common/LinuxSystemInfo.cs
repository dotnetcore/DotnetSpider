using System;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;

public class LinuxSystemInfo
	{
		public float LoadAvg { get; set; }
		public int FreeMemory { get; set; }
		public int TotalMemory { get; set; }
		public int CpuCount { get; set; }

		private static readonly object _locker = new object();
 
        private static readonly Regex _processorRegex=new Regex("processor"); 
        
		public static LinuxSystemInfo GetSystemInfo()
		{
			lock (_locker)
			{
				 try
                 {
                 LinuxSystemInfo  _systemInfo = new LinuxSystemInfo();
					// cpu load 
					string loadAvg =RunCommand("cat","/proc/loadavg");
                    _systemInfo.LoadAvg=float.Parse(loadAvg.Split(' ')[2]);
                    
                    // cpu count
                    string cpuInfo=RunCommand("cat","/proc/cpuinfo");
                    _systemInfo.CpuCount= _processorRegex.Matches(cpuInfo).Count;
 
                    var memInfo=RunCommand("free","-m").Split(new []{" "},StringSplitOptions.RemoveEmptyEntries);
                    int totalMem=int.Parse(memInfo[6]);
                    _systemInfo.TotalMemory=totalMem;
                    
                    int usedMem=int.Parse(memInfo[7]);
                    _systemInfo.FreeMemory=(totalMem-usedMem) ;
					return _systemInfo;
                 }
                 catch (System.Exception)
                 {
                     
                     throw;
                 }

		 
			}
		}
        
        private static string RunCommand(string command, string arguments)
        {
            ProcessStartInfo startInfo=new ProcessStartInfo(command,arguments);
            startInfo.RedirectStandardOutput=true;
			System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo=startInfo;
            process.Start();
            process.WaitForExit(1500);
			return process.StandardOutput.ReadToEnd();
        }
 
	}