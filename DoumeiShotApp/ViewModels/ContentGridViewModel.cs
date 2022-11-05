using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DoumeiShotApp.Contracts.Services;
using DoumeiShotApp.Contracts.ViewModels;
using DoumeiShotApp.Models;
using Microsoft.UI.Dispatching;

namespace DoumeiShotApp.ViewModels;

public class ContentGridViewModel : ObservableRecipient, INavigationAware
{
    private readonly IWatchingFolderService _watchingFolderService;
    private readonly IFrameImageSelectorService _frameImageSelectorService;
    private readonly INavigationService _navigationService;
    private readonly ITakenPhotoService _takenPhotoService;
    private readonly IPrinterSelectorService _printerSelectorService;

    private Windows.Storage.Search.StorageFileQueryResult? _queryResult;
    private bool _isDesc = true;

    public ICommand ItemClickCommand
    {
        get;
    }

    private DispatcherQueue DispatcherQueue
    {
        get;
    }

    public ObservableCollection<TakenPhoto> Source { get; } = new ObservableCollection<TakenPhoto>();
    public bool IsDesc
    {
        get => _isDesc;
        internal set => SetProperty(ref _isDesc, value);
    }

    public int SortOrderIndex
    {
        get => _isDesc ? 0 : 1;
        internal set => SetProperty(ref _isDesc, Convert.ToBoolean(value));
    }

    public ContentGridViewModel(
        INavigationService navigationService,
        ITakenPhotoService takenPhotoService,
        IWatchingFolderService watchingFolderService,
        IFrameImageSelectorService frameImageSelectorService,
        IPrinterSelectorService printerSelectorService)
    {
        _navigationService = navigationService;
        _takenPhotoService = takenPhotoService;
        _watchingFolderService = watchingFolderService;
        _frameImageSelectorService = frameImageSelectorService;
        _printerSelectorService = printerSelectorService;

        ItemClickCommand = new RelayCommand<TakenPhoto>(OnItemClick);
        DispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    public async void OnNavigatedTo(object parameter)
    {
        await _printerSelectorService.InitializeAsync();
        await _watchingFolderService.InitializeAsync();
        await _frameImageSelectorService.InitializeAsync();
        await FileQueryInit();
        await UpdateGridList();
    }

    public void OnNavigatedFrom()
    {
    }

    private async Task UpdateGridList()
    {
        Source.Clear();

        var pictures = await _takenPhotoService.GetContentGridDataAsync();
        if (IsDesc) pictures = pictures.Reverse().ToArray();

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

    private async Task FileQueryInit()
    {
        if (_watchingFolderService.WatchingFolder == null)
        {
            return;
        }

        // 監視を行うファイルの拡張子をリストに追加する
        // 全てのファイルを監視する場合はQueryOptionsで以下のList<string>を指定する
        // ・"*"の単一のエントリを含む配列
        // ・空の配列
        // ・null
        // 複数の拡張子を監視する場合は.Addで監視対象の拡張子をリストに追加する
        var fileTypeFilter = new List<string>
        {
            ".jpg",
            ".JPG"
        };

        // ファイルリストのソート順とフィルターを指定する
        // OrderByNameではファイルは名前順にソートされる
        var queryOptions =
            new Windows.Storage.Search.QueryOptions
                (Windows.Storage.Search.CommonFileQuery.OrderByName, fileTypeFilter);

        // 監視を行うディレクトリに作成した QueryOptions を設定する(本例はLocalStateフォルダ)
        // 戻り値として StorageFileQueryResult のインスタンスが取得できる
        _queryResult = _watchingFolderService.WatchingFolder.CreateFileQueryWithOptions(queryOptions);

        // イベントを登録
        _queryResult.ContentsChanged += FileQueryResult_Changed;

        // 監視を開始する
        await _queryResult.GetFilesAsync();
    }

    private async void FileQueryResult_Changed(Windows.Storage.Search.IStorageQueryResultBase sender, object args)
    {
        await Task.Run(() =>
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                await UpdateGridList();
            });
        });
    }
}
