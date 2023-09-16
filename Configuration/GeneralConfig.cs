[Config]
public class GeneralConfig
{
    public HostEnvironment? Environment {get; set;}
}

public enum HostEnvironment {
    Development,
    Production
}