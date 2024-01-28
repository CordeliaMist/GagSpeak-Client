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

// namespace GagSpeak.Services;
// /// <summary>
// /// This is a new serviceManager class to replace the older one. It is pretty much ripped from glamourer's service manager. 
// /// and also more nessisary the more complex this code gets, and the more things it requires.
// /// </summary>
// public class ServiceManager : IDisposable
// {
//     // create a dalamud service wrapper
//     private class DalamudServiceWrapper<T> {
//         [PluginService]
//         [RequiredVersion("1.0")]
//         public T Service { get; private set; } = default(T);
//         public DalamudServiceWrapper(DalamudPluginInterface pi) {
//             pi.Inject(this);
//         }
//     }

//     private readonly Logger _logger;
//     // create the service collection here instead of the handler
//     private readonly ServiceCollection _collection = new ServiceCollection();
//     // create the timers tracker
//     public readonly StartTimeTracker Timers = new StartTimeTracker();
//     // create the provider
//     public ServiceProvider? Provider { get; private set; }
//     // Default Constructor
//     public ServiceManager(Logger logger)
//     {
//         _logger = logger;
//         _collection.AddSingleton(_logger);
//         _collection.AddSingleton(this);
//     }

//     /// <summary> Gets the services that we are implementing to our service manager.
//     /// <list type="bullet">
//     /// <item><c>T</c><typeparam name="T"> The service type to implement.</typeparam></item>
//     /// </list> </summary>
//     public IEnumerable<T> GetServicesImplementing<T>() {
//         // we will want to break out if the provider is null
//         if (Provider == null)
//             yield break;

//         // otherwise, we will want to get the type of the service
//         Type type = typeof(T);
//         // and then we will want to loop through the collection
//         foreach (ServiceDescriptor typeDescriptor in _collection) {
//             // if descriptor is singleton & an assignable service to the type, then return the service type
//             if (typeDescriptor.Lifetime == ServiceLifetime.Singleton && typeDescriptor.ServiceType.IsAssignableTo(type)) {
//                 yield return (T)Provider.GetRequiredService(typeDescriptor.ServiceType);
//             }
//         }
//     }

//     /// <summary> Gets the service that we are implementing to our service manager.
//     /// <list type="bullet">
//     /// <item><c>T</c><typeparam name="T"> The type of service we are getting.</typeparam></item>
//     /// </list> </summary>
//     /// <returns> The service that we are implementing to our service manager.</returns>
//     public T GetService<T>() where T : class {
//         return Provider.GetRequiredService<T>();
//     }

//     /// <summary> Creates the provider for the service manager. </summary>
//     public ServiceProvider CreateProvider() {
//         if (Provider != null) {
//             return Provider;
//         }
//         Provider = _collection.BuildServiceProvider(new ServiceProviderOptions {
//             ValidateOnBuild = true,
//             ValidateScopes = false
//         });
//         return Provider;
//     }

//     /// <summary> Ensures that the required services are implemented. </summary>
//     public void EnsureRequiredServices() {
//         CreateProvider();
//         foreach (ServiceDescriptor item in _collection) {
//             if (item.ServiceType.IsAssignableTo(typeof(IRequiredService))) {
//                 Provider?.GetRequiredService(item.ServiceType);
//             }
//         }
//     }

//     /// <summary> Adds a singleton service to the service manager.
//     /// <list type="bullet">
//     /// <item><c>Func(IServiceProvider, T)</c><typeparam name="T"> The singleton we're adding to our service provider factory.</typeparam></item>
//     /// </list> </summary>
//     /// <returns> The service manager with the singleton service added.</returns>
//     public ServiceManager AddSingleton<T>(Func<IServiceProvider, T> factory) where T : class {
//         Func<IServiceProvider, T> factory2 = factory;
//         _collection.AddSingleton(Func);
//         return this;
//         T Func(IServiceProvider p) {
//             Logger logger = _logger;
//             Logger logger2 = logger;
//             bool isEnabled;
//             Logger.VerboseInterpolatedStringHandler builder = new Logger.VerboseInterpolatedStringHandler(51, 1, logger, out isEnabled);
//             if (isEnabled) {
//                 builder.AppendLiteral("Constructing Service ");
//                 builder.AppendFormatted(typeof(T).Name);
//                 builder.AppendLiteral(" with custom factory function.");
//             }

//             logger2.Verbose(builder);
//             using (Timers.Measure(typeof(T).Name)) {
//                 return factory2(p);
//             }
//         }
//     }

//     /// <summary> Adds ISingleton service to the service manager.
//     /// <list type="bullet">
//     /// <item><c>Assembly</c><paramref name="assembly"> The assembly to add the ISingleton service from.</paramref></item>
//     /// </list> </summary>
//     public void AddIServices(Assembly assembly) {
//         GagSpeak.Log.Debug($"Adding IServices from {assembly.FullName}");
//         Type iType = typeof(IService);
//         foreach (Type type in assembly.ExportedTypes.Where((Type t) => (object)t != null && !t.IsInterface && !t.IsAbstract && iType.IsAssignableFrom(t))) {
//             if (_collection.All((ServiceDescriptor t) => t.ServiceType != type)) {
//                 AddSingleton(type);
//             }
//         }
//     }

//     /// <summary> Adds a Dalamud Singleton Service to the service manager.
//     /// <list type="bullet">
//     /// <item><c>DalamudPluginInterface</c><typeparam name="pi"> The pluginInterface we are adding the dalamudService for.</typeparam></item>
//     /// </list> </summary>
//     public ServiceManager AddDalamudService<T>(DalamudPluginInterface pi) where T : class {
//         GagSpeak.Log.Debug($"Adding Dalamud Service {typeof(T).Name}");
//         DalamudServiceWrapper<T> dalamudServiceWrapper = new DalamudServiceWrapper<T>(pi);
//         _collection.AddSingleton(dalamudServiceWrapper.Service);
//         _collection.AddSingleton(pi);
//         return this;
//     }

//     /// <summary> Adds a Dalamud Singleton Service to the service manager. </summary>
//     public ServiceManager AddExistingService<T>(T service) where T : class {
//         GagSpeak.Log.Debug($"Adding Existing Service {typeof(T).Name}");
//         _collection.AddSingleton(service);
//         return this;
//     }

//     /// <summary> Disposes of our service manager. </summary>
//     public void Dispose() {
//         GagSpeak.Log.Debug("Disposing all services.");
//         Provider?.Dispose();
//         GagSpeak.Log.Debug("Disposed all services.");
//         GC.SuppressFinalize(this);
//     }

//     /// <summary> Adds a singleton service to the service manager of type T. </summary>
//     public ServiceManager AddSingleton<T>() {
//         return AddSingleton(typeof(T));
//     }

//     /// <summary> Adds Singleton Service with a type name to the service manager.
//     /// <list type="bullet">
//     /// <item><c>Type</c><paramref name="type"> The type of service we are adding.</paramref></item>
//     /// </list> </summary>
//     /// <returns> The service manager with the singleton service added.</returns>
//     private ServiceManager AddSingleton(Type type) {
//         GagSpeak.Log.Debug($"Adding Service {type.FullName}");
//         Type type2 = type;
//         _collection.AddSingleton(type2, Func);
//         return this;
//         object Func(IServiceProvider p) {
//             IServiceProvider p2 = p;
//             ConstructorInfo? constructorInfo = type2.GetConstructors().MaxBy((ConstructorInfo c) => c.GetParameters().Length);
//             if (!(constructorInfo == null))
//             {
//                 ParameterInfo[] parameters = constructorInfo.GetParameters();
//                 object[] parameters2 = parameters.Select((ParameterInfo t) => {
//                 try
//                 {
//                     return p2.GetRequiredService(t.ParameterType);
//                 }
//                 catch (InvalidOperationException)
//                 {
//                     GagSpeak.Log.Error($"Failed to resolve service for type {t.ParameterType.FullName} while initializing {type2.FullName}");
//                     throw;
//                 }}).ToArray();
//                 Logger logger = _logger;
//                 Logger logger2 = logger;
//                 bool isEnabled;
//                 Logger.VerboseInterpolatedStringHandler builder = new Logger.VerboseInterpolatedStringHandler(28, 2, logger, out isEnabled);
//                 if (isEnabled)
//                 {
//                     builder.AppendLiteral("Constructing Service ");
//                     builder.AppendFormatted(type2.Name);
//                     builder.AppendLiteral(" with ");
//                     builder.AppendFormatted(string.Join(", ", parameters.Select((ParameterInfo name) => $"{name.ParameterType}")));
//                     builder.AppendLiteral(".");
//                 }

//                 logger2.Verbose(builder);
//                 using (Timers.Measure(type2.Name))
//                 {
//                     return constructorInfo.Invoke(parameters2);
//                 }
//             }

//             return Activator.CreateInstance(type2) ?? throw new Exception("No constructor available for " + type2.Name + ".");
//         }
//     }
// }