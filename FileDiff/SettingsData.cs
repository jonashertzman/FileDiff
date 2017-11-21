using System.Windows;
using System.Windows.Media;

namespace FileDiff
{

	public class SettingsData
	{

		public bool IgnoreWhiteSpace { get; set; } = true;

		public bool ShowLineChanges { get; set; } = true;

		// Minimum character length of substrings used when looking for similarities between two lines.
		public int CharacterMatchThreshold { get; set; } = 4;

		// The percentage of matching characters two lines must have in common to be considered partially matched lines.
		public float LineSimilarityThreshold { get; set; } = 0.4f;

		public Color FullMatchForeground { get; set; } = AppSettings.DefaultFullMatchForeground;
		public Color FullMatchBackground { get; set; } = AppSettings.DefaultFullMatchBackground;

		public Color PartialMatchForeground { get; set; } = AppSettings.DefaultPartialMatchForeground;
		public Color PartialMatchBackground { get; set; } = AppSettings.DefaultPartialMatchBackground;

		public Color DeletedForeground { get; set; } = AppSettings.DefaultDeletedForeground;
		public Color DeletedBackground { get; set; } = AppSettings.DefaultDeletedBackground;

		public Color NewForeground { get; set; } = AppSettings.DefaultNewForeground;
		public Color NewBackground { get; set; } = AppSettings.DefaultNewBackground;

		public string Font { get; set; } = AppSettings.DefaultFont;
		public int FontSize { get; set; } = AppSettings.DefaultFontSize;

		public double PositionLeft { get; set; }
		public double PositionTop { get; set; }
		public double Width { get; set; } = 700;
		public double Height { get; set; } = 500;
		public WindowState WindowState { get; set; }

	}
}
