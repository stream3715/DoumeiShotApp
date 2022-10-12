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
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using System.Reflection;
using Microsoft.Windows.ApplicationModel.Resources;

namespace DoumeiShotApp.ViewModels;

public class ContentGridDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly ITakenPhotoService _takenPhotoService;
    private readonly IImageEditService _imageEditService;
    private readonly IS3Service _s3Service;
    private readonly ResourceLoader _resourceLoader;

    private TakenPhoto? _item;
    
    private BitmapImage _framedImage;
    private string _framedImagePath;

    private string _uploadButtonText;
    private ICommand? _uploadButtonCommand;

    public XamlRoot? XamlRoot
    {
        get;
        set;
    }

    public TakenPhoto? Item
    {
        get => _item;
        set => SetProperty(ref _item, value);
    }

    public ICommand? UploadCommand
    {
        get => _uploadButtonCommand;
        internal set => SetProperty(ref _uploadButtonCommand, value);
    }

    public ICommand CancelCommand
    {
        get;
    }

    public string ButtonUploadContent
    {
        get => _uploadButtonText;
        set => SetProperty(ref _uploadButtonText, value);
    }

    public ContentGridDetailViewModel(ITakenPhotoService takenPhotoService, IImageEditService imageEditService, INavigationService navigationService, IS3Service s3Service)
    {
        _resourceLoader = new ResourceLoader();

        _takenPhotoService = takenPhotoService;
        _imageEditService = imageEditService;
        _navigationService = navigationService;
        _s3Service = s3Service;

        _framedImage = new BitmapImage();
        _framedImagePath = string.Empty;
        _uploadButtonText = string.Empty;
        XamlRoot = null;

        UploadCommand = null;
        CancelCommand = new RelayCommand(ButtonCancel);
        ButtonUploadContent = string.Empty;
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
            // Get image data
            var data = await _takenPhotoService.GetContentGridDataAsync();
            Item = data.First(i => i.File!.Path == filePath);

            // Overlay frame image
            _framedImagePath = OverlayImage(filePath, @"C:\Users\strea\Downloads\枠 仮@2x.png");
            FramedImage.UriSource = new Uri(_framedImagePath);

            // Check if image is already uploaded
            var framedFileName = Path.GetFileName(_framedImagePath);
            if(await _s3Service.CheckFileExists(framedFileName))
            {
                Item.IsUploaded = true;
                ButtonUploadContent = _resourceLoader.GetString("SimpleTextReprint");
                UploadCommand = new RelayCommand(ReprintFramedUri);
            } else
            {
                Item.IsUploaded = false;
                ButtonUploadContent = _resourceLoader.GetString("SimpleTextUpload");
                UploadCommand = new RelayCommand(UploadFramedImage);
            }
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private string OverlayImage(string baseImagePath, string coverImagePath)
    {
        return _imageEditService.OverlayImage(baseImagePath, coverImagePath);
    }

    private async void UploadFramedImage()
    {
        var url = _s3Service.Upload(_framedImagePath);

        var contentDialog = new ContentDialog
        {
            Title = "アップロード完了",
            Content = "アップロードが完了しました。\n" + url,
            PrimaryButtonText = "開く",
            CloseButtonText="OK",
            XamlRoot = XamlRoot!
        };

        var result = await contentDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            OpenUrl(url);
        }
        else
        {
            ButtonCancel();
        }
    }

    private async void ReprintFramedUri()
    {
        var url = _s3Service.GetPreSignedURLFromFolderPath(_framedImagePath);

        var contentDialog = new ContentDialog
        {
            Title = "再取得完了",
            Content = "URL再取得が完了しました。\n" + url,
            PrimaryButtonText = "開く",
            CloseButtonText = "OK",
            XamlRoot = XamlRoot!
        };

        var result = await contentDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            OpenUrl(url);
        }
        else
        {
            ButtonCancel();
        }
    }

    private static Process? OpenUrl(string url)
    {
        ProcessStartInfo pi = new ProcessStartInfo()
        {
            FileName = url,
            UseShellExecute = true,
        };

        return Process.Start(pi);
    }

    private void ButtonCancel()
    {
        _navigationService.NavigateTo(typeof(ContentGridViewModel).FullName!);
    }
}
