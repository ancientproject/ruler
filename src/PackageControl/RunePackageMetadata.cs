namespace ruler.Features
{
    using Google.Cloud.Firestore;

    [FirestoreData]
    public class RunePackageMetadata
    {
        public RunePackageMetadata() { }

        [FirestoreProperty]
        public MetadataStatusType Status { get; set; }
        [FirestoreProperty]
        public ulong DownloadCount { get; set; }
    }
}