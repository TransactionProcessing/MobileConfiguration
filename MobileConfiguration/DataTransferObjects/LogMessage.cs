namespace MobileConfiguration.DataTransferObjects;

public class LogMessage
{
    public DateTime EntryDateTime { get; set; }
    public String LogLevel { get; set; }
    public String Message { get; set; }
}