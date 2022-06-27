﻿using System.Windows.Media;

namespace FileDiff;

public static class DefaultSettings
{

	internal static string Font { get; } = "Courier New";
	internal static int FontSize { get; } = 11;
	internal static int TabSize { get; } = 2;

	internal static ThemeColors DarkTheme { get;  } = new ThemeColors()
	{
		FullMatchForeground = Colors.Black,
		FullMatchBackground = Colors.White,

		PartialMatchForeground = Colors.Black,
		PartialMatchBackground = Color.FromRgb(220, 220, 255),

		DeletedForeground = Color.FromRgb(200, 0, 0),
		DeletedBackground = Color.FromRgb(255, 220, 220),

		NewForeground = Color.FromRgb(0, 120, 0),
		NewBackground = Color.FromRgb(220, 255, 220),

		IgnoredForeground = Color.FromRgb(90, 90, 90),
		IgnoredBackground = Color.FromRgb(220, 220, 220),

		MovedFromdBackground = Color.FromRgb(255, 240, 240),

		MovedToBackground = Color.FromRgb(240, 255, 240),

		SelectionBackground = Color.FromArgb(50, 0, 150, 210),

		LineNumberColor = Color.FromRgb(88, 88, 88),
		CurrentDiffColor = Color.FromRgb(183, 183, 183),
		SnakeColor = Color.FromRgb(210, 210, 210),
	};

	internal static ThemeColors LightTheme { get; } = new ThemeColors()
	{
		FullMatchForeground = Colors.Black,
		FullMatchBackground = Colors.White,

		PartialMatchForeground = Colors.Black,
		PartialMatchBackground = Color.FromRgb(220, 220, 255),

		DeletedForeground = Color.FromRgb(200, 0, 0),
		DeletedBackground = Color.FromRgb(255, 220, 220),

		NewForeground = Color.FromRgb(0, 120, 0),
		NewBackground = Color.FromRgb(220, 255, 220),

		IgnoredForeground = Color.FromRgb(90, 90, 90),
		IgnoredBackground = Color.FromRgb(220, 220, 220),

		MovedFromdBackground = Color.FromRgb(255, 240, 240),

		MovedToBackground = Color.FromRgb(240, 255, 240),

		SelectionBackground = Color.FromArgb(50, 0, 150, 210),

		LineNumberColor = Color.FromRgb(88, 88, 88),
		CurrentDiffColor = Color.FromRgb(183, 183, 183),
		SnakeColor = Color.FromRgb(210, 210, 210),
	}; 

}
