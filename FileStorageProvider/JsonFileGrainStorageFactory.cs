using System;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Storage;

namespace net.jommy.Orleans;

internal static class JsonFileGrainStorageFactory
{
    internal static IGrainStorage Create(IServiceProvider services, string name)
    {
        var optionsMonitor = services.GetRequiredService<IOptionsMonitor<FileGrainStorageOptions>>();

        return ActivatorUtilities.CreateInstance<JsonFileGrainStorage>(
            services,
            optionsMonitor.Get(name),
            services.GetProviderClusterOptions(name),
            services.GetRequiredService<JsonSerializerOptions>());
    }
}