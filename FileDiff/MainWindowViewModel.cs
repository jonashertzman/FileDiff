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

		ObservableCollection<Line> leftFile = new ObservableCollection<Line>();
		public ObservableCollection<Line> LeftFile
		{
			get { return leftFile; }
			set { leftFile = value; OnPropertyChangedRepaint(nameof(LeftFile)); }
		}

		ObservableCollection<Line> rightFile = new ObservableCollection<Line>();
		public ObservableCollection<Line> RightFile
		{
			get { return rightFile; }
			set { rightFile = value; OnPropertyChangedRepaint(nameof(RightFile)); }
		}

		ObservableCollection<FileItem> leftFolder = new ObservableCollection<FileItem>();
		public ObservableCollection<FileItem> LeftFolder
		{
			get { return leftFolder; }
			set { leftFolder = value; OnPropertyChangedRepaint(nameof(LeftFolder)); }
		}

		ObservableCollection<FileItem> rightFolder = new ObservableCollection<FileItem>();
		public ObservableCollection<FileItem> RightFolder
		{
			get { return rightFolder; }
			set { rightFolder = value; OnPropertyChangedRepaint(nameof(RightFolder)); }
		}

		int currentDiff = -1;
		public int CurrentDiff
		{
			get { return currentDiff; }
			set { currentDiff = value; OnPropertyChangedRepaint(nameof(CurrentDiff)); }
		}

		int currentDiffLength;
		public int CurrentDiffLength
		{
			get { return currentDiffLength; }
			set { currentDiffLength = value; OnPropertyChangedRepaint(nameof(currentDiffLength)); }
		}

		CompareMode mode;
		public CompareMode Mode
		{
			get { return mode; }
			set
			{
				mode = value;
				OnPropertyChangedRepaint(nameof(FileVissible));
				OnPropertyChangedRepaint(nameof(FolderVissible));
				OnPropertyChangedRepaint(nameof(FolderRowHeight));
				OnPropertyChangedRepaint(nameof(SplitterRowHeight));
				OnPropertyChangedRepaint(nameof(FileRowHeight));
			}
		}

		public bool FileVissible
		{
			get
			{
				return mode == CompareMode.File || (mode == CompareMode.Folder && MasterDetail);
			}
		}

		public bool FolderVissible
		{
			get
			{
				return mode == CompareMode.Folder;
			}
		}

		public GridLength FolderRowHeight
		{
			get
			{
				if (mode == CompareMode.File)
				{
					return new GridLength(0);
				}
				else if (MasterDetail)
				{
					return new GridLength(AppSettings.FolderRowHeight, GridUnitType.Pixel);
				}
				return new GridLength(1, GridUnitType.Star);
			}
			set { AppSettings.FolderRowHeight = value.Value; OnPropertyChanged(nameof(MasterDetail)); OnPropertyChanged(nameof(FolderRowHeight)); }
		}

		public GridLength SplitterRowHeight
		{
			get
			{
				if (mode == CompareMode.File)
				{
					return new GridLength(0);
				}
				if (MasterDetail)
				{
					return new GridLength(0, GridUnitType.Auto);
				}
				return new GridLength(0);
			}
		}

		public GridLength FileRowHeight
		{
			get
			{
				if (mode == CompareMode.Folder)
				{
					if (MasterDetail)
					{
						return new GridLength(1, GridUnitType.Star);
					}
					return new GridLength(0);
				}
				return new GridLength(1, GridUnitType.Star);
			}
		}

		string leftPath;
		public string LeftPath
		{
			get { return leftPath; }
			set
			{
				leftPath = value;

				LeftFile = new ObservableCollection<Line>();
				RightFile = new ObservableCollection<Line>();

				LeftFolder = new ObservableCollection<FileItem>();
				RightFolder = new ObservableCollection<FileItem>();

				OnPropertyChangedRepaint(nameof(LeftPath));
			}
		}

		string rightPath;
		public string RightPath
		{
			get { return rightPath; }
			set
			{
				rightPath = value;

				LeftFile = new ObservableCollection<Line>();
				RightFile = new ObservableCollection<Line>();

				LeftFolder = new ObservableCollection<FileItem>();
				RightFolder = new ObservableCollection<FileItem>();

				OnPropertyChangedRepaint(nameof(RightPath));
			}
		}

		public bool IgnoreWhiteSpace
		{
			get { return AppSettings.IgnoreWhiteSpace; }
			set { AppSettings.IgnoreWhiteSpace = value; OnPropertyChangedRepaint(nameof(IgnoreWhiteSpace)); }
		}

		public bool ShowLineChanges
		{
			get { return AppSettings.ShowLineChanges; }
			set { AppSettings.ShowLineChanges = value; OnPropertyChangedRepaint(nameof(ShowLineChanges)); }
		}

		public bool MasterDetail
		{
			get { return AppSettings.MasterDetail; }
			set
			{
				AppSettings.MasterDetail = value;

				OnPropertyChanged(nameof(MasterDetail));
				OnPropertyChanged(nameof(Mode));

				OnPropertyChangedRepaint(nameof(FileVissible));
				OnPropertyChangedRepaint(nameof(FolderVissible));
				OnPropertyChangedRepaint(nameof(FolderRowHeight));
				OnPropertyChangedRepaint(nameof(SplitterRowHeight));
				OnPropertyChangedRepaint(nameof(FileRowHeight));
			}
		}

		public double NameColumnWidth
		{
			get { return AppSettings.NameColumnWidth; }
			set { AppSettings.NameColumnWidth = value; OnPropertyChangedRepaint(nameof(NameColumnWidth)); }
		}

		public double SizeColumnWidth
		{
			get { return AppSettings.SizeColumnWidth; }
			set { AppSettings.SizeColumnWidth = value; OnPropertyChangedRepaint(nameof(SizeColumnWidth)); }
		}

		public double DateColumnWidth
		{
			get { return AppSettings.DateColumnWidth; }
			set { AppSettings.DateColumnWidth = value; OnPropertyChangedRepaint(nameof(DateColumnWidth)); }
		}

		public SolidColorBrush FullMatchForeground
		{
			get { return AppSettings.FullMatchForeground; }
			set { AppSettings.FullMatchForeground = value; OnPropertyChangedRepaint(nameof(FullMatchForeground)); }
		}

		public SolidColorBrush FullMatchBackground
		{
			get { return AppSettings.FullMatchBackground; }
			set { AppSettings.FullMatchBackground = value; OnPropertyChangedRepaint(nameof(FullMatchBackground)); }
		}

		public SolidColorBrush PartialMatchForeground
		{
			get { return AppSettings.PartialMatchForeground; }
			set { AppSettings.PartialMatchForeground = value; OnPropertyChangedRepaint(nameof(PartialMatchForeground)); }
		}

		public SolidColorBrush PartialMatchBackground
		{
			get { return AppSettings.PartialMatchBackground; }
			set { AppSettings.PartialMatchBackground = value; OnPropertyChangedRepaint(nameof(PartialMatchBackground)); }
		}

		public SolidColorBrush DeletedForeground
		{
			get { return AppSettings.DeletedForeground; }
			set { AppSettings.DeletedForeground = value; OnPropertyChangedRepaint(nameof(DeletedForeground)); }
		}

		public SolidColorBrush DeletedBackground
		{
			get { return AppSettings.DeletedBackground; }
			set { AppSettings.DeletedBackground = value; OnPropertyChangedRepaint(nameof(DeletedBackground)); }
		}

		public SolidColorBrush NewForeground
		{
			get { return AppSettings.NewForeground; }
			set { AppSettings.NewForeground = value; OnPropertyChangedRepaint(nameof(NewForeground)); }
		}

		public SolidColorBrush NewBackground
		{
			get { return AppSettings.NewBackground; }
			set { AppSettings.NewBackground = value; OnPropertyChangedRepaint(nameof(NewBackground)); }
		}

		public FontFamily Font
		{
			get { return AppSettings.Font; }
			set { AppSettings.Font = value; OnPropertyChangedRepaint(nameof(Font)); }
		}

		public int FontSize
		{
			get { return AppSettings.FontSize; }
			set { AppSettings.FontSize = value; OnPropertyChangedRepaint(nameof(FontSize)); }
		}

		public int TabSize
		{
			get { return AppSettings.TabSize; }
			set { AppSettings.TabSize = Math.Max(1, value); OnPropertyChangedRepaint(nameof(TabSize)); }
		}

		int updateTrigger;
		public int UpdateTrigger
		{
			get { return updateTrigger; }
			set { updateTrigger = value; OnPropertyChanged(nameof(UpdateTrigger)); }
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChangedRepaint(string name)
		{
			UpdateTrigger++;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}
}
