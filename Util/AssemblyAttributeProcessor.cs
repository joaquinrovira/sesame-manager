using System.Reflection;

public abstract class AssemblyAttrbiuteProcessor<TAttribute> where TAttribute : Attribute
{
    protected IServiceCollection Collection;

    protected AssemblyAttrbiuteProcessor(IServiceCollection collection) { Collection = collection; }

    public void AutoRegisterServices(Assembly assembly, params Assembly[] assemblies)
    {
        AutoRegisterFromAssembly(assembly);
        foreach (var a in assemblies) AutoRegisterFromAssembly(a);
    }

    private void AutoRegisterFromAssembly(Assembly assembly)
    {
        IList<(Type, TAttribute)> services = FindItemsInAssembly(assembly);
        foreach (var (clazz, attribute) in services) ProcessItem(clazz, attribute);
    }
    private IList<(Type, TAttribute)> FindItemsInAssembly(Assembly assembly)
    {
        var result = new List<(Type, TAttribute)>();
        foreach (var clazz in assembly.GetTypes())
        {
            var attributes = clazz.GetCustomAttributes<TAttribute>(true);
            foreach (var attribute in attributes) result.Add((clazz, attribute));
        }
        return result;
    }

    private void ProcessItem(Type clazz, TAttribute attribute)
    {
        /**
        Collection.AddOptions<>() does not provide a non-generic instance (i.e., accepting Type as function paramenters)
        As such we must move from a 'Type clazz' instance to the world of generics through this method.
        **/
        var ThisType = GetType();
        var AttributeType = typeof(TAttribute);
        MethodInfo? GenericProcessItem = ThisType.GetRuntimeMethods()
            .Where(m => m.Name == "ProcessItem")
            .Select(m => new
            {
                Method = m,
                Params = m.GetParameters(),
                Args = m.GetGenericArguments()
            })
            .Where(x => x.Params.Length == 1 && x.Params[0].ParameterType == typeof(TAttribute)
                        && x.Args.Length == 1)
            .Select(x => x.Method)
            .FirstOrDefault();

        if (GenericProcessItem is null) throw new NullReferenceException($"unable to reflect on '{ThisType.Name}.ProcessItem<{clazz.Name}>({AttributeType.Name})'");
        var method = GenericProcessItem.MakeGenericMethod(clazz);
        method.Invoke(this, new object?[] { attribute });
    }


    protected abstract void ProcessItem<T>(TAttribute attribute) where T : class;

}