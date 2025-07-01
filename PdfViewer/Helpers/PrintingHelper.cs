using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;

namespace PdfViewer.Helpers;

public static class PrintingHelper
{
    /// <summary>
    /// Основная функция печати. Определяет формат, не печатает если Unknown, иначе печатает с правильным размером
    /// </summary>
     public static string PrintImageSource(
        ImageSource imageSource,
        string printerName,
        double? widthMm = null,
        double? heightMm = null,
        Action<string>? onStatusUpdate = null)
    {
        // 1. Получаем DPI и размеры
        double dpiX = 96, dpiY = 96;
        int pixelWidth = 0, pixelHeight = 0;
        if (imageSource is BitmapSource bmp)
        {
            dpiX = bmp.DpiX > 1 ? bmp.DpiX : 96;
            dpiY = bmp.DpiY > 1 ? bmp.DpiY : 96;
            pixelWidth = bmp.PixelWidth;
            pixelHeight = bmp.PixelHeight;
        }
        else
        {
            
            return "ImageSource должен быть BitmapSource";
        }
        
        var format = PaperFormatHelper.DetectPaperFormat(bmp, out double width, out double height);
        
        if (format == PaperFormatHelper.PaperFormat.Unknown)
        {
            string supported = string.Join(", ", PaperFormatHelper.StandardSizes.Select(s => $"{s.Item1}: {s.Item2}x{s.Item3}mm"));
            return $"Ошибка: формат изображения не распознан!\n" +
                   $"Поддерживаются только: {supported}.\n" +
                   $"Размер изображения: {widthMm:F0} x {heightMm:F0} мм.";
        }
        
        // 2. Размеры в мм
        double calcWidthMm = pixelWidth / dpiX * 25.4;
        double calcHeightMm = pixelHeight / dpiY * 25.4;
        double wMm = widthMm ?? calcWidthMm;
        double hMm = heightMm ?? calcHeightMm;
        double widthPx = wMm / 25.4 * 96;
        double heightPx = hMm / 25.4 * 96;

        // 3. Создаём Image для печати
        var imgCtrl = new Image
        {
            Source = imageSource,
            Width = widthPx,
            Height = heightPx,
            Stretch = Stretch.Uniform
        };

        // 4. Создаём страницу
        var fixedPage = new FixedPage
        {
            Width = widthPx,
            Height = heightPx
        };
        FixedPage.SetLeft(imgCtrl, 0);
        FixedPage.SetTop(imgCtrl, 0);
        fixedPage.Children.Add(imgCtrl);

        // 5. В документ
        var pageContent = new PageContent();
        ((IAddChild)pageContent).AddChild(fixedPage);

        var doc = new FixedDocument
        {
            DocumentPaginator =
            {
                PageSize = new Size(widthPx, heightPx)
            }
        };
        doc.Pages.Add(pageContent);

        // 6. Настраиваем принтер
        var server = new LocalPrintServer();
        PrintQueue queue;
        try
        {
            queue = server.GetPrintQueue(printerName);
        }
        catch
        {
            return $"Принтер '{printerName}' не найден.";
        }

        var ticket = queue.DefaultPrintTicket.Clone();
        ticket.PageMediaSize = new PageMediaSize(
            PageMediaSizeName.Unknown,
            widthPx,
            heightPx
        );

        // 7. Печатаем
        try
        {
            onStatusUpdate?.Invoke("Отправка на печать...");
            var writer = PrintQueue.CreateXpsDocumentWriter(queue);
            writer.Write(doc.DocumentPaginator, ticket);
        }
        catch (Exception ex)
        {
            return "Ошибка печати: " + ex.Message;
        }
        return "Печать завершена";
    }
}
