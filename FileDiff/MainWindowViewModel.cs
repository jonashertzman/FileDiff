using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace FileDiff;

public class MainWindowViewModel : INotifyPropertyChanged
{

	#region Members

	readonly DispatcherTimer timer = new DispatcherTimer();

	#endregion

	#region Constructor

	public MainWindowViewModel()
	{
		timer.Interval = new TimeSpan(300000);
		timer.Tick += Timer_Tick;
	}

	#endregion

	#region Properties

	public string Title
	{
		get { return "File Diff"; }
	}

	public string Version
	{
		get { return "1.4"; }
	}

	public string BuildNumber
	{
		get
		{
			DateTime buildDate = new FileInfo(Environment.ProcessPath).LastWriteTime;
			return $"{buildDate:yy}{buildDate.DayOfYear:D3}";
		}
	}

	bool newBuildAvailable = false;
	public bool NewBuildAvailable
	{
		get { return newBuildAvailable; }
		set { newBuildAvailable = value; OnPropertyChanged(nameof(NewBuildAvailable)); }
	}

	public string ApplicationName
	{
		get { return $"{Title} {Version}"; }
	}

	public string FullApplicationName
	{
		get { return $"{Title} {Version} (Build {BuildNumber})"; }
	}

	public bool CheckForUpdates
	{
		get { return AppSettings.CheckForUpdates; }
		set { AppSettings.CheckForUpdates = value; OnPropertyChanged(nameof(CheckForUpdates)); }
	}

	bool guiFrozen = false;
	public bool GuiFrozen
	{
		get { return guiFrozen; }
		set { guiFrozen = value; OnPropertyChanged(nameof(GuiFrozen)); }
	}

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
			if (LeftFileDescription != null && RightFileDescription != null && LeftFileDescription != RightFileDescription)
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

	DiffRange currentDiff;
	public DiffRange CurrentDiff
	{
		get { return currentDiff; }
		set { currentDiff = value; OnPropertyChangedRepaint(nameof(CurrentDiff)); }
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

	string leftPath = "";
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

	string rightPath = "";
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

	public bool DetectMovedLines
	{
		get { return AppSettings.DetectMovedLines; }
		set { AppSettings.DetectMovedLines = value; OnPropertyChanged(nameof(DetectMovedLines)); }
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


	public ColorTheme Theme
	{
		get { return AppSettings.Theme; }
		set { AppSettings.Theme = value; OnPropertyChangedRepaint(nameof(Theme)); }
	}


	// Text colors
	public Brush FullMatchForeground
	{
		get { return AppSettings.FullMatchForeground; }
		set { AppSettings.FullMatchForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(FullMatchForeground)); }
	}

	public Brush FullMatchBackground
	{
		get { return AppSettings.FullMatchBackground; }
		set { AppSettings.FullMatchBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(FullMatchBackground)); }
	}

	public Brush PartialMatchForeground
	{
		get { return AppSettings.PartialMatchForeground; }
		set { AppSettings.PartialMatchForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(PartialMatchForeground)); }
	}

	public Brush PartialMatchBackground
	{
		get { return AppSettings.PartialMatchBackground; }
		set { AppSettings.PartialMatchBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(PartialMatchBackground)); }
	}

	public Brush DeletedForeground
	{
		get { return AppSettings.DeletedForeground; }
		set { AppSettings.DeletedForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(DeletedForeground)); }
	}

	public Brush DeletedBackground
	{
		get { return AppSettings.DeletedBackground; }
		set { AppSettings.DeletedBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(DeletedBackground)); }
	}

	public Brush NewForeground
	{
		get { return AppSettings.NewForeground; }
		set { AppSettings.NewForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(NewForeground)); }
	}

	public Brush NewBackground
	{
		get { return AppSettings.NewBackground; }
		set { AppSettings.NewBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(NewBackground)); }
	}

	public Brush IgnoredForeground
	{
		get { return AppSettings.IgnoredForeground; }
		set { AppSettings.IgnoredForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(IgnoredForeground)); }
	}

	public Brush IgnoredBackground
	{
		get { return AppSettings.IgnoredBackground; }
		set { AppSettings.IgnoredBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(IgnoredBackground)); }
	}

	public Brush MovedFromdBackground
	{
		get { return AppSettings.MovedFromdBackground; }
		set { AppSettings.MovedFromdBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(MovedFromdBackground)); }
	}

	public Brush MovedToBackground
	{
		get { return AppSettings.MovedToBackground; }
		set { AppSettings.MovedToBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(MovedToBackground)); }
	}

	public Brush SelectionBackground
	{
		get { return AppSettings.SelectionBackground; }
		set { AppSettings.SelectionBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(SelectionBackground)); }
	}


	// GUI colors
	public Brush WindowForeground
	{
		get { return AppSettings.WindowForeground; }
		set { AppSettings.WindowForeground = value as SolidColorBrush; OnPropertyChanged(nameof(WindowForeground)); }
	}

	public Brush WindowBackground
	{
		get { return AppSettings.WindowBackgruond; }
		set { AppSettings.WindowBackgruond = value as SolidColorBrush; OnPropertyChanged(nameof(WindowBackground)); }
	}

	public Brush ControlBackground
	{
		get { return AppSettings.ControlBackground; }
		set { AppSettings.ControlBackground = value as SolidColorBrush; OnPropertyChanged(nameof(ControlBackground)); }
	}

	public Brush BorderColor
	{
		get { return AppSettings.BorderColor; }
		set { AppSettings.BorderColor = value as SolidColorBrush; OnPropertyChanged(nameof(BorderColor)); }
	}

	public Brush LineNumberColor
	{
		get { return AppSettings.LineNumberColor; }
		set { AppSettings.LineNumberColor = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(LineNumberColor)); }
	}

	public Brush SnakeColor
	{
		get { return AppSettings.SnakeColor; }
		set { AppSettings.SnakeColor = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(SnakeColor)); }
	}

	public Brush CurrentDiffColor
	{
		get { return AppSettings.CurrentDiffColor; }
		set { AppSettings.CurrentDiffColor = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(CurrentDiffColor)); }
	}


	// Font
	public FontFamily Font
	{
		get { return AppSettings.Font; }
		set
		{
			AppSettings.Font = value;
			Zoom = 0;
			OnPropertyChangedRepaint(nameof(Font));
		}
	}

	public int FontSize
	{
		get { return AppSettings.FontSize; }
		set
		{
			AppSettings.FontSize = value;
			Zoom = 0;
			OnPropertyChangedRepaint(nameof(FontSize)); OnPropertyChanged(nameof(ZoomedFontSize));
		}
	}

	public int Zoom
	{
		get { return AppSettings.Zoom; }
		set { AppSettings.Zoom = Math.Max(value, 1 - FontSize); OnPropertyChanged(nameof(Zoom)); OnPropertyChanged(nameof(ZoomedFontSize)); }
	}

	public int ZoomedFontSize
	{
		get { return Math.Max(FontSize + Zoom, 1); }
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

	public void Clear()
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
