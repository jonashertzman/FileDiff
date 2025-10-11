namespace FileDiff;

public static class DefaultSettings
{

	internal static string Font { get; } = "Courier New";
	internal static int FontSize { get; } = 11;
	internal static int TabSize { get; } = 2;

	internal static ColorTheme DarkTheme { get; } = new ColorTheme()
	{
		// Folder diff colors
		FolderFullMatchForeground = "#FFD8D8D8",
		FolderFullMatchBackground = "#FF1B1B1B",

		FolderNewForeground = "#FF51C153",
		FolderNewBackground = "#FF264D26",

		FolderDeletedForeground = "#FFFF8387",
		FolderDeletedBackground = "#FF5C2626",

		FolderPartialMatchForeground = "#FF9698FF",
		FolderPartialMatchBackground = "#FF2B293A",

		FolderIgnoredForeground = "#FF7E7E7E",
		FolderIgnoredBackground = "#FF212121",

		// File diff colors
		FullMatchForeground = "#FFD8D8D8",
		FullMatchBackground = "#FF1B1B1B",

		NewForeground = "#FF51C153",
		NewBackground = "#FF264D26",

		MovedToBackground = "#FF1B261B",

		DeletedForeground = "#FFFF8387",
		DeletedBackground = "#FF5C2626",

		MovedFromBackground = "#FF2D1B1B",

		PartialMatchForeground = "#FF9698FF",
		PartialMatchBackground = "#FF2B293A",

		IgnoredForeground = "#FF7E7E7E",
		IgnoredBackground = "#FF212121",

		// Editor colors
		WhiteSpaceForeground = "#FF3871A7",
		SelectionBackground = "#320096D2",
		LineNumberColor = "#FF797979",
		CurrentDiffColor = "#FF252525",
		SnakeColor = "#FF383838",

		// UI colors
		NormalText = "#FFD8D8D8",
		DisabledText = "#FF888888",
		DisabledBackground = "#FF444444",

		WindowBackground = "#FF0B0B0B",
		DialogBackground = "#FF171717",

		ControlLightBackground = "#FF262626",
		ControlDarkBackground = "#FF3F3F3F",

		BorderLight = "#FF323232",
		BorderDark = "#FF595959",

		HighlightBackground = "#FF112E3C",
		HighlightBorder = "#FF2F7999",

		AttentionBackground = "#FF5C2626",
	};

	internal static ColorTheme LightTheme { get; } = new ColorTheme()
	{
		// Folder diff colors
		FolderFullMatchForeground = "#FF000000",
		FolderFullMatchBackground = "#FFFFFFFF",

		FolderNewForeground = "#FF007800",
		FolderNewBackground = "#FFDCFFDC",

		FolderDeletedForeground = "#FFC80000",
		FolderDeletedBackground = "#FFFFDCDC",

		FolderPartialMatchForeground = "#FF000000",
		FolderPartialMatchBackground = "#FFDCDCFF",

		FolderIgnoredForeground = "#FF5A5A5A",
		FolderIgnoredBackground = "#FFDCDCDC",

		// File diff colors
		FullMatchForeground = "#FF000000",
		FullMatchBackground = "#FFFFFFFF",

		NewForeground = "#FF007800",
		NewBackground = "#FFDCFFDC",

		MovedToBackground = "#FFF0FFF0",

		DeletedForeground = "#FFC80000",
		DeletedBackground = "#FFFFDCDC",

		MovedFromBackground = "#FFFFF0F0",

		PartialMatchForeground = "#FF000000",
		PartialMatchBackground = "#FFDCDCFF",

		IgnoredForeground = "#FF5A5A5A",
		IgnoredBackground = "#FFDCDCDC",

		// Editor colors
		WhiteSpaceForeground = "#FF2E8CB5",
		SelectionBackground = "#320096D2",
		LineNumberColor = "#FF585858",
		CurrentDiffColor = "#FFB7B7B7",
		SnakeColor = "#FFD2D2D2",

		// UI colors
		NormalText = "#FF000000",
		DisabledText = "#FF888888",
		DisabledBackground = "#FFAAAAAA",

		WindowBackground = "#FFFFFFFF",
		DialogBackground = "#FFEBEBEB",

		ControlLightBackground = "#FFFFFFFF",
		ControlDarkBackground = "#FFD9D9D9",

		BorderLight = "#FFCFCFCF",
		BorderDark = "#FFAAAAAA",

		HighlightBackground = "#FFDCECFC",
		HighlightBorder = "#FF7EB4EA",

		AttentionBackground = "#FFFF9F9D",
	};

}
