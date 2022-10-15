using DoumeiShotApp.Contracts.Services;
using DoumeiShotApp.Helpers;

using Microsoft.UI.Xaml;

namespace DoumeiShotApp.Services;

public class PrinterSelectorService : IPrinterSelectorService
{
    private const string MethodSettingsKey = "AppPrinterConnectionMethod";
    private const string TargetSettingsKey = "AppPrinterConnectionTarget";

    public string? Target
    {
        get; set;
    }
    public int Method
    {
        get; set;
    }

    private readonly ILocalSettingsService _localSettingsService;

    public PrinterSelectorService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        (Method, Target) = await LoadPrinterFromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetPrinterAsync(int method, string target)
    {
        await SavePrinterInSettingsAsync(method, target);
    }

    private async Task<(int, string?)> LoadPrinterFromSettingsAsync()
    {
        var method = await _localSettingsService.ReadSettingAsync<string>(MethodSettingsKey);
        var methodId = -1;
        if (method != null) int.TryParse(method, out methodId);
        var target = await _localSettingsService.ReadSettingAsync<string>(TargetSettingsKey);

        return (methodId, target);
    }

    private async Task SavePrinterInSettingsAsync(int method, string target)
    {
        await _localSettingsService.SaveSettingAsync(MethodSettingsKey, method.ToString());
        await _localSettingsService.SaveSettingAsync(TargetSettingsKey, target);
    }
}
