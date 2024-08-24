using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans;
using Orleans.Runtime;
using Orleans.Storage;

namespace net.jommy.Orleans;

public static class FileStorageProviderSiloExtensions
{
    /// <summary>
    /// Configure silo to use FileStorage for grain storage.
    /// </summary>
    public static ISiloBuilder AddFileStorageGrainStorage(this ISiloBuilder builder, string name,
        Action<FileStorageProviderOptions> configureOptions)
    {
        return builder.ConfigureServices(services => services.AddFileStorageGrainStorage(name, configureOptions));
    }

    /// <summary>
    /// Configure silo to use FileStorage as the default grain storage.
    /// </summary>
    public static ISiloBuilder AddFileStorageGrainStorageAsDefault(this ISiloBuilder builder,
        Action<OptionsBuilder<FileStorageProviderOptions>> configureOptions = null)
    {
        return builder.AddFileStorageGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, configureOptions);
    }

    /// <summary>
    /// Configure silo to use FileStorage for grain storage.
    /// </summary>
    private static ISiloBuilder AddFileStorageGrainStorage(this ISiloBuilder builder, string name,
        Action<OptionsBuilder<FileStorageProviderOptions>> configureOptions = null)
    {
        return builder.ConfigureServices(services => services.AddFileStorageGrainStorage(name, configureOptions));
    }

    /// <summary>
    /// Configure silo to use FileStorage as the default grain storage.
    /// </summary>
    public static IServiceCollection AddFileStorageGrainStorageAsDefault(this IServiceCollection services,
        Action<FileStorageProviderOptions> configureOptions)
    {
        return services.AddFileStorageGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, ob => ob.Configure(configureOptions));
    }

    /// <summary>
    /// Configure silo to use FileStorage for grain storage.
    /// </summary>
    private static IServiceCollection AddFileStorageGrainStorage(this IServiceCollection services, string name,
        Action<FileStorageProviderOptions> configureOptions)
    {
        return services.AddFileStorageGrainStorage(name, ob => ob.Configure(configureOptions));
    }

    /// <summary>
    /// Configure silo to use FileStorage as the default grain storage.
    /// </summary>
    public static IServiceCollection AddFileStorageGrainStorageAsDefault(this IServiceCollection services,
        Action<OptionsBuilder<FileStorageProviderOptions>> configureOptions = null)
    {
        return services.AddFileStorageGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, configureOptions);
    }

    /// <summary>
    /// Configure silo to use FileStorage for grain storage.
    /// </summary>
    private static IServiceCollection AddFileStorageGrainStorage(
        this IServiceCollection services, 
        string name,
        Action<OptionsBuilder<FileStorageProviderOptions>> configureOptions = null)
    {
        configureOptions?.Invoke(services.AddOptions<FileStorageProviderOptions>(name));

        services.TryAddSingleton(sp => sp.GetRequiredKeyedService<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
        services.ConfigureNamedOptionForLogging<FileStorageProviderOptions>(name);

        services.AddKeyedSingleton(name, FileSystemGrainStorageFactory.Create);
        services.AddKeyedSingleton(name, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredKeyedService<IGrainStorage>(n));

        return services;
    }
}