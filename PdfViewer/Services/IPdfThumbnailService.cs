
using PdfViewer.Models;

namespace PdfViewer.Services;

public interface IPdfThumbnailService
{
    PdfSourceDocument LoadAndRasterize(string pdfPath);
}
