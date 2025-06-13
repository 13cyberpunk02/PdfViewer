using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PdfViewer.Helpers.Converters;

public class SelectedBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
           (value is bool b && b) ? new SolidColorBrush(Color.FromArgb(0xD0, 0x00, 0x00, 0x00)) : Brushes.Transparent;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
