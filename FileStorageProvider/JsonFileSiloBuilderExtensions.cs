using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Hosting;
using Orleans.Storage;

namespace net.jommy.Orleans;

public static class JsonFileSiloBuilderExtensions
{
    public static ISiloBuilder AddJsonFileGrainStorage(this ISiloBuilder builder, string providerName, Action<FileGrainStorageOptions> options) =>
        builder.ConfigureServices(
            services => services.AddJsonFileGrainStorage(
                providerName, options));

    public static IServiceCollection AddJsonFileGrainStorage(this IServiceCollection services, string providerName, Action<FileGrainStorageOptions> options)
    {
        services
            .AddOptions<FileGrainStorageOptions>(providerName)
            .Configure(options);

        services.AddTransient<IPostConfigureOptions<FileGrainStorageOptions>,
            DefaultStorageProviderSerializerOptionsConfigurator<FileGrainStorageOptions>>();
        return services.AddGrainStorage(providerName, JsonFileGrainStorageFactory.Create)
            .AddKeyedSingleton(providerName,
                (p, n) =>
                    (ILifecycleParticipant<ISiloLifecycle>)p.GetRequiredKeyedService<IGrainStorage>(n));;
    }
}