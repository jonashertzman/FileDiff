using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace FileDiff
{
	public class WindowData : INotifyPropertyChanged
	{

		const string SETTINGS_DIRECTORY = "FileDiff";
		const string SETTINGS_FILE_NAME = "Settings.xml";

		#region Properties

		public SettingsData Settings { get; set; } = new SettingsData();

		ObservableCollection<Line> leftSide = new ObservableCollection<Line>();
		public ObservableCollection<Line> LeftSide
		{
			get { return leftSide; }
			set { leftSide = value; OnPropertyChanged(nameof(LeftSide)); }
		}

		ObservableCollection<Line> rightSide = new ObservableCollection<Line>();
		public ObservableCollection<Line> RightSide
		{
			get { return rightSide; }
			set { rightSide = value; OnPropertyChanged(nameof(RightSide)); }
		}

		string leftPath;
		public string LeftPath
		{
			get { return leftPath; }
			set { leftPath = value; OnPropertyChanged(nameof(LeftPath)); }
		}

		string rightPath;
		public string RightPath
		{
			get { return rightPath; }
			set { rightPath = value; OnPropertyChanged(nameof(RightPath)); }
		}

		public bool IgnoreWhiteSpace
		{
			get { return Settings.IgnoreWhiteSpace; }
			set
			{
				Settings.IgnoreWhiteSpace = value;
				OnPropertyChanged(nameof(IgnoreWhiteSpace));
			}
		}

		public bool ShowLineChanges
		{
			get
			{
				return Settings.ShowLineChanges;
			}
			set
			{
				Settings.ShowLineChanges = value;
				OnPropertyChanged(nameof(ShowLineChanges));
			}
		}

		SolidColorBrush fullMatchForeground;
		public SolidColorBrush FullMatchForeground
		{
			get { return fullMatchForeground; }
			set { fullMatchForeground = value; Settings.FullMatchForeground = value.Color; OnPropertyChanged(nameof(FullMatchForeground)); }
		}

		SolidColorBrush fullMatchBackground;
		public SolidColorBrush FullMatchBackground
		{
			get { return fullMatchBackground; }
			set { fullMatchBackground = value; Settings.FullMatchBackground = value.Color; OnPropertyChanged(nameof(FullMatchBackground)); }
		}

		SolidColorBrush partialMatchForeground;
		public SolidColorBrush PartialMatchForeground
		{
			get { return partialMatchForeground; }
			set { partialMatchForeground = value; Settings.PartialMatchForeground = value.Color; OnPropertyChanged(nameof(PartialMatchForeground)); }
		}

		SolidColorBrush partialMatchBackground;
		public SolidColorBrush PartialMatchBackground
		{
			get { return partialMatchBackground; }
			set { partialMatchBackground = value; Settings.PartialMatchBackground = value.Color; OnPropertyChanged(nameof(PartialMatchBackground)); }
		}

		SolidColorBrush deletedForeground;
		public SolidColorBrush DeletedForeground
		{
			get { return deletedForeground; }
			set { deletedForeground = value; Settings.DeletedForeground = value.Color; OnPropertyChanged(nameof(DeletedForeground)); }
		}

		SolidColorBrush deletedBackground;
		public SolidColorBrush DeletedBackground
		{
			get { return deletedBackground; }
			set { deletedBackground = value; Settings.DeletedBackground = value.Color; OnPropertyChanged(nameof(DeletedBackground)); }
		}

		SolidColorBrush newForeground;
		public SolidColorBrush NewForeground
		{
			get { return newForeground; }
			set { newForeground = value; Settings.NewForeground = value.Color; OnPropertyChanged(nameof(NewForeground)); }
		}

		SolidColorBrush newBackground;
		public SolidColorBrush NewBackground
		{
			get { return newBackground; }
			set { newBackground = value; Settings.NewBackground = value.Color; OnPropertyChanged(nameof(NewBackground)); }
		}

		#endregion

		#region Methods

		internal void ReadSettingsFromDisk()
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

			ClearBrushCache();
			OnPropertyChanged("");
		}

		internal void WriteSettingsToDisk()
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

		private void ClearBrushCache()
		{
			fullMatchForeground = new SolidColorBrush(Settings.FullMatchForeground);
			fullMatchBackground = new SolidColorBrush(Settings.FullMatchBackground);

			partialMatchForeground = new SolidColorBrush(Settings.PartialMatchForeground);
			partialMatchBackground = new SolidColorBrush(Settings.PartialMatchBackground);

			deletedForeground = new SolidColorBrush(Settings.DeletedForeground);
			deletedBackground = new SolidColorBrush(Settings.DeletedBackground);

			newForeground = new SolidColorBrush(Settings.NewForeground);
			newBackground = new SolidColorBrush(Settings.NewBackground);
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}
}
