using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extentions
{
    public static class Helpers
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll" , CharSet = CharSet.Auto , SetLastError = true)]
        public static extern bool WaitNamedPipe(string name , int timeout);

        /// <summary>
        /// Provides an indication if the named pipe exists. 
        /// This has to prove the pipe does not exist. If there is any doubt, this
        /// returns that it does exist and it is up to the caller to attempt to connect
        /// to that server. The means that there is a wide variety of errors that can occur that
        /// will be ignored - for example, a pipe name that contains invalid characters will result
        /// in a return value of false.
        /// 
        /// </summary>
        /// <param name="pipeName">The pipe to connect to</param>
        /// <returns>false if it can be proven it does not exist, otherwise true</returns>
        /// <remarks>
        /// Attempts to check if the pipe server exists without incurring the cost
        /// of calling NamedPipeClientStream.Connect. This is because Connect either 
        /// times out and throws an exception or goes into a tight spin loop burning
        /// up cpu cycles if the server does not exist.
        /// 
        /// Common Error codes from WinError.h
        /// ERROR_FILE_NOT_FOUND 2L
        /// ERROR_BROKEN_PIPE =  109 (0x6d)
        /// ERROR_BAD_PATHNAME  161L The specified path is invalid.
        /// ERROR_BAD_PIPE =  230  (0xe6) The pipe state is invalid.
        /// ERROR_PIPE_BUSY =  231 (0xe7) All pipe instances are busy.
        /// ERROR_NO_DATA =   232   (0xe8) the pipe is being closed
        /// ERROR_PIPE_NOT_CONNECTED 233L No process is on the other end of the pipe.
        /// ERROR_PIPE_CONNECTED        535L There is a process on other end of the pipe.
        /// ERROR_PIPE_LISTENING        536L Waiting for a process to open the other end of the pipe.
        /// 
        /// </remarks>
        static public bool NamedPipeExists(string pipeName)
        {
            try
            {
                string normalizedPath = Path.GetFullPath($@"\\.\pipe\{pipeName}");
                bool exists = WaitNamedPipe(normalizedPath , 0);
                if (!exists)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == 0) // pipe does not exist
                        return false;
                    else if (error == 2) // win32 error code for file not found
                        return false;
                    // all other errors indicate other issues
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failure in WaitNamedPipe() at NamedPipeExists()\n{ex.ToString()}");
                return false;
            }
        }

        public static byte[] ReadReversedBytes(this BinaryReader br , int count)
        {
            var bytes = br.ReadBytes(count);
            Array.Reverse(bytes);
            return bytes;
        }
        public static string ReadZString(this BinaryReader br)
        {            
            StringBuilder s = new StringBuilder();
            try
            {
                char t;
                
                while (br.PeekChar() != -1 && (t = br.ReadChar()) > 0)
                    s.Append(t);
                return s.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return s.ToString();

            }
        }
        public static string ReadCountedString(this BinaryReader br , int count)
        {
            StringBuilder s = new StringBuilder();

            for (int i = 1; i <= count; i++)
            {
                s.Append(br.ReadChar());
            }
            return s.ToString();
        }
        public static uint ReadZUInt32(this BinaryReader br)
        {
            return BitConverter.ToUInt32(br.ReadReversedBytes(4) , 0);
        }
        public static ulong ReadZUInt64(this BinaryReader br)
        {
            return BitConverter.ToUInt64(br.ReadReversedBytes(8) , 0);
        }
        public static ushort ReadZUInt16(this BinaryReader br)
        {
            return BitConverter.ToUInt16(br.ReadReversedBytes(2) , 0);
        }
        public static float ReadZFloat(this BinaryReader br)
        {
            return BitConverter.ToSingle(br.ReadReversedBytes(4) , 0);
        }

        public static T Find<T>(this IEnumerable<T> Source , Predicate<T> predicate)
        {
            if (Source == null || predicate == null)
            {
                return default(T);
            }

            int count = Source.Count();
            for (int i = 0; i < count; i++)
            {
                var elem = Source.ElementAt(i);
                if (predicate(elem))
                {
                    return elem;
                }
            }
            return default(T);
        }
    }
}
