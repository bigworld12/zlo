using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Zlo.Extentions.Helpers;
namespace Zlo
{
    internal class ZloTCPClient
    {
        public delegate void ZloPacketReceivedEventHandler(byte pid, byte[] data);
        public event ZloPacketReceivedEventHandler ZloPacketReceived;

        public bool IsConnected
        {
            get { return Client.Connected; }
        }

        private TcpClient Client { get; set; }

        private API_ZloClient Parent { get; set; }

        public ZloTCPClient(API_ZloClient c)
        {
            Parent = c;
            Client = new TcpClient();
            ListenerThread = new Thread(ReadLoop)
            {
                IsBackground = true
            };
        }
        public void Connect()
        {
            IsOn = true;
            if (Client == null)
            {
                Client = new TcpClient();
            }
            if (ListenerThread == null)
            {
                ListenerThread = new Thread(ReadLoop)
                {
                    IsBackground = true
                };
            }
            Client.Connect("127.0.0.1", 48486);
            ListenerThread.Start();
        }
        public bool ReConnect()
        {
            try
            {
                if (IsOn)
                {
                    Disconnect();
                }
                Client = new TcpClient();
                ListenerThread = new Thread(ReadLoop)
                {
                    IsBackground = true
                };
                Connect();
                return true;
            }
            catch
            {
                IsOn = false;
                return false;
            }
        }
        public void Disconnect()
        {
            IsOn = false;
            if (Client != null)
            {
                Client.Close();
            }
            if (ListenerThread != null)
            {
                ListenerThread.Abort();
            }
        }

        public bool IsOn = false;

        Thread ListenerThread;

        private NetworkStream ns;

        List<byte> CurrentBuffer = new List<byte>();
        int pid = -1;
        uint packetlen;
        bool iswaitingforlen = true;
        int counter = 0;
        private void ReadLoop()
        {
            while (true)
            {
                if (!IsOn)
                {
                    return;
                }
                //try to connect
                try
                {
                    if (!Client.Connected)
                    {
                        Client.Client?.Disconnect(true);
                        Client.Connect("127.0.0.1", 48486);
                    }
                }
                catch (Exception ex)
                {
                    if (counter >= 5)
                    {
                        counter = 0;
                        Parent.RaiseError(ex, "Error occured when connecting to tcp server");
                        IsOn = false;
                        return;
                    }
                    else
                    {
                        counter++;
                    }
                    Thread.Sleep(500);
                }

                if (Client.Connected)
                {
                    using (ns = Client.GetStream())
                    {
                        byte[] _buffer = new byte[1024];
                        while (Client.Connected)
                        {
                            //try to read, gives true when there are bytes to read
                            //should move the parsing loop outside
                            if (ns.CanRead)
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
                                    Parent.RaiseError(ex, "Failed to read the data from the network stream");
                                }
                            }
                            else
                            {

                            }
                            //wait 200 ms until it is able to read
                            Thread.Sleep(200);
                        }
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
                if (Client.Connected)
                {
                    Client.Client.Send(info);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Parent.RaiseError(ex, "Error occured when writing to network stream");
                return false;
            }

        }
    }
}
