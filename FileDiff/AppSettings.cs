using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace FileDiff
{
	public static class AppSettings
	{

		#region Members

		private const string SETTINGS_DIRECTORY = "FileDiff";
		private const string SETTINGS_FILE_NAME = "Settings.xml";

		public const string HOMEPAGE = @"https://github.com/jonashertzman/FileDiff";

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

		public static int TabSize
		{
			get { return Settings.TabSize; }
			set { Settings.TabSize = value; }
		}

		private static SolidColorBrush fullMatchForeground;
		public static SolidColorBrush FullMatchForeground
		{
			get { return fullMatchForeground; }
			set { fullMatchForeground = value; Settings.FullMatchForeground = value.Color; }
		}

		private static SolidColorBrush fullMatchBackground;
		public static SolidColorBrush FullMatchBackground
		{
			get { return fullMatchBackground; }
			set { fullMatchBackground = value; Settings.FullMatchBackground = value.Color; }
		}

		private static SolidColorBrush partialMatchForeground;
		public static SolidColorBrush PartialMatchForeground
		{
			get { return partialMatchForeground; }
			set { partialMatchForeground = value; Settings.PartialMatchForeground = value.Color; }
		}

		private static SolidColorBrush partialMatchBackground;
		public static SolidColorBrush PartialMatchBackground
		{
			get { return partialMatchBackground; }
			set { partialMatchBackground = value; Settings.PartialMatchBackground = value.Color; }
		}

		private static SolidColorBrush deletedForeground;
		public static SolidColorBrush DeletedForeground
		{
			get { return deletedForeground; }
			set { deletedForeground = value; Settings.DeletedForeground = value.Color; }
		}

		private static SolidColorBrush deletedBackground;
		public static SolidColorBrush DeletedBackground
		{
			get { return deletedBackground; }
			set { deletedBackground = value; Settings.DeletedBackground = value.Color; }
		}

		private static SolidColorBrush newForeground;
		public static SolidColorBrush NewForeground
		{
			get { return newForeground; }
			set { newForeground = value; Settings.NewForeground = value.Color; }
		}

		private static SolidColorBrush newBackground;
		public static SolidColorBrush NewBackground
		{
			get { return newBackground; }
			set { newBackground = value; Settings.NewBackground = value.Color; }
		}

		private static SolidColorBrush ignoredForeground;
		public static SolidColorBrush IgnoredForeground
		{
			get { return ignoredForeground; }
			set { ignoredForeground = value; Settings.IgnoredForeground = value.Color; }
		}

		private static SolidColorBrush ignoredBackground;
		public static SolidColorBrush IgnoredBackground
		{
			get { return ignoredBackground; }
			set { ignoredBackground = value; Settings.IgnoredBackground = value.Color; }
		}

		private static SolidColorBrush selectionBackground;
		public static SolidColorBrush SelectionBackground
		{
			get { return selectionBackground; }
			set { selectionBackground = value; Settings.SelectionBackground = value.Color; }
		}

		public static double NameColumnWidth { get; internal set; } = 300;

		public static double SizeColumnWidth { get; internal set; } = 70;

		public static double DateColumnWidth { get; internal set; } = 120;

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
				using (var xmlReader = XmlReader.Create(settingsPath))
				{
					try
					{
						Settings = (SettingsData)xmlSerializer.ReadObject(xmlReader);
					}
					catch (Exception e)
					{
						MessageBox.Show(e.Message, "Error Parsing XML", MessageBoxButton.OK, MessageBoxImage.Error);
					}
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

				using (var xmlWriter = XmlWriter.Create(Path.Combine(settingsPath, SETTINGS_FILE_NAME), xmlWriterSettings))
				{
					xmlSerializer.WriteObject(xmlWriter, Settings);
				}
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

			IgnoredForeground = new SolidColorBrush(Settings.IgnoredForeground);
			IgnoredBackground = new SolidColorBrush(Settings.IgnoredBackground);

			SelectionBackground = new SolidColorBrush(Settings.SelectionBackground);
		}

		#endregion

	}
}
