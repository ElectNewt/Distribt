# Outbox Pattern Implementation for Products Service

## Overview

This document describes the implementation of the outbox pattern for the Products service to ensure reliable event propagation in the distributed system. The outbox pattern guarantees that when a Product is created or updated, the corresponding events will be reliably published to the message bus.

## Problem Statement

Previously, the Products service directly published events after database operations, which could lead to:
- **Data Inconsistency**: If the database transaction succeeds but event publishing fails
- **Lost Events**: Network issues or service failures could cause events to be lost
- **Duplicate Events**: Retry mechanisms could lead to duplicate event publishing

## Solution: Outbox Pattern

The outbox pattern solves these issues by:
1. Storing events in the same database transaction as the business operation
2. Using a background service to read and publish events
3. Marking events as processed after successful publication
4. Implementing retry logic for failed event publishing

## Architecture

### Components

#### 1. OutboxMessage Entity
```csharp
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = null!;
    public string EventData { get; set; } = null!;
    public string? RoutingKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public bool IsProcessed { get; set; } = false;
    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
}
```

#### 2. OutboxRepository
- Manages CRUD operations for outbox messages
- Provides filtering for unprocessed messages
- Handles retry logic and failure tracking

#### 3. OutboxMessageService
- Serializes events to JSON for storage
- Deserializes and publishes events
- Handles error management and retry logic

#### 4. OutboxProcessorService (Background Service)
- Runs every 30 seconds to check for pending messages
- Processes messages in batches of 10
- Logs processing activities and errors

#### 5. Updated Use Cases
- `CreateProductDetailsWithOutbox`: Creates products with outbox events
- `UpdateProductDetailsWithOutbox`: Updates products with outbox events
- Adapter classes maintain backward compatibility

## Database Schema

### OutboxMessages Table
```sql
CREATE TABLE `OutboxMessages` (
    `Id` CHAR(36) PRIMARY KEY,
    `EventType` VARCHAR(255) NOT NULL,
    `EventData` LONGTEXT NOT NULL,
    `RoutingKey` VARCHAR(100) NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `ProcessedAt` DATETIME(6) NULL,
    `IsProcessed` BOOLEAN NOT NULL DEFAULT FALSE,
    `RetryCount` INT NOT NULL DEFAULT 0,
    `ErrorMessage` VARCHAR(1000) NULL,
    INDEX `IX_OutboxMessages_IsProcessed_RetryCount` (`IsProcessed`, `RetryCount`),
    INDEX `IX_OutboxMessages_CreatedAt` (`CreatedAt`)
);
```

## Flow Diagrams

### Product Creation Flow
```
1. Begin Database Transaction
2. Insert Product Record
3. Serialize ProductCreated Event
4. Insert OutboxMessage Record
5. Commit Transaction
```

### Event Processing Flow
```
1. Background Service Wakes Up (every 30s)
2. Query Unprocessed Messages (batch of 10)
3. For each message:
   - Deserialize Event
   - Publish to Message Bus
   - Mark as Processed (or Failed)
```

## Configuration

### Dependency Injection
```csharp
builder.Services
    .AddScoped<IOutboxRepository, OutboxRepository>()
    .AddScoped<IOutboxMessageService, OutboxMessageService>()
    .AddScoped<ICreateProductDetailsWithOutbox, CreateProductDetailsWithOutbox>()
    .AddScoped<IUpdateProductDetailsWithOutbox, UpdateProductDetailsWithOutbox>()
    .AddHostedService<OutboxProcessorService>();
```

### Background Service Settings
- **Processing Interval**: 30 seconds
- **Batch Size**: 10 messages per cycle
- **Max Retry Count**: 3 attempts before marking as failed
- **Error Handling**: Continues processing other messages if one fails

## Error Handling

### Retry Logic
- Messages are retried up to 3 times
- Failed messages are marked with error details
- Background service continues processing other messages

### Logging
- Processing activities are logged at Information level
- Errors are logged at Error level with full exception details
- Metrics include message counts and processing times

## Testing

### Unit Tests Coverage
- **OutboxRepository**: 6 test methods covering CRUD and filtering
- **OutboxMessageService**: 6 test methods covering serialization and publishing
- **Background Service**: 4 test methods covering processing cycles and error handling
- **Use Cases**: 6 test methods covering product operations with outbox

### Test Categories
1. **Happy Path Tests**: Normal operation scenarios
2. **Error Handling Tests**: Exception and failure scenarios
3. **Edge Case Tests**: Boundary conditions and retry logic
4. **Integration Tests**: End-to-end workflow validation

## Monitoring and Observability

### Key Metrics
- Number of unprocessed messages
- Processing success/failure rates
- Average processing time per batch
- Retry counts and failure reasons

### Health Checks
The system can be monitored by:
- Checking unprocessed message count
- Monitoring background service logs
- Database performance metrics

## Migration Strategy

### Backward Compatibility
- Original interfaces (`ICreateProductDetails`, `IUpdateProductDetails`) are maintained
- Adapter pattern ensures existing code continues to work
- Gradual migration path available

### Deployment Steps
1. Deploy new database schema (OutboxMessages table)
2. Deploy updated application with outbox pattern
3. Monitor message processing
4. Verify event publication continues correctly

## Benefits

### Reliability
- **ACID Compliance**: Events are stored in the same transaction
- **Guaranteed Delivery**: Events will eventually be published
- **Retry Logic**: Automatic retry for transient failures

### Performance
- **Decoupled Publishing**: Event publishing doesn't block business operations
- **Batch Processing**: Efficient handling of multiple events
- **Optimistic Processing**: Only processes when messages exist

### Maintainability
- **Clear Separation**: Business logic separate from event publishing
- **Testability**: All components are unit testable
- **Observability**: Comprehensive logging and monitoring

## Future Enhancements

### Potential Improvements
1. **Dead Letter Queue**: For permanently failed messages
2. **Event Ordering**: Ensure events are processed in order
3. **Partitioning**: Scale by partitioning outbox messages
4. **Metrics Dashboard**: Real-time monitoring interface
5. **Event Versioning**: Handle schema evolution for events

### Performance Optimization
1. **Configurable Intervals**: Adjust processing frequency based on load
2. **Dynamic Batch Sizes**: Scale batch size based on message volume
3. **Connection Pooling**: Optimize database connection usage

## Troubleshooting

### Common Issues
1. **Messages Not Processing**: Check background service is running
2. **High Retry Counts**: Investigate message bus connectivity
3. **Growing Outbox Table**: Consider adding cleanup for old processed messages

### Debug Steps
1. Check database for unprocessed messages
2. Review background service logs
3. Verify message bus configuration
4. Test event deserialization manually 