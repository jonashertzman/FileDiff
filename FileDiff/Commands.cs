using System.Windows.Input;

namespace FileDiff;

public static class Commands
{

	public static readonly RoutedUICommand Exit = new(
		"Exit", nameof(Exit), typeof(Commands),
		[new KeyGesture(Key.F4, ModifierKeys.Alt)]
	);

	public static readonly RoutedUICommand About = new(
		"About", nameof(About), typeof(Commands)
	);

	public static readonly RoutedUICommand Compare = new(
		"Compare", nameof(Compare), typeof(Commands),
		[new KeyGesture(Key.F5)]
	);

	public static readonly RoutedUICommand CancelCompare = new(
		"Cancel Compare", nameof(CancelCompare), typeof(Commands),
		[new KeyGesture(Key.Escape)]
	);

	public static readonly RoutedUICommand ExperimentalCompare = new(
		"Compare", nameof(ExperimentalCompare), typeof(Commands),
		[new KeyGesture(Key.F5, ModifierKeys.Shift)]
	);

	public static readonly RoutedUICommand SaveLeftFile = new(
		"Save Left File", nameof(SaveLeftFile), typeof(Commands)
	);

	public static readonly RoutedUICommand SaveRightFile = new(
		"Save Right File", nameof(SaveRightFile), typeof(Commands)
	);

	public static readonly RoutedUICommand Edit = new(
		"Enable Editing", nameof(Edit), typeof(Commands)
	);

	public static readonly RoutedUICommand Swap = new(
		"Swap", nameof(Swap), typeof(Commands)
	);

	public static readonly RoutedUICommand Up = new(
		"Go to Parent Folder", nameof(Up), typeof(Commands)
	);

	public static readonly RoutedUICommand CopyLeftDiff = new(
		"Copy Left Diff Right", nameof(CopyLeftDiff), typeof(Commands),
		[new KeyGesture(Key.Right, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand CopyRightDiff = new(
		"Copy Right Diff Left", nameof(CopyRightDiff), typeof(Commands),
		[new KeyGesture(Key.Left, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand PreviousDiff = new(
		"Move to Previous Diff", nameof(PreviousDiff), typeof(Commands),
		[new KeyGesture(Key.Up)]
	);

	public static readonly RoutedUICommand NextDiff = new(
		"Move to Next Diff", nameof(NextDiff), typeof(Commands),
		[new KeyGesture(Key.Down)]
	);

	public static readonly RoutedUICommand FirstDiff = new(
		"Move to First Diff", nameof(FirstDiff), typeof(Commands),
		[new KeyGesture(Key.Up, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand CurrentDiff = new(
		"Move to Current Diff", nameof(CurrentDiff), typeof(Commands),
		[new KeyGesture(Key.Space, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand LastDiff = new(
		"Move to Last Diff", nameof(LastDiff), typeof(Commands),
		[new KeyGesture(Key.Down, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand NextFile = new(
		"Select Next Changed File", nameof(NextFile), typeof(Commands),
		[new KeyGesture(Key.PageDown, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand PreviousFile = new(
		"Select Previous Changed File", nameof(PreviousFile), typeof(Commands),
		[new KeyGesture(Key.PageUp, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand BrowseLeft = new(
		"Browse Left", nameof(BrowseLeft), typeof(Commands),
		[new KeyGesture(Key.D1, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand BrowseRight = new(
		"Browse Right", nameof(BrowseRight), typeof(Commands),
		[new KeyGesture(Key.D2, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand Options = new(
		"Options", nameof(Options), typeof(Commands)
	);

	public static readonly RoutedUICommand Find = new(
		"Find", nameof(Find), typeof(Commands),
		[new KeyGesture(Key.F, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand FindNext = new(
		"Find Next", nameof(FindNext), typeof(Commands),
		[new KeyGesture(Key.G, ModifierKeys.Control), new KeyGesture(Key.F3)]
	);

	public static readonly RoutedUICommand FindPrevious = new(
		"Find Previous", nameof(FindPrevious), typeof(Commands),
		[new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift)]
	);

	public static readonly RoutedUICommand CloseFind = new(
		"Close Find", nameof(CloseFind), typeof(Commands),
		[new KeyGesture(Key.Escape)]
	);

	public static readonly RoutedUICommand ZoomIn = new(
		"Zoom In", nameof(ZoomIn), typeof(Commands),
		[new KeyGesture(Key.OemPlus, ModifierKeys.Control, "Ctrl++")]
	);

	public static readonly RoutedUICommand ZoomOut = new(
		"Zoom Out", nameof(ZoomOut), typeof(Commands),
		[new KeyGesture(Key.OemMinus, ModifierKeys.Control, "Ctrl+-")]
	);

	public static readonly RoutedUICommand ResetZoom = new(
		"Reset Zoom", nameof(ResetZoom), typeof(Commands),
		[new KeyGesture(Key.D0, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand OpenContainingFolder = new(
		"Open Containing Folder", nameof(OpenContainingFolder), typeof(Commands)
	);

	public static readonly RoutedUICommand CopyPathToClipboard = new(
		"Copy Path to Clipboard", nameof(CopyPathToClipboard), typeof(Commands)
	);

}
