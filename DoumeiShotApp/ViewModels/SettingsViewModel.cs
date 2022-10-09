using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DoumeiShotApp.Contracts.Services;
using DoumeiShotApp.Helpers;

using Microsoft.UI.Xaml;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace DoumeiShotApp.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IWatchingFolderService _watchingFolderService;
    private ElementTheme _elementTheme;
    private StorageFolder? _watchingFolder;
    private string _selectedFolderPath;
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

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public ICommand PickDirectoryCommand
    {
        get;
    }

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, IWatchingFolderService watchingFolderService)
    {
        _themeSelectorService = themeSelectorService;
        _watchingFolderService = watchingFolderService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
        _selectedFolderPath = (_watchingFolderService.WatchingFolder != null && _watchingFolderService.WatchingFolder.Path != null) ? _watchingFolderService.WatchingFolder.Path : "未設定";
        _watchingFolder = null;

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        PickDirectoryCommand = new RelayCommand<StorageFolder>(
            async (param) =>
            {
                var folderPicker = new FolderPicker();
                folderPicker.FileTypeFilter.Add("*");

                InitializeWithWindow.Initialize(folderPicker, WindowNative.GetWindowHandle(MainWindow.Handle));

                var folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    WatchingFolder = folder;
                    await _watchingFolderService.SetFolderAsync(WatchingFolder);
                    SelectedFolderPath = WatchingFolder.Path;
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
