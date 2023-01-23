﻿using System.Reflection;

namespace FileDiff;

public class ColorTheme
{

	#region Properties

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

	public string MovedFromBackground { get; set; }

	public string SelectionBackground { get; set; }

	public string WindowForeground { get; set; }
	public string DisabledForeground { get; set; }
	public string WindowBackground { get; set; }

	public string DialogBackground { get; set; }

	public string ControlBackground { get; set; }
	public string ControlDarkBackground { get; set; }

	public string HighlightBackground { get; set; }
	public string HighlightBorder { get; set; }

	public string BorderForeground { get; set; }
	public string LineNumberColor { get; set; }
	public string CurrentDiffColor { get; set; }
	public string SnakeColor { get; set; }

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
