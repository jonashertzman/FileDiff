using System.Windows.Media;

namespace FileDiff;

public static class DefaultSettings
{

	internal static string Font { get; } = "Courier New";
	internal static int FontSize { get; } = 11;
	internal static int TabSize { get; } = 2;

	internal static ThemeColors DarkTheme { get; } = new ThemeColors()
	{
		FullMatchForeground = Colors.White.ToString(),
		FullMatchBackground = Colors.Black.ToString(),

		PartialMatchForeground = Colors.Black.ToString(),
		PartialMatchBackground = Color.FromRgb(220, 220, 255).ToString(),

		DeletedForeground = Color.FromRgb(200, 0, 0).ToString(),
		DeletedBackground = Color.FromRgb(255, 220, 220).ToString(),

		NewForeground = Color.FromRgb(0, 120, 0).ToString(),
		NewBackground = Color.FromRgb(220, 255, 220).ToString(),

		IgnoredForeground = Color.FromRgb(90, 90, 90).ToString(),
		IgnoredBackground = Color.FromRgb(220, 220, 220).ToString(),

		MovedFromdBackground = Color.FromRgb(255, 240, 240).ToString(),

		MovedToBackground = Color.FromRgb(240, 255, 240).ToString(),

		SelectionBackground = Color.FromArgb(50, 0, 150, 210).ToString(),

		WindowForeground = Colors.Cyan.ToString(),
		WindowBackground = Colors.Red.ToString(),
		ControlBackground = Colors.Gray.ToString(),
		BorderColor = Colors.Blue.ToString(),
		LineNumberColor = Color.FromRgb(88, 88, 88).ToString(),
		CurrentDiffColor = Color.FromRgb(183, 183, 183).ToString(),
		SnakeColor = Color.FromRgb(210, 210, 210).ToString(),
	};

	internal static ThemeColors LightTheme { get; } = new ThemeColors()
	{
		FullMatchForeground = Colors.Black.ToString(),
		FullMatchBackground = Colors.White.ToString(),

		PartialMatchForeground = Colors.Black.ToString(),
		PartialMatchBackground = Color.FromRgb(220, 220, 255).ToString(),

		DeletedForeground = Color.FromRgb(200, 0, 0).ToString(),
		DeletedBackground = Color.FromRgb(255, 220, 220).ToString(),

		NewForeground = Color.FromRgb(0, 120, 0).ToString(),
		NewBackground = Color.FromRgb(220, 255, 220).ToString(),

		IgnoredForeground = Color.FromRgb(90, 90, 90).ToString(),
		IgnoredBackground = Color.FromRgb(220, 220, 220).ToString(),

		MovedFromdBackground = Color.FromRgb(255, 240, 240).ToString(),

		MovedToBackground = Color.FromRgb(240, 255, 240).ToString(),

		SelectionBackground = Color.FromArgb(50, 0, 150, 210).ToString(),

		WindowForeground = Colors.Cyan.ToString(),
		WindowBackground = Colors.Red.ToString(),
		ControlBackground = Colors.Gray.ToString(),
		BorderColor = Colors.Blue.ToString(),
		LineNumberColor = Color.FromRgb(88, 88, 88).ToString(),
		CurrentDiffColor = Color.FromRgb(183, 183, 183).ToString(),
		SnakeColor = Color.FromRgb(210, 210, 210).ToString(),
	};

}
