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

		public static readonly RoutedUICommand About = new RoutedUICommand("About", "About", typeof(Commands));

		public static readonly RoutedUICommand Compare = new RoutedUICommand("Compare", "Compare", typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.F5)
			}
		);

		public static readonly RoutedUICommand PreviousDiff = new RoutedUICommand("Previous Diff", "PreviousDiff", typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.Up)
			}
		);

		public static readonly RoutedUICommand NextDiff = new RoutedUICommand("Next Diff", "NextDiff", typeof(Commands),
			new InputGestureCollection()
			{
					new KeyGesture(Key.Down)
			}
		);

		public static readonly RoutedUICommand BrowseLeft = new RoutedUICommand("Browse Left", "BrowseLeft", typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.D1, ModifierKeys.Control)
			}
		);

		public static readonly RoutedUICommand BrowseRight = new RoutedUICommand("Browse Right", "BrowseRight", typeof(Commands),
			new InputGestureCollection()
			{
					new KeyGesture(Key.D2, ModifierKeys.Control)
			}
		);

		public static readonly RoutedUICommand Options = new RoutedUICommand("Options", "Options", typeof(Commands));

	}
}
