using CommunityToolkit.WinUI.UI.Animations;

using DoumeiShotApp.Contracts.Services;
using DoumeiShotApp.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DoumeiShotApp.Views;

public sealed partial class ContentGridDetailPage : Page
{
    public ContentGridDetailViewModel ViewModel
    {
        get;
    }

    public ContentGridDetailPage()
    {
        ViewModel = App.GetService<ContentGridDetailViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        if (e.NavigationMode == NavigationMode.Back)
        {
            var navigationService = App.GetService<INavigationService>();

            if (ViewModel.Item != null)
            {
                navigationService.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
            }
        }
    }

    private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }
}
