using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PdfViewer.ViewModels;

public partial class PagePreviewViewModel : ObservableObject
{
    [ObservableProperty]
    private ImageSource? fullPageImage;

    [ObservableProperty]
    private bool toolbarVisible = true;

    public IRelayCommand ZoomInCommand { get; }
    public IRelayCommand ZoomOutCommand { get; }
    public IRelayCommand ResetZoomCommand { get; }
    public IRelayCommand RotateLeftCommand { get; }
    public IRelayCommand RotateRightCommand { get; }

    public PagePreviewViewModel()
    {
        ZoomInCommand = new RelayCommand(() => ZoomRequested?.Invoke(1.25));
        ZoomOutCommand = new RelayCommand(() => ZoomRequested?.Invoke(0.8));
        ResetZoomCommand = new RelayCommand(() => ZoomResetRequested?.Invoke());
        RotateLeftCommand = new RelayCommand(() => RotateRequested?.Invoke(-90));
        RotateRightCommand = new RelayCommand(() => RotateRequested?.Invoke(90));
    }

    public event Action<double>? ZoomRequested;
    public event Action? ZoomResetRequested;
    public event Action<double>? RotateRequested;
}