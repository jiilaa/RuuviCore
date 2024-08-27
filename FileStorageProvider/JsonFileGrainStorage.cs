using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

namespace net.jommy.Orleans;

public sealed class JsonFileGrainStorage : IGrainStorage
{
    private readonly FileGrainStorageOptions _options;
    private readonly ClusterOptions _clusterOptions;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public JsonFileGrainStorage(FileGrainStorageOptions options, IOptions<ClusterOptions> clusterOptions, JsonSerializerOptions jsonSerializerOptions)
    {
        _options = options;
        _clusterOptions = clusterOptions.Value;
        _jsonSerializerOptions = jsonSerializerOptions;
        Directory.CreateDirectory(_options.RootDirectory);
    }

    public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        throw new NotImplementedException();
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var fName = GetKeyString(stateName, grainId);
        var path = Path.Combine(_options.RootDirectory, fName!);
        var fileInfo = new FileInfo(path);
        if (fileInfo is { Exists: false })
        {
            grainState.State = (T)Activator.CreateInstance(typeof(T))!;
            return;
        }

        using var stream = fileInfo.OpenText();
        var storedData = await stream.ReadToEndAsync();
    
        grainState.State = JsonSerializer.Deserialize<T>(new BinaryData(storedData));
        grainState.ETag = fileInfo.LastWriteTimeUtc.ToString(CultureInfo.InvariantCulture);
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var storedData = JsonSerializer.Serialize(grainState.State, _jsonSerializerOptions);
        var fName = GetKeyString(stateName, grainId);
        var path = Path.Combine(_options.RootDirectory, fName!);
        var fileInfo = new FileInfo(path);
        if (fileInfo.Exists && fileInfo.LastWriteTimeUtc.ToString(CultureInfo.InvariantCulture) != grainState.ETag)
        {
            throw new InconsistentStateException($"""
                                                  Version conflict (WriteState): ServiceId={_clusterOptions.ServiceId}
                                                  GrainType={typeof(T)} GrainReference={grainId}.
                                                  """);
        }

        await File.WriteAllBytesAsync(path, new BinaryData(storedData).ToArray());

        fileInfo.Refresh();
        grainState.ETag = fileInfo.LastWriteTimeUtc.ToString(CultureInfo.InvariantCulture);    
    }

    private string GetKeyString(string grainType, GrainId grainId) =>
        $"{_clusterOptions.ServiceId}.{Normalize(grainId.Key)}.{grainType}.json";

    private static string Normalize(IdSpan id)
    {
        var idString = id.ToString();
        return idString.Replace('/', '_').Replace('\\', '_').Replace(':', '_');
    }
}