using DoumeiShotApp.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;

namespace DoumeiShotApp.Views;

public sealed partial class ContentGridPage : Page
{
    public ContentGridViewModel ViewModel
    {
        get;
    }

    public ContentGridPage()
    {
        ViewModel = App.GetService<ContentGridViewModel>();
        InitializeComponent();
    }
}
