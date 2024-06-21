namespace Poseidon.Postman
{
    using System;
    using System.Collections.Generic;
    using Interfaces;

    public static class Postman
    {
        #region Private Variables
        // Private dictionary to hold receivers mapped by message type
        private static Dictionary<Type, List<Action<object>>> receivers = new Dictionary<Type, List<Action<object>>>();
        #endregion

        #region Public API
        // Register a receiver for its corresponding message types
        public static void RegisterReceiver(IMessageReceiver receiver)
        {
            foreach (var listenedType in receiver.ListenedTypes)
            {
                if (receivers.TryGetValue(listenedType, out var existingItem))
                {
                    // Add receiver's method to the existing list for the message type
                    existingItem.Add(receiver.OnMessageReceived);
                    continue;
                }
                // Create a new list for the message type and add the receiver's method
                receivers.Add(listenedType, new List<Action<object>>(){receiver.OnMessageReceived});
            }
        }
    
        // Unregister a receiver from its corresponding message types
        public static void UnregisterReceiver(IMessageReceiver receiver)
        {
            List<Type> entriesToRemove = new List<Type>();
            foreach (var listenedType in receiver.ListenedTypes)
            {
                if (receivers.TryGetValue(listenedType, out var existingItem))
                {
                    // Remove receiver's method from the list for the message type
                    existingItem.Remove(receiver.OnMessageReceived);
                    // Mark message type for removal if no receivers remain
                    if (existingItem.Count == 0)
                    {
                        entriesToRemove.Add(listenedType);
                    }
                }
            }
            // Remove empty dictionary entries
            foreach (var entryToRemove in entriesToRemove)
            {
                receivers.Remove(entryToRemove);
            }
        }
    
        // Send a message to all registered receivers for T type
        public static void Send<T>(T message)
        {
            if (!receivers.TryGetValue(typeof(T), out var receiversList)) return;
            for (var index = receiversList.Count - 1; index >= 0; index--)
            {
                // Ensure the index is still valid
                if (index >= receiversList.Count) continue;
                // Invoke each receiver's method with the message
                var receiver = receiversList[index];
                receiver.Invoke(message);
            }
        }
        #endregion
    }

}