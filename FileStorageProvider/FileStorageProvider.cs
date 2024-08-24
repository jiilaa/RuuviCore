using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Orleans.Storage;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace net.jommy.Orleans;

public class FileStorageProvider : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly FileStorageProviderOptions _options;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
    private readonly string _name;
    private readonly string _serviceId;

    public FileStorageProvider(IOptions<FileStorageProviderOptions> options)
    {
        _options = options.Value;
        _serviceId = _options.ServiceId;
        _name = _options.Name;
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var fileInfo = new FileInfo(GetFilePath(grainId));

        if (!fileInfo.Exists)
        {
            return;
        }

        using var stream = fileInfo.OpenText();
        var storedData = await stream.ReadToEndAsync();

        grainState.State = JsonSerializer.Deserialize<T>(storedData, _jsonSerializerOptions);
        grainState.ETag = fileInfo.LastWriteTimeUtc.Ticks.ToString();
    }

    /// <inheritdoc />
    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var storedData = JsonSerializer.Serialize(grainState.State, _jsonSerializerOptions);
        var fileInfo = new FileInfo(GetFilePath(grainId));
        if (fileInfo.Exists && fileInfo.LastWriteTimeUtc.Ticks.ToString() != grainState.ETag)
        {
            throw new InconsistentStateException(
                $"Version conflict (WriteState): ServiceId={_serviceId} " +
                $"ProviderName={_name} GrainType={grainId.Type} " +
                $"GrainReference={grainId.ToString()}.");
        }

        await using var stream = new StreamWriter(fileInfo.Open(FileMode.Create,FileAccess.Write));
        await stream.WriteAsync(storedData);
    }

    private string GetFilePath(GrainId grainId)
    {
        // grainType should contain a fully qualified name of a class. It is way too long for a file name, so just use the classname part. 
        // if (grainType.Contains("."))
        // {
        //     grainType = grainType.Substring(grainType.LastIndexOf(".", StringComparison.InvariantCulture)+1);
        // }
        return Path.Combine(_options.Directory, $"{grainId.Type}_{grainId.Key}.json");
    }

    /// <inheritdoc />
    public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var fileInfo = new FileInfo(GetFilePath(grainId));
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