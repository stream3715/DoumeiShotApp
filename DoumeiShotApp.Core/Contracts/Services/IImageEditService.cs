namespace DoumeiShotApp.Core.Contracts.Services;

public interface IImageEditService
{
    string OverlayImage(string baseImagePath, string coverImagePath);
}
