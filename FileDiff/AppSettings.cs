using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace FileDiff
{
	public static class AppSettings
	{

		const string SETTINGS_DIRECTORY = "FileDiff";
		const string SETTINGS_FILE_NAME = "Settings.xml";

		#region Defaults 

		internal static Color DefaultFullMatchForeground = Colors.Black;
		internal static Color DefaultFullMatchBackground = Colors.White;

		internal static Color DefaultPartialMatchForeground = Colors.Black;
		internal static Color DefaultPartialMatchBackground = Color.FromRgb(220, 220, 255);

		internal static Color DefaultDeletedForeground = Color.FromRgb(200, 0, 0);
		internal static Color DefaultDeletedBackground = Color.FromRgb(255, 220, 220);

		internal static Color DefaultNewForeground = Color.FromRgb(0, 120, 0);
		internal static Color DefaultNewBackground = Color.FromRgb(220, 255, 220);

		internal const string DefaultFont = "Courier New";
		internal const int DefaultFontSize = 12;

		#endregion

		public static SettingsData Settings { get; private set; } = new SettingsData();


		public static SolidColorBrush fullMatchForegroundBrush { get; set; }

		public static SolidColorBrush fullMatchBackgroundBrush { get; set; }

		public static SolidColorBrush partialMatchForegroundBrush { get; set; }

		public static SolidColorBrush partialMatchBackgroundBrush { get; set; }

		public static SolidColorBrush deletedForegroundBrush { get; set; }

		public static SolidColorBrush deletedBackgroundBrush { get; set; }

		public static SolidColorBrush newForegroundBrush { get; set; }

		public static SolidColorBrush newBackgrounBrush { get; set; }



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

			UpdateBrushCache();
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
				MessageBox.Show(e.Message);
			}
		}

		private static void UpdateBrushCache()
		{
			fullMatchForegroundBrush = new SolidColorBrush(Settings.FullMatchForeground);
			fullMatchBackgroundBrush = new SolidColorBrush(Settings.FullMatchBackground);

			partialMatchForegroundBrush = new SolidColorBrush(Settings.PartialMatchForeground);
			partialMatchBackgroundBrush = new SolidColorBrush(Settings.PartialMatchBackground);

			deletedForegroundBrush = new SolidColorBrush(Settings.DeletedForeground);
			deletedBackgroundBrush = new SolidColorBrush(Settings.DeletedBackground);

			newForegroundBrush = new SolidColorBrush(Settings.NewForeground);
			newBackgrounBrush = new SolidColorBrush(Settings.NewBackground);
		}

	}
}
