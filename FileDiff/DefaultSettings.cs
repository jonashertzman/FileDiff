namespace FileDiff;

public static class DefaultSettings
{

	internal static string Font { get; } = "Courier New";
	internal static int FontSize { get; } = 11;
	internal static int TabSize { get; } = 2;

	internal static ThemeColors DarkTheme { get; } = new ThemeColors()
	{
		// Diff colors
		FullMatchForeground = "#FFFFFFFF",
		FullMatchBackground = "#1B1B1B",

		PartialMatchForeground = "#FF9698FF",
		PartialMatchBackground = "#FF2B293A",

		DeletedForeground = "#FFFFDCDC",
		DeletedBackground = "#FF820000",

		NewForeground = "#FF81FF72",
		NewBackground = "#FF006900",

		IgnoredForeground = "#FFDCDCDC",
		IgnoredBackground = "#FF5A5A5A",

		MovedFromdBackground = "#FF420000",

		MovedToBackground = "#FF002800",

		SelectionBackground = "#320096D2",

		// GUI colors
		WindowForeground = "#FFFFFFFF",
		WindowBackground = "#FF1F1F1F",

		DialogBackground = "#FF1F1F1F",

		ControlBackground = "#FF3D3D3D",
		ControlDarkBackground = "#FFACACAC",

		HighlightBackground = "#213E4C",
		HighlightBorder = "#26A0DA",

		BorderForeground = "#FF424242",

		LineNumberColor = "#FF585858",
		CurrentDiffColor = "#FFB7B7B7",
		SnakeColor = "#FFD2D2D2",
	};

	internal static ThemeColors LightTheme { get; } = new ThemeColors()
	{
		FullMatchForeground = "#FF000000",
		FullMatchBackground = "#FFFFFFFF",

		PartialMatchForeground = "#FF000000",
		PartialMatchBackground = "#FFDCDCFF",

		DeletedForeground = "#FFC80000",
		DeletedBackground = "#FFFFDCDC",

		NewForeground = "#FF007800",
		NewBackground = "#FFDCFFDC",

		IgnoredForeground = "#FF5A5A5A",
		IgnoredBackground = "#FFDCDCDC",

		MovedFromdBackground = "#FFFFF0F0",

		MovedToBackground = "#FFF0FFF0",

		SelectionBackground = "#320096D2",

		WindowForeground = "#FF000000",
		WindowBackground = "#FFFFFFFF",

		DialogBackground = "#FFF0F0F0",

		ControlBackground = "#FFDDDDDD",
		ControlDarkBackground = "#FFACACAC",

		HighlightBackground = "#DCECFC",
		HighlightBorder = "#7EB4EA",

		BorderForeground = "#FFD6D6D6",

		LineNumberColor = "#FF585858",
		CurrentDiffColor = "#FFB7B7B7",
		SnakeColor = "#FFD2D2D2",
	};

}
