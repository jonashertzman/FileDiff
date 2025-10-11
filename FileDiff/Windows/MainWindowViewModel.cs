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

	public bool NewBuildAvailable
	{
		get;
		set { field = value; OnPropertyChanged(nameof(NewBuildAvailable)); }
	} = false;

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

	public bool GuiFrozen
	{
		get;
		set { field = value; OnPropertyChanged(nameof(GuiFrozen)); }
	} = false;


	public ObservableCollection<Line> LeftFile
	{
		get;
		set { field = value; OnPropertyChangedRepaint(nameof(LeftFile)); }
	} = [];

	public FileEncoding LeftFileEncoding
	{
		get;
		set { field = value; OnPropertyChanged(nameof(LeftFileEncoding)); OnPropertyChanged(nameof(LeftFileDescription)); OnPropertyChanged(nameof(EncodingBackground)); }
	} = null;

	public bool LeftFileDirty
	{
		get { return field || LeftFileEdited; }
		set { field = value; OnPropertyChanged(nameof(LeftFileDirty)); }
	} = false;

	public string LeftFileDescription
	{
		get { return LeftFileEncoding?.ToString(); }
	}

	public bool LeftFileEdited
	{
		get;
		set { field = value; OnPropertyChanged(nameof(LeftFileEdited)); OnPropertyChanged(nameof(LeftFileDirty)); }
	} = false;

	public bool FindPanelRight
	{
		get;
		set { field = value; OnPropertyChanged(nameof(FindPanelRight)); }
	} = false;

	public ObservableCollection<Line> RightFile
	{
		get;
		set { field = value; OnPropertyChangedRepaint(nameof(RightFile)); }
	} = [];

	public FileEncoding RightFileEncoding
	{
		get;
		set { field = value; OnPropertyChanged(nameof(RightFileEncoding)); OnPropertyChanged(nameof(RightFileDescription)); OnPropertyChanged(nameof(EncodingBackground)); }
	} = null;

	public bool RightFileDirty
	{
		get { return field || RightFileEdited; }
		set { field = value; OnPropertyChanged(nameof(RightFileDirty)); }
	} = false;

	public string RightFileDescription
	{
		get { return RightFileEncoding?.ToString(); }
	}

	public bool RightFileEdited
	{
		get;
		set { field = value; OnPropertyChanged(nameof(RightFileEdited)); OnPropertyChanged(nameof(RightFileDirty)); }
	} = false;


	public Brush EncodingBackground
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

	public bool EditMode
	{
		get;
		set { field = value; OnPropertyChanged(nameof(EditMode)); }
	} = false;

	public ObservableCollection<FileItem> LeftFolder
	{
		get;
		set { field = value; OnPropertyChangedRepaint(nameof(LeftFolder)); }
	} = [];

	public ObservableCollection<FileItem> RightFolder
	{
		get;
		set { field = value; OnPropertyChangedRepaint(nameof(RightFolder)); }
	} = [];

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
			return Math.Max(MaxLeftVerticalScroll, MaxRightVerticalScroll);
		}
	}

	public int MaxLeftVerticalScroll
	{
		get;
		set { field = value; OnPropertyChanged(nameof(MaxLeftVerticalScroll)); OnPropertyChanged(nameof(MaxVerticalScroll)); }
	}

	public int MaxRightVerticalScroll
	{
		get;
		set { field = value; OnPropertyChanged(nameof(MaxRightVerticalScroll)); OnPropertyChanged(nameof(MaxVerticalScroll)); }
	}

	public int VisibleLines
	{
		get;
		set { field = value; OnPropertyChanged(nameof(VisibleLines)); OnPropertyChanged(nameof(MaxVerticalScroll)); }
	}

	public DiffRange CurrentDiff
	{
		get;
		set { field = value; OnPropertyChangedRepaint(nameof(CurrentDiff)); }
	}

	public CompareMode Mode
	{
		get;
		set
		{
			field = value;
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
			return Mode == CompareMode.File || Mode == CompareMode.Folder && MasterDetail;
		}
	}

	public bool FolderVisible
	{
		get
		{
			return Mode == CompareMode.Folder;
		}
	}

	public GridLength FolderRowHeight
	{
		get
		{
			if (Mode == CompareMode.File)
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
			if (Mode == CompareMode.File)
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
			if (Mode == CompareMode.Folder)
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

	public string LeftPath
	{
		get;
		set
		{
			field = value;
			OnPropertyChangedRepaint(nameof(LeftPath));
			Clear();
		}
	} = "";

	public string RightPath
	{
		get;
		set
		{
			field = value;
			OnPropertyChangedRepaint(nameof(RightPath));
			Clear();
		}
	} = "";

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

	public int UpdateTrigger
	{
		get;
		set { field = value; OnPropertyChanged(nameof(UpdateTrigger)); }
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
