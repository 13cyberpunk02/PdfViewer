namespace PdfViewer.Services;

public interface IPdfDocumentService
{
    Task<ICollection<string>> RasterizeByDpi(string input, int dpi);
    Task<ICollection<string>> Rasterize(string inputPdfDocument);
    Task<string> RasterizeByPage(int dpi, int pageNumber, string inputPdfDocument);
    Task<List<string>> RasterizeByPages(int dpi, int[] pageNumbers, string inputPdfDocument);
    Task<string> RasterizeByRange(int dpi, int startPageNumber, int lastPageNumber, string inputPdfDocument);
    Task<ICollection<string>> MakeThumbnails(string inputPdfDocument);

    Task<bool> ImageToPdf(string image, string outputPdfFileName);
    Task<bool> ImagesToPdf(string[] images, string outputPdfFileName);
}
