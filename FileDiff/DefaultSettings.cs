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

		LineNumberColor = "#FF585858",
		CurrentDiffColor = "#FFB7B7B7",
		SnakeColor = "#FFD2D2D2",

		SelectionBackground = "#320096D2",

		// GUI colors
		NormalText = "#FFFFFFFF",
		DisabledText = "#FF888888",

		WindowBackground = "#FF0B0B0B",
		DialogBackground = "#FF171717",

		LightControlBackground = "#FFACACAC",
		DarkControlBackground = "#FF3D3D3D",

		BorderLight = "#FF303030",
		BorderDark = "#FF6A6A6A",

		HighlightBackground = "#213E4C",
		HighlightBorder = "#26A0DA",
	};

	internal static ColorTheme LightTheme { get; } = new ColorTheme()
	{
		// Diff colors
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

		LineNumberColor = "#FF585858",
		CurrentDiffColor = "#FFB7B7B7",
		SnakeColor = "#FFD2D2D2",

		SelectionBackground = "#320096D2",

		// GUI colors
		NormalText = "#FF000000",
		DisabledText = "#FF888888",

		WindowBackground = "#FFFFFFFF",
		DialogBackground = "#FFF0F0F0",

		LightControlBackground = "#FFACACAC",
		DarkControlBackground = "#FFDDDDDD",

		BorderLight = "#FFD6D6D6",
		BorderDark = "#FFA9A9A9",

		HighlightBackground = "#DCECFC",
		HighlightBorder = "#7EB4EA",
	};

}
