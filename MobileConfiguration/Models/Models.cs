namespace MobileConfiguration.Models
{
    public class MobileConfiguration
    {
        public String ClientId { get; set; }
        public String ClientSecret { get; set; }

        public String DeviceIdentifier { get; set; }

        public Boolean EnableAutoUpdates { get; set; }

        public List<HostAddress>? HostAddresses { get; set; }

        public String Id { get; set; }
        
        public LoggingLevel LogLevel { get; set; }

        public ConfigurationType ConfigurationType { get; set; }
    }

    public class HostAddress
    {
        public ServiceType ServiceType { get; set; }

        public String Uri { get; set; }
    }

    public enum ServiceType
    {
        EstateManagement = 0,
        Security = 1,
        TransactionProcessorAcl = 2,
        VoucherManagementAcl = 3,
    }

    public enum LoggingLevel
    {
        Fatal = 0,
        Error = 1,
        Warning = 2,
        Information = 3,
        Debug = 4,
        Trace = 5
    }

    public enum ConfigurationType
    {
        TransactionMobile,
        VoucherRedemption
        
    }

    public class ApplicationCentreConfiguration
    {
        public String ApplicationId { get; set; }
        public String AndroidKey { get; set; }
        public String IosKey { get; set; }
        public String MacosKey { get; set; }
        public String WindowsKey { get; set; }
    }
}
