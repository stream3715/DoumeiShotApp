using Microsoft.UI.Xaml;
using Windows.Storage;

namespace DoumeiShotApp.Contracts.Services;

public interface IWatchingFolderService
{
    StorageFolder? WatchingFolder
    {
        get;
    }

    Task InitializeAsync();

    Task SetFolderAsync(StorageFolder folder);
}
