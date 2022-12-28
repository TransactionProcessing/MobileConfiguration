namespace MobileConfiguration.Database.Entities;

using System.ComponentModel.DataAnnotations;

public class LoggingLevels
{
    [Key]
    public int LogLevelId { get; set; }

    public string Description { get; set; }
}