using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace ReqResGraph
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class Graph : MonoBehaviour
    {
        private const int width = 500;
        private const int height = 100;

        private Rect windowPos = new Rect(80, 80, 400, 50);
        private bool showUI = false;
        readonly Texture2D texGraph = new Texture2D(width, height);
        private double[] values = new double[width];

        int valIndex = 0;
        int lastRendered = 0;

        long startTime;
        long ticksPerSec;
        long ticksAtStart;

        bool fullUpdate = false;

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

            redLine = new Color[height];
            greenLine = new Color[height];
            blueLine = new Color[height];
            blackLine = new Color[height];
            for (int i = 0; i < blackLine.Length; i++)
            {
                blackLine[i] = Color.black;
                redLine[i] = Color.red;
                greenLine[i] = Color.green;
                blueLine[i] = Color.blue;
            }

            for (int i = 0; i < width; i++)
                values[i] = 0.0;

            startTime = Stopwatch.GetTimestamp();
            ticksPerSec = Stopwatch.Frequency;
            print("ticksPerSec = " + ticksPerSec);
            ticksAtStart = TimerLib.Utils.GetTotalTicks();
            print("ticksAtStart = " + ticksAtStart);
        }

        internal void OnDestroy()
        {
        }

        public void FixedUpdate()
        {
            // First thing is to record the times for this frame
            long endTime = Stopwatch.GetTimestamp();
            long timeDelta = endTime - startTime;
            //print("timeDelta = " + timeDelta);
            if (timeDelta > ticksPerSec)
            {
                print("timeDelta = " + timeDelta);
                long ticksAtEnd = TimerLib.Utils.GetTotalTicks();
                //print("ticksAtEnd = " + ticksAtEnd);
                long ticksDelta = ticksAtEnd - ticksAtStart;
                print("ticksDelta = " + ticksDelta);
                values[valIndex] = ((double)ticksDelta * 100.0 / (double)timeDelta);
                print("value = " + values[valIndex]);
                //print("calls = " + TimerLib.Utils.GetNumCalls());

                startTime = endTime;
                ticksAtStart = ticksAtEnd;
                valIndex = (valIndex + 1) % width;
            }
        }

        public void Update()
        {
            //print("Update Start");

            if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.Minus))
            {
                showUI = !showUI;
            }

            if (!showUI)
                return;

            if (fullUpdate)
            {
                fullUpdate = false;
                //lastRendered = (frameIndex - 1) % width;
            }

            // If we want to update this time
            if (lastRendered != valIndex)
            {
                // Update the columns from lastRendered to frameIndex
                if (lastRendered >= valIndex)
                {
                    for (int x = lastRendered; x < width; x++)
                    {
                        DrawColumn(texGraph, x, (int)values[x], redLine);
                    }

                    lastRendered = 0;
                }

                for (int x = lastRendered; x < valIndex; x++)
                {
                    DrawColumn(texGraph, x, (int)values[x], redLine);
                }

                lastRendered = valIndex;

                if (valIndex < width)
                    texGraph.SetPixels(valIndex, 0, 1, height, blackLine);
                if (valIndex != width - 2)
                    texGraph.SetPixels((valIndex + 1) % width, 0, 1, height, blackLine);
                texGraph.Apply();
            }
            //print("Update End");
        }

        private void DrawColumn(Texture2D tex, int x, int y, Color[] col)
        {
            //print("drawcol(" + x + ", " + y + ")");
            if (y > height - 1)
                y = height - 1;
            tex.SetPixels(x, 0, 1, y + 1, col);
            if (y < height - 1)
                tex.SetPixels(x, y + 1, 1, height - 1 - y, blackLine);
        }

        public void OnGUI()
        {
            if (labelStyle == null)
                labelStyle = new GUIStyle(GUI.skin.label);

            if (wndWidth == null)
                wndWidth = GUILayout.Width(width);
            if (wndHeight == null)
                wndHeight = GUILayout.Height(height);
            if (graphHeight == null)
                graphHeight = GUILayout.Height(height);

            if (showUI)
                windowPos = GUILayout.Window(2461275, windowPos, WindowGUI, "PhysicsGraph", wndWidth, wndHeight);
        }

        public void WindowGUI(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.Box(texGraph, wndWidth, graphHeight);
            GUILayout.EndVertical();

            GUI.DragWindow();
        }
    }
}
