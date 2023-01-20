namespace FileDiff;

public static class DefaultSettings
{

	internal static string Font { get; } = "Courier New";
	internal static int FontSize { get; } = 11;
	internal static int TabSize { get; } = 2;

	internal static ColorTheme DarkTheme { get; } = new ColorTheme()
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

		IgnoredForeground = "#FF7E7E7E",
		IgnoredBackground = "#FF212121",

		MovedFromBackground = "#FF420000",

		MovedToBackground = "#FF002800",

		SelectionBackground = "#320096D2",

		// GUI colors
		WindowForeground = "#FFFFFFFF",
		DisabledForeground = "#FF888888",
		WindowBackground = "#FF0B0B0B",

		DialogBackground = "#FF171717",

		ControlBackground = "#FF3D3D3D",
		ControlDarkBackground = "#FFACACAC",

		HighlightBackground = "#213E4C",
		HighlightBorder = "#26A0DA",

		BorderForeground = "#FF303030",

		LineNumberColor = "#FF585858",
		CurrentDiffColor = "#FFB7B7B7",
		SnakeColor = "#FFD2D2D2",
	};

	internal static ColorTheme LightTheme { get; } = new ColorTheme()
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

		MovedFromBackground = "#FFFFF0F0",

		MovedToBackground = "#FFF0FFF0",

		SelectionBackground = "#320096D2",

		// GUI colors
		WindowForeground = "#FF000000",
		DisabledForeground = "#FF888888",
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
