using DoumeiShotApp.Models;

namespace DoumeiShotApp.Contracts.Services;

public interface ITakenPhotoService
{
    Task<IList<TakenPhoto>> GetContentGridDataAsync();
}
