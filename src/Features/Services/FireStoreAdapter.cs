namespace ruler.Features
{
    using Google.Cloud.Firestore;

    public class FireStoreAdapter : IFireStoreAdapter
    {
        private FirestoreDb db { get; }
        public FireStoreAdapter() => db = FirestoreDb.Create();


        public CollectionReference Cluster => db.Collection("cluster");
    }

    public interface IFireStoreAdapter
    {
        CollectionReference Cluster { get; }
    }
}