using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ADE.Reporting.Pdf;

public sealed class ThumbnailManager
{
    private const int ThumbnailSize = 320;

    public string Generate(string imageFile)
    {
        string evidenceFolder =
            Path.GetDirectoryName(imageFile)!;

        string thumbnailFolder =
            Path.Combine(
                evidenceFolder,
                ".thumbs");

        Directory.CreateDirectory(thumbnailFolder);

        string fileName =
            Path.GetFileNameWithoutExtension(imageFile) + ".jpg";

        string thumbnailFile =
            Path.Combine(
                thumbnailFolder,
                fileName);

        if (!File.Exists(thumbnailFile))
        {
            using var original =
                Image.FromFile(imageFile);

            int width;
            int height;

            if (original.Width >= original.Height)
            {
                width = ThumbnailSize;
                height = original.Height * ThumbnailSize / original.Width;
            }
            else
            {
                height = ThumbnailSize;
                width = original.Width * ThumbnailSize / original.Height;
            }

            using Bitmap bitmap =
                new(width, height);

            using Graphics graphics =
                Graphics.FromImage(bitmap);

            graphics.InterpolationMode =
                InterpolationMode.HighQualityBicubic;

            graphics.SmoothingMode =
                SmoothingMode.HighQuality;

            graphics.DrawImage(
                original,
                0,
                0,
                width,
                height);

            bitmap.Save(
                thumbnailFile,
                ImageFormat.Jpeg);
        }

        return Path.Combine(
            "evidencias",
            ".thumbs",
            fileName);
    }
}