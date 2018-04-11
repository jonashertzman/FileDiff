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

		public static readonly RoutedUICommand Swap = new RoutedUICommand("Swap", "Swap", typeof(Commands));

		public static readonly RoutedUICommand Up = new RoutedUICommand("Up", "Up", typeof(Commands));

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

		public static readonly RoutedUICommand FirstDiff = new RoutedUICommand("First Diff", "FirstDiff", typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.Up, ModifierKeys.Control)
			}
		);

		public static readonly RoutedUICommand LastDiff = new RoutedUICommand("Last Diff", "LastDiff", typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.Down, ModifierKeys.Control)
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

		public static readonly RoutedUICommand Find = new RoutedUICommand("Find", "Find", typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.F, ModifierKeys.Control)
			}
		);

		public static readonly RoutedUICommand FindNext = new RoutedUICommand("Find Next", "FindNext", typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.G, ModifierKeys.Control),
				new KeyGesture(Key.F3)
			}
		);

		public static readonly RoutedUICommand FindPrevious = new RoutedUICommand("Find Previous", "FindPrevious", typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.G, ModifierKeys.Control|ModifierKeys.Shift)
			}
		);

		public static readonly RoutedUICommand CloseFind = new RoutedUICommand("Close Find", "CloseFind", typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.Escape)
			}
		);

	}
}
