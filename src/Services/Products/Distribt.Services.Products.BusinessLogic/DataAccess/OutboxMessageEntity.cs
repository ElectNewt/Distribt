namespace Distribt.Services.Products.BusinessLogic.DataAccess;

// SQL Schema
// CREATE TABLE `OutboxMessages` (
//     `Id` BIGINT NOT NULL AUTO_INCREMENT,
//     `Type` VARCHAR(500) NOT NULL,
//     `Payload` TEXT NOT NULL,
//     `CreatedUtc` DATETIME NOT NULL,
//     `SentUtc` DATETIME NULL,
//     PRIMARY KEY (`Id`)
// ) AUTO_INCREMENT = 1;

public class OutboxMessageEntity
{
    public long Id { get; set; }
    public string Type { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTime CreatedUtc { get; set; }
    public DateTime? SentUtc { get; set; }
}
