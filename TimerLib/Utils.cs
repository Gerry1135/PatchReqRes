using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace TimerLib
{
    public class Utils
    {
        private static long numCalls = 0;
        private static long totalTicks = 0;
        private static long startTicks = 0;

        public static void StartTimed()
        {
            startTicks = Stopwatch.GetTimestamp();
            //MonoBehaviour.print("StartTimed start = " + startTicks);
        }

        public static void StopTimed()
        {
            long endTicks = Stopwatch.GetTimestamp();
            long deltaTicks = endTicks - startTicks;
            totalTicks += deltaTicks;
            numCalls++;
            //MonoBehaviour.print("StopTimed end = " + endTicks + "  delta = " + deltaTicks);
        }

        public static long GetNumCalls()
        {
            return numCalls;
        }

        public static long GetTotalTicks()
        {
            return totalTicks;
        }

        public static void ResetTotalTicks()
        {
            totalTicks = 0;
        }
    }
}
