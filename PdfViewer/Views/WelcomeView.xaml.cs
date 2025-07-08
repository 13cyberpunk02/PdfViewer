using PdfViewer.Services;
using PdfViewer.ViewModels;

namespace PdfViewer.Views
{
    public partial class WelcomeView 
    {
        private WelcomeViewModel _viewModel;
        public WelcomeView()
        {
            InitializeComponent();
            _viewModel = new(new PdfDocumentService());
            DataContext = _viewModel;
        }
    }
}
