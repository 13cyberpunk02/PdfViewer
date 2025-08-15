namespace PdfViewer.Services;

public interface IPdfDocumentService
{
    Task<ICollection<string>> RasterizeByDpi(string input, int dpi);
    Task<ICollection<string>> Rasterize(string inputPdfDocument);
    Task<string> RasterizeByPage(int pageNumber, string inputPdfDocument);
    Task<List<string>> RasterizeByPages(int[] pageNumbers, string inputPdfDocument);
    Task<string> RasterizeByRange(int startPageNumber, int lastPageNumber, string inputPdfDocument);
    Task<ICollection<string>> MakeThumbnails(string inputPdfDocument);

    bool ImageToPdf(string image, string outputPdfFileName);
    Task<bool> ImagesToPdf(List<string>? images, string outputPdfFileName, CancellationToken ct = default);
}
