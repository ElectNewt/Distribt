USE `distribt`;

CREATE TABLE `Products` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `Name` VARCHAR(150) NOT NULL,
            `Description` VARCHAR(150) NOT NULL,
            PRIMARY KEY (`Id`)
) AUTO_INCREMENT = 1;

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

INSERT INTO `distribt`.`Products` (`Id`, `Name`, `Description`) VALUES ('1', 'Producto 1', 'La descripción dice qu es el primer producto');
INSERT INTO `distribt`.`Products` (`Id`, `Name`, `Description`) VALUES ('2', 'Segundo producto', 'Este es el producto numero 2');
INSERT INTO `distribt`.`Products` (`Id`, `Name`, `Description`) VALUES ('3', 'Tercer', 'Terceras Partes nunca fueron buenas');
