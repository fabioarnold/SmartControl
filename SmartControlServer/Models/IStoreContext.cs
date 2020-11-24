using System;
using System.Collections.Generic;

namespace SmartControlServer.Models
{
    public interface IStoreContext
    {
        List<StoreContext.Client> GetAllClients();
        StoreContext.Client GetClientState(Guid id, string name);
        void SetClientState(Guid id, string state);
    }
}