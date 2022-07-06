namespace FileDiff;

public static class DefaultSettings
{

	internal static string Font { get; } = "Courier New";
	internal static int FontSize { get; } = 11;
	internal static int TabSize { get; } = 2;

	internal static ThemeColors DarkTheme { get; } = new ThemeColors()
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
		ControlBackground = "#FFEDEDED",
		BorderColor = "#FFCECECE",

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
		ControlBackground = "#FFEDEDED",
		BorderColor = "#FFCECECE",

		LineNumberColor = "#FF585858",
		CurrentDiffColor = "#FFB7B7B7",
		SnakeColor = "#FFD2D2D2",
	};

}
