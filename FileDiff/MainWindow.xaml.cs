using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FileDiff
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		#region Members

		public MainWindowViewModel ViewModel { get; set; } = new MainWindowViewModel();

		private SettingsData Settings { get; set; } = new SettingsData();

		int currentLine = -1;
		int firstDiff = -1;
		int lastDiff = -1;

		DiffControl activeDiff;

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

		#region Methods

		private void Compare()
		{
			if (File.Exists(ViewModel.LeftPath) && File.Exists(ViewModel.RightPath))
			{
				ViewModel.FileMode = true;
				CompareFiles();
			}
			else if (Directory.Exists(ViewModel.LeftPath) && Directory.Exists(ViewModel.RightPath))
			{
				ViewModel.FileMode = false;
				CompareDirectories();
			}
		}

		private void CompareFiles()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			Mouse.OverrideCursor = Cursors.Wait;

			List<Line> leftSide = new List<Line>();
			List<Line> rightSide = new List<Line>();

			int i = 0;
			foreach (string s in File.ReadAllLines(ViewModel.LeftPath))
			{
				leftSide.Add(new Line() { Type = TextState.Deleted, Text = s, LineIndex = i++ });
			}

			i = 0;
			foreach (string s in File.ReadAllLines(ViewModel.RightPath))
			{
				rightSide.Add(new Line() { Type = TextState.New, Text = s, LineIndex = i++ });
			}

			if (leftSide.Count > 0 && rightSide.Count > 0)
			{
				MatchLines(leftSide, rightSide);
			}

			FillViewModel(leftSide, rightSide);

			InitNavigationButtons();
			InitScrollbars();

			LeftDiff.Focus();

			Mouse.OverrideCursor = null;

			stopwatch.Stop();
			Statusbar.Text = $"Compare time {stopwatch.ElapsedMilliseconds}ms  left side {leftSide.Count} lines  right site {rightSide.Count} lines";
		}

		private void CompareDirectories()
		{
			ObservableCollection<FileItem> leftItems = new ObservableCollection<FileItem>();
			ObservableCollection<FileItem> rightItems = new ObservableCollection<FileItem>();

			SearchDirectory(ViewModel.LeftPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), leftItems, ViewModel.RightPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rightItems);

			LeftFolder.ItemsSource = leftItems;
			RightFolder.ItemsSource = rightItems;
		}

		private void SearchDirectory(string leftPath, ObservableCollection<FileItem> leftItems, string rightPath, ObservableCollection<FileItem> rightItems)
		{
			SortedDictionary<string, FileItemPair> allItems = new SortedDictionary<string, FileItemPair>();

			if (leftPath != null)
			{
				foreach (string directoryPath in Directory.GetDirectories(leftPath))
				{
					string directoryName = directoryPath.Substring(leftPath.Length + 1);
					allItems.Add("*" + directoryName, new FileItemPair(new FileItem(directoryName, true, TextState.Deleted), new FileItem("", true, TextState.Filler)));
				}
			}

			if (rightPath != null)
			{
				foreach (string directoryPath in Directory.GetDirectories(rightPath))
				{
					string directoryName = directoryPath.Substring(rightPath.Length + 1);
					string key = "*" + directoryName;
					if (!allItems.ContainsKey(key))
					{
						allItems.Add(key, new FileItemPair(new FileItem("", true, TextState.Filler), new FileItem(directoryName, true, TextState.New)));
					}
					else
					{
						allItems[key].RightItem = new FileItem(directoryName, true, TextState.FullMatch);
						allItems[key].LeftItem.Type = TextState.FullMatch;
					}
				}
			}

			if (leftPath != null)
			{
				foreach (string filePath in Directory.GetFiles(leftPath))
				{
					string fileName = filePath.Substring(leftPath.Length + 1);
					allItems.Add(fileName, new FileItemPair(new FileItem(fileName, false, TextState.Deleted), new FileItem("", false, TextState.Filler)));
				}
			}

			if (rightPath != null)
			{
				foreach (string filePath in Directory.GetFiles(rightPath))
				{
					string fileName = filePath.Substring(rightPath.Length + 1);
					if (!allItems.ContainsKey(fileName))
					{
						allItems.Add(fileName, new FileItemPair(new FileItem("", false, TextState.Filler), new FileItem(fileName, false, TextState.New)));
					}
					else
					{
						allItems[fileName].RightItem = new FileItem(fileName, false, TextState.FullMatch);
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
					SearchDirectory(leftItem.Name == "" ? null : Path.Combine(leftPath, leftItem.Name), leftItem.Children, rightItem.Name == "" ? null : Path.Combine(rightPath, rightItem.Name), rightItem.Children);
				}

				leftItems.Add(leftItem);
				rightItems.Add(rightItem);
			}


		}

		private void InitNavigationButtons()
		{
			currentLine = -1;
			firstDiff = -1;
			lastDiff = -1;

			for (int i = 0; i < ViewModel.LeftSide.Count; i++)
			{
				if (i == 0 || ViewModel.LeftSide[i - 1].Type == TextState.FullMatch)
				{
					if (firstDiff == -1 && ViewModel.LeftSide[i].Type != TextState.FullMatch)
					{
						firstDiff = i;
						currentLine = i;
						CenterOnLine(i);
					}
					if (ViewModel.LeftSide[i].Type != TextState.FullMatch)
					{
						lastDiff = i;
					}
				}
			}
		}

		private void InitScrollbars()
		{
			int visibleLines = (int)(LeftDiff.ActualHeight / OneCharacter.ActualHeight);
			VerticalScrollbar.Maximum = LeftDiff.Lines.Count - visibleLines;
			VerticalScrollbar.ViewportSize = visibleLines;
		}

		private void FillViewModel(List<Line> leftSide, List<Line> rightSide)
		{
			int rightIndex = 0;

			ViewModel.LeftSide = new ObservableCollection<Line>();
			ViewModel.RightSide = new ObservableCollection<Line>();

			for (int leftIndex = 0; leftIndex < leftSide.Count; leftIndex++)
			{
				if (leftSide[leftIndex].MatchingLineIndex == null)
				{
					ViewModel.LeftSide.Add(leftSide[leftIndex]);
					ViewModel.RightSide.Add(new Line() { Type = TextState.Filler });
				}
				else
				{
					while (rightIndex < leftSide[leftIndex].MatchingLineIndex)
					{
						ViewModel.LeftSide.Add(new Line() { Type = TextState.Filler });
						ViewModel.RightSide.Add(rightSide[rightIndex]);
						rightIndex++;
					}
					ViewModel.LeftSide.Add(leftSide[leftIndex]);
					ViewModel.RightSide.Add(rightSide[rightIndex]);
					rightIndex++;
				}
			}
			while (rightIndex < rightSide.Count)
			{
				ViewModel.LeftSide.Add(new Line() { Type = TextState.Filler });
				ViewModel.RightSide.Add(rightSide[rightIndex]);
				rightIndex++;
			}
		}

		private void MatchPartialLines(List<Line> leftRange, List<Line> rightRange)
		{
			int matchingCharacters = 0;
			int bestMatchingCharacters = 0;
			int bestLeft = 0;
			int bestRight = 0;

			bool lastLine = leftRange.Count == 1 || rightRange.Count == 1;

			for (int leftIndex = 0; leftIndex < leftRange.Count; leftIndex++)
			{
				if (leftRange[leftIndex].IsWhitespaceLine)
				{
					continue;
				}

				if (leftRange[leftIndex].TrimmedCharacters.Count > bestMatchingCharacters)
				{
					for (int rightIndex = 0; rightIndex < rightRange.Count; rightIndex++)
					{
						if (rightRange[rightIndex].IsWhitespaceLine)
						{
							continue;
						}

						if (rightRange[rightIndex].TrimmedCharacters.Count > bestMatchingCharacters)
						{
							matchingCharacters = CountMatchingCharacters(leftRange[leftIndex].TrimmedCharacters, rightRange[rightIndex].TrimmedCharacters, lastLine);
							if (matchingCharacters > bestMatchingCharacters)
							{
								bestMatchingCharacters = matchingCharacters;
								bestLeft = leftIndex;
								bestRight = rightIndex;
							}
						}
					}
				}
			}

			float leftMatching = (float)bestMatchingCharacters / leftRange[bestLeft].TrimmedText.Length;
			float rightMatching = (float)bestMatchingCharacters / rightRange[bestRight].TrimmedText.Length;

			if (leftMatching + rightMatching > Settings.LineSimilarityThreshold * 2 || leftRange[bestLeft].IsWhitespaceLine || rightRange[bestRight].IsWhitespaceLine || leftRange[bestLeft].TrimmedText == rightRange[bestRight].TrimmedText)
			{
				leftRange[bestLeft].MatchingLineIndex = rightRange[bestRight].LineIndex;
				rightRange[bestRight].MatchingLineIndex = leftRange[bestLeft].LineIndex;

				leftRange[bestLeft].Type = TextState.PartialMatch;
				rightRange[bestRight].Type = TextState.PartialMatch;

				if (leftRange[bestLeft].GetHashCode() == rightRange[bestRight].GetHashCode())
				{
					leftRange[bestLeft].Type = TextState.FullMatch;
					rightRange[bestRight].Type = TextState.FullMatch;
				}
				else if (Settings.ShowLineChanges)
				{
					leftRange[bestLeft].TextSegments.Clear();
					rightRange[bestRight].TextSegments.Clear();
					HighlightCharacterMatches(leftRange[bestLeft], rightRange[bestRight], leftRange[bestLeft].Characters, rightRange[bestRight].Characters);
				}

				if (bestLeft > 0 && bestRight > 0)
				{
					MatchPartialLines(leftRange.GetRange(0, bestLeft), rightRange.GetRange(0, bestRight));
				}

				if (leftRange.Count > bestLeft + 1 && rightRange.Count > bestRight + 1)
				{
					MatchPartialLines(leftRange.GetRange(bestLeft + 1, leftRange.Count - (bestLeft + 1)), rightRange.GetRange(bestRight + 1, rightRange.Count - (bestRight + 1)));
				}
			}
		}

		private void HighlightCharacterMatches(Line leftLine, Line rightLine, List<object> leftRange, List<object> rightRange)
		{
			FindLongestMatch(leftRange, rightRange, out int matchIndex, out int matchingIndex, out int matchLength);

			if (matchLength == 0 || (matchLength < Settings.CharacterMatchThreshold && (leftLine.TrimmedText.Length > Settings.CharacterMatchThreshold || rightLine.TrimmedText.Length > Settings.CharacterMatchThreshold)))
			{
				leftLine.TextSegments.Add(new TextSegment(CharactersToString(leftRange), TextState.Deleted));
				rightLine.TextSegments.Add(new TextSegment(CharactersToString(rightRange), TextState.New));
				return;
			}

			if (matchIndex > 0 && matchingIndex > 0)
			{
				HighlightCharacterMatches(leftLine, rightLine, leftRange.GetRange(0, matchIndex), rightRange.GetRange(0, matchingIndex));
			}
			else if (matchIndex > 0)
			{
				leftLine.TextSegments.Add(new TextSegment(CharactersToString(leftRange.GetRange(0, matchIndex)), TextState.Deleted));
			}
			else if (matchingIndex > 0)
			{
				rightLine.TextSegments.Add(new TextSegment(CharactersToString(rightRange.GetRange(0, matchingIndex)), TextState.New));
			}

			leftLine.TextSegments.Add(new TextSegment(CharactersToString(leftRange.GetRange(matchIndex, matchLength)), TextState.PartialMatch));
			rightLine.TextSegments.Add(new TextSegment(CharactersToString(rightRange.GetRange(matchingIndex, matchLength)), TextState.PartialMatch));

			if (leftRange.Count > matchIndex + matchLength && rightRange.Count > matchingIndex + matchLength)
			{
				HighlightCharacterMatches(leftLine, rightLine, leftRange.GetRange(matchIndex + matchLength, leftRange.Count - (matchIndex + matchLength)), rightRange.GetRange(matchingIndex + matchLength, rightRange.Count - (matchingIndex + matchLength)));
			}
			else if (leftRange.Count > matchIndex + matchLength)
			{
				leftLine.TextSegments.Add(new TextSegment(CharactersToString(leftRange.GetRange(matchIndex + matchLength, leftRange.Count - (matchIndex + matchLength))), TextState.Deleted));
			}
			else if (rightRange.Count > matchingIndex + matchLength)
			{
				rightLine.TextSegments.Add(new TextSegment(CharactersToString(rightRange.GetRange(matchingIndex + matchLength, rightRange.Count - (matchingIndex + matchLength))), TextState.New));
			}
		}

		private string CharactersToString(List<object> characters)
		{
			var sb = new StringBuilder();
			foreach (var c in characters)
			{
				sb.Append(c);
			}
			return sb.ToString();
		}

		private int CountMatchingCharacters(List<object> leftRange, List<object> rightRange, bool lastLine)
		{
			FindLongestMatch(leftRange, rightRange, out int matchIndex, out int matchingIndex, out int matchLength);

			if (lastLine)
			{
				if (matchLength == 0)
				{
					return 0;
				}
			}
			else
			{
				if (matchLength < Settings.CharacterMatchThreshold)
				{
					return 0;
				}
			}

			if (matchIndex > 0 && matchingIndex > 0)
			{
				matchLength += CountMatchingCharacters(leftRange.GetRange(0, matchIndex), rightRange.GetRange(0, matchingIndex), lastLine);
			}

			if (leftRange.Count > matchIndex + matchLength && rightRange.Count > matchingIndex + matchLength)
			{
				matchLength += CountMatchingCharacters(leftRange.GetRange(matchIndex + matchLength, leftRange.Count - (matchIndex + matchLength)), rightRange.GetRange(matchingIndex + matchLength, rightRange.Count - (matchingIndex + matchLength)), lastLine);
			}

			return matchLength;
		}

		private void MatchLines(List<Line> leftRange, List<Line> rightRange)
		{
			FindLongestMatch(new List<object>(leftRange.ToArray()), new List<object>(rightRange.ToArray()), out int matchIndex, out int matchingIndex, out int matchLength);

			// Single line matches and ranges containing only whitespace are in most cases false positives.
			if (matchLength < 2 || WhitespaceRange(leftRange.GetRange(matchIndex, matchLength)))
			{
				MatchPartialLines(leftRange, rightRange);
				return;
			}

			for (int i = 0; i < matchLength; i++)
			{
				leftRange[matchIndex + i].MatchingLineIndex = rightRange[matchingIndex + i].LineIndex;
				leftRange[matchIndex + i].Type = TextState.FullMatch;

				rightRange[matchingIndex + i].MatchingLineIndex = leftRange[matchIndex + i].LineIndex;
				rightRange[matchingIndex + i].Type = TextState.FullMatch;
			}

			if (matchIndex > 0 && matchingIndex > 0)
			{
				MatchLines(leftRange.GetRange(0, matchIndex), rightRange.GetRange(0, matchingIndex));
			}

			if (leftRange.Count > matchIndex + matchLength && rightRange.Count > matchingIndex + matchLength)
			{
				MatchLines(leftRange.GetRange(matchIndex + matchLength, leftRange.Count - (matchIndex + matchLength)), rightRange.GetRange(matchingIndex + matchLength, rightRange.Count - (matchingIndex + matchLength)));
			}
		}

		private bool WhitespaceRange(List<Line> range)
		{
			foreach (Line l in range)
			{
				if (!l.IsWhitespaceLine)
				{
					return false;
				}
			}
			return true;
		}

		private void FindLongestMatch(List<object> leftRange, List<object> rightRange, out int longestMatchIndex, out int longestMatchingIndex, out int longestMatchLength)
		{
			int matchIndex = -1;

			longestMatchIndex = 0;
			longestMatchingIndex = 0;
			longestMatchLength = 0;

			for (int i = 0; i < leftRange.Count - longestMatchLength; i++)
			{
				int matchLength = 0;
				int matchingIndex = 0;

				for (int j = 0; j < rightRange.Count - longestMatchLength; j++)
				{
					while (leftRange[i + matchLength].GetHashCode() == rightRange[j + matchLength].GetHashCode())
					{
						if (matchIndex == -1)
						{
							matchIndex = i;
							matchingIndex = j;
						}
						matchLength++;

						if (i + matchLength >= leftRange.Count || j + matchLength >= rightRange.Count)
						{
							break;
						}
					}
					if (matchIndex != -1)
					{
						j += matchLength - 1;
						if (matchLength > longestMatchLength)
						{
							longestMatchIndex = matchIndex;
							longestMatchingIndex = matchingIndex;
							longestMatchLength = matchLength;
						}
						matchIndex = -1;
						matchLength = 0;
					}
				}
			}
		}

		private void MoveToFirstDiff()
		{
			currentLine = -1;
			MoveToNextDiff();
		}

		private void MoveToLastDiff()
		{
			currentLine = ViewModel.LeftSide.Count;
			MoveToPrevoiusDiff();
		}

		private void MoveToPrevoiusDiff()
		{
			for (int i = currentLine - 1; i >= 0; i--)
			{
				if (i == 0 || ViewModel.LeftSide[i - 1].Type == TextState.FullMatch)
				{
					if (ViewModel.LeftSide[i].Type != TextState.FullMatch || ViewModel.RightSide[i].Type != TextState.FullMatch)
					{
						currentLine = i;
						CenterOnLine(i);
						return;
					}
				}
			}
		}

		private void MoveToNextDiff()
		{
			for (int i = currentLine + 1; i < ViewModel.LeftSide.Count; i++)
			{
				if (i == 0 || ViewModel.LeftSide[i - 1].Type == TextState.FullMatch)
				{
					if (ViewModel.LeftSide[i].Type != TextState.FullMatch || ViewModel.RightSide[i].Type != TextState.FullMatch)
					{
						currentLine = i;
						CenterOnLine(i);
						return;
					}
				}
			}
		}

		private void CenterOnLine(int i)
		{
			int visibleLines = (int)(LeftDiff.ActualHeight / OneCharacter.ActualHeight);
			VerticalScrollbar.Value = i - (visibleLines / 2);
		}

		private void LoadSettings()
		{
			AppSettings.ReadSettingsFromDisk();
			Settings = AppSettings.Settings;


			this.Left = Settings.PositionLeft;
			this.Top = Settings.PositionTop;
			this.Width = Settings.Width;
			this.Height = Settings.Height;
			this.WindowState = Settings.WindowState;
		}

		private void SaveSettings()
		{
			Settings.PositionLeft = this.Left;
			Settings.PositionTop = this.Top;
			Settings.Width = this.Width;
			Settings.Height = this.Height;

			Settings.WindowState = this.WindowState;

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
			Compare();
		}

		private void ToggleButtonShowLineChanges_Click(object sender, RoutedEventArgs e)
		{
			Compare();
		}

		private void LeftSide_DragEnter(object sender, DragEventArgs e)
		{
			e.Effects = DragDropEffects.All;
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

		private void RightSide_DragEnter(object sender, DragEventArgs e)
		{
			e.Effects = DragDropEffects.All;
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

		private void LeftDiff_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			InitScrollbars();
		}

		private void GridMainContent_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			int lines = SystemParameters.WheelScrollLines * e.Delta / 120;
			VerticalScrollbar.Value -= lines;
		}

		private void LeftHorizontalScrollbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			RightHorizontalScrollbar.Value = e.NewValue;
		}

		private void RightHorizontalScrollbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			LeftHorizontalScrollbar.Value = e.NewValue;
		}

		private void LeftFolder_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
		{
			DependencyObject border = VisualTreeHelper.GetChild(RightFolder, 0);
			if (border != null)
			{
				ScrollViewer scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
				scrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
				scrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
			}
		}

		private void RightFolder_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
		{
			DependencyObject border = VisualTreeHelper.GetChild(LeftFolder, 0);
			if (border != null)
			{
				ScrollViewer scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
				scrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
				scrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
			}
		}

		private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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
		}

		private void LeftDiff_GotFocus(object sender, RoutedEventArgs e)
		{
			activeDiff = LeftDiff;
		}

		#endregion

		#region Commands

		private void CommandExit_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
		}

		private void CommnadOptions_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			OptionsWindow optionsWindow = new OptionsWindow() { DataContext = ViewModel };
			optionsWindow.ShowDialog();
		}

		private void CommandAbout_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			AboutWindow aboutWindow = new AboutWindow();
			aboutWindow.ShowDialog();
		}

		private void CommandCompare_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			// Make sure view model is updated with both paths before we compare
			TextBoxRightPath.Focus();
			TextBoxLeftPath.Focus();

			Compare();
		}

		private void CommandBrowseLeft_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				ViewModel.LeftPath = ofd.FileName;
			}
		}

		private void CommandBrowseRight_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				ViewModel.RightPath = ofd.FileName;
			}
		}

		private void CommandPreviousDiff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MoveToPrevoiusDiff();
		}

		private void CommandPreviousDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = firstDiff < currentLine;
		}

		private void CommandNextDiff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MoveToNextDiff();
		}

		private void CommandNextDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = lastDiff > currentLine;
		}

		private void CommandFirstDiff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MoveToFirstDiff();
		}

		private void CommandFirstDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = firstDiff < currentLine;
		}

		private void CommandLastDiff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MoveToLastDiff();
		}

		private void CommandLastDiff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = lastDiff > currentLine;
		}

		private void CommandFind_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			SearchPanel.Visibility = Visibility.Visible;
			SearchBox.Focus();
		}

		private void CommandFindNext_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ProcessSearchResult(activeDiff.SearchNext(SearchBox.Text, MatchCase.IsChecked == true));
		}

		private void CommandFindNext_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SearchBox.Text != "" && activeDiff.Lines.Count > 0;
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

		#endregion

	}
}
