using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DoumeiShotApp.Contracts.Services;
using DoumeiShotApp.Core.Contracts.Services;
using DoumeiShotApp.Core.Services;
using DoumeiShotApp.Helpers;

using Microsoft.UI.Xaml;
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
    private ElementTheme _elementTheme;
    private StorageFolder? _watchingFolder;
    private string _selectedFolderPath;
    private string _selectedFrameImagePath;
    private string _versionDescription;

    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
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

    public ICommand PickPosPrinterCommand
    {
        get;
    }

    public ICommand PickDirectoryCommand
    {
        get;
    }

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public ICommand PickFrameCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, IWatchingFolderService watchingFolderService, IFrameImageSelectorService frameImageSelectorService, IPosPrinterService posPrinterService)
    {
        // Service init
        _themeSelectorService = themeSelectorService;
        _watchingFolderService = watchingFolderService;
        _frameImageSelectorService = frameImageSelectorService;
        _posPrinterService = posPrinterService;

        // internal var init
        _watchingFolder = null;

        // UI init
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        // Button init
        _selectedFolderPath = (_watchingFolderService.WatchingFolder != null && _watchingFolderService.WatchingFolder.Path != null) ? _watchingFolderService.WatchingFolder.Path : "未設定";
        _selectedFrameImagePath = _frameImageSelectorService.ImagePath != string.Empty ? _frameImageSelectorService.ImagePath : "未設定";

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        PickPosPrinterCommand = new RelayCommand<PosPrinter>(
            (param) =>
            {
                _posPrinterService.ConnectPrinter();
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
