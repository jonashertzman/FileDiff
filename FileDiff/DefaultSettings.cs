using System.Windows.Media;

namespace FileDiff
{
	public static class DefaultSettings
	{

		internal static Color FullMatchForeground { get; } = Colors.Black;
		internal static Color FullMatchBackground { get; } = Colors.White;

		internal static Color PartialMatchForeground { get; } = Colors.Black;
		internal static Color PartialMatchBackground { get; } = Color.FromRgb(220, 220, 255);

		internal static Color DeletedForeground { get; } = Color.FromRgb(200, 0, 0);
		internal static Color DeletedBackground { get; } = Color.FromRgb(255, 220, 220);

		internal static Color NewForeground { get; } = Color.FromRgb(0, 120, 0);
		internal static Color NewBackground { get; } = Color.FromRgb(220, 255, 220);

		internal static Color IgnoredForeground { get; } = Color.FromRgb(90, 90, 90);
		internal static Color IgnoredBackground { get; } = Color.FromRgb(220, 220, 220);

		internal static Color SelectionBackground { get; } = Color.FromArgb(50, 0, 150, 210);

		internal static string Font { get; } = "Courier New";
		internal static int FontSize { get; } = 11;
		internal static int TabSize { get; } = 2;

	}
}
