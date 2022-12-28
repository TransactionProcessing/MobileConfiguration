namespace MobileConfiguration.Database.Entities;

using System.ComponentModel.DataAnnotations;

public class Configuration
{
    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public string DeviceIdentifier { get; set; }

    public bool EnableAutoUpdates { get; set; }

    public string HostAddresses { get; set; }

    [Key]
    public string Id { get; set; }

    public int ConfigType { get; set; }

    public int LogLevelId { get; set; }
}

public class ApplicationCentreConfiguration
{
    public String ApplicationId { get; set; }
    public String AndroidKey { get; set; }
    public String IosKey { get; set; }
    public String MacosKey { get; set; }
    public String WindowsKey { get; set; }
}