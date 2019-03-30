using System;
using System.IO;
using System.Linq;

namespace DotnetSpider.Core
{
    public static class LinuxCpuLoad
    {
        class CpuTick
        {
            public long U { get; set; }
            public long N { get; set; }
            public long S { get; set; }
            public long I { get; set; }
            public long W { get; set; }
            public long X { get; set; }
            public long Y { get; set; }
            public long Z { get; set; }

            public static float operator -(CpuTick l, CpuTick r)
            {
                if (l.U < r.U ||
                    l.N < r.N ||
                    l.S < r.S ||
                    l.I < r.I ||
                    l.W < r.W ||
                    l.X < r.X ||
                    l.Y < r.Y ||
                    l.Z < r.Z)
                {
                    return 0;
                }
                var uFrme = l.U - r.U;
                var sFrme = l.S - r.S;
                var nFrme = l.N - r.N;
                var iFrme = l.I - r.I;
                iFrme = iFrme > 0 ? iFrme : 0;
                var wFrme = l.W - r.W;
                var xFrme = l.X - r.X;
                var yFrme = l.Y - r.Y;
                var zFrme = l.Z - r.Z;

                var totFrme = uFrme + sFrme + nFrme + iFrme + wFrme + xFrme + yFrme + zFrme;
                totFrme = totFrme < 1 ? 1 : totFrme;
                var idle = l.I - r.I;
                var percent = (totFrme - idle) / (float)totFrme * 100;
                return percent;
            }
        }

        private static CpuTick _preTick = new CpuTick { U = 0, N = 0, S = 0, I = 0, W = 0, X = 0, Y = 0, Z = 0 };

        public static decimal Get()
        {
            var currentTick = GetCurrentTick();
            var percent = currentTick - _preTick;
            _preTick = currentTick;
            return (decimal)percent;
        }

        private static CpuTick GetCurrentTick()
        {
            var line = File.ReadAllLines("/proc/stat")[0];

            var datas = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).Take(8).Select(n => long.Parse(n)).ToArray();
            return new CpuTick
            {
                U = datas[0],
                N = datas[1],
                S = datas[2],
                I = datas[3],
                W = datas[4],
                X = datas[5],
                Y = datas[6],
                Z = datas[7]
            };
        }
    }
}