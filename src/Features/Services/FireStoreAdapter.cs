namespace ruler.Features
{
    using Google.Cloud.Firestore;

    public class FireStoreAdapter : IFireStoreAdapter
    {
        private FirestoreDb db { get; }
        public FireStoreAdapter() => db = FirestoreDb.Create("ruler-rune");


        public CollectionReference Cluster => db.Collection("cluster");
        public CollectionReference Packages => db.Collection("packages");
        public CollectionReference Tokens => db.Collection("tokens");
    }

    public interface IFireStoreAdapter
    {
        CollectionReference Cluster { get; }
        CollectionReference Packages { get; }
        CollectionReference Tokens { get; }
    }
}