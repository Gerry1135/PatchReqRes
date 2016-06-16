using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace ProfileGraph
{
    public class ChannelData
    {
        public ChannelData(string name)
        {
            this.name = name;
        }

        public string name;
        public long totalTicks = 0;
        public long startTicks = 0;
    }
    
    public class Utils
    {
        const String cfgFilename = "profile.cfg";

        private static LogMsg Log;

        private static int NumChannels;
        private static List<ChannelData> channels;

        private static long depth = 0;
        private static int CurrentChannel = -1;

        public static void InitChannels()
        {
            if (Log == null)
                Log = new LogMsg();
            
            if (channels == null)
                channels = new List<ChannelData>();

            NumChannels = 0;

            Log.buf.Append("[ProfileGraph] Loading config from ");
            Log.buf.AppendLine(cfgFilename);
            if (File.Exists<Utils>(cfgFilename))
            {
                String[] lines = File.ReadAllLines<Utils>(cfgFilename);

                for (int i = 0; i < lines.Length; i++)
                {
                    String[] line = lines[i].Split(',');
                    if (line.Length > 1)
                    {
                        var name = line[0].Trim();
                        Log.buf.Append("Channel ");
                        Log.buf.Append(NumChannels);
                        Log.buf.Append(": ");
                        Log.buf.AppendLine(name);
                        channels.Add(new ChannelData(name));
                        NumChannels++;
                    }
                    else
                    {
                        Log.buf.Append("Ignoring invalid line in settings: '");
                        Log.buf.Append(lines[i]);
                        Log.buf.AppendLine("'");
                    }
                }
            }
            else
                Log.buf.AppendLine("Can't find profile.cfg");

            Log.Flush();
        }

        public static void StartTimed(int Channel)
        {
            if (Channel < 0 || Channel >= NumChannels)
                return;

            if (depth == 0)
            {
                CurrentChannel = Channel;
                ChannelData data = channels[Channel];
                data.startTicks = Stopwatch.GetTimestamp();
                // Convert this to Log.buf
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
                ChannelData data = channels[CurrentChannel];
                long endTicks = Stopwatch.GetTimestamp();
                long deltaTicks = endTicks - data.startTicks;
                data.totalTicks += deltaTicks;
                // Convert this to Log.buf
                //MonoBehaviour.print("StopTimed " + CurrentChannel + " end = " + endTicks + "  delta = " + deltaTicks);
                CurrentChannel = -1;
            }
        }

        public static int GetNumChannels()
        {
            return NumChannels;
        }

        public static string GetChannelName(int Channel)
        {
            if (Channel < 0 || Channel >= NumChannels)
                return null;

            return channels[Channel].name;
        }

        public static long GetTotalTicks(int Channel)
        {
            if (Channel < 0 || Channel >= NumChannels)
                return 0;

            return channels[Channel].totalTicks;
        }

        public static void ResetTotalTicks(int Channel)
        {
            if (Channel < 0 || Channel >= NumChannels)
                return;

            channels[Channel].totalTicks = 0;
        }
    }
}
