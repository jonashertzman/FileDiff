using System.Reflection;

namespace FileDiff;

public class ColorTheme
{

	#region Properties

	public required string FullMatchForeground { get; set; }
	public required string FullMatchBackground { get; set; }

	public required string PartialMatchForeground { get; set; }
	public required string PartialMatchBackground { get; set; }

	public required string DeletedForeground { get; set; }
	public required string DeletedBackground { get; set; }

	public required string NewForeground { get; set; }
	public required string NewBackground { get; set; }

	public required string IgnoredForeground { get; set; }
	public required string IgnoredBackground { get; set; }

	public required string MovedToBackground { get; set; }

	public required string MovedFromBackground { get; set; }

	public required string SelectionBackground { get; set; }

	public required string WindowForeground { get; set; }
	public required string DisabledForeground { get; set; }
	public required string WindowBackground { get; set; }

	public required string DialogBackground { get; set; }

	public required string ControlBackground { get; set; }
	public required string ControlDarkBackground { get; set; }

	public required string HighlightBackground { get; set; }
	public required string HighlightBorder { get; set; }

	public required string BorderForeground { get; set; }
	public required string LineNumberColor { get; set; }
	public required string CurrentDiffColor { get; set; }
	public required string SnakeColor { get; set; }

	#endregion

	#region Methods

	public ColorTheme Clone()
	{
		return (ColorTheme)MemberwiseClone();
	}

	internal void SetDefaultsIfNull(ColorTheme defaultTheme)
	{
		foreach (PropertyInfo propertyInfo in this.GetType().GetProperties())
		{
			if (propertyInfo.GetValue(this) == null)
			{
				propertyInfo.SetValue(this, propertyInfo.GetValue(defaultTheme));
			}
		}
	}

	#endregion

}
