using System.ComponentModel.DataAnnotations;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(255)]
    public string EventType { get; set; } = null!;
    
    [Required]
    public string EventData { get; set; } = null!;
    
    [MaxLength(100)]
    public string? RoutingKey { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ProcessedAt { get; set; }
    
    public bool IsProcessed { get; set; } = false;
    
    public int RetryCount { get; set; } = 0;
    
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
} 