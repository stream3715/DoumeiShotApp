using Microsoft.UI.Xaml;
using Windows.Storage;

namespace DoumeiShotApp.Contracts.Services;

public interface IFrameImageSelectorService
{
    string ImagePath
    {
        get;
    }

    Task InitializeAsync();

    Task SetPathAsync(string imagePath);
}
