using System.Windows.Input;

namespace FileDiff;

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

	public static readonly RoutedUICommand CancelCompare = new RoutedUICommand("Cancel Compare", "CancelCompare", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.Escape)
		}
	);

	public static readonly RoutedUICommand ExperimentalCompare = new RoutedUICommand("Compare", "Compare", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.F5, ModifierKeys.Shift)
		}
	);

	public static readonly RoutedUICommand SaveLeftFile = new RoutedUICommand("Save Left File", "SaveLeftFile", typeof(Commands));

	public static readonly RoutedUICommand SaveRightFile = new RoutedUICommand("Save Right File", "SaveRightFile", typeof(Commands));

	public static readonly RoutedUICommand Edit = new RoutedUICommand("Enable Editing", "Edit", typeof(Commands));

	public static readonly RoutedUICommand Swap = new RoutedUICommand("Swap", "Swap", typeof(Commands));

	public static readonly RoutedUICommand Up = new RoutedUICommand("Go to Parent Folder", "Up", typeof(Commands));

	public static readonly RoutedUICommand CopyLeftDiff = new RoutedUICommand("Copy Left Diff Right", "CopyLeftDiff", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.Right, ModifierKeys.Control)
		}
	);

	public static readonly RoutedUICommand CopyRightDiff = new RoutedUICommand("Copy Right Diff Left", "CopyRightDiff", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.Left, ModifierKeys.Control)
		}
	);

	public static readonly RoutedUICommand PreviousDiff = new RoutedUICommand("Move to Previous Diff", "PreviousDiff", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.Up)
		}
	);

	public static readonly RoutedUICommand NextDiff = new RoutedUICommand("Move to Next Diff", "NextDiff", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.Down)
		}
	);

	public static readonly RoutedUICommand FirstDiff = new RoutedUICommand("Move to First Diff", "FirstDiff", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.Up, ModifierKeys.Control)
		}
	);

	public static readonly RoutedUICommand CurrentDiff = new RoutedUICommand("Move to Current Diff", "CurrentDiff", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.Space, ModifierKeys.Control)
		}
	);

	public static readonly RoutedUICommand LastDiff = new RoutedUICommand("Move to Last Diff", "LastDiff", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.Down, ModifierKeys.Control)
		}
	);

	public static readonly RoutedUICommand NextFile = new RoutedUICommand("Select Next Changed File", "NextFile", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.PageDown, ModifierKeys.Control)
		}
	);

	public static readonly RoutedUICommand PreviousFile = new RoutedUICommand("Select Previous Changed File", "PreviousFile", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.PageUp, ModifierKeys.Control)
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

	public static readonly RoutedUICommand ZoomIn = new RoutedUICommand("Zoom In", "ZoomIn", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.OemPlus, ModifierKeys.Control, "Ctrl++")
		}
	);

	public static readonly RoutedUICommand ZoomOut = new RoutedUICommand("Zoom Out", "ZoomOut", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.OemMinus, ModifierKeys.Control, "Ctrl+-")
		}
	);

	public static readonly RoutedUICommand ResetZoom = new RoutedUICommand("Reset Zoom", "ResetZoom", typeof(Commands),
		new InputGestureCollection()
		{
			new KeyGesture(Key.D0, ModifierKeys.Control)
		}
	);

	public static readonly RoutedUICommand OpenContainingFolder = new RoutedUICommand("Open Containing Folder", "OpenContainingFolder", typeof(Commands));

	public static readonly RoutedUICommand CopyPathToClipboard = new RoutedUICommand("Copy Path to Clipboard", "CopyPathToClipboard", typeof(Commands));

}
