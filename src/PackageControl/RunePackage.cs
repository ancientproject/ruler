namespace ruler.Features
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using NuGet.Versioning;

    public class RunePackage : IDisposable, IAsyncDisposable
    {
        public string ID { get; set; }
        public NuGetVersion Version { get; set; }
        public MemoryStream Content { get; set; }

        public void Dispose() 
            => Content?.Dispose();
        public ValueTask DisposeAsync() 
            => Content?.DisposeAsync() ?? new ValueTask(Task.CompletedTask);
    }
}