using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace FileDiff
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{

		#region Members

		DispatcherTimer timer = new DispatcherTimer();

		#endregion

		#region Constructor

		public MainWindowViewModel()
		{
			timer.Interval = new TimeSpan(300000);
			timer.Tick += Timer_Tick;
		}

		#endregion

		#region Properties

		ObservableCollection<Line> leftFile = new ObservableCollection<Line>();
		public ObservableCollection<Line> LeftFile
		{
			get { return leftFile; }
			set { leftFile = value; OnPropertyChangedRepaint(nameof(LeftFile)); }
		}

		FileEncoding leftFileEncoding = null;
		public FileEncoding LeftFileEncoding
		{
			get { return leftFileEncoding; }
			set { leftFileEncoding = value; OnPropertyChanged(nameof(LeftFileEncoding)); OnPropertyChanged(nameof(LeftFileDescription)); OnPropertyChanged(nameof(EncodingBackground)); }
		}

		bool leftFileDirty = false;
		public bool LeftFileDirty
		{
			get { return leftFileDirty || leftFileEdited; }
			set { leftFileDirty = value; OnPropertyChanged(nameof(LeftFileDirty)); }
		}

		public string LeftFileDescription
		{
			get { return LeftFileEncoding?.ToString(); }
		}

		bool leftFileEdited = false;
		public bool LeftFileEdited
		{
			get { return leftFileEdited; }
			set { leftFileEdited = value; OnPropertyChanged(nameof(LeftFileEdited)); OnPropertyChanged(nameof(LeftFileDirty)); }
		}

		bool findPanelRight = false;
		public bool FindPanelRight
		{
			get { return findPanelRight; }
			set { findPanelRight = value; OnPropertyChanged(nameof(FindPanelRight)); }
		}


		ObservableCollection<Line> rightFile = new ObservableCollection<Line>();
		public ObservableCollection<Line> RightFile
		{
			get { return rightFile; }
			set { rightFile = value; OnPropertyChangedRepaint(nameof(RightFile)); }
		}

		FileEncoding rightFileEncoding = null;
		public FileEncoding RightFileEncoding
		{
			get { return rightFileEncoding; }
			set { rightFileEncoding = value; OnPropertyChanged(nameof(RightFileEncoding)); OnPropertyChanged(nameof(RightFileDescription)); OnPropertyChanged(nameof(EncodingBackground)); }
		}

		bool rightFileDirty = false;
		public bool RightFileDirty
		{
			get { return rightFileDirty || RightFileEdited; }
			set { rightFileDirty = value; OnPropertyChanged(nameof(RightFileDirty)); }
		}

		public string RightFileDescription
		{
			get { return RightFileEncoding?.ToString(); }
		}

		bool rightFileEdited = false;
		public bool RightFileEdited
		{
			get { return rightFileEdited; }
			set { rightFileEdited = value; OnPropertyChanged(nameof(RightFileEdited)); OnPropertyChanged(nameof(RightFileDirty)); }
		}


		public SolidColorBrush EncodingBackground
		{
			get
			{
				if (LeftFileDescription != RightFileDescription)
				{
					return AppSettings.PartialMatchBackground;
				}
				return SystemColors.ControlBrush;
			}
		}

		bool editMode = false;
		public bool EditMode
		{
			get { return editMode; }
			set { editMode = value; OnPropertyChanged(nameof(EditMode)); }
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

		public ObservableCollection<TextAttribute> IgnoredFolders
		{
			get { return AppSettings.IgnoredFolders; }
			set { AppSettings.IgnoredFolders = value; OnPropertyChangedRepaint(nameof(IgnoredFolders)); }
		}

		public ObservableCollection<TextAttribute> IgnoredFiles
		{
			get { return AppSettings.IgnoredFiles; }
			set { AppSettings.IgnoredFiles = value; OnPropertyChangedRepaint(nameof(IgnoredFiles)); }
		}

		public int MaxVerialcalScroll
		{
			get
			{
				return Math.Max(maxLeftVerialcalScroll, maxRightVerialcalScroll);
			}
		}

		int maxLeftVerialcalScroll;
		public int MaxLeftVerialcalScroll
		{
			get { return maxLeftVerialcalScroll; }
			set { maxLeftVerialcalScroll = value; OnPropertyChanged(nameof(MaxLeftVerialcalScroll)); OnPropertyChanged(nameof(MaxVerialcalScroll)); }
		}

		int maxRightVerialcalScroll;
		public int MaxRightVerialcalScroll
		{
			get { return maxRightVerialcalScroll; }
			set { maxRightVerialcalScroll = value; OnPropertyChanged(nameof(MaxRightVerialcalScroll)); OnPropertyChanged(nameof(MaxVerialcalScroll)); }
		}

		int visibleLines;
		public int VisibleLines
		{
			get { return visibleLines; }
			set { visibleLines = value; OnPropertyChanged(nameof(VisibleLines)); OnPropertyChanged(nameof(MaxVerialcalScroll)); }
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
				OnPropertyChangedRepaint(nameof(LeftPath));
				Clear();
			}
		}

		string rightPath;
		public string RightPath
		{
			get { return rightPath; }
			set
			{
				rightPath = value;
				OnPropertyChangedRepaint(nameof(RightPath));
				Clear();
			}
		}

		public bool IgnoreWhiteSpace
		{
			get { return AppSettings.IgnoreWhiteSpace; }
			set { AppSettings.IgnoreWhiteSpace = value; OnPropertyChangedRepaint(nameof(IgnoreWhiteSpace)); }
		}

		public bool ShowWhiteSpaceCharacters
		{
			get { return AppSettings.ShowWhiteSpaceCharacters; }
			set { AppSettings.ShowWhiteSpaceCharacters = value; OnPropertyChangedRepaint(nameof(ShowWhiteSpaceCharacters)); }
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
			set { AppSettings.NameColumnWidth = value; OnPropertyChangedSlowRepaint(nameof(NameColumnWidth)); }
		}

		public double SizeColumnWidth
		{
			get { return AppSettings.SizeColumnWidth; }
			set { AppSettings.SizeColumnWidth = value; OnPropertyChangedSlowRepaint(nameof(SizeColumnWidth)); }
		}

		public double DateColumnWidth
		{
			get { return AppSettings.DateColumnWidth; }
			set { AppSettings.DateColumnWidth = value; OnPropertyChangedSlowRepaint(nameof(DateColumnWidth)); }
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

		public SolidColorBrush IgnoredForeground
		{
			get { return AppSettings.IgnoredForeground; }
			set { AppSettings.IgnoredForeground = value; OnPropertyChangedRepaint(nameof(IgnoredForeground)); }
		}

		public SolidColorBrush IgnoredBackground
		{
			get { return AppSettings.IgnoredBackground; }
			set { AppSettings.IgnoredBackground = value; OnPropertyChangedRepaint(nameof(IgnoredBackground)); }
		}

		public SolidColorBrush SelectionBackground
		{
			get { return AppSettings.SelectionBackground; }
			set { AppSettings.SelectionBackground = value; OnPropertyChangedRepaint(nameof(SelectionBackground)); }
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

		#region Methods

		private void Clear()
		{
			LeftFile = new ObservableCollection<Line>();
			LeftFileEncoding = null;

			RightFile = new ObservableCollection<Line>();
			RightFileEncoding = null;

			LeftFolder = new ObservableCollection<FileItem>();
			RightFolder = new ObservableCollection<FileItem>();
		}

		#endregion

		#region Events

		private void Timer_Tick(object sender, EventArgs e)
		{
			timer.Stop();
			UpdateTrigger++;
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChangedSlowRepaint(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

			if (!timer.IsEnabled)
			{
				timer.Start();
			}
		}

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
