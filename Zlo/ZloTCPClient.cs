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
        public delegate void ZloPacketReceivedEventHandler(byte pid , byte[] data);
        public event ZloPacketReceivedEventHandler ZloPacketReceived;

        public bool IsConnected
        {
            get { return Client.Connected; }
        }

        private TcpClient Client { get; set; }

        private API_ZloClient parent { get; set; }

        public ZloTCPClient(API_ZloClient c)
        {
            Client = new TcpClient();
            parent = c;
            ListenerThread = new Thread(ReadLoop);
            ListenerThread.IsBackground = true;
        }
        public void Connect()
        {
            IsOn = true;    
            Client.Connect("127.0.0.1" , 48486);
            ListenerThread.Start();
        }
        public void Disconnect()
        {
            IsOn = false;
            Client.Close();            
        }

        public bool IsOn = false;

        Thread ListenerThread;

        private NetworkStream ns;

        List<byte> CurrentBuffer;
        int pid = -1;
        uint packetlen;
        bool iswaitingforlen = false;
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
                        Client.Connect("127.0.0.1" , 48486);
                    }                    
                }
                catch (Exception ex)
                {
                    parent.RaiseError(ex , "Error occured when connecting to tcp server");
                    Thread.Sleep(500);
                }

                if (Client.Connected)
                {
                    using (ns = Client.GetStream())
                    {
                        byte[] _buffer = new byte[1024];
                        while (Client.Connected)
                        {                            
                            //try to read
                            if (ns.CanRead)
                            {

                                //read the available data
                                int numberOfBytesRead;
                                try
                                {
                                    while ((numberOfBytesRead = ns.Read(_buffer , 0 , _buffer.Length)) > 0)
                                    {

                                        //only dealth with the data if numberOfBytesRead was greater than 0
                                        var read_buffer = new List<byte>(_buffer);

                                        //some bytes were read
                                        if (pid == -1)
                                        {
                                            //receive the header
                                            pid = read_buffer[0];
                                            read_buffer.RemoveAt(0);

                                            CurrentBuffer = new List<byte>();
                                            CurrentBuffer.AddRange(read_buffer.GetRange(0 , numberOfBytesRead - 1));
                                            if (CurrentBuffer.Count >= 4)
                                            {
                                                using (var actual_stream = new MemoryStream(CurrentBuffer.ToArray()))
                                                using (var br = new BinaryReader(actual_stream , Encoding.ASCII))
                                                {
                                                    packetlen = br.ReadZUInt32();
                                                    CurrentBuffer.RemoveRange(0 , 4);
                                                }
                                            }
                                            else
                                            {
                                                packetlen = 0;
                                                iswaitingforlen = true;
                                            }
                                        }
                                        else
                                        {
                                            //receive the actual packet                                    
                                            CurrentBuffer.AddRange(read_buffer.GetRange(0 , numberOfBytesRead));
                                            if (iswaitingforlen)
                                            {
                                                if (CurrentBuffer.Count >= 4)
                                                {
                                                    using (var actual_stream = new MemoryStream(CurrentBuffer.ToArray()))
                                                    using (var br = new BinaryReader(actual_stream , Encoding.ASCII))
                                                    {
                                                        packetlen = br.ReadZUInt32();
                                                        CurrentBuffer.RemoveRange(0 , 4);
                                                        iswaitingforlen = false;
                                                    }

                                                    if (CurrentBuffer.Count > packetlen)
                                                    {
                                                        while (CurrentBuffer.Count > packetlen)
                                                        {

                                                            //combined packets got sent
                                                            byte[] wanted_packet = CurrentBuffer.GetRange(0 , (int)packetlen).ToArray();
                                                            SendPacketToAPI((byte)pid , wanted_packet);
                                                            CurrentBuffer.RemoveRange(0 , (int)packetlen);

                                                            //CurrentBuffer now holds the new packet
                                                            pid = CurrentBuffer[0];
                                                            CurrentBuffer.RemoveAt(0);
                                                            //reprocess initial packet
                                                            if (CurrentBuffer.Count >= 4)
                                                            {
                                                                //it includes len
                                                                using (var actual_stream = new MemoryStream(CurrentBuffer.ToArray()))
                                                                using (var br = new BinaryReader(actual_stream , Encoding.ASCII))
                                                                {
                                                                    packetlen = br.ReadZUInt32();
                                                                    CurrentBuffer.RemoveRange(0 , 4);
                                                                    iswaitingforlen = false;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                packetlen = 0;
                                                                iswaitingforlen = true;
                                                            }
                                                        }
                                                    }
                                                    else if (CurrentBuffer.Count == packetlen)
                                                    {
                                                        SendPacketToAPI((byte)pid , CurrentBuffer.ToArray());
                                                        pid = -1;
                                                    }


                                                }
                                                else
                                                {
                                                    packetlen = 0;
                                                    iswaitingforlen = true;
                                                }
                                            }
                                            else
                                            {
                                                //count bytes until len is over
                                                if (CurrentBuffer.Count > packetlen)
                                                {
                                                    while (CurrentBuffer.Count > packetlen)
                                                    {
                                                        //combined packets got sent
                                                        byte[] wanted_packet = CurrentBuffer.GetRange(0 , (int)packetlen).ToArray();
                                                        SendPacketToAPI((byte)pid , wanted_packet);
                                                        CurrentBuffer.RemoveRange(0 , (int)packetlen);

                                                        //CurrentBuffer now holds the new packet
                                                        pid = CurrentBuffer[0];
                                                        CurrentBuffer.RemoveAt(0);
                                                        //reprocess initial packet
                                                        if (CurrentBuffer.Count >= 4)
                                                        {
                                                            //it includes len
                                                            using (var actual_stream = new MemoryStream(CurrentBuffer.ToArray()))
                                                            using (var br = new BinaryReader(actual_stream , Encoding.ASCII))
                                                            {
                                                                packetlen = br.ReadZUInt32();
                                                                CurrentBuffer.RemoveRange(0 , 4);
                                                                iswaitingforlen = false;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            packetlen = 0;
                                                            iswaitingforlen = true;
                                                        }
                                                    }
                                                }
                                                else if (CurrentBuffer.Count == packetlen)
                                                {
                                                    SendPacketToAPI((byte)pid , CurrentBuffer.ToArray());
                                                    pid = -1;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    parent.RaiseError(ex , "Failed to read the data from the network stream");
                                }
                                
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




        private void SendPacketToAPI(byte spid , byte[] buffer)
        {
            ZloPacketReceived?.Invoke(spid , buffer);
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
                return false;
            }
            catch (Exception ex)
            {
                parent.RaiseError(ex , "Error occured when writing to network stream");
                return false;
            }

        }
    }
}
