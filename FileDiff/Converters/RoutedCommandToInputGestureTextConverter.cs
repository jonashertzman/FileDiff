using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace FileDiff;

public class RoutedCommandToInputGestureTextConverter : IValueConverter
{

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is RoutedCommand command)
		{
			InputGestureCollection gestures = command.InputGestures;

			if ((gestures != null) && (gestures.Count > 0))
			{
				foreach (KeyGesture keyGesture in gestures)
				{
					if (keyGesture != null)
					{
						return keyGesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
					}
				}
			}
		}

		return Binding.DoNothing;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Binding.DoNothing;
	}

}