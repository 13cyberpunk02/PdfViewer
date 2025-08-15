using ImageMagick;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace PdfViewer.Services;

public class PdfDocumentService : IPdfDocumentService
{
    private readonly string _gsExeFilePath = "gswin64c.exe";
    private readonly int _dpi = 300;

    public async Task<ICollection<string>> Rasterize(string inputPdfDocument)
    {
        string tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);
        string outputPattern = Path.Combine(tmpDir, "page_%d.jpg");
        var pageCount = GetPagesCount(inputPdfDocument);
        string escapedPath = inputPdfDocument.Replace("\"", "\\\"");
        string gsArgs = $"-dNOPAUSE -dBATCH -q " +
            $"-sDEVICE=jpeg " +
            $"-r{_dpi} -dJPEGQ=95 -dFirstPage=1 -dLastPage={pageCount} -sOutputFile=\"{outputPattern}\" \"{escapedPath}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _gsExeFilePath,
                Arguments = gsArgs,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };
        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            MessageBox.Show("Произошла ошибка при обработке файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        List<string> pages = new();

        for (int i = 0; i < pageCount; i++)
        {
            string jpgPath = Path.Combine(tmpDir, $"page_{i + 1}.jpg");
            byte[] imageBytes = await File.ReadAllBytesAsync(jpgPath);
            string base64 = Convert.ToBase64String(imageBytes);
            pages.Add(base64);
        }
        return pages;
    }

    public async Task<string> RasterizeByPage(int pageNumber, string inputPdfDocument)
    {
        string tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);
        string outputPattern = Path.Combine(tmpDir, "page.jpg");        
        string escapedPath = inputPdfDocument.Replace("\"", "\\\"");
        string gsArgs = $"-dNOPAUSE -dBATCH -q " +
            $"-sDEVICE=jpeg " +
            $"-r{_dpi} -dJPEGQ=95 -sPageList={pageNumber} -sOutputFile=\"{outputPattern}\" \"{escapedPath}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _gsExeFilePath,
                Arguments = gsArgs,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };
        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            MessageBox.Show("Произошла ошибка при обработке файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }        
        return outputPattern;
    }    


    public async Task<ICollection<string>> MakeThumbnails(string inputPdfDocument)
    {
        string tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);

        try
        {
            int pageCount = GetPagesCount(inputPdfDocument);
            string outputPattern = Path.Combine(tmpDir, "page_%d.jpg");
            string escapedPath = inputPdfDocument.Replace("\"", "\\\"");
            string gsArgs = $"-dNOPAUSE -dBATCH -q " +
                $"-sDEVICE=jpeg " +
                $"-r96 -dJPEGQ=95 -dFirstPage=1 -dLastPage={pageCount} -sOutputFile=\"{outputPattern}\" \"{escapedPath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _gsExeFilePath,
                    Arguments = gsArgs,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                MessageBox.Show("Произошла ошибка при обработке файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            var result = new List<string>();
            for (int i = 0; i < pageCount; i++)
            {
                string pngPath = Path.Combine(tmpDir, $"page_{i + 1}.jpg");
                byte[] imageBytes = await File.ReadAllBytesAsync(pngPath);
                string base64 = Convert.ToBase64String(imageBytes);
                result.Add(base64);
            }
            return result;
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, recursive: true);
        }
    }

    public async Task<ICollection<string>> RasterizeByDpi(string input, int dpi)
    {
        string tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);
        string outputPattern = Path.Combine(tmpDir, "page_%d.jpg");
        var pageCount = GetPagesCount(input);
        string escapedPath = input.Replace("\"", "\\\"");
        string gsArgs = $"-dNOPAUSE -dBATCH -q " +
            $"-sDEVICE=jpeg " +
            $"-r{dpi} -dJPEGQ=95 -dFirstPage=1 -dLastPage={pageCount} -sOutputFile=\"{outputPattern}\" \"{escapedPath}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _gsExeFilePath,
                Arguments = gsArgs,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };
        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            MessageBox.Show("Произошла ошибка при обработке файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        List<string> pages = new();

        for(int i = 0; i < pageCount; i++)
        {
            string jpgPath = Path.Combine(tmpDir, $"page_{i + 1}.jpg");
            byte[] imageBytes = await File.ReadAllBytesAsync(jpgPath);
            string base64 = Convert.ToBase64String(imageBytes);
            pages.Add(base64);
        }        
        return pages;
    }

    private int GetPagesCount(string inputPdfFilePath)
    {
        if (!File.Exists(inputPdfFilePath))
        {            
            MessageBox.Show("PDF файл был не найден, \nБыл передан данный файл: " + inputPdfFilePath, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return 0;
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = _gsExeFilePath,
            Arguments = $"-q -dNODISPLAY -dPDFINFO \"{inputPdfFilePath}\" -c quit",
            UseShellExecute = false,     
            CreateNoWindow = true,       
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            WindowStyle = ProcessWindowStyle.Hidden 
        };

        using (var process = new Process { StartInfo = processInfo })
        {
            process.Start();
            string output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
                
            }

            var match = Regex.Match(output, @"File has (\d+) (page|pages)");
            if (!match.Success)
            {                
                MessageBox.Show("Не могу определить количество страниц", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return int.Parse(match.Groups[1].Value);
        }
    }

    public async Task<string> RasterizeByRange(int startPageNumber, int lastPageNumber, string inputPdfDocument)
    {
        string tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);
        string outputPattern = Path.Combine(tmpDir, "page.jpg");
        string escapedPath = inputPdfDocument.Replace("\"", "\\\"");
        string gsArgs = $"-dNOPAUSE -dBATCH -q " +
            $"-sDEVICE=jpeg " +
            $"-r{_dpi} -dJPEGQ=95  -dFirstPage={startPageNumber} -dLastPage={lastPageNumber}  -sOutputFile=\"{outputPattern}\" \"{escapedPath}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _gsExeFilePath,
                Arguments = gsArgs,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };
        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            MessageBox.Show("Произошла ошибка при обработке файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
        return outputPattern;
    }

    public async Task<List<string>> RasterizeByPages(int[] pageNumbers, string inputPdfDocument)
    {
        string tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);
        List<string> pages = new List<string>();
        var pagesNumber = string.Join(",", pageNumbers); // убрал пробел после запятой

        var outputPattern = Path.Combine(tmpDir, "page_%d.jpg");
        string gsArgs = $"-dNOPAUSE -dBATCH -q " +
            $"-sDEVICE=jpeg " +
            $"-r{_dpi} -dJPEGQ=95 " +
            $"-sPageList={pagesNumber} " +
            $"-sOutputFile=\"{outputPattern}\" \"{inputPdfDocument}\"";

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _gsExeFilePath,
                    Arguments = gsArgs,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                MessageBox.Show($"Ошибка при обработке файла: {error}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                Directory.Delete(tmpDir, true);
                return null;
            }

            // Собираем реальные имена файлов
            for(var pageNum = 1; pageNum <= pageNumbers.Length; pageNum++)
            {
                string pagePath = Path.Combine(tmpDir, $"page_{pageNum}.jpg");
                if (File.Exists(pagePath))
                {
                    pages.Add(pagePath);
                }
            }

            return pages;
        }
        catch
        {
            Directory.Delete(tmpDir, true);
            throw;
        }
    }

    public bool ImageToPdf(string imagePath, string outputPdfFileName)
    {
        try
        {
            var settings = new MagickReadSettings
            {
                Density = new Density(_dpi, _dpi),
                ColorSpace = ColorSpace.sRGB
            };

            using var image = new MagickImage(imagePath, settings);
            image.Format = MagickFormat.Pdf;
            image.Write(outputPdfFileName);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    public async Task<bool> ImagesToPdf(List<string>? imagePaths, string outputPdfFileName, CancellationToken ct = default)
    {
        if (imagePaths is null || imagePaths.Count == 0)
            return false;

        try
        {
            var settings = new MagickReadSettings
            {
                Density = new Density(_dpi, _dpi),
                ColorSpace = ColorSpace.sRGB
            };

            using var images = new MagickImageCollection();
            
            foreach (var path in imagePaths)
            {
                var img = new MagickImage(path, settings);
                images.Add(img);
            }
            await images.WriteAsync(new FileInfo(outputPdfFileName), ct);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    /*
    public async Task<bool> ImagesToPdf(List<string>? images, string outputPdfFileName)
    {
        if(images is null || images.Count == 0)
            return false;

        string tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);
        List<string> pages = new List<string>();
        var pagesNumber = string.Join(",", ); // убрал пробел после запятой

        var outputPattern = Path.Combine(tmpDir, "page_%d.jpg");
        string gsArgs = $"-dNOPAUSE -dBATCH -q " +
                        $"-sDEVICE=jpeg " +
                        $"-r{dpi} -dJPEGQ=95 " +
                        $"-sPageList={pagesNumber} " +
                        $"-sOutputFile=\"{outputPattern}\" \"{inputPdfDocument}\"";
        
        
        
        
        
        var imagesCollection = string.Join(" ", images);
        string gsArgs = $"-sDEVICE=pdfwrite -o {outputPdfFileName} {imagesCollection}";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _gsExeFilePath,
                Arguments = gsArgs,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };
        process.Start();
        await process.WaitForExitAsync();
        return process.ExitCode != 0;        
    }*/
}
