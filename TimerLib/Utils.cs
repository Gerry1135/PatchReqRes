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
        private static long depth = 0;

        public static void StartTimed()
        {
            if (depth == 0)
            {
                startTicks = Stopwatch.GetTimestamp();
                //MonoBehaviour.print("StartTimed start = " + startTicks);
            }
            depth++;
        }

        public static void StopTimed()
        {
            depth--;
            if (depth == 0)
            {
                long endTicks = Stopwatch.GetTimestamp();
                long deltaTicks = endTicks - startTicks;
                totalTicks += deltaTicks;
                //MonoBehaviour.print("StopTimed end = " + endTicks + "  delta = " + deltaTicks);
            }
            numCalls++;
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
