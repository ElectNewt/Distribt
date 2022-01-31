namespace Distribt.Shared.Secrets;

public record VaultSettings
{
    public string? VaultUrl { get; init; }
    public string? TokenApi { get; init; }
}