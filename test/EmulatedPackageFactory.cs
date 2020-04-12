namespace ProjectTest
{
    using System.IO;
    using NuGet.Versioning;
    using System.IO.Compression;
    using Ancient.ProjectSystem;
    using Newtonsoft.Json;

    public static class EmulatedPackageFactory
    {
        public static RunePackage Create(string ID, NuGetVersion version)
        {
            var folder = Path.GetTempPath();

            var formatted = $"{ID}-{version}";

            var spec = CastToJson(GetSpec(ID, version));
            var file = Path.Combine(folder, $"{formatted}.rspec");
            File.WriteAllText(file, spec);

            using var memoryStream = new MemoryStream();
            using var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create);
            zip.CreateEntryFromFile(file, $"{formatted}.rspec");

            File.WriteAllBytes(Path.Combine(folder, $"{formatted}.rpkg"), memoryStream.ToArray());

            return new RunePackage()
            {
                Content = memoryStream,
                ID = ID,
                Version = version
            };
        }

        public static RunePackageSpecification GetSpec(string ID, NuGetVersion version)
        {
            return new RunePackageSpecification
            {
                ID = ID,
                Version = $"{version}",
                Owner = "test@bu.ka"
            };
        }

        public static string CastToJson<T>(T t) => JsonConvert.SerializeObject(t, Formatting.Indented);
    }

    public class RunePackageSpecification
    {
        public string ID { get; set; }
        public string Version { get; set; }
        public string Owner { get; set; }
    }
}