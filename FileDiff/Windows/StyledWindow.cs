using System.Windows;

namespace FileDiff;

public class StyledWindow : Window
{

	public StyledWindow()
	{
		WindowStartupLocation = WindowStartupLocation.CenterOwner;

		SetResourceReference(StyleProperty, typeof(StyledWindow));

		SourceInitialized += Window_SourceInitialized;
		Activated += Window_Activated;
		Deactivated += Window_Deactivated;
	}

	private void Window_SourceInitialized(object sender, EventArgs e)
	{
		WinApi.SetTitleBarColor(this, true);
	}

	private void Window_Activated(object sender, EventArgs e)
	{
		WinApi.SetTitleBarColor(this, true);
	}

	private void Window_Deactivated(object sender, EventArgs e)
	{
		WinApi.SetTitleBarColor(this, false);
	}

}
