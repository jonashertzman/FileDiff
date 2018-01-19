using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace FileDiff
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{

		#region Properties

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

		bool fileMode;
		public bool FileMode
		{
			get { return fileMode; }
			set { fileMode = value; OnPropertyChanged(nameof(FolderView)); OnPropertyChanged(nameof(FileView)); }
		}

		public Visibility FileView
		{
			get { return fileMode ? Visibility.Visible : Visibility.Hidden; }
		}

		public Visibility FolderView
		{
			get { return fileMode ? Visibility.Hidden : Visibility.Visible; }
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
			get { return AppSettings.Settings.IgnoreWhiteSpace; }
			set { AppSettings.Settings.IgnoreWhiteSpace = value; OnPropertyChanged(nameof(IgnoreWhiteSpace)); }
		}

		public bool ShowLineChanges
		{
			get { return AppSettings.Settings.ShowLineChanges; }
			set { AppSettings.Settings.ShowLineChanges = value; OnPropertyChanged(nameof(ShowLineChanges)); }
		}

		public SolidColorBrush FullMatchForeground
		{
			get { return AppSettings.fullMatchForegroundBrush; }
			set { AppSettings.fullMatchForegroundBrush = value; AppSettings.Settings.FullMatchForeground = value.Color; OnPropertyChanged(nameof(FullMatchForeground)); }
		}

		public SolidColorBrush FullMatchBackground
		{
			get { return AppSettings.fullMatchBackgroundBrush; }
			set { AppSettings.fullMatchBackgroundBrush = value; AppSettings.Settings.FullMatchBackground = value.Color; OnPropertyChanged(nameof(FullMatchBackground)); }
		}

		public SolidColorBrush PartialMatchForeground
		{
			get { return AppSettings.partialMatchForegroundBrush; }
			set { AppSettings.partialMatchForegroundBrush = value; AppSettings.Settings.PartialMatchForeground = value.Color; OnPropertyChanged(nameof(PartialMatchForeground)); }
		}

		public SolidColorBrush PartialMatchBackground
		{
			get { return AppSettings.partialMatchBackgroundBrush; }
			set { AppSettings.partialMatchBackgroundBrush = value; AppSettings.Settings.PartialMatchBackground = value.Color; OnPropertyChanged(nameof(PartialMatchBackground)); }
		}

		public SolidColorBrush DeletedForeground
		{
			get { return AppSettings.deletedForegroundBrush; }
			set { AppSettings.deletedForegroundBrush = value; AppSettings.Settings.DeletedForeground = value.Color; OnPropertyChanged(nameof(DeletedForeground)); }
		}

		public SolidColorBrush DeletedBackground
		{
			get { return AppSettings.deletedBackgroundBrush; }
			set { AppSettings.deletedBackgroundBrush = value; AppSettings.Settings.DeletedBackground = value.Color; OnPropertyChanged(nameof(DeletedBackground)); }
		}

		public SolidColorBrush NewForeground
		{
			get { return AppSettings.newForegroundBrush; }
			set { AppSettings.newForegroundBrush = value; AppSettings.Settings.NewForeground = value.Color; OnPropertyChanged(nameof(NewForeground)); }
		}

		public SolidColorBrush NewBackground
		{
			get { return AppSettings.newBackgrounBrush; }
			set { AppSettings.newBackgrounBrush = value; AppSettings.Settings.NewBackground = value.Color; OnPropertyChanged(nameof(NewBackground)); }
		}

		public FontFamily Font
		{
			get { System.Diagnostics.Debug.Print("generate font"); return new FontFamily(AppSettings.Settings.Font); }
			set { AppSettings.Settings.Font = value.ToString(); OnPropertyChanged(nameof(Font)); }
		}

		public int FontSize
		{
			get { return AppSettings.Settings.FontSize; }
			set { AppSettings.Settings.FontSize = value; OnPropertyChanged(nameof(FontSize)); }
		}

		public int TabSize
		{
			get { return AppSettings.Settings.TabSize; }
			set { AppSettings.Settings.TabSize = Math.Max(1, value); OnPropertyChanged(nameof(TabSize)); }
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
