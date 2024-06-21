namespace Data.Storage.Messages
{
    public class OnStorageItemDestroyedMessage
    {
        public readonly StorageItem StorageItem;

        public OnStorageItemDestroyedMessage(StorageItem storageItem)
        {
            StorageItem = storageItem;
        }
    }
}