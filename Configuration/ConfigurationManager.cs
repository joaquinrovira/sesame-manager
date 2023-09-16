using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class ConfigAttribute : Attribute
{
    public readonly Maybe<string> Section = Maybe.None;

    public ConfigAttribute(params string[] ConfigPath)
    {
        if (ConfigPath.Length != 0) Section = string.Join(":", ConfigPath);
    }
}

public static class ConfigCollectionExtensions
{
    public static void AutoRegisterConfiguration(this IServiceCollection collection, IConfiguration configuration, Assembly assembly, params Assembly[] assemblies)
        => ConfigAttributeProcessor.Apply(collection, configuration, assembly, assemblies);
    public static void AutoRegisterConfiguration<T>(this IServiceCollection collection, IConfiguration configuration)
        => collection.AutoRegisterConfiguration(configuration, typeof(T).Assembly);
}


public class ConfigAttributeProcessor : AssemblyAttrbiuteProcessor<ConfigAttribute>
{
    IConfiguration Configuration;
    private ConfigAttributeProcessor(IServiceCollection collection, IConfiguration configuration) : base(collection)
    {
        collection.AddOptions();
        Configuration = configuration;
    }
    public static void Apply(IServiceCollection collection, IConfiguration configuration, Assembly assembly, params Assembly[] assemblies)
        => new ConfigAttributeProcessor(collection, configuration).AutoRegisterServices(assembly, assemblies);

    // protected override void ProcessItem(Type clazz, ConfigAttribute attribute) 
    // {
    //     Collection.AddSingleton(clazz, services => {
    //         // Initialize config instance with DI
    //         var value = ActivatorUtilities.CreateInstance(services, clazz);

    //         // Bind to the corresponding section
    //         var section = Configuration;
    //         if(attribute.Section.HasValue) section = Configuration.GetSection(attribute.Section.Value);
    //         section.Bind(value);

    //         // Validate value
    //         var context = new ValidationContext(value, services, null);
    //         var validationResults = new List<ValidationResult>();
    //         bool isValid = Validator.TryValidateObject(value, context, validationResults, true);
    //         if (!isValid) throw new ValidationException(validationResults.ToArray().ToString());

    //         // Return bound
    //         return value;
    //     });
    // }


    protected override void ProcessItem(Type clazz, ConfigAttribute attribute)
    {
        /**
        Collection.AddOptions<>() does not provide a non-generic instance (i.e., accepting Type as function paramenters)
        As such we must move from a Type clazz instance to the world of generics through this method.
        **/
        if (GenericProcessItem is null) throw new NullReferenceException($"unable to reflect on 'ConfigAttributeProcessor.ProcessItem<{clazz.Name}>(ConfigAttribute)'");
        var method = GenericProcessItem.MakeGenericMethod(clazz);
        method.Invoke(this, new object?[] { attribute });
    }
    MethodInfo? GenericProcessItem = typeof(ConfigAttributeProcessor).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                                        .Where(m => m.Name == "ProcessItem")
                                        .Select(m => new
                                        {
                                            Method = m,
                                            Params = m.GetParameters(),
                                            Args = m.GetGenericArguments()
                                        })
                                        .Where(x => x.Params.Length == 1 && x.Params[0].ParameterType == typeof(ConfigAttribute)
                                                    && x.Args.Length == 1)
                                        .Select(x => x.Method)
                                        .FirstOrDefault();


#pragma warning disable IDE0051 // NOTE: this method is called through reflection
    private void ProcessItem<TConfig>(ConfigAttribute attribute) where TConfig : class
    {
        var section = attribute.Section.Match(
            Some: section => Configuration.GetSection(section),
            None: () => Configuration
        );

        Collection.AddOptions<TConfig>()
        .Bind(attribute.Section.Match(
            Some: section => Configuration.GetSection(section),
            None: () => Configuration))
        .ValidateMiniValidation()
        .ValidateOnStart();
    }
#pragma warning restore IDE0051
}
