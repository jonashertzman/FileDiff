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

	}
}
