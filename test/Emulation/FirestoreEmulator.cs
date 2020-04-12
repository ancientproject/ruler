namespace ProjectTest.Emulation
{
    using System;
    using Google.Api.Gax;
    using Google.Cloud.Firestore;
    using Google.Cloud.Firestore.V1;
    using Grpc.Core;
    using ruler.Features;

    public class FirestoreEmulator : IFireStoreAdapter
    {
        private FirestoreDb db { get; }
        public FirestoreEmulator()
        {
            Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", "::1:8832");
            db = new FirestoreDbBuilder
            {
                ProjectId = "ruler-rune",
                EmulatorDetection = EmulatorDetection.EmulatorOnly
            }.Build();
        }


        public CollectionReference Cluster => db.Collection("cluster");
        public CollectionReference Packages => db.Collection("packages");
        public CollectionReference Tokens => db.Collection("tokens");
    }
}