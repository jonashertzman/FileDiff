using System.Windows.Media;

namespace FileDiff;

public class ThemeColors
{
	public ThemeColors Default;

	public Color FullMatchForeground { get; set; } 
	public Color FullMatchBackground { get; set; } 

	public Color PartialMatchForeground { get; set; } 
	public Color PartialMatchBackground { get; set; }

	public Color DeletedForeground { get; set; } 
	public Color DeletedBackground { get; set; } 

	public Color NewForeground { get; set; } 
	public Color NewBackground { get; set; } 

	public Color IgnoredForeground { get; set; }
	public Color IgnoredBackground { get; set; }

	public Color MovedToBackground { get; set; }

	public Color MovedFromdBackground { get; set; } 

	public Color SelectionBackground { get; set; } 

	public Color LineNumberColor { get; set; } 
	public Color CurrentDiffColor { get; set; }
	public Color SnakeColor { get; set; } 

	public ThemeColors Clone()
	{
		ThemeColors clone = (ThemeColors)MemberwiseClone();
		clone.Default = this;
		return clone;
	}

}
