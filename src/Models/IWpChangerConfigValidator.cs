namespace Wallsh.Models;

public interface IWpChangerConfigValidator
{
    (bool Success, string? Message) ValidateConfiguration();
}
