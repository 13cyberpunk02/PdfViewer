
using CommunityToolkit.Mvvm.ComponentModel;

namespace PdfViewer.ViewModels;

public partial class PdfPageViewModel : ObservableObject
{
    [ObservableProperty]
    private int pageNumber;

    [ObservableProperty]
    private string pageThumbnail;

    [ObservableProperty]
    private bool isSelected;
}
