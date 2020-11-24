using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class NotificationService : INotificationService
{
    private Dictionary<Guid, Dictionary<Guid, Client>> _clients = new Dictionary<Guid, Dictionary<Guid, Client>>();

    public class Client
    {
        public Guid Id { get; set; }
        public TaskCompletionSource Tcs { get; set; }
    }

    public void Subscribe(Guid clientId, Guid topicId, TaskCompletionSource tcs)
    {
        var client = new Client
        {
            Id = clientId,
            Tcs = tcs,
        };
        lock (_clients)
        {
            if (!_clients.ContainsKey(topicId))
            {
                _clients[topicId] = new Dictionary<Guid, Client>();
            }
           _clients[topicId][clientId] = client;
        }
    }

    public void Unsubscribe(Guid clientId, Guid topicId)
    {
        lock (_clients)
        {
            if (_clients.ContainsKey(topicId))
            {
                _clients[topicId].Remove(clientId);
                if (_clients[topicId].Count == 0)
                {
                    _clients.Remove(topicId);
                }
            }
        }
    }

    public void Notify(Guid topicId)
    {
        List<Client> clients = null;
        lock (_clients)
        {
            clients = _clients.GetValueOrDefault(topicId)?.Values.ToList();
        }
        if (clients != null)
        {
            foreach (var client in clients)
            {
                client.Tcs.TrySetResult();
            }
        }
    }
}