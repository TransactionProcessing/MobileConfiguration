using System.ComponentModel;

namespace MobileConfiguration.DataTransferObjects;

[Description("A log event submitted by a mobile client.")]
public class LogMessage
{
    [Description("Timestamp associated with the log event.")]
    public DateTime EntryDateTime { get; set; }
    [Description("Log severity label supplied by the client.")]
    public String LogLevel { get; set; }
    [Description("Text of the log event.")]
    public String Message { get; set; }
}