using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zlo.Extras.BF_Servers
{
    public class API_BFServerListBase<T> : IBFServerList<T> where T : class, IBFServerBase
    {
        internal API_BFServerListBase(Func<uint, T> createServer)
        {
            this.createServer = createServer;
        }
        private readonly List<T> store = new List<T>();
        private readonly Dictionary<uint, T> storeDict = new Dictionary<uint, T>();
        public T this[int index] => store[index];

        internal IBFServerList<T> AsEditable => this;

        public int Count => store.Count;

        private readonly Func<uint, T> createServer;
        Func<uint, T> IBFServerList<T>.CreateServer => createServer;

        public event API_BFServerEventHandler<T> ServerChanged;
        public T Find(uint ServerID)
        {
            storeDict.TryGetValue(ServerID, out var res);
            return res;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return store.GetEnumerator();
        }

        void IBFServerList<T>.AddServer(T server)
        {
            if (!storeDict.ContainsKey(server.ServerID))
            {
                store.Add(server);
                storeDict[server.ServerID] = server;
                ServerChanged?.Invoke(this, server.ServerID, server, ServerChangeTypes.Add);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return store.GetEnumerator();
        }

        void IBFServerList.RemoveServer(uint ServerID)
        {
            if (storeDict.TryGetValue(ServerID, out var server))
            {
                store.Remove(server);
                storeDict.Remove(ServerID);
                ServerChanged?.Invoke(this, server.ServerID, server, ServerChangeTypes.Remove);
            }
        }
        void IBFServerList.UpdateServerInfo(uint ServerID, byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                var asInterface = (IBFServerList<T>)this;
                serv = createServer(ServerID);
                serv.Parse(info);
                AsEditable.AddServer(serv);
            }
            else
            {
                //server exists
                serv.Parse(info);
                ServerChanged?.Invoke(this, ServerID, serv, ServerChangeTypes.Update);
            }
        }



        void IBFServerList.UpdateServerPlayers(uint ServerID, byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = createServer(ServerID);
                AsEditable.AddServer(serv);
            }
            //server exists
            serv.Players.Parse(info);
            ServerChanged?.Invoke(this, ServerID, serv, ServerChangeTypes.Update);
        }
    }
}
