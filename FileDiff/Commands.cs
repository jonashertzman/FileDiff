using System.Windows.Input;

namespace FileDiff;

public static class Commands
{

	public static readonly RoutedUICommand Exit = new("Exit", "Exit", typeof(Commands),
		[new KeyGesture(Key.F4, ModifierKeys.Alt)]
	);

	public static readonly RoutedUICommand About = new("About", "About", typeof(Commands));

	public static readonly RoutedUICommand Compare = new("Compare", "Compare", typeof(Commands),
		[new KeyGesture(Key.F5)]
	);

	public static readonly RoutedUICommand CancelCompare = new("Cancel Compare", "CancelCompare", typeof(Commands),
		[new KeyGesture(Key.Escape)]
	);

	public static readonly RoutedUICommand ExperimentalCompare = new("Compare", "Compare", typeof(Commands),
		[new KeyGesture(Key.F5, ModifierKeys.Shift)]
	);

	public static readonly RoutedUICommand SaveLeftFile = new("Save Left File", "SaveLeftFile", typeof(Commands));

	public static readonly RoutedUICommand SaveRightFile = new("Save Right File", "SaveRightFile", typeof(Commands));

	public static readonly RoutedUICommand Edit = new("Enable Editing", "Edit", typeof(Commands));

	public static readonly RoutedUICommand Swap = new("Swap", "Swap", typeof(Commands));

	public static readonly RoutedUICommand Up = new("Go to Parent Folder", "Up", typeof(Commands));

	public static readonly RoutedUICommand CopyLeftDiff = new("Copy Left Diff Right", "CopyLeftDiff", typeof(Commands),
		[new KeyGesture(Key.Right, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand CopyRightDiff = new("Copy Right Diff Left", "CopyRightDiff", typeof(Commands),
		[new KeyGesture(Key.Left, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand PreviousDiff = new("Move to Previous Diff", "PreviousDiff", typeof(Commands),
		[new KeyGesture(Key.Up)]
	);

	public static readonly RoutedUICommand NextDiff = new("Move to Next Diff", "NextDiff", typeof(Commands),
		[new KeyGesture(Key.Down)]
	);

	public static readonly RoutedUICommand FirstDiff = new("Move to First Diff", "FirstDiff", typeof(Commands),
		[new KeyGesture(Key.Up, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand CurrentDiff = new("Move to Current Diff", "CurrentDiff", typeof(Commands),
		[new KeyGesture(Key.Space, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand LastDiff = new("Move to Last Diff", "LastDiff", typeof(Commands),
		[new KeyGesture(Key.Down, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand NextFile = new("Select Next Changed File", "NextFile", typeof(Commands),
		[new KeyGesture(Key.PageDown, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand PreviousFile = new("Select Previous Changed File", "PreviousFile", typeof(Commands),
		[new KeyGesture(Key.PageUp, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand BrowseLeft = new("Browse Left", "BrowseLeft", typeof(Commands),
		[new KeyGesture(Key.D1, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand BrowseRight = new("Browse Right", "BrowseRight", typeof(Commands),
		[new KeyGesture(Key.D2, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand Options = new("Options", "Options", typeof(Commands));

	public static readonly RoutedUICommand Find = new("Find", "Find", typeof(Commands),
		[new KeyGesture(Key.F, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand FindNext = new("Find Next", "FindNext", typeof(Commands),
		[new KeyGesture(Key.G, ModifierKeys.Control), new KeyGesture(Key.F3)]
	);

	public static readonly RoutedUICommand FindPrevious = new("Find Previous", "FindPrevious", typeof(Commands),
		[new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift)]
	);

	public static readonly RoutedUICommand CloseFind = new("Close Find", "CloseFind", typeof(Commands),
		[new KeyGesture(Key.Escape)]
	);

	public static readonly RoutedUICommand ZoomIn = new("Zoom In", "ZoomIn", typeof(Commands),
		[new KeyGesture(Key.OemPlus, ModifierKeys.Control, "Ctrl++")]
	);

	public static readonly RoutedUICommand ZoomOut = new("Zoom Out", "ZoomOut", typeof(Commands),
		[new KeyGesture(Key.OemMinus, ModifierKeys.Control, "Ctrl+-")]
	);

	public static readonly RoutedUICommand ResetZoom = new("Reset Zoom", "ResetZoom", typeof(Commands),
		[new KeyGesture(Key.D0, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand OpenContainingFolder = new("Open Containing Folder", "OpenContainingFolder", typeof(Commands));

	public static readonly RoutedUICommand CopyPathToClipboard = new("Copy Path to Clipboard", "CopyPathToClipboard", typeof(Commands));

}
