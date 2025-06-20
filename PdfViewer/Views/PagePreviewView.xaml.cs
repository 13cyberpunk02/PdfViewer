using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PdfViewer.ViewModels;

namespace PdfViewer.Views;

public partial class PagePreviewView
{
    private readonly PagePreviewViewModel _vm;
    private readonly DispatcherTimer _toolbarTimer = new() { Interval = TimeSpan.FromSeconds(3) };
    
    public PagePreviewView(ImageSource image)
    {
        InitializeComponent();
        _vm = new PagePreviewViewModel(image);
        DataContext = _vm;
        
        _toolbarTimer.Tick += (_, __) =>
        {
            _vm.HideToolbar();
            _toolbarTimer.Stop();
        };

        // События мыши
        ImageBorder.MouseEnter += (_, __) => ShowToolbar();
        ImageBorder.MouseLeave += (_, __) => { _toolbarTimer.Start(); _vm.StopDrag(); Mouse.OverrideCursor = null; };
        ImageBorder.MouseMove  += ImageBorder_MouseMove;
        ImageBorder.MouseDown  += ImageBorder_MouseDown;
        ImageBorder.MouseUp    += ImageBorder_MouseUp;
        ZoomableImage.MouseWheel += ZoomableImage_MouseWheel;
    }

    private void ImageBorder_MouseEnter(object sender, MouseEventArgs e)
    {
        ShowToolbar();
    }

    private void ImageBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && _vm.Scale > 1.01)
        {
            _vm.UpdateDrag(e.GetPosition(ImageBorder));
        }
        ShowToolbar();
    }

    private void ImageBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && _vm.Scale > 1.01)
        {
            _vm.StartDrag(e.GetPosition(ImageBorder));
            Mouse.OverrideCursor = Cursors.Hand;
            ImageBorder.CaptureMouse();
        }
    }

    private void ImageBorder_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _vm.StopDrag();
        Mouse.OverrideCursor = null;
        ImageBorder.ReleaseMouseCapture();
    }

    private void ZoomableImage_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        _vm.MouseWheelZoom(e.Delta);
        ShowToolbar();
        e.Handled = true;
    }

    private void ShowToolbar()
    {
        _vm.ShowToolbar();
        _toolbarTimer.Stop();
        _toolbarTimer.Start();
    }
}