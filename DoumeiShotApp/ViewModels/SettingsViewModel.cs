using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DoumeiShotApp.Contracts.Services;
using DoumeiShotApp.Core.Contracts.Services;
using DoumeiShotApp.Helpers;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.Devices.PointOfService;
using Windows.Storage;
using WinRT.Interop;

namespace DoumeiShotApp.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    private readonly IWatchingFolderService _watchingFolderService;
    private readonly IFrameImageSelectorService _frameImageSelectorService;
    private readonly IPosPrinterService _posPrinterService;
    private readonly IS3Service _s3Service;
    private readonly IPrinterSelectorService _printerSelectorService;
    private StorageFolder? _watchingFolder;
    private string _selectedFolderPath;
    private string _selectedFrameImagePath;
    private string _versionDescription;
    private int _connectionMethodId;
    private string? _connectionTarget;
    private Visibility _printerSettingsUSBVisibility;
    private Visibility _printerSettingsEthernetVisibility;

    public XamlRoot? XamlRoot
    {
        get;
        set;
    }

    public StorageFolder? WatchingFolder
    {
        get => _watchingFolder ?? null;
        set => SetProperty(ref _watchingFolder, value);
    }

    public string SelectedFolderPath
    {
        get => _selectedFolderPath;
        set => SetProperty(ref _selectedFolderPath, value);
    }

    public string SelectedFrameImagePath
    {
        get => _selectedFrameImagePath;
        set => SetProperty(ref _selectedFrameImagePath, value);
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public int ConnectionMethodID
    {
        get => _connectionMethodId;
        set => SetProperty(ref _connectionMethodId, value);
    }
    public string? ConnectionTarget
    {
        get => _connectionTarget;
        set => SetProperty(ref _connectionTarget, value);
    }

    public ICommand PickSavePosPrinterCommand
    {
        get;
    }

    public ICommand PickDirectoryCommand
    {
        get;
    }

    public ICommand WipeS3BucketCommand
    {
        get;
    }

    public RoutedEventHandler PickUSBHandler
    {
        get;
    }

    public RoutedEventHandler PickEthernetHandler
    {
        get;
    }

    public ICommand PickFrameCommand
    {
        get;
    }

    public Visibility PrinterSettingsUSBVisibility
    {
        get => _printerSettingsUSBVisibility;
        set => SetProperty(ref _printerSettingsUSBVisibility, value);
    }

    public Visibility PrinterSettingsEthernetVisibility
    {
        get => _printerSettingsEthernetVisibility;
        set => SetProperty(ref _printerSettingsEthernetVisibility, value);
    }

    public SettingsViewModel(
        IWatchingFolderService watchingFolderService,
        IFrameImageSelectorService frameImageSelectorService,
        IPosPrinterService posPrinterService,
        IPrinterSelectorService printerSelectorService,
        IS3Service s3Service)
    {
        // Service init
        _watchingFolderService = watchingFolderService;
        _frameImageSelectorService = frameImageSelectorService;
        _printerSelectorService = printerSelectorService;
        _posPrinterService = posPrinterService;
        _s3Service = s3Service;

        // internal var init
        _watchingFolder = null;

        // UI init
        _versionDescription = GetVersionDescription();

        // Button init
        _selectedFolderPath = (_watchingFolderService.WatchingFolder != null && _watchingFolderService.WatchingFolder.Path != null) ? _watchingFolderService.WatchingFolder.Path : "未設定";
        _selectedFrameImagePath = _frameImageSelectorService.ImagePath != string.Empty ? _frameImageSelectorService.ImagePath : "未設定";

        ConnectionMethodID = _printerSelectorService.Method;
        ConnectionTarget = _printerSelectorService.Target;

        _selectedFrameImagePath = _frameImageSelectorService.ImagePath != string.Empty ? _frameImageSelectorService.ImagePath : "未設定";

        PickSavePosPrinterCommand = new RelayCommand<PosPrinter>(
            async (param) =>
            {
                try
                {
                    string target;
                    if (ConnectionMethodID == 0)
                    {
                        target = "COM2";
                    }
                    else if (ConnectionMethodID == 1)
                    {
                        target = "192.168.1.240";
                    }
                    else
                    {
                        throw new Exception("METHOD_UNKNOWN");
                    }
                    await _posPrinterService.ConnectPrinter(ConnectionMethodID, target);
                    await _printerSelectorService.SetPrinterAsync(ConnectionMethodID, target);
                }
                catch (Exception)
                {
                    var contentDialog = new ContentDialog
                    {
                        Title = "エラー",
                        Content = "プリンターに接続できません。",
                        CloseButtonText = "OK",
                        XamlRoot = XamlRoot!
                    };

                    var result = await contentDialog.ShowAsync();
                }
            });

        PickDirectoryCommand = new RelayCommand<StorageFolder>(
            async (param) =>
            {
                if (await MainWindow.ShowFolderPickerAsync(WindowNative.GetWindowHandle(MainWindow.Handle)) is var folder && folder != null)
                {
                    WatchingFolder = folder;
                    await _watchingFolderService.SetFolderAsync(WatchingFolder);
                    SelectedFolderPath = WatchingFolder.Path;
                }
            });

        PickFrameCommand = new RelayCommand<string>(
            async (param) =>
            {
                if (await MainWindow.ShowFilePickerAsync(WindowNative.GetWindowHandle(MainWindow.Handle), new string[] { ".png" }) is var file && file != null)
                {
                    SelectedFrameImagePath = file.Path;
                    await _frameImageSelectorService.SetPathAsync(SelectedFrameImagePath);
                }
            });

        PickUSBHandler = new RoutedEventHandler(
            (sender, e) =>
            {
                ConnectionMethodID = 0;
                PrinterSettingsUSBVisibility = Visibility.Visible;
                PrinterSettingsEthernetVisibility = Visibility.Collapsed;
            });

        PickEthernetHandler = new RoutedEventHandler(
            (sender, e) =>
            {
                ConnectionMethodID = 1;
                PrinterSettingsUSBVisibility = Visibility.Collapsed;
                PrinterSettingsEthernetVisibility = Visibility.Visible;
            });

        WipeS3BucketCommand = new RelayCommand<string>(
            async (param) =>
            {
                var contentDialog = new ContentDialog
                {
                    Title = "警告",
                    Content = "このコマンドはクラウド・ローカルの全画像を消去します。本当によろしいですか？",
                    PrimaryButtonText = "OK",
                    SecondaryButtonText = "キャンセル",
                    XamlRoot = XamlRoot!
                };

                var result = await contentDialog.ShowAsync();
                if (result == ContentDialogResult.Secondary) { return; }

                var text = await InputTextDialogAsync("再確認", "パスワードを入力してください。", XamlRoot!);
                if (text == null) { return; }

                var textHash = GetHashString(text, SHA512.Create());
                if (textHash != "01B7949021DBA875A18F060488B571A95A63573E60401FFEBE3EFC1B84E00966BAEA1BC9E659038164BE65CC8CBF95A068E5600CC8DC365409BE7AEF50642386")
                {
                    await new ContentDialog
                    {
                        Title = "警告",
                        Content = "パスワードが間違っています。",
                        PrimaryButtonText = "OK",
                        XamlRoot = XamlRoot!
                    }.ShowAsync();
                }
                else
                {
                    await _s3Service.WipeBucket();
                    Delete(_selectedFolderPath);
                    await new ContentDialog
                    {
                        Title = "消去完了",
                        Content = "ストレージの初期化が完了しました。",
                        PrimaryButtonText = "OK",
                        XamlRoot = XamlRoot!
                    }.ShowAsync();
                }

            });
    }

    private void Delete(string targetDirectoryPath)
    {
        if (!Directory.Exists(targetDirectoryPath))
        {
            return;
        }

        //ディレクトリ以外の全ファイルを削除
        var filePaths = Directory.GetFiles(targetDirectoryPath);
        foreach (var filePath in filePaths)
        {
            File.SetAttributes(filePath, System.IO.FileAttributes.Normal);
            File.Delete(filePath);
        }

        //ディレクトリの中のディレクトリも再帰的に削除
        var directoryPaths = Directory.GetDirectories(targetDirectoryPath);
        foreach (var directoryPath in directoryPaths)
        {
            Delete(directoryPath);
        }

        //中が空になったらディレクトリ自身も削除
        if (targetDirectoryPath != _selectedFolderPath)
        {
            Directory.Delete(targetDirectoryPath, false);
        }
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    private async Task<string?> InputTextDialogAsync(string title, string body, XamlRoot xamlRoot)
    {
        TextBox inputTextBox = new TextBox();
        inputTextBox.AcceptsReturn = false;
        inputTextBox.Height = 32;
        inputTextBox.PlaceholderText = body;
        ContentDialog dialog = new ContentDialog();
        dialog.Content = inputTextBox;
        dialog.Title = title;
        dialog.IsSecondaryButtonEnabled = true;
        dialog.PrimaryButtonText = "OK";
        dialog.SecondaryButtonText = "キャンセル";
        dialog.XamlRoot = xamlRoot;
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            return inputTextBox.Text;
        else
            return null;
    }

    public static string GetHashString(string text, HashAlgorithm algorithm)
    {
        // 文字列をバイト型配列に変換する
        var data = Encoding.UTF8.GetBytes(text);

        // ハッシュ値を計算する
        var bs = algorithm.ComputeHash(data);

        // リソースを解放する
        algorithm.Clear();

        // バイト型配列を16進数文字列に変換
        var result = new StringBuilder();
        foreach (var b in bs)
        {
            result.Append(b.ToString("X2"));
        }
        return result.ToString();
    }
}
