using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveSockets;

namespace Zlo
{
    public class ZloClient
    {
        /*
         connect to localhost:48486
         packet - 4byte size, 1byte type, payload[size]
         */
        private ReactiveClient m_client;
        public ReactiveClient ListenerClient
        {
            get { return m_client; }
        }

        public ZloClient()
        {
            try
            {
                m_client = new ReactiveClient("127.0.0.1" , 48486);


                ListenerClient.Connected += Client_Connected;
                ListenerClient.Disconnected += Client_Disconnected;

                ListenerClient.Receiver.SubscribeOn(TaskPoolScheduler.Default).Subscribe(
                    s => ZloClient_DataReceived(s) ,
                    e => Console.WriteLine(e.ToString()) ,
                    () => Console.WriteLine("Socket receiver completed")
                    );


                ListenerClient.Sender.SubscribeOn(TaskPoolScheduler.Default).Subscribe(
                s => ZloClient_DataSent(s) ,
                   e => Console.WriteLine(e.ToString()) ,
                   () => Console.WriteLine("Socket Sender completed")
               );

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ZloClient_DataReceived(byte obj)
        {
            Console.WriteLine(obj);
        }
        private void ZloClient_DataSent(byte obj)
        {
            Console.WriteLine(obj);
        }
        private void Client_Disconnected(object sender , EventArgs e)
        {
            Console.WriteLine("Log 0 : Client Disconnected");
        }

        private void Client_Connected(object sender , EventArgs e)
        {
            Console.WriteLine("Log 0 : Client Connected");
        }


        public async void Connect()
        {
            await ListenerClient.ConnectAsync();
            await ListenerClient.SendAsync(new byte[] { 1,0,0,0 });
           
            Console.WriteLine("Done");
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
