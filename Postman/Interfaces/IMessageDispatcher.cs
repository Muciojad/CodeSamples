namespace Poseidon.Postman.Interfaces
{
    public interface IMessageDispatcher
    {
        void RegisterReceiver(IMessageReceiver receiver);
        void UnregisterReceiver(IMessageReceiver receiver);
        void Send<T>(T message);
    }
}