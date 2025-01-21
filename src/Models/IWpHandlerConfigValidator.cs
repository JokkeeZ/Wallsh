namespace Wallsh.Models;

public interface IWpHandlerConfigValidator
{
    (bool Success, string Message) ValidateConfiguration();
}
