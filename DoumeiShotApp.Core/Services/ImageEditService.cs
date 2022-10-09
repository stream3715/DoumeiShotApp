using System.Drawing;
using System.Text;

using DoumeiShotApp.Core.Contracts.Services;

using Newtonsoft.Json;

namespace DoumeiShotApp.Core.Services;

public class ImageEditService : IImageEditService
{
    public string OverlayImage(string baseImagePath, string coverImagePath)
    {
        var baseImage = new Bitmap(baseImagePath);
        var coverImage = new Bitmap(coverImagePath);
        var newImage = new Bitmap(baseImage);

        Graphics g = Graphics.FromImage(newImage);
        g.DrawImage(coverImage, 0, 0, coverImage.Width, coverImage.Height);

        g.Dispose();
        baseImage.Dispose();
        coverImage.Dispose();

        newImage.Save(baseImagePath + ".frame.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        newImage.Dispose();
        return baseImagePath + ".frame.jpg";
    }
}
