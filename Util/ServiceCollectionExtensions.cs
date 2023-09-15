using System.Collections.ObjectModel;
using System.Reflection;
using CSharpFunctionalExtensions;

namespace Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
public class ServiceAttribute : Attribute {
    public readonly ServiceLifetime ServiceLifetime;
    public readonly Type[] Interfaces;

    public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton, params Type[] interfazes) {
        ServiceLifetime = lifetime;
        Interfaces = interfazes;
    }
}

public static class ServiceCollectionExtensions {
    public static void AutoRegister(this IServiceCollection collection, Assembly assembly, params Assembly[] assemblies) 
    {
        AutoRegisterFromAssembly(collection, assembly);
        foreach (var a in assemblies) AutoRegisterFromAssembly(collection, a);
    }

    private static void AutoRegisterFromAssembly(IServiceCollection collection, Assembly assembly) 
    {
        IList<(Type, ServiceAttribute)> services = FindServicesInAssembly(assembly);
        foreach (var (clazz, attribute) in services) RegisterService(collection, clazz, attribute);
    }
    private static IList<(Type, ServiceAttribute)> FindServicesInAssembly(Assembly assembly) 
    {
        var result = new List<(Type, ServiceAttribute)>();
        foreach(var clazz in assembly.GetTypes()) 
        {
            var attributes = clazz.GetCustomAttributes<ServiceAttribute>(true);
            foreach (var attribute in attributes) result.Add((clazz, attribute));
        }
        return result;
    }

    private static void RegisterService(IServiceCollection collection, Type clazz, ServiceAttribute attribute) 
    {
        // Register the service with a lifetime as described by the attribute
        collection.Add(new(clazz, clazz, attribute.ServiceLifetime));

        // Register the service as all relevant interfaces
        foreach (var interfaze in RegisterableInterfaces(clazz, attribute)) {
            collection.Add(
                new(
                    interfaze, 
                    collection => collection.GetRequiredService(clazz), 
                    attribute.ServiceLifetime
            ));
        }
    }

    // Retrieve registerable interfaces either from attribute or infere them from implemented interfaces
    private static IEnumerable<Type> RegisterableInterfaces(Type clazz, ServiceAttribute attribute) 
    {
        if (attribute.Interfaces.Length != 0) return attribute.Interfaces;
        else return GetImplementedInterfaces(clazz);
    }

    // Infere interfaces from the class' imeplemented interfaces
    private static IEnumerable<Type> GetImplementedInterfaces(Type clazz) 
        => clazz.GetInterfaces().Where(AcceptType);

    // Ignored interfaces, we will ignore the following for registration purposes
    private static readonly Type[] IgnoredTypes = new Type[]
    {
        typeof(IEquatable<>)
    }; 

    private static bool AcceptType(Type interfaze) {
        return IgnoredTypes.Where(ingoredClass => {
            // Normal comparison between types
            if(!interfaze.IsGenericType) return interfaze == ingoredClass;

            // Generics are a bit different
            var asGeneric = interfaze.GetGenericTypeDefinition();
            return asGeneric == ingoredClass;
        }).Count() == 0;
    }
}