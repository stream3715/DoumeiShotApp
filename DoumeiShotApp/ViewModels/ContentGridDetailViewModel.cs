using CommunityToolkit.Mvvm.ComponentModel;

using DoumeiShotApp.Contracts.ViewModels;
using DoumeiShotApp.Contracts.Services;
using DoumeiShotApp.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing;
using DoumeiShotApp.Core.Contracts.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using DoumeiShotApp.Services;

namespace DoumeiShotApp.ViewModels;

public class ContentGridDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly ITakenPhotoService _takenPhotoService;
    private readonly IImageEditService _imageEditService;
    private readonly IS3Service _s3Service;
    private TakenPhoto? _item;
    private BitmapImage _framedImage;
    private string _framedImagePath;

    public TakenPhoto? Item
    {
        get => _item;
        set => SetProperty(ref _item, value);
    }

    public ICommand UploadCommand
    {
        get;
    }

    public ICommand CancelCommand
    {
        get;
    }

    public ContentGridDetailViewModel(ITakenPhotoService takenPhotoService, IImageEditService imageEditService, INavigationService navigationService, IS3Service s3Service)
    {
        _takenPhotoService = takenPhotoService;
        _imageEditService = imageEditService;
        _navigationService = navigationService;
        _s3Service = s3Service;

        _framedImage = new BitmapImage();

        UploadCommand = new RelayCommand(UploadFramedImage);
        CancelCommand = new RelayCommand(UploadCancel);
    }

    public BitmapImage FramedImage
    {
        get => _framedImage;
        set => SetProperty(ref _framedImage, value);
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is string filePath)
        {
            var data = await _takenPhotoService.GetContentGridDataAsync();
            Item = data.First(i => i.File!.Path == filePath);
            _framedImagePath = OverlayImage(filePath, @"C:\Users\strea\Desktop\mcqueen.png");
            
            FramedImage.UriSource = new Uri(_framedImagePath);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private string OverlayImage(string baseImagePath, string coverImagePath)
    {
        return _imageEditService.OverlayImage(baseImagePath, coverImagePath);
    }

    private void UploadFramedImage()
    {
        string url = _s3Service.Upload(_framedImagePath);
    }

    private void UploadCancel()
    {
        _navigationService.NavigateTo(typeof(ContentGridViewModel).FullName!);
    }
}
