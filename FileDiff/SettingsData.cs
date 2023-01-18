using System.Collections.ObjectModel;
using System.Windows;

namespace FileDiff;

public class SettingsData
{

	public string Id { get; set; } = Guid.NewGuid().ToString();

	public DateTime LastUpdateTime { get; set; } = DateTime.MinValue;

	public bool CheckForUpdates { get; set; } = true;

	public bool DetectMovedLines { get; set; } = true;

	public bool IgnoreWhiteSpace { get; set; } = true;

	public bool ShowWhiteSpaceCharacters { get; set; } = false;

	public bool ShowLineChanges { get; set; } = true;

	public bool MasterDetail { get; set; } = false;

	// Minimum character length of substrings used when looking for similarities between two lines.
	public int CharacterMatchThreshold { get; set; } = 4;

	// The percentage of matching characters two lines must have in common to be considered partially matched lines.
	public float LineSimilarityThreshold { get; set; } = 0.4f;

	public string Font { get; set; } = DefaultSettings.Font;
	public int FontSize { get; set; } = DefaultSettings.FontSize;
	public int Zoom { get; set; } = 0;
	public int TabSize { get; set; } = DefaultSettings.TabSize;

	public Themes Theme { get; set; } = Themes.Light;
	public ColorTheme DarkTheme { get; set; } = DefaultSettings.DarkTheme.Clone();
	public ColorTheme LightTheme { get; set; } = DefaultSettings.LightTheme.Clone();

	public double PositionLeft { get; set; }
	public double PositionTop { get; set; }
	public double Width { get; set; } = 700;
	public double Height { get; set; } = 500;
	public double FolderRowHeight { get; set; } = 300;
	public WindowState WindowState { get; set; }

	public ObservableCollection<TextAttribute> IgnoredFolders { get; set; } = new ObservableCollection<TextAttribute>();

	public ObservableCollection<TextAttribute> IgnoredFiles { get; set; } = new ObservableCollection<TextAttribute>();

	[System.Runtime.Serialization.OnDeserialized]
	void OnDeserialized(System.Runtime.Serialization.StreamingContext c)
	{
		DarkTheme = DefaultSettings.DarkTheme.Clone();
		LightTheme = DefaultSettings.LightTheme.Clone();
	}

}
