using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zlo
{
    public static class Log
    {
        public static void WriteLog(string log)
        {
            ToWrite.Enqueue(log);
            if (ToWrite.Count == 1)
            {
                ActualWriteLog();
            }
        }

        static Queue<string> ToWrite = new Queue<string>();
        private static void ActualWriteLog()
        {
            Task.Run(() =>
            {
                string line;
                lock (ToWrite)
                {
                    if (ToWrite.Count == 0) return;
                    line = ToWrite.Dequeue();
                }
                start:
                try
                {
                    File.AppendAllText(@".\ZloAPILog.txt", $"\n================================\n{DateTime.Now.ToString()}\n{line}\n================================");
                }
                catch
                {
                    Thread.Sleep(200);
                    goto start;
                }
                ActualWriteLog();
            });
        }

    }
}
