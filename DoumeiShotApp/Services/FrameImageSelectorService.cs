using DoumeiShotApp.Contracts.Services;
using Windows.Storage;

namespace DoumeiShotApp.Services;

public class FrameImageSelectorService : IFrameImageSelectorService
{
    private const string SettingsKey = "AppFrameImagePath";

    public string ImagePath{ get; set; } = string.Empty;

    public string? FrameImage => throw new NotImplementedException();

    private readonly ILocalSettingsService _localSettingsService;

    public FrameImageSelectorService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        await LoadFolderPathfromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetPathAsync(string imagePath)
    {
        ImagePath = imagePath;
        await SaveFolderPathInSettingsAsync(ImagePath);
    }

    private async Task LoadFolderPathfromSettingsAsync()
    {
        var folderPath = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);
        ImagePath = folderPath ?? string.Empty;
    }

    private async Task SaveFolderPathInSettingsAsync(string imagePath)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, imagePath);
    }
}
