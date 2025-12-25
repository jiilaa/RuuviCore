using Orleans.Storage;

namespace net.jommy.Orleans;

public sealed class FileGrainStorageOptions : IStorageProviderSerializerOptions
{
    public required string RootDirectory { get; set; }

    public required IGrainStorageSerializer GrainStorageSerializer { get; set; }
}