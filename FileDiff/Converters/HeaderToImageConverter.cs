using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace FileDiff;

[ValueConversion(typeof(string), typeof(bool))]
public class HeaderToImageConverter : IValueConverter
{

	public static HeaderToImageConverter Instance = new();

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is DriveInfo)
		{
			return Application.Current.FindResource("DriveIcon");
		}
		else if (value is DirectoryInfo)
		{
			return Application.Current.FindResource("FolderIcon");
		}
		else
		{
			return Application.Current.FindResource("FileIcon");
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException("Cannot convert back");
	}

}
