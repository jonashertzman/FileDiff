using System.Windows.Media;

namespace FileDiff;

public class ThemeColors
{

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

	public Color MovedToBackground { get; set; } = DefaultSettings.MovedToBackground;

	public Color MovedFromdBackground { get; set; } = DefaultSettings.MovedFromdBackground;

	public Color SelectionBackground { get; set; } = DefaultSettings.SelectionBackground;

	public Color LineNumberColor { get; set; } = DefaultSettings.LineNumberColor;
	public Color CurrentDiffColor { get; set; } = DefaultSettings.CurrentDiffColor;
	public Color SnakeColor { get; set; } = DefaultSettings.SnakeColor;

}
