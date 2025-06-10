
using System.Drawing;
using System.Windows.Media.Imaging;
using PdfViewer.ViewModels;

namespace PdfViewer.Views;

public partial class PagePreviewView
{
    public PagePreviewView(BitmapImage image)
    {
        InitializeComponent();
        DataContext = new { FullPageImage = image };
    }
}