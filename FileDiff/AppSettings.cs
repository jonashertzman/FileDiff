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


	private static SolidColorBrush fullMatchForeground;
	public static SolidColorBrush FullMatchForeground
	{
		get { return fullMatchForeground; }
		set
		{
			fullMatchForeground = value;
			fullMatchForeground.Freeze();
			Settings.FullMatchForeground = value.Color;
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
			Settings.FullMatchBackground = value.Color;
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
			Settings.PartialMatchForeground = value.Color;
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
			Settings.PartialMatchBackground = value.Color;
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
			Settings.DeletedForeground = value.Color;
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
			Settings.DeletedBackground = value.Color;
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
			Settings.NewForeground = value.Color;
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
			Settings.NewBackground = value.Color;
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
			Settings.IgnoredForeground = value.Color;
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
			Settings.IgnoredBackground = value.Color;
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
			Settings.MovedFromdBackground = value.Color;
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
			Settings.MovedToBackground = value.Color;
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
			Settings.SelectionBackground = value.Color;
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
			Settings.LineNumberColor = value.Color;
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
			Settings.CurrentDiffColor = value.Color;
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
			Settings.SnakeColor = value.Color;
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

		FullMatchForeground = new SolidColorBrush(Settings.FullMatchForeground);
		FullMatchBackground = new SolidColorBrush(Settings.FullMatchBackground);

		PartialMatchForeground = new SolidColorBrush(Settings.PartialMatchForeground);
		PartialMatchBackground = new SolidColorBrush(Settings.PartialMatchBackground);

		DeletedForeground = new SolidColorBrush(Settings.DeletedForeground);
		DeletedBackground = new SolidColorBrush(Settings.DeletedBackground);

		NewForeground = new SolidColorBrush(Settings.NewForeground);
		NewBackground = new SolidColorBrush(Settings.NewBackground);

		MovedFromdBackground = new SolidColorBrush(Settings.MovedFromdBackground);

		MovedToBackground = new SolidColorBrush(Settings.MovedToBackground);

		IgnoredForeground = new SolidColorBrush(Settings.IgnoredForeground);
		IgnoredBackground = new SolidColorBrush(Settings.IgnoredBackground);

		SelectionBackground = new SolidColorBrush(Settings.SelectionBackground);

		LineNumberColor = new SolidColorBrush(Settings.LineNumberColor);
		CurrentDiffColor = new SolidColorBrush(Settings.CurrentDiffColor);
		SnakeColor = new SolidColorBrush(Settings.SnakeColor);
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
