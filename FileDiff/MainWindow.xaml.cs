using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
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

		private WindowData windowData = new WindowData();

		#endregion

		#region Constructor

		public MainWindow()
		{
			InitializeComponent();

			this.DataContext = this.windowData;
		}

		#endregion

		private void Compare()
		{
			if (File.Exists(windowData.LeftPath) && File.Exists(windowData.RightPath))
			{
				CompareFiles(windowData.LeftPath, windowData.RightPath);
			}
			else if (Directory.Exists(windowData.LeftPath) && Directory.Exists(windowData.RightPath))
			{
				CompareDirectories(windowData.LeftPath, windowData.RightPath);
			}
		}

		private void CompareDirectories(string leftPath, string rightPath)
		{
		}

		private void CompareFiles(string leftPath, string rightPath)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			Mouse.OverrideCursor = Cursors.Wait;

			windowData.LeftSide.Clear();
			windowData.RightSide.Clear();

			List<Line> leftSide = new List<Line>();
			List<Line> rightSide = new List<Line>();

			int i = 0;
			foreach (string s in File.ReadAllLines(windowData.LeftPath))
			{
				leftSide.Add(new Line() { Text = s, LineIndex = i++, Type = MatchType.NoMatch, Foreground = Settings.DeletedForeground, Background = Settings.DeletedBackground });
			}

			i = 0;
			foreach (string s in File.ReadAllLines(windowData.RightPath))
			{
				rightSide.Add(new Line() { Text = s, LineIndex = i++, Type = MatchType.NoMatch, Foreground = Settings.AddedForeground, Background = Settings.AddedBackground });
			}

			MatchLines(leftSide, rightSide);

			DisplayLines(leftSide, rightSide);

			Mouse.OverrideCursor = null;

			stopwatch.Stop();
			Statusbar.Text = $"Compare time {stopwatch.ElapsedMilliseconds}ms  left side {leftSide.Count}  right site {rightSide.Count}";
		}

		private void DisplayLines(List<Line> leftSide, List<Line> rightSide)
		{
			int rightIndex = 0;

			for (int leftIndex = 0; leftIndex < leftSide.Count; leftIndex++)
			{
				if (leftSide[leftIndex].MatchingLineIndex == null)
				{
					windowData.LeftSide.Add(leftSide[leftIndex]);
					windowData.RightSide.Add(new Line());
				}
				else
				{
					while (rightIndex < leftSide[leftIndex].MatchingLineIndex)
					{
						windowData.LeftSide.Add(new Line());
						windowData.RightSide.Add(rightSide[rightIndex]);
						rightIndex++;
					}
					windowData.LeftSide.Add(leftSide[leftIndex]);
					windowData.RightSide.Add(rightSide[rightIndex]);
					rightIndex++;
				}
			}
			while (rightIndex < rightSide.Count)
			{
				windowData.LeftSide.Add(new Line());
				windowData.RightSide.Add(rightSide[rightIndex]);
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
				if (leftRange[leftIndex].TrimmedCharacters.Count > bestMatchingCharacters)
				{
					for (int rightIndex = 0; rightIndex < rightRange.Count; rightIndex++)
					{
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

			if (leftMatching + rightMatching > Settings.LineSimilarityThreshold * 2)
			{
				leftRange[bestLeft].MatchingLineIndex = rightRange[bestRight].LineIndex;
				rightRange[bestRight].MatchingLineIndex = leftRange[bestLeft].LineIndex;

				leftRange[bestLeft].Type = MatchType.PartialMatch;
				rightRange[bestRight].Type = MatchType.PartialMatch;

				if (leftRange[bestLeft].GetHashCode() == rightRange[bestRight].GetHashCode())
				{
					leftRange[bestLeft].Type = MatchType.FullMatch;
					rightRange[bestRight].Type = MatchType.FullMatch;
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

			if (matchLength < Settings.CharacterMatchThreshold)
			{
				leftLine.TextSegments.Add(new TextSegment(CharactersToString(leftRange), Settings.DeletedForeground, Settings.DeletedBackground));
				rightLine.TextSegments.Add(new TextSegment(CharactersToString(rightRange), Settings.AddedForeground, Settings.AddedBackground));
				return;
			}

			if (matchIndex > 0 && matchingIndex > 0)
			{
				HighlightCharacterMatches(leftLine, rightLine, leftRange.GetRange(0, matchIndex), rightRange.GetRange(0, matchingIndex));
			}
			else if (matchIndex > 0)
			{
				leftLine.TextSegments.Add(new TextSegment(CharactersToString(leftRange.GetRange(0, matchIndex)), Settings.DeletedForeground, Settings.DeletedBackground));
			}
			else if (matchingIndex > 0)
			{
				rightLine.TextSegments.Add(new TextSegment(CharactersToString(rightRange.GetRange(0, matchingIndex)), Settings.AddedForeground, Settings.AddedBackground));
			}

			leftLine.TextSegments.Add(new TextSegment(CharactersToString(leftRange.GetRange(matchIndex, matchLength)), Settings.ModifiedForeground, Settings.ModifiedBackground));
			rightLine.TextSegments.Add(new TextSegment(CharactersToString(rightRange.GetRange(matchingIndex, matchLength)), Settings.ModifiedForeground, Settings.ModifiedBackground));

			if (leftRange.Count > matchIndex + matchLength && rightRange.Count > matchingIndex + matchLength)
			{
				HighlightCharacterMatches(leftLine, rightLine, leftRange.GetRange(matchIndex + matchLength, leftRange.Count - (matchIndex + matchLength)), rightRange.GetRange(matchingIndex + matchLength, rightRange.Count - (matchingIndex + matchLength)));
			}
			else if (leftRange.Count > matchIndex + matchLength)
			{
				leftLine.TextSegments.Add(new TextSegment(CharactersToString(leftRange.GetRange(matchIndex + matchLength, leftRange.Count - (matchIndex + matchLength))), Settings.DeletedForeground, Settings.DeletedBackground));
			}
			else if (rightRange.Count > matchingIndex + matchLength)
			{
				rightLine.TextSegments.Add(new TextSegment(CharactersToString(rightRange.GetRange(matchingIndex + matchLength, rightRange.Count - (matchingIndex + matchLength))), Settings.AddedForeground, Settings.AddedBackground));
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

			if (matchLength == 0 || (matchLength == 1 && leftRange[matchIndex].TrimmedText.Length <= Settings.FullMatchLineLengthThreshold))
			{
				MatchPartialLines(leftRange, rightRange);
				return;
			}

			for (int i = 0; i < matchLength; i++)
			{
				leftRange[matchIndex + i].MatchingLineIndex = rightRange[matchingIndex + i].LineIndex;
				leftRange[matchIndex + i].Type = MatchType.FullMatch;

				rightRange[matchingIndex + i].MatchingLineIndex = leftRange[matchIndex + i].LineIndex;
				rightRange[matchingIndex + i].Type = MatchType.FullMatch;
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

		private void FindLongestMatch(List<object> leftRange, List<object> rightRange, out int longestMatchIndex, out int longestMatchingIndex, out int longestMatchLength)
		{
			int matchIndex = -1;

			longestMatchIndex = 0;
			longestMatchingIndex = 0;
			longestMatchLength = 0;

			for (int i = 0; i < leftRange.Count; i++)
			{
				int matchLength = 0;
				int matchingIndex = 0;

				for (int j = 0; j < rightRange.Count; j++)
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

		#region Events

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (Environment.GetCommandLineArgs().Length > 2)
			{
				windowData.LeftPath = Environment.GetCommandLineArgs()[1];
				windowData.RightPath = Environment.GetCommandLineArgs()[2];
				Compare();
			}
		}

		private void CommandCompare_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			Compare();
		}

		private void CommandExit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			this.Close();
		}

		private void RightScroll_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
		{
			LeftScroll.ScrollToVerticalOffset(e.VerticalOffset);
			LeftScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
		}

		private void LeftScroll_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
		{
			RightScroll.ScrollToVerticalOffset(e.VerticalOffset);
			RightScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
		}

		private void ToggleButtonIgnoreWhiteSpace_Click(object sender, RoutedEventArgs e)
		{
			Compare();
		}

		private void ToggleButtonShowLineChanges_Click(object sender, RoutedEventArgs e)
		{
			Compare();
		}

		private void BrowseLeft_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				windowData.LeftPath = ofd.FileName;
			}
		}

		private void BrowseRight_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				windowData.RightPath = ofd.FileName;
			}
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
				windowData.LeftPath = paths[0];
				windowData.RightPath = paths[1];
			}
			else if (paths?.Length >= 1)
			{
				windowData.LeftPath = paths[0];
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
				windowData.LeftPath = paths[0];
				windowData.RightPath = paths[1];
			}
			else if (paths?.Length >= 1)
			{
				windowData.RightPath = paths[0];
			}
		}

		#endregion

	}
}
