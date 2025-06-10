using PdfiumViewer;
using PdfViewer.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PdfViewer.Services;

public class PdfThumbnailService : IPdfThumbnailService
{
    public PdfSourceDocument LoadAndRasterize(string filePath)
    {
        var sourceDocument = new PdfSourceDocument
        {
            Source = filePath,
            Filename = Path.GetFileName(filePath),
            Pages = new List<PdfDocumentPages>()
        };

        using var document = PdfDocument.Load(filePath);
        sourceDocument.PagesCount = document.PageCount;
        for(int i = 0; i < document.PageCount; i++)
        {
            using var image = document.Render(i, 96,96, dpiX: 96, dpiY: 96, PdfRenderFlags.Annotations);
            var base64 = ImageToBase64(image, ImageFormat.Png);
            sourceDocument.Pages.Add(new PdfDocumentPages
            {
                PageNumber = i + 1,
                PageThumbnail = base64
            });
        }

        return sourceDocument;
    }

    private string ImageToBase64(Image image, ImageFormat format)
    {
        using var ms = new MemoryStream();
        image.Save(ms, format);
        return Convert.ToBase64String(ms.ToArray());
    }
}
