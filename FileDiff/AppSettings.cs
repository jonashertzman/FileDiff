using System.Windows.Media;

namespace FileDiff
{
	public static class AppSettings
	{

		#region Defaults 

		internal static Color DefaultFullMatchForeground = Colors.Black;
		internal static Color DefaultFullMatchBackground = Colors.White;

		internal static Color DefaultPartialMatchForeground = Colors.Black;
		internal static Color DefaultPartialMatchBackground = Color.FromRgb(220, 220, 255);

		internal static Color DefaultDeletedForeground = Color.FromRgb(200, 0, 0);
		internal static Color DefaultDeletedBackground = Color.FromRgb(255, 220, 220);

		internal static Color DefaultNewForeground = Color.FromRgb(0, 120, 0);
		internal static Color DefaultNewBackground = Color.FromRgb(220, 255, 220);

		internal const string DefaultFont = "Courier New";
		internal const int DefaultFontSize = 12;

		#endregion

		public static SettingsData Settings { get; set; }

	}
}
