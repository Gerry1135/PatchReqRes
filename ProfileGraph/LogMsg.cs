using System.Text;

using UnityEngine;

namespace ProfileGraph
{
    public class LogMsg
    {
        public StringBuilder buf;

        public LogMsg()
        {
            buf = new StringBuilder(65536);
        }

        public void Flush()
        {
            if (buf.Length > 0)
                MonoBehaviour.print(buf);
            buf.Length = 0;
        }
    }
}
