using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace FileDiff;

public static class AppSettings
{

	#region Members

	private static readonly SettingsData Settings = new();

	private static readonly string AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FileDiff");
	public static readonly string SettingsPath = Path.Combine(AppDataDirectory, "Settings.xml");
	public static readonly string LogPath = Path.Combine(AppDataDirectory, "FileDiff.log");

	#endregion

	#region Properies

	public static string Id
	{
		get { return Settings.Id; }
	}

	public static string BuildNumber
	{
		get
		{
			DateTime buildDate = new FileInfo(Environment.ProcessPath).LastWriteTime;
			return $"{buildDate:yy}{buildDate.DayOfYear:D3}";
		}
	}

	public static DateTime LastUpdateTime
	{
		get { return Settings.LastUpdateTime; }
		set { Settings.LastUpdateTime = value; }
	}

	public static bool CheckForUpdates
	{
		get { return Settings.CheckForUpdates; }
		set { Settings.CheckForUpdates = value; }
	}

	public static bool DetectMovedLines
	{
		get { return Settings.DetectMovedLines; }
		set { Settings.DetectMovedLines = value; }
	}

	public static bool IgnoreWhiteSpace
	{
		get { return Settings.IgnoreWhiteSpace; }
		set { Settings.IgnoreWhiteSpace = value; }
	}

	public static bool ShowWhiteSpaceCharacters
	{
		get { return Settings.ShowWhiteSpaceCharacters; }
		set { Settings.ShowWhiteSpaceCharacters = value; }
	}

	public static bool MasterDetail
	{
		get { return Settings.MasterDetail; }
		set { Settings.MasterDetail = value; }
	}

	public static double PositionLeft
	{
		get { return Settings.PositionLeft; }
		set { Settings.PositionLeft = value; }
	}

	public static double PositionTop
	{
		get { return Settings.PositionTop; }
		set { Settings.PositionTop = value; }
	}

	public static double Width
	{
		get { return Settings.Width; }
		set { Settings.Width = value; }
	}

	public static double Height
	{
		get { return Settings.Height; }
		set { Settings.Height = value; }
	}

	public static double FolderRowHeight
	{
		get { return Settings.FolderRowHeight; }
		set { Settings.FolderRowHeight = value; }
	}

	public static WindowState WindowState
	{
		get { return Settings.WindowState; }
		set { Settings.WindowState = value; }
	}

	public static float LineSimilarityThreshold
	{
		get { return Settings.LineSimilarityThreshold; }
		set { Settings.LineSimilarityThreshold = value; }
	}

	public static bool ShowLineChanges
	{
		get { return Settings.ShowLineChanges; }
		set { Settings.ShowLineChanges = value; }
	}

	public static int CharacterMatchThreshold
	{
		get { return Settings.CharacterMatchThreshold; }
		set { Settings.CharacterMatchThreshold = value; }
	}

	public static FontFamily Font
	{
		get;
		set { field = value; Settings.Font = value.ToString(); }
	}

	public static int FontSize
	{
		get { return Settings.FontSize; }
		set { Settings.FontSize = value; }
	}

	public static int Zoom
	{
		get { return Settings.Zoom; }
		set { Settings.Zoom = value; }
	}

	public static int TabSize
	{
		get { return Settings.TabSize; }
		set { Settings.TabSize = value; }
	}

	public static Themes Theme
	{
		get { return Settings.Theme; }
		set
		{
			Settings.Theme = value;

			UpdateCachedSettings();
			NotifyStaticPropertyChanged(nameof(WindowForegroundColor));
			NotifyStaticPropertyChanged(nameof(WindowBackgroundColor));
			NotifyStaticPropertyChanged(nameof(DisabledForegroundColor));
			NotifyStaticPropertyChanged(nameof(DialogBackgroundColor));
			NotifyStaticPropertyChanged(nameof(ControlLightBackgroundColor));
			NotifyStaticPropertyChanged(nameof(BorderForegroundColor));
		}
	}

	public static ColorTheme DarkTheme
	{
		get { return Settings.DarkTheme; }
		set { Settings.DarkTheme = value; }
	}

	public static ColorTheme LightTheme
	{
		get { return Settings.LightTheme; }
		set { Settings.LightTheme = value; }
	}

	public static ColorTheme CurrentTheme
	{
		get
		{
			return Theme switch
			{
				Themes.Light => Settings.LightTheme,
				Themes.Dark => Settings.DarkTheme,
				_ => throw new NotImplementedException(),
			};
		}
	}


	// Folder diff colors
	public static Brush FolderFullMatchForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.FolderFullMatchForeground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(FolderFullMatchForeground));
		}
	}

	public static Brush FolderFullMatchBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.FolderFullMatchBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(FolderFullMatchBackground));
		}
	}

	public static Brush FolderNewForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.FolderNewForeground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(FolderNewForeground));
		}
	}

	public static Brush FolderNewBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.FolderNewBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(FolderNewBackground));
		}
	}

	public static Brush FolderDeletedForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.FolderDeletedForeground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(FolderDeletedForeground));
		}
	}

	public static Brush FolderDeletedBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.FolderDeletedBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(FolderDeletedBackground));
		}
	}

	public static Brush FolderPartialMatchForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.FolderPartialMatchForeground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(FolderPartialMatchForeground));
		}
	}

	public static Brush FolderPartialMatchBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.FolderPartialMatchBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(FolderPartialMatchBackground));
		}
	}

	public static Brush FolderIgnoredForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.FolderIgnoredForeground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(FolderIgnoredForeground));
		}
	}

	public static Brush FolderIgnoredBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.FolderIgnoredBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(FolderIgnoredBackground));
		}
	}


	// File diff colors
	public static Brush FullMatchForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.FullMatchForeground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(FullMatchForeground));
		}
	}

	public static Brush FullMatchBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.FullMatchBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(FullMatchBackground));
		}
	}

	public static Brush NewForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.NewForeground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(NewForeground));
		}
	}

	public static Brush NewBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.NewBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(NewBackground));
		}
	}

	public static Brush MovedToBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.MovedToBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(MovedToBackground));
		}
	}

	public static Brush DeletedForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.DeletedForeground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(DeletedForeground));
		}
	}

	public static Brush DeletedBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.DeletedBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(DeletedBackground));
		}
	}

	public static Brush MovedFromBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.MovedFromBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(MovedFromBackground));
		}
	}

	public static Brush PartialMatchForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.PartialMatchForeground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(PartialMatchForeground));
		}
	}

	public static Brush PartialMatchBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.PartialMatchBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(PartialMatchBackground));
		}
	}


	// Editor colors
	public static Brush WhiteSpaceForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.WhiteSpaceForeground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(WhiteSpaceForeground));
		}
	}

	public static Brush SelectionBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.SelectionBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(SelectionBackground));
		}
	}

	public static Brush LineNumberColor
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.LineNumberColor = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(LineNumberColor));
		}
	}

	public static Brush CurrentDiffColor
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.CurrentDiffColor = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(CurrentDiffColor));
		}
	}

	public static Brush SnakeColor
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.SnakeColor = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(SnakeColor));
		}
	}


	// UI colors
	public static Brush WindowForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.NormalText = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(WindowForeground));
			NotifyStaticPropertyChanged(nameof(WindowForegroundColor));
		}
	} = DefaultSettings.LightTheme.NormalText.ToBrush();
	public static Color WindowForegroundColor
	{
		get
		{
			return ((SolidColorBrush)WindowForeground).Color;
		}
	}

	public static Brush DisabledForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.DisabledText = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(DisabledForeground));
			NotifyStaticPropertyChanged(nameof(DisabledForegroundColor));
		}
	} = DefaultSettings.LightTheme.DisabledText.ToBrush();
	public static Color DisabledForegroundColor
	{
		get
		{
			return ((SolidColorBrush)DisabledForeground).Color;
		}
	}

	public static Brush DisabledBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.DisabledBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(DisabledBackground));
			NotifyStaticPropertyChanged(nameof(DisabledBackgroundColor));
		}
	} = DefaultSettings.LightTheme.DisabledBackground.ToBrush();
	public static Color DisabledBackgroundColor
	{
		get { return ((SolidColorBrush)DisabledBackground).Color; }
	}

	public static Brush WindowBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.WindowBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(WindowBackground));
			NotifyStaticPropertyChanged(nameof(WindowBackgroundColor));
		}
	} = DefaultSettings.LightTheme.WindowBackground.ToBrush();
	public static Color WindowBackgroundColor
	{
		get
		{
			return ((SolidColorBrush)WindowBackground).Color;
		}
	}

	public static Brush DialogBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.DialogBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(DialogBackground));
			NotifyStaticPropertyChanged(nameof(DialogBackgroundColor));
		}
	} = DefaultSettings.LightTheme.DialogBackground.ToBrush();
	public static Color DialogBackgroundColor
	{
		get
		{
			return ((SolidColorBrush)DialogBackground).Color;
		}
	}

	public static Brush ControlLightBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.ControlLightBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(ControlLightBackground));
			NotifyStaticPropertyChanged(nameof(ControlLightBackgroundColor));
		}
	} = DefaultSettings.LightTheme.ControlLightBackground.ToBrush();
	public static Color ControlLightBackgroundColor
	{
		get
		{
			return ((SolidColorBrush)ControlLightBackground).Color;
		}
	}

	public static Brush ControlDarkBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.ControlDarkBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(ControlDarkBackground));
			NotifyStaticPropertyChanged(nameof(ControlDarkBackgroundColor));
		}
	} = DefaultSettings.LightTheme.ControlDarkBackground.ToBrush();
	public static Color ControlDarkBackgroundColor
	{
		get
		{
			return ((SolidColorBrush)ControlDarkBackground).Color;
		}
	}

	public static Brush BorderForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.BorderLight = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(BorderForeground));
			NotifyStaticPropertyChanged(nameof(BorderForegroundColor));
		}
	} = DefaultSettings.LightTheme.BorderLight.ToBrush();
	public static Color BorderForegroundColor
	{
		get
		{
			return ((SolidColorBrush)BorderForeground).Color;
		}
	}

	public static Brush BorderDarkForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.BorderDark = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(BorderDarkForeground));
			NotifyStaticPropertyChanged(nameof(BorderDarkForegroundColor));
		}
	} = DefaultSettings.LightTheme.BorderDark.ToBrush();
	public static Color BorderDarkForegroundColor
	{
		get
		{
			return ((SolidColorBrush)BorderDarkForeground).Color;
		}
	}

	public static Brush HighlightBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.HighlightBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(HighlightBackground));
			NotifyStaticPropertyChanged(nameof(HighlightBackgroundColor));
		}
	} = DefaultSettings.LightTheme.HighlightBackground.ToBrush();
	public static Color HighlightBackgroundColor
	{
		get
		{
			return ((SolidColorBrush)HighlightBackground).Color;
		}
	}

	public static Brush HighlightBorder
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.HighlightBorder = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(HighlightBorder));
			NotifyStaticPropertyChanged(nameof(HighlightBorderColor));
		}
	} = DefaultSettings.LightTheme.HighlightBorder.ToBrush();
	public static Color HighlightBorderColor
	{
		get
		{
			return ((SolidColorBrush)HighlightBorder).Color;
		}
	}

	public static Brush AttentionBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.AttentionBackground = ((SolidColorBrush)value).Color.ToString();
			NotifyStaticPropertyChanged(nameof(AttentionBackground));
			NotifyStaticPropertyChanged(nameof(AttentionBackgroundColor));
		}
	} = DefaultSettings.LightTheme.AttentionBackground.ToBrush();
	public static Color AttentionBackgroundColor
	{
		get
		{
			return ((SolidColorBrush)AttentionBackground).Color;
		}
	}


	public static double NameColumnWidth { get; internal set; } = 300;

	public static double SizeColumnWidth { get; internal set; } = 70;

	public static double DateColumnWidth { get; internal set; } = 120;

	public static bool DrawGridLines { get; internal set; } = false;


	public static ObservableCollection<TextAttribute> IgnoredFolders
	{
		get { return Settings.IgnoredFolders; }
		set { Settings.IgnoredFolders = value; }
	}

	public static ObservableCollection<TextAttribute> IgnoredFiles
	{
		get { return Settings.IgnoredFiles; }
		set { Settings.IgnoredFiles = value; }
	}

	#endregion

	#region Methods

	internal static void LoadSettings()
	{
		SettingsData storedSettings = ReadSettingsFromDisk();

		if (storedSettings != null)
		{
			MergeSettings(Settings, storedSettings);
		}

		UpdateCachedSettings();
	}

	private static void MergeSettings(object source, object addition)
	{
		foreach (var property in addition.GetType().GetProperties())
		{
			if (property.PropertyType.Name == nameof(ColorTheme))
			{
				MergeSettings(property.GetValue(source), property.GetValue(addition));
			}
			else
			{
				if (property.GetValue(addition) != null)
				{
					property.SetValue(source, property.GetValue(addition));
				}
			}
		}
	}

	private static SettingsData ReadSettingsFromDisk()
	{
		DataContractSerializer xmlSerializer = new(typeof(SettingsData));

		if (File.Exists(SettingsPath))
		{
			using var xmlReader = XmlReader.Create(SettingsPath);
			try
			{
				return (SettingsData)xmlSerializer.ReadObject(xmlReader);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Error Parsing Settings File", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		return null;
	}

	internal static void WriteSettingsToDisk()
	{
		try
		{
			DataContractSerializer xmlSerializer = new(typeof(SettingsData));
			var xmlWriterSettings = new XmlWriterSettings { Indent = true, IndentChars = " " };

			Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));

			using XmlWriter xmlWriter = XmlWriter.Create(SettingsPath, xmlWriterSettings);

			xmlSerializer.WriteObject(xmlWriter, Settings);
		}
		catch (Exception e)
		{
			MessageBox.Show(e.Message, "Error Saving Settings", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private static void UpdateCachedSettings()
	{
		try
		{
			Font = new FontFamily(Settings.Font);

			// Folder diff colors
			FolderFullMatchForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FolderFullMatchForeground));
			FolderFullMatchBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FolderFullMatchBackground));

			FolderNewForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FolderNewForeground));
			FolderNewBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FolderNewBackground));

			FolderDeletedForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FolderDeletedForeground));
			FolderDeletedBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FolderDeletedBackground));

			FolderPartialMatchForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FolderPartialMatchForeground));
			FolderPartialMatchBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FolderPartialMatchBackground));

			FolderIgnoredForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FolderIgnoredForeground));
			FolderIgnoredBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FolderIgnoredBackground));

			// File diff colors
			FullMatchForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FullMatchForeground));
			FullMatchBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FullMatchBackground));

			NewForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.NewForeground));
			NewBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.NewBackground));

			MovedToBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.MovedToBackground));

			DeletedForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DeletedForeground));
			DeletedBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DeletedBackground));

			MovedFromBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.MovedFromBackground));

			PartialMatchForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.PartialMatchForeground));
			PartialMatchBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.PartialMatchBackground));

			// Editor colors
			WhiteSpaceForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.WhiteSpaceForeground));
			SelectionBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.SelectionBackground));
			LineNumberColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.LineNumberColor));
			CurrentDiffColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.CurrentDiffColor));
			SnakeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.SnakeColor));

			// UI colors
			WindowForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.NormalText));
			DisabledForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DisabledText));
			DisabledBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DisabledBackground));

			WindowBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.WindowBackground));
			DialogBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DialogBackground));

			ControlLightBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.ControlLightBackground));
			ControlDarkBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.ControlDarkBackground));

			BorderForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.BorderLight));
			BorderDarkForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.BorderDark));

			HighlightBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.HighlightBackground));
			HighlightBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.HighlightBorder));

			AttentionBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.AttentionBackground));
		}
		catch
		{

		}
	}

	internal static Brush GetFileForeground(TextState state)
	{
		switch (state)
		{
			case TextState.Deleted:
			case TextState.MovedFrom:
				return DeletedForeground;
			case TextState.New:
			case TextState.MovedTo:
				return NewForeground;
			case TextState.PartialMatch:
				return PartialMatchForeground;

			default:
				return FullMatchForeground;
		}
	}

	internal static Brush GetFileBackground(TextState state)
	{
		switch (state)
		{
			case TextState.Deleted:
				return DeletedBackground;
			case TextState.New:
				return NewBackground;
			case TextState.MovedFrom:
				return MovedFromBackground;
			case TextState.MovedTo:
				return MovedToBackground;
			case TextState.PartialMatch:
				return PartialMatchBackground;

			default:
				return FullMatchBackground;
		}
	}

	internal static Brush GetFolderBackground(TextState state)
	{
		switch (state)
		{
			case TextState.Deleted:
				return FolderDeletedBackground;
			case TextState.New:
				return FolderNewBackground;
			case TextState.PartialMatch:
				return FolderPartialMatchBackground;
			case TextState.Ignored:
				return FolderIgnoredBackground;

			default:
				return FolderFullMatchBackground;
		}
	}

	internal static Brush GetFolderForeground(TextState state)
	{
		switch (state)
		{
			case TextState.Deleted:
				return FolderDeletedForeground;
			case TextState.New:
				return FolderNewForeground;
			case TextState.PartialMatch:
				return FolderPartialMatchForeground;
			case TextState.Ignored:
				return FolderIgnoredForeground;

			default:
				return FolderFullMatchForeground;
		}
	}

	public static void ResetCurrentTheme()
	{
		ColorTheme themeDefaults = Theme switch
		{
			Themes.Light => DefaultSettings.LightTheme,
			Themes.Dark => DefaultSettings.DarkTheme,
			_ => throw new NotImplementedException(),
		};

		MergeSettings(CurrentTheme, themeDefaults);

		UpdateCachedSettings();
	}

	#endregion

	#region NotifyStaticPropertyChanged

	public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

	private static void NotifyStaticPropertyChanged(string propertyName)
	{
		StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
	}

	#endregion

}
