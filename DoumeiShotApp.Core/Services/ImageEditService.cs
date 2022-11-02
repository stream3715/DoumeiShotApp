using DoumeiShotApp.Core.Contracts.Services;
using SkiaSharp;

namespace DoumeiShotApp.Core.Services;

public class ImageEditService : IImageEditService
{
    public string OverlayImageAsync(string baseImagePath, string coverImagePath)
    {
        SKBitmap rotatedBaseImage;
        using (var baseImage = SKBitmap.Decode(baseImagePath))
        {
            SKEncodedOrigin baseOrientation;
            using (var fstream = File.OpenRead(baseImagePath))
            using (var inputStream = new SKManagedStream(fstream))
            {
                using var codec = SKCodec.Create(inputStream);
                baseOrientation = codec.EncodedOrigin;
            }

            rotatedBaseImage = AutoOrient(baseImage, baseOrientation);
        }

        using var coverImage = SKBitmap.Decode(coverImagePath);
        using var newImage = new SKBitmap(coverImage.Width, coverImage.Height);
        using var surface = SKSurface.Create(new SKImageInfo(newImage.Width, newImage.Height));

        var scale = CalcScale(rotatedBaseImage.Width, rotatedBaseImage.Height, coverImage.Width, coverImage.Height);
        var x = (newImage.Width - (scale * rotatedBaseImage.Width)) / 2;
        var y = (newImage.Height - (scale * rotatedBaseImage.Height)) / 2;

        var canvas = surface.Canvas;
        var y_offset = 50;

        SKRect destRect = new(0, y_offset, scale * rotatedBaseImage.Width, y_offset + scale * rotatedBaseImage.Height);
        SKPaint skPaint = new()
        {
            TextSize = 90.0f,
            IsAntialias = true,
            Color = new SKColor(0, 0, 0)
        };

        canvas.DrawBitmap(rotatedBaseImage, destRect);
        canvas.DrawBitmap(coverImage, 0, 0);
        canvas.DrawText(DateTime.Now.ToString(), newImage.Width * 0.01f, newImage.Height * 0.995f, skPaint);
        canvas.Flush();

        rotatedBaseImage.Dispose();
        newImage.Dispose();
        coverImage.Dispose();

        var outImageData = surface.Snapshot().Encode(SKEncodedImageFormat.Jpeg, 100);
        surface.Dispose();

        var outImageTmpPath = Path.GetDirectoryName(baseImagePath) + @"\" + Guid.NewGuid().ToString() + ".jpg";
        using var stream = File.Create(outImageTmpPath);
        outImageData.SaveTo(stream);
        stream.Flush();
        stream.Dispose();
        stream.Close();

        return outImageTmpPath;
    }

    private static float CalcScale(int width, int height, int targetWidth, int targetHeight)
    {
        return Math.Min((float)targetWidth / width, (float)targetHeight / height);
    }

    private static SKBitmap AutoOrient(SKBitmap bitmap, SKEncodedOrigin origin)
    {
        SKBitmap rotated;
        switch (origin)
        {
            case SKEncodedOrigin.BottomRight:
                using (var surface = new SKCanvas(bitmap))
                {
                    surface.RotateDegrees(180, bitmap.Width / 2, bitmap.Height / 2);
                    surface.DrawBitmap(bitmap.Copy(), 0, 0);
                }
                return bitmap;
            case SKEncodedOrigin.RightTop:
                rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                using (var surface = new SKCanvas(rotated))
                {
                    surface.Translate(rotated.Width, 0);
                    surface.RotateDegrees(90);
                    surface.DrawBitmap(bitmap, 0, 0);
                }
                return rotated;
            case SKEncodedOrigin.LeftBottom:
                rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                using (var surface = new SKCanvas(rotated))
                {
                    surface.Translate(0, rotated.Height);
                    surface.RotateDegrees(270);
                    surface.DrawBitmap(bitmap, 0, 0);
                }
                return rotated;
            default:
                return bitmap;
        }
    }
}
