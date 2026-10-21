using System.ComponentModel;

namespace MobileConfiguration.DataTransferObjects;

[Description("Configuration settings associated with a Transaction Mobile device.")]
public class Configuration
{
    [Description("Client identifier used by the mobile device.")]
    public string ClientId { get; set; }

    [Description("Client secret used by the mobile device. Treat this value as sensitive.")]
    public string ClientSecret { get; set; }

    [Description("Identifier of the mobile device.")]
    public string DeviceIdentifier { get; set; }

    [Description("Whether the mobile device should install updates automatically.")]
    public bool EnableAutoUpdates { get; set; }

    [Description("Service URI values available to the mobile device.")]
    public List<HostAddress> HostAddresses { get; set; }

    [Description("Unique identifier for this configuration.")]
    public string Id { get; set; }

    [Description("Minimum severity level written by the mobile client. Enum values are numeric in JSON.")]
    public LoggingLevel LogLevel { get; set; }
}