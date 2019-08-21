using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Zlo.Extras;
using static Zlo.Extentions.Helpers;
namespace Zlo
{
    internal class ZloTCPClient : IDisposable
    {
        public delegate void ZloPacketReceivedEventHandler(byte pid, byte[] data);
        public event ZloPacketReceivedEventHandler ZloPacketReceived;
        public event API_ConnectionStateChanged IsConnectedChanged;

        private bool m_IsConnected;
        private readonly object _lock = new object();
        public bool IsConnected
        {
            get { lock (_lock) { return m_IsConnected; }; }
            private set
            {
                lock (_lock)
                {
                    if (m_IsConnected == value)
                        return;
                    m_IsConnected = value;
                    IsConnectedChanged?.Invoke(value);
                }
            }
        }

        private double ReconnectIntervals => Parent.ReconnectInterval.TotalMilliseconds;

        private TcpClient Client { get; set; }
        public API_ZloClient Parent { get; }

        public ZloTCPClient(API_ZloClient c)
        {
            Parent = c;
            ListenerThread = new Thread(ReadLoop)
            {
                IsBackground = true
            };
            ListenerThread.Start();
        }

        public bool Connect()
        {
            if (IsConnected)
                return true;
            try
            {
                if (Client != null)
                {
                    Client.Close();
                }
            }
            catch { }

            try
            {
                Client = new TcpClient("127.0.0.1", 48486);
                IsConnected = Client.Connected;
                return IsConnected;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                return IsConnected;
            }
            finally
            {
                StartReconnectTimer();
            }
        }

       
        Thread ListenerThread { get; }
        private NetworkStream ns;
        List<byte> CurrentBuffer = new List<byte>();
        int pid = -1;
        uint packetlen;
        bool iswaitingforlen = true;
        private void ReadLoop()
        {
            while (true)
            {
                if (!IsConnected || Client == null || !Client.Connected)
                {
                    StartReconnectTimer();
                    Thread.Sleep(500);
                    continue;
                }
                using (ns = Client.GetStream())
                {
                    byte[] _buffer = new byte[1024];
                    while (Client.Connected)
                    {
                        //read the available data
                        int numberOfBytesRead;
                        try
                        {
                            while ((numberOfBytesRead = ns.Read(_buffer, 0, _buffer.Length)) > 0)
                            {
                                //only deal with the data if numberOfBytesRead was greater than 0
                                var read_buffer = GetRange(_buffer, 0, numberOfBytesRead);
                                CurrentBuffer.AddRange(read_buffer);
                                ParsingStep_PID();
                            }
                        }
                        catch (Exception ex)
                        {
                            IsConnected = false;
                            StartReconnectTimer();
                            break;
                        }
                        //wait 200 ms until it is able to read
                        Thread.Sleep(200);
                    }
                }

                //wait 200 ms until next connect
                Thread.Sleep(200);
            }
        }

        private void ParsingStep_PID()
        {
            if (CurrentBuffer.Count == 0)
            {
                return;
            }
            if (pid == -1)
            {
                pid = CurrentBuffer[0];
                CurrentBuffer.RemoveAt(0);
            }
            ParsingStep_Len();
        }

        private void ParsingStep_Len()
        {
            if (!iswaitingforlen)
            {
                ParsingStep_Data();
                return;
            }
            if (CurrentBuffer.Count >= 4)
            {
                using (var actual_stream = new MemoryStream(CurrentBuffer.ToArray()))
                using (var br = new BinaryReader(actual_stream, Encoding.ASCII))
                {
                    packetlen = br.ReadZUInt32();
                    CurrentBuffer.RemoveRange(0, 4);
                    iswaitingforlen = false;
                }
                //do actual packet reading step
                ParsingStep_Data();
            }
            else
            {
                packetlen = 0;
                iswaitingforlen = true;
            }
        }

        private void ParsingStep_Data()
        {
            //read [packetlen] bytes, if they don't exist fully yet, just return
            if (CurrentBuffer.Count >= packetlen)
            {
                SendPacketToAPI((byte)pid, GetRange(CurrentBuffer, 0, packetlen));
                CurrentBuffer.RemoveRange(0, (int)packetlen);
                //go to the first step without adding any buffer
                pid = -1;
                iswaitingforlen = true;
                ParsingStep_PID();
            }
        }

        private byte[] GetRange(byte[] toget, int startindex, int count)
        {
            var ret = new byte[count];
            int finalindx = startindex + count;
            for (int i = startindex; i < finalindx; i++)
            {
                ret[i - startindex] = toget[i];
            }
            return ret;
        }
        private byte[] GetRange(List<byte> toget, uint startindex, uint count)
        {
            var final = new List<byte>();
            uint finalindx = startindex + count;
            for (uint i = startindex; i < finalindx; i++)
            {
                final.Add(toget[(int)i]);
            }
            return final.ToArray();
        }


        private void SendPacketToAPI(byte spid, byte[] buffer)
        {
            ZloPacketReceived?.Invoke(spid, buffer);
        }
        public bool WritePacket(byte[] info)
        {
            try
            {
                if (IsConnected && Client != null && Client.Connected)
                {
                    Client.Client.Send(info);
                    return true;
                }
                else
                {
                    IsConnected = false;
                    return IsConnected;
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                return IsConnected;
            }
            finally
            {
                if (!IsConnected)
                {
                    //start reconnect timer here
                    StartReconnectTimer();
                }
            }
        }

        private System.Timers.Timer reconnectTimer = new System.Timers.Timer();
        internal void StartReconnectTimer()
        {
            lock (reconnectTimer)
            {
                if (reconnectTimer.Enabled || IsConnected)
                    return;
                reconnectTimer.AutoReset = true;
                reconnectTimer.Interval = ReconnectIntervals;
                reconnectTimer.Elapsed += Elapsed;
                void Elapsed(object sender, ElapsedEventArgs e)
                {
                    if (IsConnected)
                    {
                        reconnectTimer.Elapsed -= Elapsed;
                        reconnectTimer.Enabled = false;
                    }
                    else
                    {
                        //connect
                        if (Connect())
                            Elapsed(null, null);
                        else
                            reconnectTimer.Interval = ReconnectIntervals;
                    }
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    IsConnected = false;
                    try
                    {
                        if (Client != null)
                        {
                            Client.Close();
                        }
                    }
                    catch
                    { }
                    finally
                    {
                        Client = null;
                    }
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ZloTCPClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
