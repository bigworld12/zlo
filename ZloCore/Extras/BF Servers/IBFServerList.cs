using System;
using System.Collections;
using System.Collections.Generic;

namespace Zlo.Extras
{
    public interface IBFServerList<T> : IReadOnlyList<T> where T : class,IBFServerBase
    {
        internal void UpdateServerInfo(uint ServerID, byte[] info);
        internal void UpdateServerPlayers(uint ServerID, byte[] info);

        internal void RemoveServer(uint ServerID);
        internal void AddServer(T server);

        internal Func<uint, T> CreateServer { get; }

        public T Find(uint ServerID);
        

        event API_BFServerEventHandler<T> ServerChanged;
    }
}
