using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace DoumeiShotApp.Models;

public class TakenPhoto
{
    public StorageFile? File
    {
        get; set;
    }
    public BitmapImage? Thumbnail
    {
        get; set;
    }
    public bool IsUploaded
    {
        get; set;
    }
}
