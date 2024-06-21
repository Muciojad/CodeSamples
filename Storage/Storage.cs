namespace Data.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.Utils;
    using Messages;
    using Poseidon.Postman;
    using Poseidon.Postman.Interfaces;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Data/Storage")]
    public class Storage : SerializedScriptableObject, IMessageReceiver
    {
        #region Inspector
        [field: ShowInInspector] public List<StorageItem> CurrentItems { get; private set; } = new();
        #endregion

        #region Public API
        // Add a new item to the storage
        public void AddItem(StorageItem storageItem)
        {
            // Check for duplicate item by GUID
            if (CurrentItems.Any(x => x.Guid == storageItem.Guid))
            {
                // Update item if duplicate found
                UpdateItem(storageItem);
                return;
            }
            // Add item if not a duplicate
            CurrentItems.Add(storageItem);
        }

        // Remove an item from the storage by GUID
        public void RemoveItem(string itemGuid)
        {
            for (var i = 0; i < CurrentItems.Count; i++)
            {
                if (CurrentItems[i].Guid != itemGuid) continue;
                // Remove item if GUID matches
                CurrentItems.RemoveAt(i);
                return;
            }
        }

        // Update an existing item in the storage
        public void UpdateItem(StorageItem storageItem)
        {
            var matchingIndex = CurrentItems.FindIndex(x => x.Guid == storageItem.Guid);
            if (matchingIndex == -1) return;
            // Replace the item at the matching index
            CurrentItems[matchingIndex] = storageItem;
        }

        // Get all items in a specific category
        public List<StorageItem> GetItemsByCategory(StorageItemCategory category)
        {
            return CurrentItems.Where(x => x.Category == category).ToList();
        }

        // Get a single item based on a condition
        public StorageItem GetItemByCondition(Predicate<StorageItem> predicateCondition)
        {
            var results = CurrentItems.Where(item => predicateCondition(item)).ToList();
            return results.Count == 0 ? null : results[0];
        }
        #endregion

        #region Private Methods
        // Register this object as a message receiver when enabled
        private void OnEnable()
        {
            Postman.RegisterReceiver(this);
        }

        // Unregister this object as a message receiver when disabled
        private void OnDisable()
        {
            Postman.UnregisterReceiver(this);
        }
        #endregion

        #region Messages
        // List of message types this object listens to
        public List<Type> ListenedTypes => new List<Type>()
        {
            typeof(OnStorageItemDestroyedMessage)
        };

        // Handle received messages
        public void OnMessageReceived(object message)
        {
            if (message is not OnStorageItemDestroyedMessage itemDestroyed)
            {
                return;
            }
            // Remove item when a destruction message is received
            RemoveItem(itemDestroyed.StorageItem.Guid);
        }
        #endregion
    }
}