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

		ObservableCollection<FileItem> leftFolder = new ObservableCollection<FileItem>();
		public ObservableCollection<FileItem> LeftFolder
		{
			get { return leftFolder; }
			set { leftFolder = value; OnPropertyChanged(nameof(LeftFolder)); }
		}

		ObservableCollection<FileItem> rightFolder = new ObservableCollection<FileItem>();
		public ObservableCollection<FileItem> RightFolder
		{
			get { return rightFolder; }
			set { rightFolder = value; OnPropertyChanged(nameof(RightFolder)); }
		}

		int currentDiff = -1;
		public int CurrentDiff
		{
			get { return currentDiff; }
			set { currentDiff = value; OnPropertyChanged(nameof(CurrentDiff)); }
		}

		int currentDiffLength;
		public int CurrentDiffLength
		{
			get { return currentDiffLength; }
			set { currentDiffLength = value; OnPropertyChanged(nameof(currentDiffLength)); }
		}

		bool fileMode;
		public bool FileMode
		{
			get { return fileMode; }
			set { fileMode = value; OnPropertyChanged(nameof(FolderView)); OnPropertyChanged(nameof(FileView)); }
		}

		public Visibility FileView
		{
			get { return fileMode ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility FolderView
		{
			get { return fileMode ? Visibility.Collapsed : Visibility.Visible; }
		}

		string leftPath;
		public string LeftPath
		{
			get { return leftPath; }
			set
			{
				leftPath = value;

				LeftSide = new ObservableCollection<Line>();
				RightSide = new ObservableCollection<Line>();

				OnPropertyChanged(nameof(LeftPath));
			}
		}

		string rightPath;
		public string RightPath
		{
			get { return rightPath; }
			set
			{
				rightPath = value;

				LeftSide = new ObservableCollection<Line>();
				RightSide = new ObservableCollection<Line>();

				OnPropertyChanged(nameof(RightPath));
			}
		}

		public bool IgnoreWhiteSpace
		{
			get { return AppSettings.IgnoreWhiteSpace; }
			set { AppSettings.IgnoreWhiteSpace = value; OnPropertyChanged(nameof(IgnoreWhiteSpace)); }
		}

		public bool ShowLineChanges
		{
			get { return AppSettings.ShowLineChanges; }
			set { AppSettings.ShowLineChanges = value; OnPropertyChanged(nameof(ShowLineChanges)); }
		}

		public SolidColorBrush FullMatchForeground
		{
			get { return AppSettings.FullMatchForeground; }
			set { AppSettings.FullMatchForeground = value; OnPropertyChanged(nameof(FullMatchForeground)); }
		}

		public SolidColorBrush FullMatchBackground
		{
			get { return AppSettings.FullMatchBackground; }
			set { AppSettings.FullMatchBackground = value; OnPropertyChanged(nameof(FullMatchBackground)); }
		}

		public SolidColorBrush PartialMatchForeground
		{
			get { return AppSettings.PartialMatchForeground; }
			set { AppSettings.PartialMatchForeground = value; OnPropertyChanged(nameof(PartialMatchForeground)); }
		}

		public SolidColorBrush PartialMatchBackground
		{
			get { return AppSettings.PartialMatchBackground; }
			set { AppSettings.PartialMatchBackground = value; OnPropertyChanged(nameof(PartialMatchBackground)); }
		}

		public SolidColorBrush DeletedForeground
		{
			get { return AppSettings.DeletedForeground; }
			set { AppSettings.DeletedForeground = value; OnPropertyChanged(nameof(DeletedForeground)); }
		}

		public SolidColorBrush DeletedBackground
		{
			get { return AppSettings.DeletedBackground; }
			set { AppSettings.DeletedBackground = value; OnPropertyChanged(nameof(DeletedBackground)); }
		}

		public SolidColorBrush NewForeground
		{
			get { return AppSettings.NewForeground; }
			set { AppSettings.NewForeground = value; OnPropertyChanged(nameof(NewForeground)); }
		}

		public SolidColorBrush NewBackground
		{
			get { return AppSettings.NewBackground; }
			set { AppSettings.NewBackground = value; OnPropertyChanged(nameof(NewBackground)); }
		}

		public FontFamily Font
		{
			get { return AppSettings.Font; }
			set { AppSettings.Font = value; OnPropertyChanged(nameof(Font)); }
		}

		public int FontSize
		{
			get { return AppSettings.FontSize; }
			set { AppSettings.FontSize = value; OnPropertyChanged(nameof(FontSize)); }
		}

		public int TabSize
		{
			get { return AppSettings.TabSize; }
			set { AppSettings.TabSize = Math.Max(1, value); OnPropertyChanged(nameof(TabSize)); }
		}

		int updateTrigger;
		public int UpdateTrigger
		{
			get { return updateTrigger; }
			set { updateTrigger = value; OnPropertyChangedNoRefresh(nameof(UpdateTrigger)); }
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			UpdateTrigger++;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public void OnPropertyChangedNoRefresh(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}
}
