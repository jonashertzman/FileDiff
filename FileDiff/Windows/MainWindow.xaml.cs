using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FileDiff;

public partial class MainWindow : Window
{

	#region Members

	MainWindowViewModel ViewModel { get; } = new MainWindowViewModel();

	List<FileItem> folderDiffItems = [];

	List<DiffRange> fileDiffs = [];
	int currentDiffIndex = -1;

	bool renderComplete = false;

	DiffControl activeDiff;

	string leftSelection = "";
	string rightSelection = "";

	List<Line> canceledLeftLines = null;
	List<Line> canceledRightLines = null;

	readonly DispatcherTimer progressTimer = new();

	#endregion

	#region Constructor

	public MainWindow()
	{
		InitializeComponent();

		progressTimer.Interval = new TimeSpan(0, 0, 0, 1);
		progressTimer.Tick += ProgressTimer_Tick;

		DataContext = ViewModel;

		SearchPanel.Visibility = Visibility.Collapsed;
		activeDiff = LeftDiff;

		Log.mainWindow = this;
	}

	#endregion

	#region Properties

	private bool NoManualEdit
	{
		get
		{
			return !ViewModel.LeftFileEdited && !ViewModel.RightFileEdited;
		}
	}

	#endregion

	#region Methods

	private void Compare()
	{
		rightSelection = "";
		leftSelection = "";
		ViewModel.LeftPath = ViewModel.LeftPath.Trim();
		ViewModel.RightPath = ViewModel.RightPath.Trim();
		TextBoxLeftPath.Error = false;
		TextBoxRightPath.Error = false;

		bool? firstResult = true;

		if (ViewModel.LeftPath == "")
		{
			BrowseFolderWindow browseLeft = new() { DataContext = ViewModel, Owner = this, Title = "Select Left Path" };
			browseLeft.ShowDialog();
			firstResult = browseLeft.DialogResult;

			if (browseLeft.DialogResult == true)
			{
				ViewModel.LeftPath = browseLeft.SelectedPath;
			}
		}

		if (ViewModel.RightPath == "" && firstResult != false)
		{
			BrowseFolderWindow browseRight = new() { DataContext = ViewModel, Owner = this, Title = "Select Right Path" };
			browseRight.ShowDialog();

			if (browseRight.DialogResult == true)
			{
				ViewModel.RightPath = browseRight.SelectedPath;
			}
		}

		if (File.Exists(ViewModel.LeftPath))
		{
			if (File.Exists(ViewModel.RightPath))
			{
				ViewModel.Mode = CompareMode.File;

				CompareFiles();
			}
			else
			{
				TextBoxRightPath.Error = true;
			}
		}
		else if (Directory.Exists(ViewModel.LeftPath))
		{
			if (Directory.Exists(ViewModel.RightPath))
			{
				ViewModel.Mode = CompareMode.Folder;

				CompareDirectories();
			}
			else
			{
				TextBoxRightPath.Error = true;
			}
		}
		else
		{
			TextBoxLeftPath.Error = true;
			if (!(File.Exists(ViewModel.RightPath) || Directory.Exists(ViewModel.RightPath)))
			{
				TextBoxRightPath.Error = true;
			}
		}
	}

	private void CompareFiles()
	{
		Debug.Print("--- CompareFiles ---");

		string leftPath = "";
		string rightPath = "";

		ProgressPanel.Visibility = Visibility.Hidden;

		if (ViewModel.Mode == CompareMode.File)
		{
			leftPath = ViewModel.LeftPath;
			rightPath = ViewModel.RightPath;
		}
		else if (ViewModel.MasterDetail && LeftFolder.SelectedFile != null && RightFolder.SelectedFile != null)
		{
			leftPath = LeftFolder.SelectedFile.Path;
			rightPath = RightFolder.SelectedFile.Path;
		}

		List<Line> leftLines = null;
		List<Line> rightLines = null;

		ViewModel.LeftFileEncoding = null;
		ViewModel.RightFileEncoding = null;

		try
		{
			if (File.Exists(leftPath))
			{
				leftLines = [];
				leftSelection = leftPath;
				ViewModel.LeftFileEncoding = Unicode.GetEncoding(leftPath);
				ViewModel.LeftFileDirty = false;

				int i = 0;
				foreach (string s in File.ReadAllLines(leftPath, ViewModel.LeftFileEncoding.Type))
				{
					leftLines.Add(new Line() { Type = TextState.Deleted, Text = s, LineIndex = i++ });
				}
			}

			if (File.Exists(rightPath))
			{
				rightLines = [];
				rightSelection = rightPath;
				ViewModel.RightFileEncoding = Unicode.GetEncoding(rightPath);
				ViewModel.RightFileDirty = false;

				int i = 0;
				foreach (string s in File.ReadAllLines(rightPath, ViewModel.RightFileEncoding.Type))
				{
					rightLines.Add(new Line() { Type = TextState.New, Text = s, LineIndex = i++ });
				}
			}
		}
		catch (Exception e)
		{
			MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			return;
		}

		if (leftLines?.Count > 0 && rightLines?.Count > 0)
		{
			ViewModel.GuiFrozen = true;

			ProgressBarCompare.Value = 0;
			ProgressBarCompare.Maximum = leftLines.Count + rightLines.Count;

			canceledLeftLines = leftLines;
			canceledRightLines = rightLines;

			BackgroundCompare.progressHandler = new Progress<int>(CompareStatusUpdate);
			Task.Run(() => BackgroundCompare.MatchFiles(leftLines, rightLines)).ContinueWith(CompareFilesFinished, TaskScheduler.FromCurrentSynchronizationContext());

			progressTimer.Start();
		}
		else
		{
			ViewModel.LeftFile = leftLines == null ? [] : new ObservableCollection<Line>(leftLines);
			ViewModel.RightFile = rightLines == null ? [] : new ObservableCollection<Line>(rightLines);
			InitNavigationState();
		}
	}

	private void CompareFilesFinished(Task<Tuple<List<Line>, List<Line>, TimeSpan>> task)
	{
		Debug.Print("--- CompareFilesFinished ---");

		progressTimer.Stop();
		ViewModel.GuiFrozen = false;

		if (!BackgroundCompare.CompareCancelled)
		{
			ViewModel.LeftFile = new ObservableCollection<Line>(task.Result.Item1);
			ViewModel.RightFile = new ObservableCollection<Line>(task.Result.Item2);
			Statusbar.Text = $"Compare time {Utils.TimeSpanToShortString(task.Result.Item3)}";
		}
		else
		{
			ViewModel.LeftFile = new ObservableCollection<Line>(canceledLeftLines);
			ViewModel.RightFile = new ObservableCollection<Line>(canceledRightLines);
			Statusbar.Text = $"Compare cancelled";
		}

		canceledLeftLines = null;
		canceledRightLines = null;

		LeftDiff.Focus();
		InitNavigationState();
	}

	private void CompareStatusUpdate(int progress)
	{
		ProgressBarCompare.Value = progress;
	}

	private void CompareDirectories()
	{
		Debug.Print("--- CompareDirectories ---");

		ProgressPanel.Visibility = Visibility.Hidden;

		Stopwatch stopwatch = new();
		stopwatch.Start();

		ViewModel.Clear();

		ViewModel.GuiFrozen = true;

		ProgressBarCompare.Value = 0;
		ProgressBarCompare.Maximum = 'Z' - 'A';

		string leftPath = ViewModel.LeftPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		string rightPath = ViewModel.RightPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

		BackgroundCompare.progressHandler = new Progress<int>(CompareStatusUpdate);
		Task.Run(() => BackgroundCompare.CompareDirectories(leftPath, rightPath)).ContinueWith(CompareDirectoriesFinished, TaskScheduler.FromCurrentSynchronizationContext());

		progressTimer.Start();
	}

	private void CompareDirectoriesFinished(Task<Tuple<ObservableCollection<FileItem>, ObservableCollection<FileItem>, TimeSpan>> task)
	{
		Debug.Print("--- CompareDirectoriesFinished ---");

		progressTimer.Stop();
		ViewModel.GuiFrozen = false;

		if (!BackgroundCompare.CompareCancelled)
		{
			LeftFolder.Init();
			RightFolder.Init();

			ViewModel.LeftFolder = task.Result.Item1;
			ViewModel.RightFolder = task.Result.Item2;
			Statusbar.Text = $"Compare time {Utils.TimeSpanToShortString(task.Result.Item3)}";

			folderDiffItems = [];
			GetFolderDiffItems(ViewModel.RightFolder, folderDiffItems);

		}
		else
		{
			ViewModel.LeftFile = [];
			ViewModel.RightFile = [];
			Statusbar.Text = $"Compare cancelled";
		}

		LeftFolder.Focus();
	}

	private void InitNavigationState()
	{
		ViewModel.EditMode = false;
		ViewModel.LeftFileDirty = false;
		ViewModel.LeftFileEdited = false;
		ViewModel.RightFileDirty = false;
		ViewModel.RightFileEdited = false;
		currentDiffIndex = -1;

		UpdateDiffRanges();

		LeftDiff.Init();
		RightDiff.Init();

		VerticalFileScrollbar.Value = 0;
		LeftHorizontalScrollbar.Value = 0;

		MoveToFirstDiff();
	}

	private void UpdateDiffRanges()
	{
		fileDiffs = [];

		DiffRange diffRange = null;

		for (int i = 0; i < ViewModel.LeftFile.Count; i++)
		{
			if (ViewModel.LeftFile[i].Type != TextState.FullMatch)
			{
				diffRange ??= new DiffRange() { Start = i };
			}

			if (diffRange != null && ViewModel.LeftFile[i].DiffId != ViewModel.LeftFile[diffRange.Start].DiffId)
			{
				diffRange.Length = i - diffRange.Start;
				fileDiffs.Add(diffRange);
				if (ViewModel.LeftFile[i].Type != TextState.FullMatch)
				{
					diffRange = new DiffRange() { Start = i };
				}
				else
				{
					diffRange = null;
				}
			}

			if (i == ViewModel.LeftFile.Count - 1 && diffRange != null)
			{
				diffRange.Length = ViewModel.LeftFile.Count - diffRange.Start;
				fileDiffs.Add(diffRange);
			}
		}

		foreach (DiffRange d in fileDiffs)
		{
			if (ViewModel.LeftFile[d.Start].Type == TextState.MovedFrom)
			{
				d.Offset = ViewModel.LeftFile[d.Start].DisplayOffset;
			}
			else if (ViewModel.LeftFile[d.Start].Type == TextState.MovedFiller)
			{
				d.Offset = ViewModel.RightFile[d.Start].DisplayOffset;
			}
		}
	}

	private void MoveToFirstDiff()
	{
		if (fileDiffs.Count > 0)
		{
			currentDiffIndex = 0;
			ViewModel.CurrentDiff = fileDiffs[currentDiffIndex];
			CenterOnLine(ViewModel.CurrentDiff.Start);
		}
	}

	private void MoveToLastDiff()
	{
		if (fileDiffs.Count > 0)
		{
			currentDiffIndex = fileDiffs.Count - 1;
			ViewModel.CurrentDiff = fileDiffs[currentDiffIndex];
			CenterOnLine(ViewModel.CurrentDiff.Start);
		}
	}

	private void MoveToPreviousDiff()
	{
		if (currentDiffIndex == -1)
		{
			MoveToLastDiff();
		}
		else
		{
			if (currentDiffIndex > 0)
			{
				ViewModel.CurrentDiff = fileDiffs[--currentDiffIndex];
				CenterOnLine(ViewModel.CurrentDiff.Start);
			}
		}
	}

	private void MoveToNextDiff()
	{
		if (currentDiffIndex == -1)
		{
			MoveToFirstDiff();
		}
		else
		{
			if (currentDiffIndex < fileDiffs.Count - 1)
			{
				ViewModel.CurrentDiff = fileDiffs[++currentDiffIndex];
				CenterOnLine(ViewModel.CurrentDiff.Start);
			}
		}
	}

	private void CenterOnLine(int i)
	{
		int visibleLines = LeftDiff.VisibleLines <= 0 ? (int)(LeftDiff.ActualHeight / OneCharacter.ActualHeight) : LeftDiff.VisibleLines;
		VerticalFileScrollbar.Value = i - (visibleLines / 2) + 1;
	}

	private void LoadSettings()
	{
		AppSettings.LoadSettings();

		this.Left = AppSettings.PositionLeft;
		this.Top = AppSettings.PositionTop;
		this.Width = AppSettings.Width;
		this.Height = AppSettings.Height;
		this.WindowState = AppSettings.WindowState;
	}

	private void SaveSettings()
	{
		AppSettings.PositionLeft = this.Left;
		AppSettings.PositionTop = this.Top;
		AppSettings.Width = this.Width;
		AppSettings.Height = this.Height;
		AppSettings.WindowState = this.WindowState;

		AppSettings.WriteSettingsToDisk();
	}

	private void ProcessSearchResult(int result)
	{
		if (result != -1)
		{
			SearchBox.Error = false;
			CenterOnLine(result);
		}
		else
		{
			SearchBox.Error = true;
		}
	}

	private void UpdateColumnWidths(Grid columnGrid)
	{
		ViewModel.NameColumnWidth = Math.Max(columnGrid.ColumnDefinitions[0].Width.Value, 20);
		ViewModel.SizeColumnWidth = Math.Max(columnGrid.ColumnDefinitions[2].Width.Value, 20);
		ViewModel.DateColumnWidth = Math.Max(columnGrid.ColumnDefinitions[4].Width.Value, 20);


		// HACK: Workaround until I figure out how to data bind the column definition widths two way.
		LeftColumns.ColumnDefinitions[0].Width = new GridLength(ViewModel.NameColumnWidth);
		RightColumns.ColumnDefinitions[0].Width = new GridLength(ViewModel.NameColumnWidth);
		LeftColumns.ColumnDefinitions[2].Width = new GridLength(ViewModel.SizeColumnWidth);
		RightColumns.ColumnDefinitions[2].Width = new GridLength(ViewModel.SizeColumnWidth);
		LeftColumns.ColumnDefinitions[4].Width = new GridLength(ViewModel.DateColumnWidth);
		RightColumns.ColumnDefinitions[4].Width = new GridLength(ViewModel.DateColumnWidth);


		double totalWidth = 0;

		foreach (ColumnDefinition d in columnGrid.ColumnDefinitions)
		{
			totalWidth += d.Width.Value;
		}

		LeftColumns.Width = totalWidth;
		LeftFolderHorizontalScrollbar.ViewportSize = LeftFolder.ActualWidth;
		LeftFolderHorizontalScrollbar.Maximum = totalWidth - LeftFolder.ActualWidth;
		LeftFolderHorizontalScrollbar.LargeChange = LeftFolder.ActualWidth;

		RightColumns.Width = totalWidth;
		RightFolderHorizontalScrollbar.ViewportSize = RightFolder.ActualWidth;
		RightFolderHorizontalScrollbar.Maximum = totalWidth - RightFolder.ActualWidth;
		RightFolderHorizontalScrollbar.LargeChange = RightFolder.ActualWidth;
	}

	private void GetFolderDiffItems(ObservableCollection<FileItem> parent, List<FileItem> items)
	{
		foreach (FileItem fileItem in parent)
		{
			if (fileItem.IsFolder)
			{
				GetFolderDiffItems(fileItem.Children, items);
			}
			else
			{
				if (fileItem.Type == TextState.PartialMatch || (fileItem.Type == TextState.Filler && fileItem.CorrespondingItem.Type == TextState.Deleted) || fileItem.Type == TextState.New)
				{
					items.Add(fileItem);
				}
			}
		}
	}

	private string GetFocusedPath(object control)
	{
		if (control == LeftDiff) return leftSelection;
		else if (control == RightDiff) return rightSelection;
		else if (control == LeftFolder && LeftFolder.SelectedFile != null) return LeftFolder.SelectedFile.Path;
		else if (control == RightFolder && RightFolder.SelectedFile != null) return RightFolder.SelectedFile.Path;

		return "";
	}

	private async void CheckForNewVersion(bool forced = false)
	{
		if (AppSettings.CheckForUpdates && AppSettings.LastUpdateTime < DateTime.Now.AddDays(-5) || forced)
		{
			try
			{
				Debug.Print("Checking for new version...");

				HttpClient httpClient = new();
				string result = await httpClient.GetStringAsync("https://jonashertzman.github.io/FileDiff/download/version.txt");

				Debug.Print($"Latest version found: {result}");
				ViewModel.NewBuildAvailable = int.Parse(result) > int.Parse(ViewModel.BuildNumber);
			}
			catch (Exception exception)
			{
				Debug.Print($"Version check failed: {exception.Message}");
			}

			AppSettings.LastUpdateTime = DateTime.Now;
		}
	}

	#endregion

	#region Events

	private void Window_ContentRendered(object sender, EventArgs e)
	{
		if (Environment.GetCommandLineArgs().Length > 2)
		{
			ViewModel.LeftPath = Environment.GetCommandLineArgs()[1];
			ViewModel.RightPath = Environment.GetCommandLineArgs()[2];
			Compare();
		}
		else if (Environment.GetCommandLineArgs().Length > 1)
		{
			if (new[] { "/?", "-h", "--help" }.Contains(Environment.GetCommandLineArgs()[1]))
			{
				MessageBox.Show(
					"Usage:\n\n" +
					"FilDiff [left-file right-file] [left-folder right-folder] [/? | -h | --help]\n\n" +
					"left-file\t\tPath to left file\n" +
					"right-file\t\tPath to right file\n" +
					"left-folder\tPath to left folder\n" +
					"right-folder\tPath to right folder\n" +
					"/? -h --help\tShows this message"
					,
					"Help");
			}
		}

		renderComplete = true;
		UpdateColumnWidths(LeftColumns);
	}

	private void Window_Initialized(object sender, EventArgs e)
	{
		LoadSettings();
		CheckForNewVersion();
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		SaveSettings();
	}

	private void ProgressTimer_Tick(object sender, EventArgs e)
	{
		ProgressPanel.Visibility = Visibility.Visible;
		progressTimer.Stop();
	}

	private void ToggleButtonNeedRecompare_Click(object sender, RoutedEventArgs e)
	{
		if (File.Exists(ViewModel.LeftPath) && File.Exists(ViewModel.RightPath))
		{
			CompareFiles();
		}
	}

	private void ToggleButtonMasterDetail_Click(object sender, RoutedEventArgs e)
	{
		if (ViewModel.FileVisible)
		{
			CompareFiles();
		}
	}

	private void LeftSide_PreviewDragOver(object sender, DragEventArgs e)
	{
		e.Effects = DragDropEffects.Move;
		e.Handled = true;
	}

	private void LeftSide_DragDrop(object sender, DragEventArgs e)
	{
		var paths = (string[])e.Data.GetData(DataFormats.FileDrop);

		if (paths?.Length >= 2)
		{
			ViewModel.LeftPath = paths[0];
			ViewModel.RightPath = paths[1];
		}
		else if (paths?.Length >= 1)
		{
			ViewModel.LeftPath = paths[0];
		}
	}

	private void RightSide_PreviewDragOver(object sender, DragEventArgs e)
	{
		e.Effects = DragDropEffects.Move;
		e.Handled = true;
	}

	private void RightSide_DragDrop(object sender, DragEventArgs e)
	{
		var paths = (string[])e.Data.GetData(DataFormats.FileDrop);

		if (paths?.Length >= 2)
		{
			ViewModel.LeftPath = paths[0];
			ViewModel.RightPath = paths[1];
		}
		else if (paths?.Length >= 1)
		{
			ViewModel.RightPath = paths[0];
		}
	}

	private void FileDiff_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		bool controlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

		if (controlPressed)
		{
			ViewModel.Zoom += e.Delta / Math.Abs(e.Delta);
		}
		else
		{
			int lines = SystemParameters.WheelScrollLines * e.Delta / 120;
			VerticalFileScrollbar.Value -= lines;
		}
	}

	private void FolderDiff_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		int lines = SystemParameters.WheelScrollLines * e.Delta / 120;
		VerticalTreeScrollbar.Value -= lines;
	}

	private void LeftHorizontalScrollbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		RightHorizontalScrollbar.Value = e.NewValue;
	}

	private void RightHorizontalScrollbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		LeftHorizontalScrollbar.Value = e.NewValue;
	}

	private void LeftFolderHorizontalScrollbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		LeftColumnScroll.ScrollToHorizontalOffset(e.NewValue);
		RightFolderHorizontalScrollbar.Value = e.NewValue;
	}

	private void RightFolderHorizontalScrollbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		RightColumnScroll.ScrollToHorizontalOffset(e.NewValue);
		LeftFolderHorizontalScrollbar.Value = e.NewValue;
	}

	private void LeftColumnScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		if (!renderComplete)
			return;

		UpdateColumnWidths(LeftColumns);
	}

	private void RightColumnScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		if (!renderComplete)
			return;

		UpdateColumnWidths(RightColumns);
	}

	private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		ProcessSearchResult(activeDiff.Search(SearchBox.Text, MatchCase.IsChecked == true));
	}

	private void MatchCase_Checked(object sender, RoutedEventArgs e)
	{
		ProcessSearchResult(activeDiff.Search(SearchBox.Text, MatchCase.IsChecked == true));
	}

	private void RightDiff_GotFocus(object sender, RoutedEventArgs e)
	{
		activeDiff = RightDiff;
		ViewModel.FindPanelRight = true;
	}

	private void LeftDiff_GotFocus(object sender, RoutedEventArgs e)
	{
		activeDiff = LeftDiff;
		ViewModel.FindPanelRight = false;
	}

	private void LeftColumns_Resized(object sender, SizeChangedEventArgs e)
	{
		if (!renderComplete)
			return;

		UpdateColumnWidths(LeftColumns);
	}

	private void RightColumns_Resized(object sender, SizeChangedEventArgs e)
	{
		if (!renderComplete)
			return;

		UpdateColumnWidths(RightColumns);
	}

	private void LeftFolder_SelectionChanged(FileItem selectedItem)
	{
		rightSelection = selectedItem.CorrespondingItem.Path;

		RightFolder.SelectedFile = selectedItem.CorrespondingItem;

		CompareFiles();
	}

	private void RightFolder_SelectionChanged(FileItem selectedItem)
	{
		leftSelection = selectedItem.CorrespondingItem.Path;

		LeftFolder.SelectedFile = selectedItem.CorrespondingItem;

		CompareFiles();
	}

	private void Hyperlink_OpenHomepage(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
	{
		Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
		e.Handled = true;
	}

	private void LightMode_Click(object sender, RoutedEventArgs e)
	{
		ViewModel.Theme = Themes.Light;
	}

	private void DarkMode_Click(object sender, RoutedEventArgs e)
	{
		ViewModel.Theme = Themes.Dark;
	}

	#endregion

	#region Commands

	private void CommandExit_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		this.Close();
	}

	private void CommandOptions_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		// Store existing settings data in case the changes are canceled.
		var oldCheckForUpdates = ViewModel.CheckForUpdates;
		var oldDetectMovedLines = ViewModel.DetectMovedLines;
		var oldFont = ViewModel.Font;
		var oldFontSize = ViewModel.FontSize;
		var oldTabSize = ViewModel.TabSize;

		var oldIgnoredFiles = new ObservableCollection<TextAttribute>(ViewModel.IgnoredFiles);
		var oldIgnoredFolders = new ObservableCollection<TextAttribute>(ViewModel.IgnoredFolders);

		var oldDarkTheme = AppSettings.DarkTheme.Clone();
		var oldLightTheme = AppSettings.LightTheme.Clone();
		var oldTheme = ViewModel.Theme;

		OptionsWindow optionsWindow = new() { DataContext = ViewModel, Owner = this };
		optionsWindow.ShowDialog();

		if (optionsWindow.DialogResult == true)
		{
			SaveSettings();
		}
		else
		{
			// Options window was canceled, revert to old settings.
			ViewModel.CheckForUpdates = oldCheckForUpdates;
			ViewModel.DetectMovedLines = oldDetectMovedLines;
			ViewModel.Font = oldFont;
			ViewModel.FontSize = oldFontSize;
			ViewModel.TabSize = oldTabSize;

			ViewModel.IgnoredFiles = new ObservableCollection<TextAttribute>(oldIgnoredFiles);
			ViewModel.IgnoredFolders = new ObservableCollection<TextAttribute>(oldIgnoredFolders);

			AppSettings.DarkTheme = oldDarkTheme;
			AppSettings.LightTheme = oldLightTheme;
			ViewModel.Theme = oldTheme;
		}
	}

	private void CommandAbout_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		CheckForNewVersion(true);

		AboutWindow aboutWindow = new() { Owner = this, DataContext = ViewModel };
		aboutWindow.ShowDialog();
	}

	private void CommandCompare_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		// Make sure view model is updated with both paths before we compare
		TextBoxRightPath.Focus();
		TextBoxLeftPath.Focus();

		Compare();
	}

	private void CommandCancelCompare_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BackgroundCompare.Cancel();
	}

	private void CommandSaveLeftFile_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		string leftPath = ViewModel.Mode == CompareMode.File ? ViewModel.LeftPath : LeftFolder.SelectedFile.Path;

		if (File.Exists(leftPath) && ViewModel.LeftFileDirty)
		{
			try
			{
				using (StreamWriter sw = new(leftPath, false, ViewModel.LeftFileEncoding.GetEncoding))
				{
					sw.NewLine = ViewModel.LeftFileEncoding.GetNewLineString;
					foreach (Line l in ViewModel.LeftFile)
					{
						if (!l.IsFiller)
						{
							sw.WriteLine(l.Text);
						}
					}
				}
				ViewModel.LeftFileDirty = false;
				ViewModel.LeftFileEdited = false;
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, "Error Saving File", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}

	private void CommandSaveLeftFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = ViewModel.LeftFileDirty;
	}

	private void CommandSaveRightFile_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		string rightPath = ViewModel.Mode == CompareMode.File ? ViewModel.RightPath : RightFolder.SelectedFile.Path;

		if (File.Exists(rightPath) && ViewModel.RightFileDirty)
		{
			try
			{
				using (StreamWriter sw = new(rightPath, false, ViewModel.RightFileEncoding.GetEncoding))
				{
					sw.NewLine = ViewModel.RightFileEncoding.GetNewLineString;
					foreach (Line l in ViewModel.RightFile)
					{
						if (!l.IsFiller)
						{
							sw.WriteLine(l.Text);
						}
					}
				}
				ViewModel.RightFileDirty = false;
				ViewModel.RightFileEdited = false;
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, "Error Saving File", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}

	private void CommandSaveRightFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = ViewModel.RightFileDirty;
	}

	private void CommandEdit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = NoManualEdit;
	}

	private void CommandBrowseLeft_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BrowseFolderWindow browseFolderWindow = new() { DataContext = ViewModel, Owner = this, SelectedPath = ViewModel.LeftPath };
		browseFolderWindow.ShowDialog();

		if (browseFolderWindow.DialogResult == true)
		{
			ViewModel.LeftPath = browseFolderWindow.SelectedPath;
		}
	}

	private void CommandBrowseRight_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BrowseFolderWindow browseFolderWindow = new() { DataContext = ViewModel, Owner = this, SelectedPath = ViewModel.RightPath };
		browseFolderWindow.ShowDialog();

		if (browseFolderWindow.DialogResult == true)
		{
			ViewModel.RightPath = browseFolderWindow.SelectedPath;
		}
	}

	private void CommandPreviousDiff_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		MoveToPreviousDiff();
	}

	private void CommandPreviousDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = ViewModel.FileVisible && currentDiffIndex > 0 && NoManualEdit;
	}

	private void CommandNextDiff_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		MoveToNextDiff();
	}

	private void CommandNextDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = ViewModel.FileVisible && currentDiffIndex < fileDiffs.Count - 1 && NoManualEdit;
	}

	private void CommandCurrentDiff_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		CenterOnLine(ViewModel.CurrentDiff.Start);
	}

	private void CommandCurrentDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = ViewModel.FileVisible && currentDiffIndex != -1 && NoManualEdit;
	}

	private void CommandFirstDiff_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		MoveToFirstDiff();
	}

	private void CommandFirstDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = ViewModel.FileVisible && currentDiffIndex != 0 && fileDiffs.Count > 0 && NoManualEdit;
	}

	private void CommandLastDiff_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		MoveToLastDiff();
	}

	private void CommandLastDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = ViewModel.FileVisible && currentDiffIndex < fileDiffs.Count - 1 && NoManualEdit;
	}

	private void CommandNextFile_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (folderDiffItems.Count > 0)
		{
			int currentIndex = folderDiffItems.IndexOf(RightFolder.SelectedFile);

			if (currentIndex == -1)
			{
				RightFolder.Select(folderDiffItems[0]);
			}
			else if (currentIndex < folderDiffItems.Count - 1)
			{
				RightFolder.Select(folderDiffItems[currentIndex + 1]);
			}
		}
	}

	private void CommandNextFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = ViewModel.FolderVisible && folderDiffItems.IndexOf(RightFolder.SelectedFile) < folderDiffItems.Count - 1; ;
	}

	private void CommandPreviousFile_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (folderDiffItems.Count > 0)
		{
			int currentIndex = folderDiffItems.IndexOf(RightFolder.SelectedFile);

			if (currentIndex == -1)
			{
				RightFolder.Select(folderDiffItems[^1]);
			}
			else if (currentIndex > 0)
			{
				RightFolder.Select(folderDiffItems[currentIndex - 1]);
			}
		}
	}

	private void CommandPreviousFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = ViewModel.FolderVisible && folderDiffItems.IndexOf(RightFolder.SelectedFile) > 0;
	}

	private void CommandFind_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		SearchPanel.Visibility = Visibility.Visible;
		SearchBox.Focus();
	}

	private void CommandFindNext_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (SearchPanel.Visibility != Visibility.Visible)
		{
			SearchPanel.Visibility = Visibility.Visible;
			SearchBox.Focus();
		}
		else
		{
			ProcessSearchResult(activeDiff.SearchNext(SearchBox.Text, MatchCase.IsChecked == true));
		}
	}

	private void CommandFindNext_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = (SearchBox.Text != "" && activeDiff.Lines.Count > 0) || SearchPanel.Visibility != Visibility.Visible;
	}

	private void CommandFindPrevious_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ProcessSearchResult(activeDiff.SearchPrevious(SearchBox.Text, MatchCase.IsChecked == true));
	}

	private void CommandFindPrevious_CanExecute_1(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = SearchBox.Text != "" && activeDiff.Lines.Count > 0;
	}

	private void CommandCloseFind_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		SearchPanel.Visibility = Visibility.Collapsed;
	}

	private void CommandZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ViewModel.Zoom += 1;
	}

	private void CommandZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ViewModel.Zoom -= 1;
	}

	private void CommandResetZoom_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ViewModel.Zoom = 0;
	}

	private void CommandSwap_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		(ViewModel.RightPath, ViewModel.LeftPath) = (ViewModel.LeftPath, ViewModel.RightPath);

		Compare();
	}

	private void CommandUp_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		try
		{
			ViewModel.LeftPath = Path.GetDirectoryName(Path.GetFullPath(ViewModel.LeftPath));
			ViewModel.RightPath = Path.GetDirectoryName(Path.GetFullPath(ViewModel.RightPath));

			Compare();
		}
		catch (ArgumentException) { }
	}

	private void CommandOpenContainingFolder_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		string args = $"/Select, {Path.GetFullPath(GetFocusedPath(e.OriginalSource))}";
		ProcessStartInfo pfi = new("Explorer.exe", args);
		Process.Start(pfi);
	}

	private void CommandOpenContainingFolder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = GetFocusedPath(e.OriginalSource) != "";
	}

	private void CommandCopyPathToClipboard_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		WinApi.CopyTextToClipboard(Path.GetFullPath(GetFocusedPath(e.OriginalSource)));
	}

	private void CommandCopyPathToClipboard_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = GetFocusedPath(e.OriginalSource) != "";
	}

	private void CommandCopyLeftDiff_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		for (int i = ViewModel.CurrentDiff.Start; i < ViewModel.CurrentDiff.Start + ViewModel.CurrentDiff.Length; i++)
		{
			ViewModel.RightFile[i].Text = ViewModel.LeftFile[i].Text;

			if (ViewModel.LeftFile[i].IsFiller)
			{
				ViewModel.RightFile[i].Type = TextState.Filler;
			}
			else
			{
				ViewModel.RightFile[i].Type = TextState.FullMatch;
				ViewModel.LeftFile[i].Type = TextState.FullMatch;
			}
		}

		ViewModel.RightFileDirty = true;
		ViewModel.UpdateTrigger++;
	}

	private void CommandCopyLeftDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = currentDiffIndex != -1 && NoManualEdit;
	}

	private void CommandCopyRightDiff_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		for (int i = ViewModel.CurrentDiff.Start; i < ViewModel.CurrentDiff.Start + ViewModel.CurrentDiff.Length; i++)
		{
			ViewModel.LeftFile[i].Text = ViewModel.RightFile[i].Text;

			if (ViewModel.RightFile[i].IsFiller)
			{
				ViewModel.LeftFile[i].Type = TextState.Filler;
			}
			else
			{
				ViewModel.RightFile[i].Type = TextState.FullMatch;
				ViewModel.LeftFile[i].Type = TextState.FullMatch;
			}
		}

		ViewModel.LeftFileDirty = true;
		ViewModel.UpdateTrigger++;
	}

	private void CommandCopyRightDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = currentDiffIndex != -1 && NoManualEdit;
	}

	#endregion

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		File.ReadAllBytes("xxx");
	}
}
