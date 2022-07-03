namespace FileDiff;

public class ThemeColors
{

	public string FullMatchForeground { get; set; }
	public string FullMatchBackground { get; set; }

	public string PartialMatchForeground { get; set; }
	public string PartialMatchBackground { get; set; }

	public string DeletedForeground { get; set; }
	public string DeletedBackground { get; set; }

	public string NewForeground { get; set; }
	public string NewBackground { get; set; }

	public string IgnoredForeground { get; set; }
	public string IgnoredBackground { get; set; }

	public string MovedToBackground { get; set; }

	public string MovedFromdBackground { get; set; }

	public string SelectionBackground { get; set; }

	public string WindowColor { get; set; }
	public string LineNumberColor { get; set; }
	public string CurrentDiffColor { get; set; }
	public string SnakeColor { get; set; }

	public ThemeColors Clone()
	{
		return (ThemeColors)MemberwiseClone();
	}

}
