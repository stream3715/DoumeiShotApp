using System.Collections;
using System.IO;
using System.Text;

using DoumeiShotApp.Models;
using DoumeiShotApp.Contracts.Services;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace DoumeiShotApp.Services;

public class TakenPhotoService : ITakenPhotoService
{
    private readonly IWatchingFolderService _watchingFolderService;

    public TakenPhotoService(IWatchingFolderService watchingFolderService)
    {
        _watchingFolderService = watchingFolderService;
    }

    private async Task<IList<TakenPhoto>> AllPhotos()
    {
        var photos = new List<TakenPhoto>();
        var folder = _watchingFolderService.WatchingFolder;

        if (folder == null)
        {
            return photos;
        }

        var files = await folder.GetFilesAsync();

        foreach (var file in files)
        {
            if (file.FileType.ToLower() != ".jpg")
            {
                continue;
            }

            var thumbImage = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 100);
            var thumbBitmap = new BitmapImage();
            thumbBitmap.SetSource(thumbImage);

            photos.Add(new TakenPhoto
            {
                File = file,
                Thumbnail = thumbBitmap,
            });
        }

        return photos;
    }

    public async Task<IList<TakenPhoto>> GetContentGridDataAsync() => await AllPhotos();
}
