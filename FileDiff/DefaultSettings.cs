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
		FolderFullMatchBackground = "#1B1B1B",

		FolderPartialMatchForeground = "#FF9698FF",
		FolderPartialMatchBackground = "#FF2B293A",

		FolderDeletedForeground = "#FFFF8387",
		FolderDeletedBackground = "#FF5C2626",

		FolderNewForeground = "#FF51C153",
		FolderNewBackground = "#FF264D26",

		FolderIgnoredForeground = "#FF7E7E7E",
		FolderIgnoredBackground = "#FF212121",

		// File diff colors
		FullMatchForeground = "#FFD8D8D8",
		FullMatchBackground = "#1B1B1B",

		PartialMatchForeground = "#FF9698FF",
		PartialMatchBackground = "#FF2B293A",

		DeletedForeground = "#FFFF8387",
		DeletedBackground = "#FF5C2626",

		NewForeground = "#FF51C153",
		NewBackground = "#FF264D26",

		IgnoredForeground = "#FF7E7E7E",
		IgnoredBackground = "#FF212121",

		MovedFromBackground = "#FF2D1B1B",
		MovedToBackground = "#FF1B261B",

		// Editor colors
		LineNumberColor = "#FF797979",
		CurrentDiffColor = "#FF252525",
		SnakeColor = "#FF383838",
		SelectionBackground = "#320096D2",

		// UI colors
		NormalText = "#FFD8D8D8",
		DisabledText = "#FF888888",

		WindowBackground = "#FF0B0B0B",
		DialogBackground = "#FF171717",

		ControlLightBackground = "#FF262626",
		ControlDarkBackground = "#FF3F3F3F",

		BorderLight = "#FF323232",
		BorderDark = "#FF595959",

		HighlightBackground = "#FF112E3C",
		HighlightBorder = "#2F7999",

		AttentionBackground = "#FF5C2626",
	};

	internal static ColorTheme LightTheme { get; } = new ColorTheme()
	{
		// Folder diff colors
		FolderFullMatchForeground = "#FF000000",
		FolderFullMatchBackground = "#FFFFFFFF",

		FolderPartialMatchForeground = "#FF000000",
		FolderPartialMatchBackground = "#FFDCDCFF",

		FolderDeletedForeground = "#FFC80000",
		FolderDeletedBackground = "#FFFFDCDC",

		FolderNewForeground = "#FF007800",
		FolderNewBackground = "#FFDCFFDC",

		FolderIgnoredForeground = "#FF5A5A5A",
		FolderIgnoredBackground = "#FFDCDCDC",

		// File diff colors
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

		// Editor colors
		LineNumberColor = "#FF585858",
		CurrentDiffColor = "#FFB7B7B7",
		SnakeColor = "#FFD2D2D2",
		SelectionBackground = "#320096D2",

		// UI colors
		NormalText = "#FF000000",
		DisabledText = "#FF888888",

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
