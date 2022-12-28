namespace MobileConfiguration.DataTransferObjects;

public class Configuration
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public string DeviceIdentifier { get; set; }

    public bool EnableAutoUpdates { get; set; }

    public List<HostAddress> HostAddresses { get; set; }

    public string Id { get; set; }

    public LoggingLevel LogLevel { get; set; }
}

public class ApplicationCentreConfiguration
{
    public String ApplicationId { get; set; }
    public String AndroidKey { get; set; }
    public String IosKey { get; set; }
    public String MacosKey { get; set; }
    public String WindowsKey { get; set; }
}


public class ConfigurationResponse
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public string DeviceIdentifier { get; set; }

    public bool EnableAutoUpdates { get; set; }

    public List<HostAddress> HostAddresses { get; set; }

    public string Id { get; set; }

    public LoggingLevel LogLevel { get; set; }

    public ApplicationCentreConfiguration ApplicationCentreConfiguration { get; set; }
}