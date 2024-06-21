namespace Data.Storage
{
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable]
    public abstract class StorageItem
    {
        #region Public Variables
        [field: SerializeField, BoxGroup("GUID")] public string Guid { get; protected set; }
        [field: SerializeField] public StorageItemCategory Category { get; protected set; }
        #endregion

        #region Public API

        public StorageItem()
        {
        }
        public StorageItem(string guid, StorageItemCategory category)
        {
            Guid = guid;
            Category = category;
        }

        #endregion

        #region Private Methods

        [Button, BoxGroup("GUID")]
        private void RegenerateGuid()
        {
            Guid = System.Guid.NewGuid().ToString();
        }

        #endregion
    }
}