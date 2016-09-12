using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace ProfileGraph
{
    public class ChannelValues
    {
        public const int width = 600;
        public const int height = 100;

        public ChannelValues()
        {
            values = new double[width];
            texGraph = new Texture2D(width, height);
        }

        public double[] values;
        public long ticksAtStart;
        public int lastValue;
        public String lastValueStr;
        public String name;
        public Texture2D texGraph;
    }
    
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class Graph : MonoBehaviour
    {
        private int NumChannels;

        private string windowTitle = "ProfileGraph 1.0.0.1";
        private Rect windowPos = new Rect(80, 80, 400, 50);
        private bool showUI = false;

        private ChannelValues[] dataarray;

        int valIndex = 0;
        int lastRendered = 0;

        long startTime;
        long ticksPerSec;

        bool fullUpdate = false;

        const String lastValuePattern = "{0}: {1}%";

        Color[] blackLine;
        Color[] redLine;
        Color[] greenLine;
        Color[] blueLine;

        private GUIStyle labelStyle;
        private GUILayoutOption wndWidth;
        private GUILayoutOption wndHeight;
        private GUILayoutOption graphHeight;

        internal void Awake()
        {
            DontDestroyOnLoad(gameObject);

            Utils.InitChannels();
            NumChannels = Utils.GetNumChannels();

            redLine = new Color[ChannelValues.height];
            greenLine = new Color[ChannelValues.height];
            blueLine = new Color[ChannelValues.height];
            blackLine = new Color[ChannelValues.height];
            for (int i = 0; i < blackLine.Length; i++)
            {
                blackLine[i] = Color.black;
                redLine[i] = Color.red;
                greenLine[i] = Color.green;
                blueLine[i] = Color.blue;
            }

            dataarray = new ChannelValues[NumChannels + 1];

            for (int i = 0; i < NumChannels + 1; i++)
            {
                dataarray[i] = new ChannelValues();
                ChannelValues data = dataarray[i];

                for (int j = 0; j < ChannelValues.width; j++)
                    data.values[j] = 0.0;

                data.name = Utils.GetChannelName(i) ?? "Total";
                data.lastValue = 0;
                data.lastValueStr = String.Format(lastValuePattern, data.name, data.lastValue.ToString("N2"));
                data.ticksAtStart = Utils.GetTotalTicks(i);
            }

            startTime = Stopwatch.GetTimestamp();
            ticksPerSec = Stopwatch.Frequency;
        }

        private void TryUpdateData()
        {
            // First thing is to record the times for this frame
            long endTime = Stopwatch.GetTimestamp();
            long timeDelta = endTime - startTime;
            //print("timeDelta = " + timeDelta);
            if (timeDelta > ticksPerSec)
            {
                //print("timeDelta = " + timeDelta);

                double totalFrac = 0.0d;

                for (int i = 0; i < NumChannels; i++)
                {
                    ChannelValues data = dataarray[i];

                    long ticksAtEnd = Utils.GetTotalTicks(i);
                    //print("ticksAtEnd = " + ticksAtEnd);
                    long ticksDelta = ticksAtEnd - data.ticksAtStart;
                    //print("ticksDelta = " + ticksDelta);
                    double frac = ((double)ticksDelta * 100.0 / (double)timeDelta);
                    //print("value = " + frac);
                    data.values[valIndex] = frac;
                    totalFrac += frac;

                    int rounded = (int)(frac * 100d + 0.5d);
                    if (rounded != data.lastValue)
                    {
                        data.lastValue = rounded;
                        data.lastValueStr = String.Format(lastValuePattern, data.name, frac.ToString("N2"));
                    }

                    data.ticksAtStart = ticksAtEnd;
                }

                ChannelValues totalData = dataarray[NumChannels];
                totalData.values[valIndex] = totalFrac;
                int roundedTotal = (int)(totalFrac * 100d + 0.5d);
                if (roundedTotal != totalData.lastValue)
                {
                    totalData.lastValue = roundedTotal;
                    totalData.lastValueStr = String.Format(lastValuePattern, totalData.name, totalFrac.ToString("N2"));
                }

                startTime = endTime;
                valIndex = (valIndex + 1) % ChannelValues.width;
            }
        }

        public void FixedUpdate()
        {
            TryUpdateData();
        }

        public void LateUpdate()
        {
            TryUpdateData();
        }

        public void Update()
        {
            //print("Update Start");

            TryUpdateData();

            if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.Minus))
            {
                showUI = !showUI;
            }

            if (!showUI)
                return;

            if (fullUpdate)
            {
                fullUpdate = false;
                lastRendered = (valIndex + 1) % ChannelValues.width;
            }

            // If we want to update this time
            if (lastRendered != valIndex)
            {
                for (int i = 0; i < NumChannels + 1; i++)
                {
                    // We're going to wrap this back round to the start so copy the value so 
                    int startlastRend = lastRendered;

                    ChannelValues data = dataarray[i];

                    // Update the columns from lastRendered to frameIndex
                    if (startlastRend >= valIndex)
                    {
                        for (int x = startlastRend; x < ChannelValues.width; x++)
                        {
                            DrawColumn(data.texGraph, x, (int)data.values[x], redLine);
                        }

                        startlastRend = 0;
                    }

                    for (int x = startlastRend; x < valIndex; x++)
                    {
                        DrawColumn(data.texGraph, x, (int)data.values[x], redLine);
                    }

                    if (valIndex < ChannelValues.width)
                        data.texGraph.SetPixels(valIndex, 0, 1, ChannelValues.height, blackLine);
                    if (valIndex != ChannelValues.width - 2)
                        data.texGraph.SetPixels((valIndex + 1) % ChannelValues.width, 0, 1, ChannelValues.height, blackLine);
                    data.texGraph.Apply();
                }

                lastRendered = valIndex;
            }
            //print("Update End");
        }

        private void DrawColumn(Texture2D tex, int x, int y, Color[] col)
        {
            //print("drawcol(" + x + ", " + y + ")");
            if (y > ChannelValues.height - 1)
                y = ChannelValues.height - 1;
            tex.SetPixels(x, 0, 1, y + 1, col);
            if (y < ChannelValues.height - 1)
                tex.SetPixels(x, y + 1, 1, ChannelValues.height - 1 - y, blackLine);
        }

        public void OnGUI()
        {
            if (labelStyle == null)
                labelStyle = new GUIStyle(GUI.skin.label);

            if (wndWidth == null)
                wndWidth = GUILayout.Width(ChannelValues.width);
            if (wndHeight == null)
                wndHeight = GUILayout.Height(ChannelValues.height);
            if (graphHeight == null)
                graphHeight = GUILayout.Height(ChannelValues.height);

            if (showUI)
                windowPos = GUILayout.Window(2461275, windowPos, WindowGUI, windowTitle, wndWidth, wndHeight);
        }

        public void WindowGUI(int windowID)
        {
            GUILayout.BeginVertical();
            for (int i = 0; i < NumChannels + 1; i++)
            {
                ChannelValues data = dataarray[i];

                GUILayout.BeginVertical();
                GUILayout.Box(data.texGraph, wndWidth, graphHeight);

                GUILayout.BeginHorizontal();
                GUILayout.Label(data.lastValueStr, labelStyle);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();

            GUI.DragWindow();
        }
    }
}
