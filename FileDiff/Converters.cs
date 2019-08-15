using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FileDiff
{

	[ValueConversion(typeof(bool), typeof(bool))]
	public class InverseBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(bool))
				throw new InvalidOperationException("The target must be a boolean");

			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class InverseBooleanToVisibilityConverter : IValueConverter
	{
		private BooleanToVisibilityConverter _converter = new BooleanToVisibilityConverter();

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var result = _converter.Convert(value, targetType, parameter, culture) as Visibility?;
			return result == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var result = _converter.ConvertBack(value, targetType, parameter, culture) as bool?;
			return result == true ? false : true;
		}
	}

	[ValueConversion(typeof(string), typeof(bool))]
	public class HeaderToImageConverter : IValueConverter
	{
		public static HeaderToImageConverter Instance = new HeaderToImageConverter();

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

}
