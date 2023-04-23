using System.Reflection;

namespace FileDiff;

public class ColorTheme
{

	#region Properties

	// Folder diff colors
	public required string FolderFullMatchForeground { get; set; }
	public required string FolderFullMatchBackground { get; set; }

	public required string FolderPartialMatchForeground { get; set; }
	public required string FolderPartialMatchBackground { get; set; }

	public required string FolderDeletedForeground { get; set; }
	public required string FolderDeletedBackground { get; set; }

	public required string FolderNewForeground { get; set; }
	public required string FolderNewBackground { get; set; }

	public required string FolderIgnoredForeground { get; set; }
	public required string FolderIgnoredBackground { get; set; }

	// File diff colors
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

	public required string MovedFromBackground { get; set; }
	public required string MovedToBackground { get; set; }

	// Editor colors
	public required string LineNumberColor { get; set; }
	public required string CurrentDiffColor { get; set; }
	public required string SnakeColor { get; set; }
	public required string SelectionBackground { get; set; }

	// GUI colors
	public required string NormalText { get; set; }
	public required string DisabledText { get; set; }

	public required string WindowBackground { get; set; }
	public required string DialogBackground { get; set; }

	public required string ControlLightBackground { get; set; }
	public required string ControlDarkBackground { get; set; }

	public required string BorderLight { get; set; }
	public required string BorderDark { get; set; }

	public required string HighlightBackground { get; set; }
	public required string HighlightBorder { get; set; }


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

	internal void SetDefaults(ColorTheme defaultTheme)
	{
		foreach (PropertyInfo propertyInfo in this.GetType().GetProperties())
		{
			propertyInfo.SetValue(this, propertyInfo.GetValue(defaultTheme));
		}
	}

	#endregion

}
