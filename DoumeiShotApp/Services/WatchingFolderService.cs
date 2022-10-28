using DoumeiShotApp.Contracts.Services;
using Windows.Storage;

namespace DoumeiShotApp.Services;

public class WatchingFolderService : IWatchingFolderService
{
    private const string SettingsKey = "AppWatchingFolder";

    public StorageFolder? WatchingFolder { get; set; } = null;

    private readonly ILocalSettingsService _localSettingsService;

    public WatchingFolderService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        await LoadFolderPathfromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetFolderAsync(StorageFolder folder)
    {
        WatchingFolder = folder;

        await SetFolderPathAsync();
        await SaveFolderPathInSettingsAsync(WatchingFolder);
    }

    public async Task SetFolderPathAsync()
    {
        // TODO なにかするならここで？
        await Task.CompletedTask;
    }

    private async Task LoadFolderPathfromSettingsAsync()
    {
        var folderPath = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);
        WatchingFolder = folderPath != null ? await StorageFolder.GetFolderFromPathAsync(folderPath) : null;
    }

    private async Task SaveFolderPathInSettingsAsync(StorageFolder folder)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, folder.Path);
    }
}
