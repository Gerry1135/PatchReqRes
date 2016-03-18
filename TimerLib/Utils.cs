using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace TimerLib
{
    public class ChannelData
    {
        public long totalTicks = 0;
        public long startTicks = 0;
    }
    
    public class Utils
    {
        private const int NumChannels = 4;
        private static ChannelData[] dataarray = new ChannelData[]
        {
            new ChannelData(),
            new ChannelData(),
            new ChannelData(),
            new ChannelData()
        };

        private static long depth = 0;
        private static int CurrentChannel = -1;

        public static void StartTimed0()
        {
            StartTimed(0);
        }

        public static void StartTimed1()
        {
            StartTimed(1);
        }

        public static void StartTimed2()
        {
            StartTimed(2);
        }

        public static void StartTimed3()
        {
            StartTimed(3);
        }

        private static void StartTimed(int Channel)
        {
            if (Channel < 0 || Channel >= NumChannels)
                return;

            if (depth == 0)
            {
                CurrentChannel = Channel;
                ChannelData data = dataarray[Channel];
                data.startTicks = Stopwatch.GetTimestamp();
                //MonoBehaviour.print("StartTimed " + Channel + " start = " + data.startTicks);
            }
            depth++;
        }

        public static void StopTimed()
        {
            // If we aren't in any function then do nothing
            if (depth == 0)
                return;

            depth--;
            if (depth == 0)
            {
                ChannelData data = dataarray[CurrentChannel];
                long endTicks = Stopwatch.GetTimestamp();
                long deltaTicks = endTicks - data.startTicks;
                data.totalTicks += deltaTicks;
                //MonoBehaviour.print("StopTimed " + CurrentChannel + " end = " + endTicks + "  delta = " + deltaTicks);
                CurrentChannel = -1;
            }
        }

        public static long GetTotalTicks(int Channel)
        {
            if (Channel < 0 || Channel >= NumChannels)
                return 0;

            return dataarray[Channel].totalTicks;
        }

        public static void ResetTotalTicks(int Channel)
        {
            if (Channel < 0 || Channel >= NumChannels)
                return;

            dataarray[Channel].totalTicks = 0;
        }
    }
}
