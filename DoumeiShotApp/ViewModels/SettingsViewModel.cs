using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DoumeiShotApp.Contracts.Services;
using DoumeiShotApp.Core.Contracts.Services;
using DoumeiShotApp.Core.Services;
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
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IWatchingFolderService _watchingFolderService;
    private readonly IFrameImageSelectorService _frameImageSelectorService;
    private readonly IPosPrinterService _posPrinterService;
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
        IThemeSelectorService themeSelectorService,
        IWatchingFolderService watchingFolderService,
        IFrameImageSelectorService frameImageSelectorService,
        IPosPrinterService posPrinterService,
        IPrinterSelectorService printerSelectorService)
    {
        // Service init
        _themeSelectorService = themeSelectorService;
        _watchingFolderService = watchingFolderService;
        _frameImageSelectorService = frameImageSelectorService;
        _printerSelectorService = printerSelectorService;
        _posPrinterService = posPrinterService;

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
}
