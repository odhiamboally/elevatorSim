using System.ComponentModel.DataAnnotations;

namespace ES.Domain.Entities;
public class Log
{
    [Key]
    public int Id { get; set; }
    public int LogLevel { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset LogDate { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
