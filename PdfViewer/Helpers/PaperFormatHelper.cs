using System.Windows.Media.Imaging;

namespace PdfViewer.Helpers;

public static class PaperFormatHelper
{
    public enum PaperFormat
    {
        Unknown,
        A0, A0x2, A0x3, 
        A1, A1x2, A1x3, A1x4, 
        A2, A2x2, A2x3, A2x4, A2x5, A2x6, 
        A3, A3x2, A3x3, A3x4, A3x5, A3x6, 
        A4x3, A4x4, A4x5, A4x6
    }

    public static readonly (PaperFormat, int, int)[]
        StandardSizes =
        [
            (PaperFormat.A0, 841, 1189),
            (PaperFormat.A0x2, 1189, 1682),
            (PaperFormat.A0x3, 1189, 3567),
            (PaperFormat.A1, 594, 841),
            (PaperFormat.A1x2, 841, 1189),
            (PaperFormat.A1x3, 841, 1783),
            (PaperFormat.A1x4, 841, 2378),
            (PaperFormat.A2, 420, 594),
            (PaperFormat.A2x2, 840, 1188),
            (PaperFormat.A2x3, 594, 1261),
            (PaperFormat.A2x4, 594, 1682),
            (PaperFormat.A2x5, 2100, 594),
            (PaperFormat.A2x6, 2520, 594),
            (PaperFormat.A3x2, 594, 420),
            (PaperFormat.A3x3, 891, 420),
            (PaperFormat.A3x4, 1188, 420),
            (PaperFormat.A3x5, 1485, 420),
            (PaperFormat.A3x6, 1782, 420),
            (PaperFormat.A4x3, 630, 297),
            (PaperFormat.A4x4, 840, 297),
            (PaperFormat.A4x5, 1050, 297),
            (PaperFormat.A4x6, 1260, 297)
        ];

    /// <summary>
    /// Определяет формат бумаги по размерам изображения (в мм и по стандартной таблице)
    /// </summary>
    public static PaperFormat DetectPaperFormat(BitmapSource image, out double widthMm, out double heightMm)
    {
        double dpiX = image.DpiX > 1 ? image.DpiX : 96;
        double dpiY = image.DpiY > 1 ? image.DpiY : 96;
        widthMm = image.PixelWidth / dpiX * 25.4;
        heightMm = image.PixelHeight / dpiY * 25.4;

        if (widthMm > heightMm)
        {
            (widthMm, heightMm) = (widthMm, heightMm);
        }
        
        const double tolerance = 20;

        foreach (var (format, w, h) in StandardSizes)
        {
            if (Math.Abs(widthMm - w) <= tolerance && Math.Abs(heightMm - h) <= tolerance)
            {
                return format;
            }
        }
        return PaperFormat.Unknown;
    }

    public static string ToString(this PaperFormat format) => format switch
    {
        PaperFormat.A0 => "A0",
        PaperFormat.A0x2 => "A0x2",
        PaperFormat.A0x3 => "A0x3",
        PaperFormat.A1 => "A1",
        PaperFormat.A1x2 => "A1x2",
        PaperFormat.A1x3 => "A1x3",
        PaperFormat.A1x4 => "A1x4",
        PaperFormat.A2 => "A2",
        PaperFormat.A2x2 => "A2x2",
        PaperFormat.A2x3 => "A2x3",
        PaperFormat.A2x4 => "A2x4",
        PaperFormat.A2x5 => "A2x5",
        PaperFormat.A2x6 => "A2x6",
        PaperFormat.A3x2 => "A3x2",
        PaperFormat.A3x3 => "A3x3",
        PaperFormat.A3x4 => "A3x4",
        PaperFormat.A3x5 => "A3x5",
        PaperFormat.A3x6 => "A3x6",
        PaperFormat.A4x3 => "A4x3",
        PaperFormat.A4x4 => "A4x4",
        PaperFormat.A4x5 => "A4x5",
        PaperFormat.A4x6 => "A4x6",
        _ => "Unknown",
    };
}