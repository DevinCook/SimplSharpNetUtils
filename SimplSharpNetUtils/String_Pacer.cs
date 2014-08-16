using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SimplSharpNetUtils
{
    internal class String_Pacer
    {
        int nChunk_Size;
        int nDelay;

        CTimer Timer;

        CMutex bMutex = new CMutex();

        public delegate void ChunkHandler(string data);
        public ChunkHandler OnChunk { set; get; }

        public int Chunk_Size { set { nChunk_Size = value; } get { return nChunk_Size; } }

        StringBuilder Buffer;

        public String_Pacer(int chunk_size, int delay)
        {
            nChunk_Size = chunk_size;
            nDelay = delay;

            Timer = new CTimer(OnTimer, this, delay, delay);
            Buffer = new StringBuilder();
        }

        void OnTimer(Object o)
        {
            bMutex.WaitForMutex();
//            CrestronConsole.PrintLine("Ping!");
            if (Buffer.Length > 0)
            {
                try
                {
                    {
                        int l = (Buffer.Length > nChunk_Size) ? nChunk_Size : Buffer.Length;

                        String s = Buffer.ToString(0, l);
                        Buffer.Remove(0, l);
//                        CrestronConsole.PrintLine("Dechunk {0}", s);
                        if (OnChunk != null)
                            OnChunk(s);
                    }
                }
                catch (Exception e)
                {
                    CrestronConsole.PrintLine(e.Message);
                    CrestronConsole.PrintLine(e.StackTrace);
                }

            }
            bMutex.ReleaseMutex();
        }

        public void Enqueue(String s)
        {
            bMutex.WaitForMutex();
            Buffer.Append(s);
            bMutex.ReleaseMutex();
        }
    }
}