using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.IoC;
using Dalamud.Plugin;
using Microsoft.Extensions.DependencyInjection;
using OtterGui.Classes;
using OtterGui.Log;
using OtterGui.Services;

namespace GagSpeak.Services;
public class ServiceManager : IDisposable
{
    // create a dalamud service wrapper
    private class DalamudServiceWrapper<T> {
        [PluginService]
        [RequiredVersion("1.0")]
        public T Service { get; private set; } = default(T);
        public DalamudServiceWrapper(DalamudPluginInterface pi) {
            pi.Inject(this);
        }
    }

    private readonly Logger _logger;
    // create the service collection here instead of the handler
    private readonly ServiceCollection _collection = new ServiceCollection();
    
    // create the timers tracker
    public readonly StartTimeTracker Timers = new StartTimeTracker();

    // create the provider
    public ServiceProvider? Provider { get; private set; }

    public ServiceManager(Logger logger)
    {
        _logger = logger;
        _collection.AddSingleton(_logger);
        _collection.AddSingleton(this);
    }

    public IEnumerable<T> GetServicesImplementing<T>()
    {
        if (Provider == null)
        {
            yield break;
        }

        Type type = typeof(T);
        foreach (ServiceDescriptor typeDescriptor in _collection)
        {
            if (typeDescriptor.Lifetime == ServiceLifetime.Singleton && typeDescriptor.ServiceType.IsAssignableTo(type))
            {
                yield return (T)Provider.GetRequiredService(typeDescriptor.ServiceType);
            }
        }
    }

    public T GetService<T>() where T : class {
        return Provider.GetRequiredService<T>();
    }

    public ServiceProvider CreateProvider() {
        if (Provider != null) {
            return Provider;
        }
        Provider = _collection.BuildServiceProvider(new ServiceProviderOptions {
            ValidateOnBuild = true,
            ValidateScopes = false
        });
        return Provider;
    }

    public void EnsureRequiredServices() {
        CreateProvider();
        foreach (ServiceDescriptor item in _collection) {
            if (item.ServiceType.IsAssignableTo(typeof(IRequiredService))) {
                Provider.GetRequiredService(item.ServiceType);
            }
        }
    }

    public ServiceManager AddSingleton<T>(Func<IServiceProvider, T> factory) where T : class {
        Func<IServiceProvider, T> factory2 = factory;
        _collection.AddSingleton(Func);
        return this;
        T Func(IServiceProvider p) {
            Logger logger = _logger;
            Logger logger2 = logger;
            bool isEnabled;
            Logger.VerboseInterpolatedStringHandler builder = new Logger.VerboseInterpolatedStringHandler(51, 1, logger, out isEnabled);
            if (isEnabled) {
                builder.AppendLiteral("Constructing Service ");
                builder.AppendFormatted(typeof(T).Name);
                builder.AppendLiteral(" with custom factory function.");
            }

            logger2.Verbose(builder);
            using (Timers.Measure(typeof(T).Name)) {
                return factory2(p);
            }
        }
    }

    public void AddIServices(Assembly assembly) {
        GagSpeak.Log.Debug($"Adding IServices from {assembly.FullName}");
        Type iType = typeof(IService);
        foreach (Type type in assembly.ExportedTypes.Where((Type t) => (object)t != null && !t.IsInterface && !t.IsAbstract && iType.IsAssignableFrom(t))) {
            if (_collection.All((ServiceDescriptor t) => t.ServiceType != type)) {
                AddSingleton(type);
            }
        }
    }
    public ServiceManager AddDalamudService<T>(DalamudPluginInterface pi) where T : class {
        GagSpeak.Log.Debug($"Adding Dalamud Service {typeof(T).Name}");
        DalamudServiceWrapper<T> dalamudServiceWrapper = new DalamudServiceWrapper<T>(pi);
        _collection.AddSingleton(dalamudServiceWrapper.Service);
        _collection.AddSingleton(pi);
        return this;
    }

    public ServiceManager AddExistingService<T>(T service) where T : class {
        GagSpeak.Log.Debug($"Adding Existing Service {typeof(T).Name}");
        _collection.AddSingleton(service);
        return this;
    }

    public void Dispose() {
        GagSpeak.Log.Debug("Disposing all services.");
        Provider?.Dispose();
        GagSpeak.Log.Debug("Disposed all services.");
        GC.SuppressFinalize(this);
    }

    public ServiceManager AddSingleton<T>() {
        return AddSingleton(typeof(T));
    }

    private ServiceManager AddSingleton(Type type) {
        GagSpeak.Log.Debug($"Adding Service {type.FullName}");
        Type type2 = type;
        _collection.AddSingleton(type2, Func);
        return this;
        object Func(IServiceProvider p) {
            IServiceProvider p2 = p;
            ConstructorInfo constructorInfo = type2.GetConstructors().MaxBy((ConstructorInfo c) => c.GetParameters().Length);
            if (!(constructorInfo == null))
            {
                ParameterInfo[] parameters = constructorInfo.GetParameters();
                object[] parameters2 = parameters.Select((ParameterInfo t) => {
                try
                {
                    return p2.GetRequiredService(t.ParameterType);
                }
                catch (InvalidOperationException)
                {
                    GagSpeak.Log.Error($"Failed to resolve service for type {t.ParameterType.FullName} while initializing {type2.FullName}");
                    throw;
                }}).ToArray();
                Logger logger = _logger;
                Logger logger2 = logger;
                bool isEnabled;
                Logger.VerboseInterpolatedStringHandler builder = new Logger.VerboseInterpolatedStringHandler(28, 2, logger, out isEnabled);
                if (isEnabled)
                {
                    builder.AppendLiteral("Constructing Service ");
                    builder.AppendFormatted(type2.Name);
                    builder.AppendLiteral(" with ");
                    builder.AppendFormatted(string.Join(", ", parameters.Select((ParameterInfo name) => $"{name.ParameterType}")));
                    builder.AppendLiteral(".");
                }

                logger2.Verbose(builder);
                using (Timers.Measure(type2.Name))
                {
                    return constructorInfo.Invoke(parameters2);
                }
            }

            return Activator.CreateInstance(type2) ?? throw new Exception("No constructor available for " + type2.Name + ".");
        }
    }
}