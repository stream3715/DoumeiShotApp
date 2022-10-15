namespace DoumeiShotApp.Contracts.Services;

public interface IPrinterSelectorService
{
    string? Target
    {
        get; set;
    }
    int Method
    {
        get; set;
    }

    Task InitializeAsync();

    Task SetPrinterAsync(int method, string target);
}
