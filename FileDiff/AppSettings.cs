using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace FileDiff;

public static class AppSettings
{

	#region Members

	private const string SETTINGS_DIRECTORY = "FileDiff";
	private const string SETTINGS_FILE_NAME = "Settings.xml";

	private static SettingsData Settings = new SettingsData();

	#endregion

	#region Properies

	public static string Id
	{
		get { return Settings.Id; }
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

	private static FontFamily font;
	public static FontFamily Font
	{
		get { return font; }
		set { font = value; Settings.Font = value.ToString(); }
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



	public static ColorTheme Theme
	{
		get { return Settings.Theme; }
		set
		{
			Settings.Theme = value;

			UpdateCachedSettings();
		}
	}



	public static ThemeColors CurrentTheme
	{
		get
		{
			return Theme switch
			{
				ColorTheme.Light => Settings.LightTheme,
				ColorTheme.Dark => Settings.DarkTheme,
				_ => throw new NotImplementedException(),
			};
		}
	}



	// Text colors
	private static SolidColorBrush fullMatchForeground;
	public static SolidColorBrush FullMatchForeground
	{
		get { return fullMatchForeground; }
		set
		{
			fullMatchForeground = value;
			fullMatchForeground.Freeze();
			CurrentTheme.FullMatchForeground = value.Color.ToString();
		}
	}

	private static SolidColorBrush fullMatchBackground;
	public static SolidColorBrush FullMatchBackground
	{
		get { return fullMatchBackground; }
		set
		{
			fullMatchBackground = value;
			fullMatchBackground.Freeze();
			CurrentTheme.FullMatchBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush partialMatchForeground;
	public static SolidColorBrush PartialMatchForeground
	{
		get { return partialMatchForeground; }
		set
		{
			partialMatchForeground = value;
			partialMatchForeground.Freeze();
			CurrentTheme.PartialMatchForeground = value.Color.ToString();
		}
	}

	private static SolidColorBrush partialMatchBackground;
	public static SolidColorBrush PartialMatchBackground
	{
		get { return partialMatchBackground; }
		set
		{
			partialMatchBackground = value;
			partialMatchBackground.Freeze();
			CurrentTheme.PartialMatchBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush deletedForeground;
	public static SolidColorBrush DeletedForeground
	{
		get { return deletedForeground; }
		set
		{
			deletedForeground = value;
			deletedForeground.Freeze();
			CurrentTheme.DeletedForeground = value.Color.ToString();
		}
	}

	private static SolidColorBrush deletedBackground;
	public static SolidColorBrush DeletedBackground
	{
		get { return deletedBackground; }
		set
		{
			deletedBackground = value;
			deletedBackground.Freeze();
			CurrentTheme.DeletedBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush newForeground;
	public static SolidColorBrush NewForeground
	{
		get { return newForeground; }
		set
		{
			newForeground = value;
			newForeground.Freeze();
			CurrentTheme.NewForeground = value.Color.ToString();
		}
	}

	private static SolidColorBrush newBackground;
	public static SolidColorBrush NewBackground
	{
		get { return newBackground; }
		set
		{
			newBackground = value;
			newBackground.Freeze();
			CurrentTheme.NewBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush ignoredForeground;
	public static SolidColorBrush IgnoredForeground
	{
		get { return ignoredForeground; }
		set
		{
			ignoredForeground = value;
			ignoredForeground.Freeze();
			CurrentTheme.IgnoredForeground = value.Color.ToString();
		}
	}

	private static SolidColorBrush ignoredBackground;
	public static SolidColorBrush IgnoredBackground
	{
		get { return ignoredBackground; }
		set
		{
			ignoredBackground = value;
			ignoredBackground.Freeze();
			CurrentTheme.IgnoredBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush movedFromdBackground;
	public static SolidColorBrush MovedFromdBackground
	{
		get { return movedFromdBackground; }
		set
		{
			movedFromdBackground = value;
			movedFromdBackground.Freeze();
			CurrentTheme.MovedFromdBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush movedToBackground;
	public static SolidColorBrush MovedToBackground
	{
		get { return movedToBackground; }
		set
		{
			movedToBackground = value;
			movedToBackground.Freeze();
			CurrentTheme.MovedToBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush selectionBackground;
	public static SolidColorBrush SelectionBackground
	{
		get { return selectionBackground; }
		set
		{
			selectionBackground = value;
			selectionBackground.Freeze();
			CurrentTheme.SelectionBackground = value.Color.ToString();
		}
	}


	// GUI colors
	private static SolidColorBrush windowForeground;
	public static SolidColorBrush WindowForeground
	{
		get { return windowForeground; }
		set
		{
			windowForeground = value;
			windowForeground.Freeze();
			CurrentTheme.WindowForeground = value.Color.ToString();
		}
	}

	private static SolidColorBrush windowBackground;
	public static SolidColorBrush WindowBackgruond
	{
		get { return windowBackground; }
		set
		{
			windowBackground = value;
			windowBackground.Freeze();
			CurrentTheme.WindowBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush controlBackground;
	public static SolidColorBrush ControlBackground
	{
		get { return controlBackground; }
		set
		{
			controlBackground = value;
			controlBackground.Freeze();
			CurrentTheme.ControlBackground = value.Color.ToString();
		}
	}

	private static SolidColorBrush borderColor;
	public static SolidColorBrush BorderColor
	{
		get { return borderColor; }
		set
		{
			borderColor = value;
			borderColor.Freeze();
			CurrentTheme.BorderColor = value.Color.ToString();
		}
	}

	private static SolidColorBrush lineNumberColor;
	public static SolidColorBrush LineNumberColor
	{
		get { return lineNumberColor; }
		set
		{
			lineNumberColor = value;
			lineNumberColor.Freeze();
			CurrentTheme.LineNumberColor = value.Color.ToString();
		}
	}

	private static SolidColorBrush currentDiffColor;
	public static SolidColorBrush CurrentDiffColor
	{
		get { return currentDiffColor; }
		set
		{
			currentDiffColor = value;
			currentDiffColor.Freeze();
			CurrentTheme.CurrentDiffColor = value.Color.ToString();
		}
	}

	private static SolidColorBrush snakeColor;
	public static SolidColorBrush SnakeColor
	{
		get { return snakeColor; }
		set
		{
			snakeColor = value;
			snakeColor.Freeze();
			CurrentTheme.SnakeColor = value.Color.ToString();
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

	internal static void ReadSettingsFromDisk()
	{
		string settingsPath = Path.Combine(Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SETTINGS_DIRECTORY), SETTINGS_FILE_NAME);
		DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(SettingsData));

		if (File.Exists(settingsPath))
		{
			using XmlReader xmlReader = XmlReader.Create(settingsPath);
			try
			{
				Settings = (SettingsData)xmlSerializer.ReadObject(xmlReader);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Error Parsing XML", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		if (Settings == null)
		{
			Settings = new SettingsData();
		}

		UpdateCachedSettings();
	}

	internal static void WriteSettingsToDisk()
	{
		try
		{
			string settingsPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SETTINGS_DIRECTORY);

			DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(SettingsData));
			var xmlWriterSettings = new XmlWriterSettings { Indent = true, IndentChars = " " };

			if (!Directory.Exists(settingsPath))
			{
				Directory.CreateDirectory(settingsPath);
			}

			using XmlWriter xmlWriter = XmlWriter.Create(Path.Combine(settingsPath, SETTINGS_FILE_NAME), xmlWriterSettings);
			xmlSerializer.WriteObject(xmlWriter, Settings);
		}
		catch (Exception e)
		{
			MessageBox.Show(e.Message, "Error Saving Settings", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private static void UpdateCachedSettings()
	{
		Font = new FontFamily(Settings.Font);

		FullMatchForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FullMatchForeground));
		FullMatchBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.FullMatchBackground));

		PartialMatchForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.PartialMatchForeground));
		PartialMatchBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.PartialMatchBackground));

		DeletedForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DeletedForeground));
		DeletedBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DeletedBackground));

		NewForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.NewForeground));
		NewBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.NewBackground));

		MovedFromdBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.MovedFromdBackground));

		MovedToBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.MovedToBackground));

		IgnoredForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.IgnoredForeground));
		IgnoredBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.IgnoredBackground));

		SelectionBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.SelectionBackground));

		WindowForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.WindowForeground));
		WindowBackgruond = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.WindowBackground));
		BorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.BorderColor));
		LineNumberColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.LineNumberColor));
		CurrentDiffColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.CurrentDiffColor));
		SnakeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.SnakeColor));
	}

	internal static SolidColorBrush GetForeground(TextState state)
	{
		switch (state)
		{
			case TextState.Deleted:
			case TextState.MovedFrom:
				return deletedForeground;
			case TextState.New:
			case TextState.MovedTo:
				return newForeground;
			case TextState.PartialMatch:
				return partialMatchForeground;
			case TextState.Ignored:
				return ignoredForeground;

			default:
				return fullMatchForeground;
		}
	}

	internal static SolidColorBrush GetBackground(TextState state)
	{
		switch (state)
		{
			case TextState.Deleted:
				return deletedBackground;
			case TextState.New:
				return newBackground;
			case TextState.MovedFrom:
				return movedFromdBackground;
			case TextState.MovedTo:
				return movedToBackground;
			case TextState.PartialMatch:
				return partialMatchBackground;
			case TextState.Ignored:
				return ignoredBackground;

			default:
				return fullMatchBackground;
		}
	}

	#endregion

}
