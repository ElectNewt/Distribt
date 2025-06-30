using System.ComponentModel.DataAnnotations;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;

public class OutboxMessage
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string EventType { get; set; } = null!;
    
    [Required]
    public string EventData { get; set; } = null!;
    
    [Required]
    [MaxLength(50)]
    public string RoutingKey { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public bool IsProcessed { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public int RetryCount { get; set; }
}