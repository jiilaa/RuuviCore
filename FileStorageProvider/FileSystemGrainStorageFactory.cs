using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Storage;

namespace net.jommy.Orleans
{
    public static class FileSystemGrainStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsSnapshot = services.GetRequiredService<IOptionsSnapshot<FileStorageProviderOptions>>();

            return ActivatorUtilities.CreateInstance<FileStorageProvider>(services, optionsSnapshot.Get(name));
        }
    }
}
