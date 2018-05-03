using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace FileDiff
{
	public class SettingsData
	{

		public bool IgnoreWhiteSpace { get; set; } = true;

		public bool ShowWhiteSpaceCharacters { get; set; } = false;

		public bool ShowLineChanges { get; set; } = true;

		public bool MasterDetail { get; set; } = false;

		// Minimum character length of substrings used when looking for similarities between two lines.
		public int CharacterMatchThreshold { get; set; } = 4;

		// The percentage of matching characters two lines must have in common to be considered partially matched lines.
		public float LineSimilarityThreshold { get; set; } = 0.4f;

		public Color FullMatchForeground { get; set; } = DefaultSettings.FullMatchForeground;
		public Color FullMatchBackground { get; set; } = DefaultSettings.FullMatchBackground;

		public Color PartialMatchForeground { get; set; } = DefaultSettings.PartialMatchForeground;
		public Color PartialMatchBackground { get; set; } = DefaultSettings.PartialMatchBackground;

		public Color DeletedForeground { get; set; } = DefaultSettings.DeletedForeground;
		public Color DeletedBackground { get; set; } = DefaultSettings.DeletedBackground;

		public Color NewForeground { get; set; } = DefaultSettings.NewForeground;
		public Color NewBackground { get; set; } = DefaultSettings.NewBackground;

		public Color IgnoredForeground { get; set; } = DefaultSettings.IgnoredForeground;
		public Color IgnoredBackground { get; set; } = DefaultSettings.IgnoredBackground;

		public string Font { get; set; } = DefaultSettings.Font;
		public int FontSize { get; set; } = DefaultSettings.FontSize;
		public int TabSize { get; set; } = DefaultSettings.TabSize;

		public double PositionLeft { get; set; }
		public double PositionTop { get; set; }
		public double Width { get; set; } = 700;
		public double Height { get; set; } = 500;
		public double FolderRowHeight { get; set; } = 300;
		public WindowState WindowState { get; set; }

		public ObservableCollection<TextAttribute> IgnoredFolders { get; set; } = new ObservableCollection<TextAttribute>();

	}
}

