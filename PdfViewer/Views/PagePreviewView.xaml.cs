using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PdfViewer.ViewModels;
using Point = System.Drawing.Point;

namespace PdfViewer.Views;

public partial class PagePreviewView
{
    private readonly PagePreviewViewModel _vm;
    private Point _start;
    private bool _drag;
    private double _startX, _startY;
    private double _currentAngle = 0;
    private DispatcherTimer _toolbarTimer;
    
    public PagePreviewView(ImageSource image)
    {
        InitializeComponent();
        _vm = new PagePreviewViewModel { FullPageImage = image };
        DataContext = _vm;

        _vm.ZoomRequested += OnZoomRequested;
        _vm.ZoomResetRequested += OnZoomResetRequested;
        _vm.RotateRequested += OnRotateRequested;

        _toolbarTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(3) };
        _toolbarTimer.Tick += (_, __) => HideToolbar();
        
    }

       private void ImageBorder_MouseEnter(object sender, MouseEventArgs e)
    {
        ShowToolbar();
    }

    private void ImageBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (_drag && e.LeftButton == MouseButtonState.Pressed)
        {
            var pos = e.GetPosition(ImageBorder);
            var dx = pos.X - _start.X;
            var dy = pos.Y - _start.Y;
            ImageTranslate.X = _startX + dx;
            ImageTranslate.Y = _startY + dy;
        }
        ShowToolbar();
    }

    private void ImageBorder_MouseLeave(object sender, MouseEventArgs e)
    {
        _toolbarTimer.Start();
        _drag = false;
    }

    private void ImageBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && (ImageScale.ScaleX > 1.01 || ImageScale.ScaleY > 1.01))
        {
            _drag = true;
            _start.X = (int)e.GetPosition(ImageBorder).X;
            _start.Y = (int)e.GetPosition(ImageBorder).Y;
            _startX = ImageTranslate.X;
            _startY = ImageTranslate.Y;
            ImageBorder.CaptureMouse();
            
            Mouse.OverrideCursor = Cursors.Hand;
        }
    }

    private void ImageBorder_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _drag = false;
        ImageBorder.ReleaseMouseCapture();
        
        Mouse.OverrideCursor = null;
    }

    private void ZoomableImage_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0)
            OnZoomRequested(1.25);
        else
            OnZoomRequested(0.8);
        e.Handled = true;
        ShowToolbar();
    }

    private void OnZoomRequested(double factor)
    {
        var newScaleX = ImageScale.ScaleX * factor;
        var newScaleY = ImageScale.ScaleY * factor;
        if (newScaleX < 1.0) newScaleX = newScaleY = 1.0;
        if (newScaleX > 10.0) newScaleX = newScaleY = 10.0;
        ImageScale.ScaleX = newScaleX;
        ImageScale.ScaleY = newScaleY;
        if (newScaleX == 1.0)
        {
            ImageTranslate.X = 0;
            ImageTranslate.Y = 0;
        }
    }

    private void OnZoomResetRequested()
    {
        ImageScale.ScaleX = ImageScale.ScaleY = 1.0;
        ImageTranslate.X = ImageTranslate.Y = 0;
    }
    
    private void OnRotateRequested(double angleDelta)
    {
        _currentAngle = (_currentAngle + angleDelta) % 360;
        if (_currentAngle < 0) _currentAngle += 360;
        ImageRotate.Angle = _currentAngle;
    }

    private void ShowToolbar()
    {
        _vm.ToolbarVisible = true;
        _toolbarTimer.Stop();
        _toolbarTimer.Start();
    }

    private void HideToolbar()
    {
        _vm.ToolbarVisible = false;
        _toolbarTimer.Stop();
    }
}