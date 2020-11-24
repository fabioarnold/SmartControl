using System;
using System.Threading.Tasks;

public interface INotificationService
{
    void Notify(Guid topicId);
    void Subscribe(Guid clientId, Guid topicId, TaskCompletionSource tcs);
    void Unsubscribe(Guid clientId, Guid topicId);
}