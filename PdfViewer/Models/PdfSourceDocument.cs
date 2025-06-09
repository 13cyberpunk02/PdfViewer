namespace PdfViewer.Models;

public class PdfSourceDocument
{
    public string Source { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public int PagesCount { get; set; }
    public List<PdfDocumentPages> Pages { get; set; } = [];
}
