using DoumeiShotApp.Core.Contracts.Services;
using SkiaSharp;

namespace DoumeiShotApp.Core.Services;

public class ImageEditService : IImageEditService
{
    public string OverlayImage(string baseImagePath, string coverImagePath)
    {
        using var baseImage = SKBitmap.Decode(baseImagePath);
        using var coverImage = SKBitmap.Decode(coverImagePath);
        using var newImage = new SKBitmap(coverImage.Width, coverImage.Height);
        using var surface = SKSurface.Create(new SKImageInfo(newImage.Width, newImage.Height));

        var canvas = surface.Canvas;
        var scale = CalcScale(baseImage.Width, baseImage.Height, coverImage.Width, coverImage.Height);

        var x = (newImage.Width - (scale * baseImage.Width)) / 2;
        var y = (newImage.Height - (scale * baseImage.Height)) / 2;

        var y_offset = 50;

        SKRect destRect = new(0, y_offset, scale * baseImage.Width, y_offset + scale * baseImage.Height);

        canvas.DrawBitmap(baseImage, destRect);
        canvas.DrawBitmap(coverImage, 0, 0);
        canvas.Flush();

        var outImageData = surface.Snapshot().Encode(SKEncodedImageFormat.Jpeg, 100);
        var outImageTmpPath = Path.GetDirectoryName(baseImagePath) + @"\" + Guid.NewGuid().ToString() + ".jpg";
        using var stream = File.Create(outImageTmpPath);
        outImageData.SaveTo(stream);

        return outImageTmpPath;
    }

    private static float CalcScale(int width, int height, int targetWidth, int targetHeight)
    {
        return Math.Min((float)targetWidth / width, (float)targetHeight / height);
    }
}
