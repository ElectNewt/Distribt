USE `distribt`;

CREATE TABLE IF NOT EXISTS `OutboxMessages` (
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