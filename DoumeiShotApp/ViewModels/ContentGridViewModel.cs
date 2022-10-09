using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DoumeiShotApp.Contracts.Services;
using DoumeiShotApp.Contracts.ViewModels;
using DoumeiShotApp.Models;
using DoumeiShotApp.Services;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;

namespace DoumeiShotApp.ViewModels;

public class ContentGridViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly ITakenPhotoService _takenPhotoService;

    public ICommand ItemClickCommand
    {
        get;
    }

    public ObservableCollection<TakenPhoto> Source { get; } = new ObservableCollection<TakenPhoto>();

    public ContentGridViewModel(INavigationService navigationService, ITakenPhotoService takenPhotoService, IWatchingFolderService watchingFolderService)
    {
        _navigationService = navigationService;
        _takenPhotoService = takenPhotoService;

        ItemClickCommand = new RelayCommand<TakenPhoto>(OnItemClick);
    }

    public async void OnNavigatedTo(object parameter)
    {
        await UpdateGridList();
    }

    public void OnNavigatedFrom()
    {
    }

    private async Task UpdateGridList()
    {
        Source.Clear();

        var pictures = await _takenPhotoService.GetContentGridDataAsync();

        foreach (var item in pictures)
        {
            Source.Add(item);
        }
    }

    private void OnItemClick(TakenPhoto? clickedItem)
    {
        if (clickedItem != null && clickedItem.File != null)
        {
            _navigationService.SetListDataItemForNextConnectedAnimation(clickedItem);
            _navigationService.NavigateTo(typeof(ContentGridDetailViewModel).FullName!, clickedItem.File.Path);
        }
    }
}
