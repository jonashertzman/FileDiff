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
	public partial class MainWindow : Window
	{

		#region Members

		MainWindowViewModel ViewModel { get; set; } = new MainWindowViewModel();

		int firstDiff = -1;
		int lastDiff = -1;
		bool renderComplete = false;

		DiffControl activeDiff;
		string activeSelection;

		string rightSelection = "";
		string leftSelection = "";

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
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			Mouse.OverrideCursor = Cursors.Wait;

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

				if (leftLines.Count > 0 && rightLines.Count > 0)
				{
					MatchLines(leftLines, rightLines);
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}

			FillViewModel(leftLines, rightLines);

			LeftDiff.Focus();

			InitNavigationState();

			Mouse.OverrideCursor = null;

			stopwatch.Stop();
			Statusbar.Text = $"Compare time {stopwatch.ElapsedMilliseconds}ms";
		}

		private void CompareDirectories()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			Mouse.OverrideCursor = Cursors.Wait;

			ObservableCollection<FileItem> leftItems = new ObservableCollection<FileItem>();
			ObservableCollection<FileItem> rightItems = new ObservableCollection<FileItem>();

			SearchDirectory(ViewModel.LeftPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), leftItems, ViewModel.RightPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rightItems, 1);

			ViewModel.LeftFolder = leftItems;
			ViewModel.RightFolder = rightItems;

			LeftFolder.Focus();

			Mouse.OverrideCursor = null;

			stopwatch.Stop();
			Statusbar.Text = $"Compare time {stopwatch.ElapsedMilliseconds}ms";
		}

		private void SearchDirectory(string leftPath, ObservableCollection<FileItem> leftItems, string rightPath, ObservableCollection<FileItem> rightItems, int level)
		{
			if (leftPath?.Length > 259 || rightPath?.Length > 259)
			{
				return;
			}

			if (Directory.Exists(leftPath) && !DirectoryAllowed(leftPath) || Directory.Exists(rightPath) && !DirectoryAllowed(rightPath))
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
						leftItem.Type = TextState.Ignored;
						rightItem.Type = TextState.Ignored;
					}
					else
					{
						SearchDirectory(leftItem.Name == "" ? null : Path.Combine(FixRootPath(leftPath), leftItem.Name), leftItem.Children, rightItem.Name == "" ? null : Path.Combine(FixRootPath(rightPath), rightItem.Name), rightItem.Children, level + 1);

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

		private bool DirectoryAllowed(string path)
		{
			try
			{
				Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
			}
			catch
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

		private void FillViewModel(List<Line> leftSide, List<Line> rightSide)
		{
			int rightIndex = 0;

			ViewModel.LeftFile = new ObservableCollection<Line>();
			ViewModel.RightFile = new ObservableCollection<Line>();

			for (int leftIndex = 0; leftIndex < leftSide.Count; leftIndex++)
			{
				if (leftSide[leftIndex].MatchingLineIndex == null)
				{
					ViewModel.LeftFile.Add(leftSide[leftIndex]);
					ViewModel.RightFile.Add(new Line() { Type = TextState.Filler });
				}
				else
				{
					while (rightIndex < leftSide[leftIndex].MatchingLineIndex)
					{
						ViewModel.LeftFile.Add(new Line() { Type = TextState.Filler });
						ViewModel.RightFile.Add(rightSide[rightIndex]);
						rightIndex++;
					}
					ViewModel.LeftFile.Add(leftSide[leftIndex]);
					ViewModel.RightFile.Add(rightSide[rightIndex]);
					rightIndex++;
				}
			}
			while (rightIndex < rightSide.Count)
			{
				ViewModel.LeftFile.Add(new Line() { Type = TextState.Filler });
				ViewModel.RightFile.Add(rightSide[rightIndex]);
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

			if (leftMatching > AppSettings.LineSimilarityThreshold || rightMatching > AppSettings.LineSimilarityThreshold || leftRange[bestLeft].IsWhitespaceLine || rightRange[bestRight].IsWhitespaceLine || leftRange[bestLeft].TrimmedText == rightRange[bestRight].TrimmedText)
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
				else
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

		private void HighlightCharacterMatches(Line leftLine, Line rightLine, List<char> leftRange, List<char> rightRange)
		{
			FindLongestMatch(leftRange, rightRange, out int matchIndex, out int matchingIndex, out int matchLength);

			bool matchTooShort = matchLength == 0;

			if (leftLine.TrimmedText.Length > AppSettings.CharacterMatchThreshold || rightLine.TrimmedText.Length > AppSettings.CharacterMatchThreshold)
			{
				if (matchLength < AppSettings.CharacterMatchThreshold)
				{
					matchTooShort = true;
				}
			}

			if (matchTooShort)
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

		private string CharactersToString(List<char> characters)
		{
			var sb = new StringBuilder();
			foreach (var c in characters)
			{
				sb.Append(c);
			}
			return sb.ToString();
		}

		private int CountMatchingCharacters(List<char> leftRange, List<char> rightRange, bool lastLine)
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
				if (matchLength < AppSettings.CharacterMatchThreshold)
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
			FindLongestMatch(leftRange, rightRange, out int matchIndex, out int matchingIndex, out int matchLength);

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

		private void FindLongestMatch<T>(List<T> leftRange, List<T> rightRange, out int longestMatchIndex, out int longestMatchingIndex, out int longestMatchLength)
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
			int visibleLines = (int)(LeftDiff.ActualHeight / OneCharacter.ActualHeight);
			VerticalFileScrollbar.Value = i - (visibleLines / 2);
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
			activeSelection = RightDiff.Lines.Count > 0 ? rightSelection : "";
		}

		private void LeftDiff_GotFocus(object sender, RoutedEventArgs e)
		{
			activeDiff = LeftDiff;
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

		private void CommandSaveLeftFile_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			string leftPath = ViewModel.Mode == CompareMode.File ? ViewModel.LeftPath : LeftFolder.SelectedFile.Path;

			if (File.Exists(leftPath) && ViewModel.LeftFileDirty)
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
			string args = $"/Select, {activeSelection}";
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
