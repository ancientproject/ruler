namespace ruler.Features
{
    using Google.Cloud.Firestore;

    public class FireStoreAdapter : IFireStoreAdapter
    {
        private FirestoreDb db { get; }
        public FireStoreAdapter() => db = FirestoreDb.Create("ruler-rune");


        public CollectionReference Cluster => db.Collection("cluster");
    }

    public interface IFireStoreAdapter
    {
        CollectionReference Cluster { get; }
    }
}