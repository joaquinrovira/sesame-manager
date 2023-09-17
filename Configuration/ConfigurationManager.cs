using System.Reflection;
using Microsoft.Extensions.Configuration;

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

public interface IConfig
{
    public Maybe<string> Section { get; }
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

    protected override void ProcessItem<TConfig>(ConfigAttribute attribute) where TConfig : class
    {
        Collection.AddOptions<TConfig>()
        .Bind(attribute.Section.Match(
            Some: section => Configuration.GetSection(section),
            None: () => Configuration))
        .ValidateMiniValidation()
        .ValidateOnStart();
    }
}