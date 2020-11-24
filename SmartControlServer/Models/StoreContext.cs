using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartControlServer.Models
{
    // TODO: Use actual database
    public class StoreContext : IStoreContext
    {
        public class Client
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
            public ulong Offset { get; set; }
        }

        private Dictionary<Guid, Client> _clients = new Dictionary<Guid, Client>();

        public List<Client> GetAllClients()
        {
            lock (_clients)
            {
                return _clients.Values.ToList();
            }
        }

        public Client GetClientState(Guid id, string name)
        {
            var client = GetClient(id);
            if (client == null)
            {
                client = new Client { Id = id, Name = name };
                SetClient(client);
            }
            return client;
        }

        public void SetClientState(Guid id, string state)
        {
            var client = GetClient(id);
            if (client != null)
            {
                client.State = state;
                client.Offset++;
            }
        }

        private Client GetClient(Guid id)
        {
            lock (_clients)
            {
                return _clients.GetValueOrDefault(id);
            }
        }

        private void SetClient(Client client)
        {
            lock (_clients)
            {
                _clients[client.Id] = client;
            }
        }
    }
}
