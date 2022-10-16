using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;

namespace DoumeiShotApp.ViewModels;

public class ThumbnailToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var thumbnail = (StorageItemThumbnail)value;
        var image = new BitmapImage();
        image.SetSource(thumbnail);
        thumbnail.Dispose();

        return image;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}