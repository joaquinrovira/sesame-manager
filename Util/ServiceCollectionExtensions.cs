using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ServiceAttribute : Attribute
{
    public readonly ServiceLifetime ServiceLifetime;
    public readonly Type[] Interfaces;

    public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton, params Type[] interfazes)
    {
        ServiceLifetime = lifetime;
        Interfaces = interfazes;
    }
}

public static class ServiceCollectionExtensions
{
    public static void AutoRegisterServices(this IServiceCollection collection, Assembly assembly, params Assembly[] assemblies)
        => ServiceAttributeProcessor.Apply(collection, assembly, assemblies);
    public static void AutoRegisterServices<T>(this IServiceCollection collection)
        => AutoRegisterServices(collection, typeof(T).Assembly);
}

public class ServiceAttributeProcessor : AssemblyAttrbiuteProcessor<ServiceAttribute>
{
    private ServiceAttributeProcessor(IServiceCollection collection) : base(collection) { }
    public static void Apply(IServiceCollection collection, Assembly assembly, params Assembly[] assemblies)
        => new ServiceAttributeProcessor(collection).AutoRegisterServices(assembly, assemblies);


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


    private static bool AcceptType(Type interfaze)
    {
        return IgnoredTypes.Where(ingoredClass =>
        {
            // Normal comparison between types
            if (!interfaze.IsGenericType) return interfaze == ingoredClass;

            // Generics are a bit different
            var asGeneric = interfaze.GetGenericTypeDefinition();
            return asGeneric == ingoredClass;
        }).Count() == 0;
    }

    protected override void ProcessItem<T>(ServiceAttribute attribute) => _ProcessItem(typeof(T), attribute);
    private void _ProcessItem(Type clazz, ServiceAttribute attribute)
    {
        // Register the service with a lifetime as described by the attribute
        Collection.Add(new(clazz, clazz, attribute.ServiceLifetime));

        // Register the service as all relevant interfaces
        foreach (var interfaze in RegisterableInterfaces(clazz, attribute))
        {
            Collection.Add(
                new(
                    interfaze,
                    collection => collection.GetRequiredService(clazz),
                    attribute.ServiceLifetime
            ));
        }
    }
}