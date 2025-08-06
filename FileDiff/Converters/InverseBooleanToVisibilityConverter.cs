using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FileDiff;

public class InverseBooleanToVisibilityConverter : IValueConverter
{

	private readonly BooleanToVisibilityConverter converter = new();

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var result = converter.Convert(value, targetType, parameter, culture) as Visibility?;
		return result == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var result = converter.ConvertBack(value, targetType, parameter, culture) as bool?;
		return result != true;
	}

}
