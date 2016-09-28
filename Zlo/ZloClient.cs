using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Zlo
{
    public class ZloClient
    {
        /*
         connect to localhost:48486
         packet - 4byte size, 1byte type, payload[size]
         */
        private TcpClient client;
        public TcpClient Client
        {
            get { return client; }
        }

        public event ClientDataReceived DataReceived;


        public delegate void ClientDataReceived(TcpClient sender , ClientDataReceivedEventArgs args);
        public ZloClient()
        {
            try
            {
                client = new TcpClient();

                DataReceived += ZloClient_DataReceived;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ZloClient_DataReceived(TcpClient sender , ClientDataReceivedEventArgs args)
        {
            Console.WriteLine(args.State.buffer.Length);
        }

        public async Task<bool> Connect()
        {
            try
            {
                
                await client.ConnectAsync("127.0.0.1" , 48486);
                var stream = client.GetStream();
                StartListenLoop(stream);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            
        }
        private void StartListenLoop(NetworkStream stream)
        {
            Task.Run(async () =>
            {
                byte[] buffer = new byte[client.ReceiveBufferSize];
                
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer , 0 , buffer.Length);
                    if (bytesRead > 0)
                    {
                        var state = new StateObject();
                        state.buffer = buffer;
                        state.workSocket = client.Client;   
                        
                                             
                        ClientDataReceivedEventArgs a = new ClientDataReceivedEventArgs(state);
                        DataReceived?.Invoke(Client , a);                      
                    }
                }

            });
        }
    }
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;    
        // Receive buffer.
        public byte[] buffer = null;
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
    public class ClientDataReceivedEventArgs : EventArgs
    {
        private StateObject m_state;

        public StateObject State
        {
            get { return m_state; }           
        }
        public ClientDataReceivedEventArgs(StateObject s)
        {
            m_state = s;
        }
    }
}
