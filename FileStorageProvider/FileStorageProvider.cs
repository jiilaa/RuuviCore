using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Orleans.Storage;
using System.Text.Json;

// https://dotnet.github.io/orleans/1.5/Tutorials/Custom-Storage-Providers.html
namespace net.jommy.Orleans
{
    public class FileStorageProvider : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly FileStorageProviderOptions _options;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        public FileStorageProvider(FileStorageProviderOptions options)
        {
            _options = options;
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var fileInfo = new FileInfo(GetFilePath(grainType, grainReference.GetPrimaryKey()));

            if (!fileInfo.Exists)
            {
                return;
            }

            using var stream = fileInfo.OpenText();
            var storedData = await stream.ReadToEndAsync();

            grainState.State = JsonSerializer.Deserialize(storedData, grainState.State.GetType());
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var storedData = JsonSerializer.Serialize(grainState.State, _jsonSerializerOptions);

            var fileInfo = new FileInfo(GetFilePath(grainType, grainReference.GetPrimaryKey()));

            await using var stream = new StreamWriter(fileInfo.Open(FileMode.Create,FileAccess.Write));
            await stream.WriteAsync(storedData);
        }

        private string GetFilePath(string grainType, Guid grainPrimaryKey)
        {
            // grainType should contain a fully qualified name of a class. It is way too long for a file name, so just use the classname part. 
            if (grainType.Contains("."))
            {
                grainType = grainType.Substring(grainType.LastIndexOf(".", StringComparison.InvariantCulture)+1);
            }
            return Path.Combine(_options.Directory, $"{grainType}_{grainPrimaryKey}.json");
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var fileInfo = new FileInfo(GetFilePath(grainType, grainReference.GetPrimaryKey()));
            fileInfo.Delete();
            return Task.CompletedTask;
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe<FileStorageProvider>(ServiceLifecycleStage.ApplicationServices, Init);
        }

        private Task Init(CancellationToken ct)
        {
            var directory = new DirectoryInfo(_options.Directory);
            if (!directory.Exists)
                directory.Create();
            return Task.CompletedTask;
        }
    }
}
