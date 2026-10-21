using System.ComponentModel;

namespace MobileConfiguration.DataTransferObjects;

[Description("A service type and the URI where the mobile client can reach it.")]
public class HostAddress
{
    [Description("Service represented by this address. Enum values are numeric in JSON.")]
    public ServiceType ServiceType { get; set; }

    [Description("URI for the service endpoint.")]
    public string Uri { get; set; }
}