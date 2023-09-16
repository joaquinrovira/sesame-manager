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
        foreach(var clazz in assembly.GetTypes()) 
        {
            var attributes = clazz.GetCustomAttributes<TAttribute>(true);
            foreach (var attribute in attributes) result.Add((clazz, attribute));
        }
        return result;
    }

    protected abstract void ProcessItem(Type clazz, TAttribute attribute);
}