namespace FileDiff;

public static class DefaultSettings
{

	internal static string Font { get; } = "Courier New";
	internal static int FontSize { get; } = 11;
	internal static int TabSize { get; } = 2;

	internal static ColorTheme DarkTheme { get; } = new ColorTheme()
	{
		// Diff colors
		FullMatchForeground = "#FFD8D8D8",
		FullMatchBackground = "#1B1B1B",

		PartialMatchForeground = "#FF9698FF",
		PartialMatchBackground = "#FF2B293A",

		DeletedForeground = "#FFFF8387",
		DeletedBackground = "#FF552F2D",

		NewForeground = "#FF51C153",
		NewBackground = "#FF153604",

		IgnoredForeground = "#FF7E7E7E",
		IgnoredBackground = "#FF212121",

		MovedFromBackground = "#FF332F2D",
		MovedToBackground = "#FF152704",

		LineNumberColor = "#FF797979",
		CurrentDiffColor = "#FF252525",
		SnakeColor = "#FF383838",

		SelectionBackground = "#320096D2",

		// GUI colors
		NormalText = "#FFD8D8D8",
		DisabledText = "#FF888888",

		WindowBackground = "#FF0B0B0B",
		DialogBackground = "#FF171717",

		ControlLightBackground = "#FF262626",
		ControlDarkBackground = "#FF3F3F3F",

		BorderLight = "#FF323232",
		BorderDark = "#FF595959",

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

		ControlLightBackground = "#FFFFFFFF",
		ControlDarkBackground = "#FFDDDDDD",

		BorderLight = "#FFD8D8D8",
		BorderDark = "#FFAAAAAA",

		HighlightBackground = "#FFDCECFC",
		HighlightBorder = "#FF7EB4EA",
	};

}
