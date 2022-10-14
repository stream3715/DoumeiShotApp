using DoumeiShotApp.Helpers;

namespace DoumeiShotApp;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();
        Handle = this;
    }

    public static MainWindow? Handle
    {
        get; private set;
    }

    public static async Task<Windows.Storage.StorageFolder> ShowFolderPickerAsync(IntPtr hWnd)
    {
        // Create a folder picker.
        var folderPicker = new Windows.Storage.Pickers.FolderPicker();

        // Initialize the folder picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

        // Use the folder picker as usual.
        folderPicker.FileTypeFilter.Add("*");
        return await folderPicker.PickSingleFolderAsync();
    }

    public static async Task<Windows.Storage.StorageFile> ShowFilePickerAsync(IntPtr hWnd, string[] fileType)
    {
        // Create a folder picker.
        var filePicker = new Windows.Storage.Pickers.FileOpenPicker();

        // Initialize the folder picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);

        if(fileType.Length <= 0)
        {
            filePicker.FileTypeFilter.Add("*");
        } else
        {
            foreach (var type in fileType)
            {
                filePicker.FileTypeFilter.Add(type);
            }
        }

        return await filePicker.PickSingleFileAsync();
    }
}
