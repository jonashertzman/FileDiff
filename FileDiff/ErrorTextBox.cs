using System.Windows;
using System.Windows.Controls;

namespace FileDiff;

public class ErrorTextBox : TextBox
{

	#region Dependency Properties

	public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register("Error", typeof(bool), typeof(ErrorTextBox));

	public bool Error
	{
		get { return (bool)GetValue(ErrorProperty); }
		set
		{
			SetValue(ErrorProperty, value);
		}
	}

	#endregion

}
