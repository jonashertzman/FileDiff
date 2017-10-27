using System.Windows.Input;

namespace FileDiff
{
	public static class Commands
	{

		public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit", "Exit", typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.F4, ModifierKeys.Alt)
			}
		);

		public static readonly RoutedUICommand Compare = new RoutedUICommand("Compare", "Compare", typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.F5)
			}
		);

		public static readonly RoutedUICommand BrowseLeft = new RoutedUICommand("BrowseLeft", "BrowseLeft", typeof(Commands),
			new InputGestureCollection()
			{
						new KeyGesture(Key.D1, ModifierKeys.Control)
			}
		);

		public static readonly RoutedUICommand BrowseRight = new RoutedUICommand("BrowseRight", "BrowseRight", typeof(Commands),
			new InputGestureCollection()
			{
						new KeyGesture(Key.D2, ModifierKeys.Control)
			}
		);

	}
}
