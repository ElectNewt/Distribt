USE `distribt`;

CREATE TABLE `Products` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `Name` VARCHAR(150) NOT NULL,
            `Description` VARCHAR(150) NOT NULL,
            PRIMARY KEY (`Id`)
) AUTO_INCREMENT = 1;

CREATE TABLE `OutboxMessages` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `EventType` VARCHAR(500) NOT NULL,
            `EventData` TEXT NOT NULL,
            `RoutingKey` VARCHAR(50) NOT NULL,
            `CreatedAt` DATETIME NOT NULL,
            `ProcessedAt` DATETIME NULL,
            `IsProcessed` BOOLEAN NOT NULL DEFAULT FALSE,
            `ErrorMessage` TEXT NULL,
            `RetryCount` int NOT NULL DEFAULT 0,
            PRIMARY KEY (`Id`),
            INDEX `IX_OutboxMessages_IsProcessed_CreatedAt` (`IsProcessed`, `CreatedAt`)
) AUTO_INCREMENT = 1;

INSERT INTO `distribt`.`Products` (`Id`, `Name`, `Description`) VALUES ('1', 'Producto 1', 'La descripción dice qu es el primer producto');
INSERT INTO `distribt`.`Products` (`Id`, `Name`, `Description`) VALUES ('2', 'Segundo producto', 'Este es el producto numero 2');
INSERT INTO `distribt`.`Products` (`Id`, `Name`, `Description`) VALUES ('3', 'Tercer', 'Terceras Partes nunca fueron buenas');
