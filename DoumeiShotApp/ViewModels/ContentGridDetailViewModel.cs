﻿using CommunityToolkit.Mvvm.ComponentModel;

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
using DoumeiShotApp.Core.Services;
using static CommunityToolkit.WinUI.UI.Animations.Expressions.ExpressionValues;

namespace DoumeiShotApp.ViewModels;

public class ContentGridDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly ITakenPhotoService _takenPhotoService;
    private readonly IFrameImageSelectorService _framedImageSelectorService;
    private readonly IImageEditService _imageEditService;
    private readonly IS3Service _s3Service;
    private readonly IPrinterSelectorService _printerSelectorService;
    private readonly IPosPrinterService _posPrinterService;
    private readonly ResourceLoader _resourceLoader;

    private TakenPhoto? _item;

    private BitmapImage _framedImage;
    private string _framedImagePath;
    private string _originalFrameImagePath;

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

    public ContentGridDetailViewModel(
        ITakenPhotoService takenPhotoService,
        IImageEditService imageEditService,
        IFrameImageSelectorService frameImageSelectorService,
        INavigationService navigationService,
        IS3Service s3Service,
        IPrinterSelectorService printerSelectorService,
        IPosPrinterService posPrinterService)
    {
        _resourceLoader = new ResourceLoader();

        _takenPhotoService = takenPhotoService;
        _imageEditService = imageEditService;
        _framedImageSelectorService = frameImageSelectorService;
        _navigationService = navigationService;
        _s3Service = s3Service;
        _printerSelectorService = printerSelectorService;
        _posPrinterService = posPrinterService;

        _framedImage = new BitmapImage();
        _framedImagePath = string.Empty;
        _originalFrameImagePath = string.Empty;
        _uploadButtonText = string.Empty;
        XamlRoot = null;

        UploadCommand = null;
        CancelCommand = new RelayCommand(ButtonCancel);
        ButtonUploadContent = string.Empty;
    }

    ~ContentGridDetailViewModel()
    {
        OnNavigatedFrom();
    }

    public BitmapImage FramedImage
    {
        get => _framedImage;
        set => SetProperty(ref _framedImage, value);
    }

    public async void OnNavigatedTo(object parameter)
    {
        await _printerSelectorService.InitializeAsync();
        if (parameter is string filePath)
        {
            // Get image data
            var data = await _takenPhotoService.GetContentGridDataAsync();
            try
            {
                Item = data.First(i => i.File!.Path == filePath);
            }
            catch (Exception)
            {
                var contentDialog = new ContentDialog
                {
                    Title = "エラー",
                    Content = "画像が見つかりません。",
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot!
                };

                await contentDialog.ShowAsync();
                ButtonCancel();
                return;
            }

            // Overlay frame image
            var framePath = _framedImageSelectorService.ImagePath;
            if (framePath == string.Empty)
            {
                var contentDialog = new ContentDialog
                {
                    Title = "エラー",
                    Content = "フレーム画像が指定されていません。\n設定よりファイルを指定して下さい。",
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot!
                };

                await contentDialog.ShowAsync();
                ButtonCancel();
            }
            else
            {
                // Check if image is already uploaded
                var framedFileName = Path.GetFileName(filePath);
                if (await _s3Service.CheckFileExists(framedFileName))
                {
                    Item.IsUploaded = true;
                    ButtonUploadContent = _resourceLoader.GetString("SimpleTextReprint");
                    UploadCommand = new RelayCommand(ReprintFramedUri);

                    FramedImage.UriSource = new Uri(filePath);
                    _originalFrameImagePath = filePath;
                }
                else
                {
                    Item.IsUploaded = false;
                    ButtonUploadContent = _resourceLoader.GetString("SimpleTextUpload");
                    UploadCommand = new RelayCommand(UploadFramedImage);

                    var tmpFile = OverlayImage(filePath, framePath);
                    _framedImagePath = tmpFile;
                    FramedImage.UriSource = new Uri(_framedImagePath);

                    _originalFrameImagePath = tmpFile;
                }
            }
        }
    }

    public void OnNavigatedFrom()
    {
        if (File.Exists(_framedImagePath))
        {
            File.Delete(_framedImagePath);
        }
    }

    private string OverlayImage(string baseImagePath, string coverImagePath)
    {
        return _imageEditService.OverlayImage(baseImagePath, coverImagePath);
    }

    private async void UploadFramedImage()
    {
        File.Copy(Item!.File!.Path, Item!.File!.Path + "_orig", true);
        File.Copy(_originalFrameImagePath, Item!.File!.Path, true);

        var (url, expires) = _s3Service.Upload(Item!.File!.Path);
        if(url != null)
        {
            Item!.IsUploaded = true;
        } else
        {
            File.Move(Item!.File!.Path + "_orig", Item!.File!.Path, true);
            throw new Exception("UPLOAD_FAILED");
        }
        
        File.Delete(Item!.File!.Path + "_orig");
        File.Delete(_originalFrameImagePath);

        var contentDialog = new ContentDialog
        {
            Title = "アップロード完了",
            Content = "アップロードが完了しました。\n" + url,
            PrimaryButtonText = "印字",
            SecondaryButtonText = "開く",
            XamlRoot = XamlRoot!
        };

        var result = await contentDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var method = _printerSelectorService.Method;
            var target = _printerSelectorService.Target;

            try
            {
                _posPrinterService.ConnectPrinter(method, target);
                _posPrinterService.PrintQRCode(url, expires);
            }
            catch (Exception)
            {

            }
        }
        else if (result == ContentDialogResult.Secondary)
        {
            OpenUrl(url);
        }
        ButtonCancel();
        return;
    }

    private async void ReprintFramedUri()
    {
        var (url, expires) = _s3Service.GetPreSignedURLFromFolderPath(_originalFrameImagePath);

        var contentDialog = new ContentDialog
        {
            Title = "再取得完了",
            Content = "URL再取得が完了しました。\n" + url,
            PrimaryButtonText = "印字",
            SecondaryButtonText = "開く",
            XamlRoot = XamlRoot!
        };

        var result = await contentDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var method = _printerSelectorService.Method;
            var target = _printerSelectorService.Target;

            try
            {
                _posPrinterService.ConnectPrinter(method, target);
                _posPrinterService.PrintQRCode(url, expires);
            }
            catch (Exception)
            {

            }
        }
        else if (result == ContentDialogResult.Secondary)
        {
            OpenUrl(url);
        }
        ButtonCancel();
        return;
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
