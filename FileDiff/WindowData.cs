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

		#region Properties

		private SettingsData Settings = AppSettings.Settings;

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
			set { Settings.IgnoreWhiteSpace = value; OnPropertyChanged(nameof(IgnoreWhiteSpace)); }
		}

		public bool ShowLineChanges
		{
			get { return Settings.ShowLineChanges; }
			set { Settings.ShowLineChanges = value; OnPropertyChanged(nameof(ShowLineChanges)); }
		}

		public SolidColorBrush FullMatchForeground
		{
			get { return AppSettings.fullMatchForegroundBrush; }
			set { AppSettings.fullMatchForegroundBrush = value; Settings.FullMatchForeground = value.Color; OnPropertyChanged(nameof(FullMatchForeground)); }
		}

		public SolidColorBrush FullMatchBackground
		{
			get { return AppSettings.fullMatchBackgroundBrush; }
			set { AppSettings.fullMatchBackgroundBrush = value; Settings.FullMatchBackground = value.Color; OnPropertyChanged(nameof(FullMatchBackground)); }
		}

		public SolidColorBrush PartialMatchForeground
		{
			get { return AppSettings.partialMatchForegroundBrush; }
			set { AppSettings.partialMatchForegroundBrush = value; Settings.PartialMatchForeground = value.Color; OnPropertyChanged(nameof(PartialMatchForeground)); }
		}

		public SolidColorBrush PartialMatchBackground
		{
			get { return AppSettings.partialMatchBackgroundBrush; }
			set { AppSettings.partialMatchBackgroundBrush = value; Settings.PartialMatchBackground = value.Color; OnPropertyChanged(nameof(PartialMatchBackground)); }
		}

		public SolidColorBrush DeletedForeground
		{
			get { return AppSettings.deletedForegroundBrush; }
			set { AppSettings.deletedForegroundBrush = value; Settings.DeletedForeground = value.Color; OnPropertyChanged(nameof(DeletedForeground)); }
		}

		public SolidColorBrush DeletedBackground
		{
			get { return AppSettings.deletedBackgroundBrush; }
			set { AppSettings.deletedBackgroundBrush = value; Settings.DeletedBackground = value.Color; OnPropertyChanged(nameof(DeletedBackground)); }
		}

		public SolidColorBrush NewForeground
		{
			get { return AppSettings.newForegroundBrush; }
			set { AppSettings.newForegroundBrush = value; Settings.NewForeground = value.Color; OnPropertyChanged(nameof(NewForeground)); }
		}

		public SolidColorBrush NewBackground
		{
			get { return AppSettings.newBackgrounBrush; }
			set { AppSettings.newBackgrounBrush = value; Settings.NewBackground = value.Color; OnPropertyChanged(nameof(NewBackground)); }
		}

		public FontFamily Font
		{
			get { System.Diagnostics.Debug.Print("generate font"); return new FontFamily(Settings.Font); }
			set { Settings.Font = value.ToString(); OnPropertyChanged(nameof(Font)); }
		}

		public int FontSize
		{
			get { return Settings.FontSize; }
			set { Settings.FontSize = value; OnPropertyChanged(nameof(FontSize)); }
		}

		#endregion

		#region Methods



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
