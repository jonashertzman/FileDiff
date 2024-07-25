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

	readonly DispatcherTimer timer = new();

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
		get { return "1.5"; }
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

	ObservableCollection<Line> leftFile = [];
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


	ObservableCollection<Line> rightFile = [];
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
			return AppSettings.DialogBackground;
		}
	}

	bool editMode = false;
	public bool EditMode
	{
		get { return editMode; }
		set { editMode = value; OnPropertyChanged(nameof(EditMode)); }
	}

	ObservableCollection<FileItem> leftFolder = [];
	public ObservableCollection<FileItem> LeftFolder
	{
		get { return leftFolder; }
		set { leftFolder = value; OnPropertyChangedRepaint(nameof(LeftFolder)); }
	}

	ObservableCollection<FileItem> rightFolder = [];
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

	public int MaxVerticalScroll
	{
		get
		{
			return Math.Max(maxLeftVerticalScroll, maxRightVerticalScroll);
		}
	}

	int maxLeftVerticalScroll;
	public int MaxLeftVerticalScroll
	{
		get { return maxLeftVerticalScroll; }
		set { maxLeftVerticalScroll = value; OnPropertyChanged(nameof(MaxLeftVerticalScroll)); OnPropertyChanged(nameof(MaxVerticalScroll)); }
	}

	int maxRightVerticalScroll;
	public int MaxRightVerticalScroll
	{
		get { return maxRightVerticalScroll; }
		set { maxRightVerticalScroll = value; OnPropertyChanged(nameof(MaxRightVerticalScroll)); OnPropertyChanged(nameof(MaxVerticalScroll)); }
	}

	int visibleLines;
	public int VisibleLines
	{
		get { return visibleLines; }
		set { visibleLines = value; OnPropertyChanged(nameof(VisibleLines)); OnPropertyChanged(nameof(MaxVerticalScroll)); }
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
			OnPropertyChangedRepaint(nameof(FileVisible));
			OnPropertyChangedRepaint(nameof(FolderVisible));
			OnPropertyChangedRepaint(nameof(FolderRowHeight));
			OnPropertyChangedRepaint(nameof(SplitterRowHeight));
			OnPropertyChangedRepaint(nameof(FileRowHeight));
		}
	}

	public bool FileVisible
	{
		get
		{
			return mode == CompareMode.File || mode == CompareMode.Folder && MasterDetail;
		}
	}

	public bool FolderVisible
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

			OnPropertyChangedRepaint(nameof(FileVisible));
			OnPropertyChangedRepaint(nameof(FolderVisible));
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


	public Themes Theme
	{
		get { return AppSettings.Theme; }
		set { AppSettings.Theme = value; OnPropertyChangedRepaint(null); } // Refresh all properties when changing theme
	}

	public bool LightTheme
	{
		get { return Theme == Themes.Light; }
	}


	// Folder diff colors
	public Brush FolderFullMatchForeground
	{
		get { return AppSettings.FolderFullMatchForeground; }
		set { AppSettings.FolderFullMatchForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(FolderFullMatchForeground)); }
	}

	public Brush FolderFullMatchBackground
	{
		get { return AppSettings.FolderFullMatchBackground; }
		set { AppSettings.FolderFullMatchBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(FolderFullMatchBackground)); }
	}

	public Brush FolderPartialMatchForeground
	{
		get { return AppSettings.FolderPartialMatchForeground; }
		set { AppSettings.FolderPartialMatchForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(FolderPartialMatchForeground)); }
	}

	public Brush FolderPartialMatchBackground
	{
		get { return AppSettings.FolderPartialMatchBackground; }
		set { AppSettings.FolderPartialMatchBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(FolderPartialMatchBackground)); }
	}

	public Brush FolderDeletedForeground
	{
		get { return AppSettings.FolderDeletedForeground; }
		set { AppSettings.FolderDeletedForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(FolderDeletedForeground)); }
	}

	public Brush FolderDeletedBackground
	{
		get { return AppSettings.FolderDeletedBackground; }
		set { AppSettings.FolderDeletedBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(FolderDeletedBackground)); }
	}

	public Brush FolderNewForeground
	{
		get { return AppSettings.FolderNewForeground; }
		set { AppSettings.FolderNewForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(FolderNewForeground)); }
	}

	public Brush FolderNewBackground
	{
		get { return AppSettings.FolderNewBackground; }
		set { AppSettings.FolderNewBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(FolderNewBackground)); }
	}

	public Brush FolderIgnoredForeground
	{
		get { return AppSettings.FolderIgnoredForeground; }
		set { AppSettings.FolderIgnoredForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(FolderIgnoredForeground)); }
	}

	public Brush FolderIgnoredBackground
	{
		get { return AppSettings.FolderIgnoredBackground; }
		set { AppSettings.FolderIgnoredBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(FolderIgnoredBackground)); }
	}


	// File diff colors
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

	public Brush MovedFromBackground
	{
		get { return AppSettings.MovedFromBackground; }
		set { AppSettings.MovedFromBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(MovedFromBackground)); }
	}

	public Brush MovedToBackground
	{
		get { return AppSettings.MovedToBackground; }
		set { AppSettings.MovedToBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(MovedToBackground)); }
	}


	// Editor colors
	public Brush SelectionBackground
	{
		get { return AppSettings.SelectionBackground; }
		set { AppSettings.SelectionBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(SelectionBackground)); }
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


	// UI colors
	public Brush WindowForeground
	{
		get { return AppSettings.WindowForeground; }
		set { AppSettings.WindowForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(WindowForeground)); }
	}

	public Brush DisabledForeground
	{
		get { return AppSettings.DisabledForeground; }
		set { AppSettings.DisabledForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(DisabledForeground)); }
	}

	public Brush WindowBackground
	{
		get { return AppSettings.WindowBackground; }
		set { AppSettings.WindowBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(WindowBackground)); }
	}

	public Brush DialogBackground
	{
		get { return AppSettings.DialogBackground; }
		set { AppSettings.DialogBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(DialogBackground)); }
	}

	public Brush ControlLightBackground
	{
		get { return AppSettings.ControlLightBackground; }
		set { AppSettings.ControlLightBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(ControlLightBackground)); }
	}

	public Brush ControlDarkBackground
	{
		get { return AppSettings.ControlDarkBackground; }
		set { AppSettings.ControlDarkBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(ControlDarkBackground)); }
	}

	public Brush BorderForeground
	{
		get { return AppSettings.BorderForeground; }
		set { AppSettings.BorderForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(BorderForeground)); }
	}

	public Brush BorderDarkForeground
	{
		get { return AppSettings.BorderDarkForeground; }
		set { AppSettings.BorderDarkForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(BorderDarkForeground)); }
	}

	public Brush HighlightBackground
	{
		get { return AppSettings.HighlightBackground; }
		set { AppSettings.HighlightBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(HighlightBackground)); }
	}

	public Brush HighlightBorder
	{
		get { return AppSettings.HighlightBorder; }
		set { AppSettings.HighlightBorder = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(HighlightBorder)); }
	}

	public Brush AttentionBackground
	{
		get { return AppSettings.AttentionBackground; }
		set { AppSettings.AttentionBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(AttentionBackground)); }
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
		LeftFile = [];
		LeftFileEncoding = null;

		RightFile = [];
		RightFileEncoding = null;

		LeftFolder = [];
		RightFolder = [];
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
