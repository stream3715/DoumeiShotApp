namespace DoumeiShotApp.Core.Contracts.Services;

public interface IImageEditService
{
    string OverlayImageAsync(string baseImagePath, string coverImagePath);
}
