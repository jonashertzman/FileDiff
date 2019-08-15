using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FileDiff
{
	public partial class MainWindow : Window
	{

		#region Members

		MainWindowViewModel ViewModel { get; set; } = new MainWindowViewModel();

		private List<FileItem> folderDiffItems = new List<FileItem>();

		int firstDiff = -1;
		int lastDiff = -1;
		bool renderComplete = false;

		DiffControl activeDiff;
		string activeSelection;

		string rightSelection = "";
		string leftSelection = "";

		bool experimentalMatching;
		bool compareCaneled = false;

		Stopwatch stopwatch = new Stopwatch();

		#endregion

		#region Constructor

		public MainWindow()
		{
			InitializeComponent();

			DataContext = ViewModel;

			SearchPanel.Visibility = Visibility.Collapsed;
			activeDiff = LeftDiff;
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
			activeSelection = "";

			if (File.Exists(ViewModel.LeftPath) && File.Exists(ViewModel.RightPath))
			{
				ViewModel.Mode = CompareMode.File;

				CompareFiles();
			}
			else if (Directory.Exists(ViewModel.LeftPath) && Directory.Exists(ViewModel.RightPath))
			{
				ViewModel.Mode = CompareMode.Folder;

				CompareDirectories();
			}
		}

		private void CompareFiles()
		{
			experimentalMatching = Keyboard.IsKeyDown(Key.LeftShift);

			stopwatch.Restart();

			string leftPath = "";
			string rightPath = "";

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

			List<Line> leftLines = new List<Line>();
			List<Line> rightLines = new List<Line>();

			ViewModel.LeftFileEncoding = null;
			ViewModel.RightFileEncoding = null;

			try
			{
				if (File.Exists(leftPath))
				{
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
			}

			if (leftLines.Count > 0 && rightLines.Count > 0)
			{
				Mouse.OverrideCursor = Cursors.Wait;

				Debug.Print("------ start thread");

				ViewModel.GuiFrozen = true;
				var progress = new Progress<int>(CompareStatusUpdate);
				Task.Run(() => BackgroundCompare.CompareFiles(progress, leftLines, rightLines)).ContinueWith(CompareFilesFinnished, TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		private void CompareFilesFinnished(Task<Tuple<List<Line>, List<Line>>> task)
		{
			Debug.Print("------ after thread");

			ViewModel.GuiFrozen = false;

			//FillViewModel(obj.Result.Item1, obj.Result.Item2);

			ViewModel.LeftFile = new ObservableCollection<Line>(task.Result.Item1);
			ViewModel.RightFile = new ObservableCollection<Line>(task.Result.Item2);


			stopwatch.Stop();
			Statusbar.Text = $"Compare time {TimeSpanToShortString(stopwatch.Elapsed)} {(experimentalMatching ? "(Experimental Matching)" : "")}";


			LeftDiff.Focus();
			InitNavigationState();

			Mouse.OverrideCursor = null;
			Debug.Print("------ end update thread");

		}

		private void CompareStatusUpdate(int obj)
		{
			throw new NotImplementedException();
		}

		private void CompareDirectories()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			ViewModel.Clear();

			Mouse.OverrideCursor = Cursors.Wait;

			ObservableCollection<FileItem> leftItems = new ObservableCollection<FileItem>();
			ObservableCollection<FileItem> rightItems = new ObservableCollection<FileItem>();

			SearchDirectory(ViewModel.LeftPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), leftItems, ViewModel.RightPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rightItems, 1);

			LeftFolder.Init();
			RightFolder.Init();

			ViewModel.LeftFolder = leftItems;
			ViewModel.RightFolder = rightItems;

			folderDiffItems = new List<FileItem>();
			GetFolderDiffItems(ViewModel.LeftFolder, folderDiffItems);

			LeftFolder.Focus();
			Mouse.OverrideCursor = null;

			stopwatch.Stop();

			Statusbar.Text = $"Compare time {TimeSpanToShortString(stopwatch.Elapsed)}";
		}

		private void SearchDirectory(string leftPath, ObservableCollection<FileItem> leftItems, string rightPath, ObservableCollection<FileItem> rightItems, int level)
		{
			if (leftPath?.Length > 259 || rightPath?.Length > 259)
			{
				return;
			}

			if (Directory.Exists(leftPath) && !Utils.DirectoryAllowed(leftPath) || Directory.Exists(rightPath) && !Utils.DirectoryAllowed(rightPath))
			{
				return;
			}

			// Sorted dictionary holding matched pairs of files and folders in the current directory.
			// Folders are prefixed with "*" to not get conflict between a file named "X" to the left, and a folder named "X" to the right.
			SortedDictionary<string, FileItemPair> allItems = new SortedDictionary<string, FileItemPair>();

			// Find directories
			if (leftPath != null)
			{
				foreach (string directoryPath in Directory.GetDirectories(FixRootPath(leftPath)))
				{
					string directoryName = directoryPath.Substring(leftPath.Length + 1);
					allItems.Add("*" + directoryName, new FileItemPair(new FileItem(directoryName, true, TextState.Deleted, directoryPath, level), new FileItem("", true, TextState.Filler, "", level)));
				}
			}

			if (rightPath != null)
			{
				foreach (string directoryPath in Directory.GetDirectories(FixRootPath(rightPath)))
				{
					string directoryName = directoryPath.Substring(rightPath.Length + 1);
					string key = "*" + directoryName;
					if (!allItems.ContainsKey(key))
					{
						allItems.Add(key, new FileItemPair(new FileItem("", true, TextState.Filler, "", level), new FileItem(directoryName, true, TextState.New, directoryPath, level)));
					}
					else
					{
						allItems[key].RightItem = new FileItem(directoryName, true, TextState.FullMatch, directoryPath, level);
						allItems[key].LeftItem.Type = TextState.FullMatch;
					}
				}
			}

			// Find files
			if (leftPath != null)
			{
				foreach (string filePath in Directory.GetFiles(FixRootPath(leftPath)))
				{
					string fileName = filePath.Substring(leftPath.Length + 1);
					allItems.Add(fileName, new FileItemPair(new FileItem(fileName, false, TextState.Deleted, filePath, level), new FileItem("", false, TextState.Filler, "", level)));
				}
			}

			if (rightPath != null)
			{
				foreach (string filePath in Directory.GetFiles(FixRootPath(rightPath)))
				{
					string fileName = filePath.Substring(rightPath.Length + 1);
					if (!allItems.ContainsKey(fileName))
					{
						allItems.Add(fileName, new FileItemPair(new FileItem("", false, TextState.Filler, "", level), new FileItem(fileName, false, TextState.New, filePath, level)));
					}
					else
					{
						allItems[fileName].RightItem = new FileItem(fileName, false, TextState.FullMatch, filePath, level);
						allItems[fileName].LeftItem.Type = TextState.FullMatch;
					}
				}
			}

			foreach (KeyValuePair<string, FileItemPair> pair in allItems)
			{
				FileItem leftItem = pair.Value.LeftItem;
				FileItem rightItem = pair.Value.RightItem;

				leftItem.CorrespondingItem = rightItem;
				rightItem.CorrespondingItem = leftItem;

				if (leftItem.IsFolder)
				{
					//leftItem.IsExpanded = true;

					if (DirectoryIsIgnored(leftItem.Name) || DirectoryIsIgnored(rightItem.Name))
					{
						if (DirectoryIsIgnored(leftItem.Name))
						{
							leftItem.Type = TextState.Ignored;
						}
						if (DirectoryIsIgnored(rightItem.Name))
						{
							rightItem.Type = TextState.Ignored;
						}
					}
					else
					{
						SearchDirectory(leftItem.Name == "" ? null : Path.Combine(FixRootPath(leftPath), leftItem.Name), leftItem.Children, rightItem.Name == "" ? null : Path.Combine(FixRootPath(rightPath), rightItem.Name), rightItem.Children, level + 1);
						foreach (FileItem child in leftItem.Children)
						{
							child.Parent = leftItem;
						}
						foreach (FileItem child in rightItem.Children)
						{
							child.Parent = rightItem;
						}

						if (leftItem.Type == TextState.FullMatch && leftItem.ChildDiffExists)
						{
							leftItem.Type = TextState.PartialMatch;
							rightItem.Type = TextState.PartialMatch;
						}
					}
				}
				else
				{
					if (FileIsIgnored(leftItem.Name) || FileIsIgnored(rightItem.Name))
					{
						leftItem.Type = TextState.Ignored;
						rightItem.Type = TextState.Ignored;
					}
					else
					{
						if (leftItem.Type == TextState.FullMatch)
						{
							if (leftItem.Size != rightItem.Size || (leftItem.Date != rightItem.Date && leftItem.Checksum != rightItem.Checksum))
							{
								leftItem.Type = TextState.PartialMatch;
								rightItem.Type = TextState.PartialMatch;
							}
						}
					}
				}

				leftItems.Add(leftItem);
				rightItems.Add(rightItem);
			}
		}

		private string TimeSpanToShortString(TimeSpan timeSpan)
		{
			if (timeSpan.Hours > 0)
			{
				return timeSpan.Hours + "h " + timeSpan.Minutes + "m";
			}
			if (timeSpan.Minutes > 0)
			{
				return timeSpan.Minutes + "m " + timeSpan.Seconds + "s";
			}
			if (timeSpan.Seconds > 0)
			{
				return timeSpan.Seconds + "." + timeSpan.Milliseconds.ToString().PadLeft(3, '0') + "s";
			}
			return timeSpan.Milliseconds.ToString() + "ms";
		}

		private string FixRootPath(string path)
		{
			// Directory.GetDirectories, Directory.GetFiles and Path.Combine does not work on root paths without trailing backslashes.
			if (path.EndsWith(":"))
			{
				return path += "\\";
			}
			return path;
		}

		private bool DirectoryIsIgnored(string directory)
		{
			foreach (TextAttribute a in ViewModel.IgnoredFolders)
			{
				if (WildcardCompare(directory, a.Text, true))
				{
					return true;
				}
			}
			return false;
		}

		private bool FileIsIgnored(string directory)
		{
			foreach (TextAttribute a in ViewModel.IgnoredFiles)
			{
				if (WildcardCompare(directory, a.Text, true))
				{
					return true;
				}
			}
			return false;
		}

		private bool WildcardCompare(string compare, string wildString, bool ignoreCase)
		{
			if (ignoreCase)
			{
				wildString = wildString.ToUpper();
				compare = compare.ToUpper();
			}

			int wildStringLength = wildString.Length;
			int CompareLength = compare.Length;

			int wildMatched = wildStringLength;
			int compareBase = CompareLength;

			int wildPosition = 0;
			int comparePosition = 0;

			// Match until first wildcard '*'
			while (comparePosition < CompareLength && (wildPosition >= wildStringLength || wildString[wildPosition] != '*'))
			{
				if (wildPosition >= wildStringLength || (wildString[wildPosition] != compare[comparePosition] && wildString[wildPosition] != '?'))
				{
					return false;
				}

				wildPosition++;
				comparePosition++;
			}

			// Process wildcard
			while (comparePosition < CompareLength)
			{
				if (wildPosition < wildStringLength)
				{
					if (wildString[wildPosition] == '*')
					{
						wildPosition++;

						if (wildPosition == wildStringLength)
						{
							return true;
						}

						wildMatched = wildPosition;
						compareBase = comparePosition + 1;

						continue;
					}

					if (wildString[wildPosition] == compare[comparePosition] || wildString[wildPosition] == '?')
					{
						wildPosition++;
						comparePosition++;

						continue;
					}
				}

				wildPosition = wildMatched;
				comparePosition = compareBase++;
			}

			while (wildPosition < wildStringLength && wildString[wildPosition] == '*')
			{
				wildPosition++;
			}

			if (wildPosition < wildStringLength)
			{
				return false;
			}

			return true;
		}

		private void InitNavigationState()
		{
			ViewModel.CurrentDiff = -1;
			ViewModel.CurrentDiffLength = 1;
			ViewModel.EditMode = false;
			ViewModel.LeftFileDirty = false;
			ViewModel.LeftFileEdited = false;
			ViewModel.RightFileDirty = false;
			ViewModel.RightFileEdited = false;

			LeftDiff.Init();
			RightDiff.Init();

			VerticalFileScrollbar.Value = 0;
			LeftHorizontalScrollbar.Value = 0;

			UpdateNavigationButtons();

			if (firstDiff != -1)
			{
				MoveToDiffLine(firstDiff);
				ViewModel.CurrentDiff = firstDiff;
			}
		}

		private void UpdateNavigationButtons()
		{
			firstDiff = -1;
			lastDiff = -1;

			for (int i = 0; i < ViewModel.LeftFile.Count; i++)
			{
				if (i == 0 || ViewModel.LeftFile[i - 1].Type == TextState.FullMatch)
				{
					if (firstDiff == -1 && ViewModel.LeftFile[i].Type != TextState.FullMatch)
					{
						firstDiff = i;

					}
					if (ViewModel.LeftFile[i].Type != TextState.FullMatch)
					{
						lastDiff = i;
					}
				}
			}

			if (lastDiff == -1)
			{
				ViewModel.CurrentDiff = -1;
				ViewModel.CurrentDiffLength = -1;
			}
		}

		private void MoveToFirstDiff()
		{
			ViewModel.CurrentDiff = -1;
			MoveToNextDiff();
		}

		private void MoveToLastDiff()
		{
			ViewModel.CurrentDiff = ViewModel.LeftFile.Count;
			MoveToPrevoiusDiff();
		}

		private void MoveToPrevoiusDiff()
		{
			for (int i = ViewModel.CurrentDiff - 1; i >= 0; i--)
			{
				if (i == 0 || ViewModel.LeftFile[i - 1].Type == TextState.FullMatch)
				{
					if (ViewModel.LeftFile[i].Type != TextState.FullMatch || ViewModel.RightFile[i].Type != TextState.FullMatch)
					{
						ViewModel.CurrentDiff = i;
						MoveToDiffLine(i);
						return;
					}
				}
			}
		}

		private void MoveToNextDiff()
		{
			for (int i = ViewModel.CurrentDiff + 1; i < ViewModel.LeftFile.Count; i++)
			{
				if (i == 0 || ViewModel.LeftFile[i - 1].Type == TextState.FullMatch)
				{
					if (ViewModel.LeftFile[i].Type != TextState.FullMatch || ViewModel.RightFile[i].Type != TextState.FullMatch)
					{
						ViewModel.CurrentDiff = i;
						MoveToDiffLine(i);
						return;
					}
				}
			}
		}

		private void MoveToDiffLine(int i)
		{
			int diffLength = 1;

			while (i + diffLength < ViewModel.LeftFile.Count && ViewModel.LeftFile[i + diffLength].Type != TextState.FullMatch)
			{
				diffLength++;
			}

			ViewModel.CurrentDiff = i;
			ViewModel.CurrentDiffLength = diffLength;

			CenterOnLine(i);
		}

		private void CenterOnLine(int i)
		{
			int visibleLines = LeftDiff.VisibleLines <= 0 ? (int)(LeftDiff.ActualHeight / OneCharacter.ActualHeight) : LeftDiff.VisibleLines;
			VerticalFileScrollbar.Value = i - (visibleLines / 2) + 1;
		}

		private void LoadSettings()
		{
			AppSettings.ReadSettingsFromDisk();

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
				SearchBox.Background = new SolidColorBrush(Colors.White);
				CenterOnLine(result);
			}
			else
			{
				SearchBox.Background = new SolidColorBrush(Colors.Tomato);
			}
		}

		private void UpdateColumnWidths(Grid columnGrid)
		{
			ViewModel.NameColumnWidth = columnGrid.ColumnDefinitions[0].Width.Value;
			ViewModel.SizeColumnWidth = columnGrid.ColumnDefinitions[2].Width.Value;
			ViewModel.DateColumnWidth = columnGrid.ColumnDefinitions[4].Width.Value;


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
				if (!fileItem.IsFolder && fileItem.Type == TextState.PartialMatch || fileItem == LeftFolder.SelectedFile)
				{
					items.Add(fileItem);
				}
				GetFolderDiffItems(fileItem.Children, items);
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

			renderComplete = true;
			UpdateColumnWidths(LeftColumns);
		}

		private void Window_Initialized(object sender, EventArgs e)
		{
			LoadSettings();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			SaveSettings();
		}

		private void ToggleButtonIgnoreWhiteSpace_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(ViewModel.LeftPath) && File.Exists(ViewModel.RightPath))
			{
				CompareFiles();
			}
		}

		private void ToggleButtonMasterDetail_Click(object sender, RoutedEventArgs e)
		{
			if (ViewModel.FileVissible)
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
			int lines = SystemParameters.WheelScrollLines * e.Delta / 120;
			VerticalFileScrollbar.Value -= lines;
		}

		private void FolderDiff_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			int lines = SystemParameters.WheelScrollLines * e.Delta / 120;
			VerticalTreeScrollbar.Value -= lines;
		}

		private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (sender is ScrollViewer && !e.Handled)
			{
				e.Handled = true;
				var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
				eventArg.RoutedEvent = UIElement.MouseWheelEvent;
				eventArg.Source = sender;
				var parent = ((Control)sender).Parent as UIElement;
				parent.RaiseEvent(eventArg);
			}
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
			activeSelection = RightDiff.Lines.Count > 0 ? rightSelection : "";
		}

		private void LeftDiff_GotFocus(object sender, RoutedEventArgs e)
		{
			activeDiff = LeftDiff;
			ViewModel.FindPanelRight = false;
			activeSelection = LeftDiff.Lines.Count > 0 ? leftSelection : "";
		}

		private void LeftFolder_GotFocus(object sender, RoutedEventArgs e)
		{
			activeSelection = leftSelection;
		}

		private void RightFolder_GotFocus(object sender, RoutedEventArgs e)
		{
			activeSelection = rightSelection;
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
			leftSelection = selectedItem.Path;
			rightSelection = selectedItem.CorrespondingItem.Path;

			RightFolder.SelectedFile = selectedItem.CorrespondingItem;

			CompareFiles();
		}

		private void RightFolder_SelectionChanged(FileItem selectedItem)
		{
			rightSelection = selectedItem.Path;
			leftSelection = selectedItem.CorrespondingItem.Path;

			LeftFolder.SelectedFile = selectedItem.CorrespondingItem;

			CompareFiles();
		}

		#endregion

		#region Commands

		private void CommandExit_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
		}

		private void CommnadOptions_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			// Store existing settings data in case the changes are canceled.
			var oldFont = ViewModel.Font;
			var oldFontSize = ViewModel.FontSize;
			var oldTabSize = ViewModel.TabSize;
			var oldDeletedBackground = ViewModel.DeletedBackground;
			var oldDeletedForeground = ViewModel.DeletedForeground;
			var oldFullMatchBackground = ViewModel.FullMatchBackground;
			var oldFullMatchForeground = ViewModel.FullMatchForeground;
			var oldIgnoredBackground = ViewModel.IgnoredBackground;
			var oldIgnoredForeground = ViewModel.IgnoredForeground;
			var oldNewBackground = ViewModel.NewBackground;
			var oldNewForeground = ViewModel.NewForeground;
			var oldPartialMatchBackground = ViewModel.PartialMatchBackground;
			var oldPartialMatchForeground = ViewModel.PartialMatchForeground;
			var oldSelectionBackground = ViewModel.SelectionBackground;
			var oldIgnoredFiles = new ObservableCollection<TextAttribute>(ViewModel.IgnoredFiles);
			var oldIgnoredFolders = new ObservableCollection<TextAttribute>(ViewModel.IgnoredFolders);

			OptionsWindow optionsWindow = new OptionsWindow() { DataContext = ViewModel, Owner = this };
			optionsWindow.ShowDialog();

			if (optionsWindow.DialogResult == true)
			{
				SaveSettings();
			}
			else
			{
				// Options window was canceled, revert to old settings.
				ViewModel.Font = oldFont;
				ViewModel.FontSize = oldFontSize;
				ViewModel.TabSize = oldTabSize;
				ViewModel.DeletedBackground = oldDeletedBackground;
				ViewModel.DeletedForeground = oldDeletedForeground;
				ViewModel.FullMatchBackground = oldFullMatchBackground;
				ViewModel.FullMatchForeground = oldFullMatchForeground;
				ViewModel.IgnoredBackground = oldIgnoredBackground;
				ViewModel.IgnoredForeground = oldIgnoredForeground;
				ViewModel.NewBackground = oldNewBackground;
				ViewModel.NewForeground = oldNewForeground;
				ViewModel.PartialMatchBackground = oldPartialMatchBackground;
				ViewModel.PartialMatchForeground = oldPartialMatchForeground;
				ViewModel.SelectionBackground = oldSelectionBackground;
				ViewModel.IgnoredFiles = new ObservableCollection<TextAttribute>(oldIgnoredFiles);
				ViewModel.IgnoredFolders = new ObservableCollection<TextAttribute>(oldIgnoredFolders);
			}
		}

		private void CommandAbout_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			AboutWindow aboutWindow = new AboutWindow() { Owner = this };
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
					using (StreamWriter sw = new StreamWriter(leftPath, false, ViewModel.LeftFileEncoding.GetEncoding))
					{
						sw.NewLine = ViewModel.LeftFileEncoding.GetNewLineString;
						foreach (Line l in ViewModel.LeftFile)
						{
							if (l.Type != TextState.Filler)
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
					using (StreamWriter sw = new StreamWriter(rightPath, false, ViewModel.RightFileEncoding.GetEncoding))
					{
						sw.NewLine = ViewModel.RightFileEncoding.GetNewLineString;
						foreach (Line l in ViewModel.RightFile)
						{
							if (l.Type != TextState.Filler)
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

		private void CommandEdit_Executed(object sender, ExecutedRoutedEventArgs e)
		{

		}

		private void CommandEdit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = NoManualEdit;
		}

		private void CommandBrowseLeft_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			BrowseFolderWindow browseFolderWindow = new BrowseFolderWindow() { DataContext = ViewModel, Owner = this, SelectedPath = ViewModel.LeftPath };
			browseFolderWindow.ShowDialog();

			if (browseFolderWindow.DialogResult == true)
			{
				ViewModel.LeftPath = browseFolderWindow.SelectedPath;
			}
		}

		private void CommandBrowseRight_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			BrowseFolderWindow browseFolderWindow = new BrowseFolderWindow() { DataContext = ViewModel, Owner = this, SelectedPath = ViewModel.RightPath };
			browseFolderWindow.ShowDialog();

			if (browseFolderWindow.DialogResult == true)
			{
				ViewModel.RightPath = browseFolderWindow.SelectedPath;
			}
		}

		private void CommandPreviousDiff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MoveToPrevoiusDiff();
		}

		private void CommandPreviousDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ViewModel.FileVissible && firstDiff < ViewModel.CurrentDiff && NoManualEdit;
		}

		private void CommandNextDiff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MoveToNextDiff();
		}

		private void CommandNextDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ViewModel.FileVissible && lastDiff > ViewModel.CurrentDiff && NoManualEdit;
		}

		private void CommandCurrentDiff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MoveToDiffLine(ViewModel.CurrentDiff);
		}

		private void CommandCurrentDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ViewModel.FileVissible && ViewModel.CurrentDiff != -1 && ViewModel.CurrentDiffLength > 0 && NoManualEdit;
		}

		private void CommandFirstDiff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MoveToFirstDiff();
		}

		private void CommandFirstDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ViewModel.FileVissible && firstDiff < ViewModel.CurrentDiff && firstDiff != -1 && NoManualEdit;
		}

		private void CommandLastDiff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MoveToLastDiff();
		}

		private void CommandLastDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ViewModel.FileVissible && lastDiff > ViewModel.CurrentDiff && NoManualEdit;
		}

		private void CommandNextFile_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (folderDiffItems.Count > 0)
			{
				int currentIndex = folderDiffItems.IndexOf(LeftFolder.SelectedFile);

				if (currentIndex == -1)
				{
					LeftFolder.Select(folderDiffItems[0]);
				}
				else if (currentIndex < folderDiffItems.Count - 1)
				{
					LeftFolder.Select(folderDiffItems[currentIndex + 1]);
				}
			}
		}

		private void CommandNextFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ViewModel.FolderVissible && folderDiffItems.IndexOf(LeftFolder.SelectedFile) < folderDiffItems.Count - 1; ;
		}

		private void CommandPreviousFile_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (folderDiffItems.Count > 0)
			{
				int currentIndex = folderDiffItems.IndexOf(LeftFolder.SelectedFile);

				if (currentIndex == -1)
				{
					LeftFolder.Select(folderDiffItems[folderDiffItems.Count - 1]);
				}
				else if (currentIndex > 0)
				{
					LeftFolder.Select(folderDiffItems[currentIndex - 1]);
				}
			}
		}

		private void CommandPreviousFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ViewModel.FolderVissible && folderDiffItems.IndexOf(LeftFolder.SelectedFile) > 0;
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

		private void CommandSwap_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			string temp = ViewModel.LeftPath;
			ViewModel.LeftPath = ViewModel.RightPath;
			ViewModel.RightPath = temp;

			Compare();
		}

		private void CommandUp_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				ViewModel.LeftPath = Path.GetDirectoryName(ViewModel.LeftPath);
				ViewModel.RightPath = Path.GetDirectoryName(ViewModel.RightPath);

				Compare();
			}
			catch (ArgumentException) { }
		}

		private void CommandOpenContainingFolder_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			string args = $"/Select, {Path.GetFullPath(activeSelection)}";
			ProcessStartInfo pfi = new ProcessStartInfo("Explorer.exe", args);
			Process.Start(pfi);
		}

		private void CommandOpenContainingFolder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = activeSelection != "";
		}

		private void CommandCopyPathToClipboard_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Clipboard.SetText(Path.GetFullPath(activeSelection));
		}

		private void CommandCopyPathToClipboard_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = activeSelection != "";
		}

		private void CommandCopyLeftDiff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			for (int i = ViewModel.CurrentDiff; i < ViewModel.CurrentDiff + ViewModel.CurrentDiffLength; i++)
			{
				ViewModel.RightFile[i].Text = ViewModel.LeftFile[i].Text;
				ViewModel.RightFile[i].Type = ViewModel.LeftFile[i].Type;
			}

			ViewModel.RightFileDirty = true;
			ViewModel.CurrentDiffLength = 0;
			ViewModel.UpdateTrigger++;

			UpdateNavigationButtons();
		}

		private void CommandCopyLeftDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ViewModel.CurrentDiff != -1 && NoManualEdit;
		}

		private void CommandCopyRightDiff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			for (int i = ViewModel.CurrentDiff; i < ViewModel.CurrentDiff + ViewModel.CurrentDiffLength; i++)
			{
				ViewModel.LeftFile[i].Text = ViewModel.RightFile[i].Text;
				ViewModel.LeftFile[i].Type = ViewModel.RightFile[i].Type;
			}

			ViewModel.LeftFileDirty = true;
			ViewModel.CurrentDiffLength = 0;
			ViewModel.UpdateTrigger++;

			UpdateNavigationButtons();
		}

		private void CommandCopyRightDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ViewModel.CurrentDiff != -1 && NoManualEdit;
		}

		#endregion

	}
}
