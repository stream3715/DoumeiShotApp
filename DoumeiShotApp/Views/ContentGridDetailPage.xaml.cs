using CommunityToolkit.WinUI.UI.Animations;

using DoumeiShotApp.Contracts.Services;
using DoumeiShotApp.ViewModels;
using Microsoft.UI.Xaml;
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

    private void ContentGridDetailPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.XamlRoot = Content.XamlRoot;
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
}
