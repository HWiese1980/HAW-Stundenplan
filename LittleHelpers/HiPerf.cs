using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LittleHelpers
{
    public class HiPerf
    {
        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long Value);

        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long Value);
    }
}
