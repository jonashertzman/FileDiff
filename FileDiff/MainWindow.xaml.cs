using System;
using System.Collections.Generic;
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

		private Brush deletedForeground = new SolidColorBrush(Color.FromRgb(200, 0, 0));
		private Brush deletedBackground = new SolidColorBrush(Color.FromRgb(255, 220, 220));

		private Brush addedForeground = new SolidColorBrush(Color.FromRgb(0, 120, 0));
		private Brush addedBackground = new SolidColorBrush(Color.FromRgb(220, 255, 220));

		private Brush modifiedForeground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
		private Brush modifiedBackground = new SolidColorBrush(Color.FromRgb(220, 220, 255));

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
			Mouse.OverrideCursor = Cursors.Wait;

			windowData.LeftSide.Clear();
			windowData.RightSide.Clear();

			List<Line> leftSide = new List<Line>();
			List<Line> rightSide = new List<Line>();

			int i = 0;
			foreach (string s in File.ReadAllLines(windowData.LeftPath))
			{
				leftSide.Add(new Line() { Text = s, LineIndex = i++, Type = MatchType.NoMatch, Foreground = deletedForeground, Background = deletedBackground });
			}

			i = 0;
			foreach (string s in File.ReadAllLines(windowData.RightPath))
			{
				rightSide.Add(new Line() { Text = s, LineIndex = i++, Type = MatchType.NoMatch, Foreground = addedForeground, Background = addedBackground });
			}

			MatchLines(leftSide, rightSide);

			DisplayLines(leftSide, rightSide);

			Mouse.OverrideCursor = null;
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
			float liknessThreshold = 0.4f;
			int matchingCharacters = 0;
			int bestMatchingCharacters = 0;
			int bestLeft = 0;
			int bestRight = 0;

			for (int leftIndex = 0; leftIndex < leftRange.Count; leftIndex++)
			{
				for (int rightIndex = 0; rightIndex < rightRange.Count; rightIndex++)
				{
					matchingCharacters = CountMatchingCharacters(leftRange[leftIndex].Characters, rightRange[rightIndex].Characters);
					if (matchingCharacters > bestMatchingCharacters)
					{
						bestMatchingCharacters = matchingCharacters;
						bestLeft = leftIndex;
						bestRight = rightIndex;
					}
				}
			}

			if (bestMatchingCharacters * 2 > (leftRange[bestLeft].Text.Length + rightRange[bestRight].Text.Length) * liknessThreshold)
			{
				leftRange[bestLeft].MatchingLineIndex = rightRange[bestRight].LineIndex;
				rightRange[bestRight].MatchingLineIndex = leftRange[bestLeft].LineIndex;

				leftRange[bestLeft].Type = MatchType.PartialMatch;
				rightRange[bestRight].Type = MatchType.PartialMatch;

				leftRange[bestLeft].TextSegments.Clear();
				rightRange[bestRight].TextSegments.Clear();
				HighlightCharacterMatches(leftRange[bestLeft], rightRange[bestRight], leftRange[bestLeft].Characters, rightRange[bestRight].Characters);

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

			if (matchLength == 0)
			{
				leftLine.TextSegments.Add(new TextSegment(CharactersToString(leftRange), deletedForeground, deletedBackground));
				rightLine.TextSegments.Add(new TextSegment(CharactersToString(rightRange), addedForeground, addedBackground));
				return;
			}

			if (matchIndex > 0 && matchingIndex > 0)
			{
				HighlightCharacterMatches(leftLine, rightLine, leftRange.GetRange(0, matchIndex), rightRange.GetRange(0, matchingIndex));
			}
			else if (matchIndex > 0)
			{
				leftLine.TextSegments.Add(new TextSegment(CharactersToString(leftRange.GetRange(0, matchIndex)), deletedForeground, deletedBackground));
			}
			else if (matchingIndex > 0)
			{
				rightLine.TextSegments.Add(new TextSegment(CharactersToString(rightRange.GetRange(0, matchingIndex)), addedForeground, addedBackground));
			}

			leftLine.TextSegments.Add(new TextSegment(CharactersToString(leftRange.GetRange(matchIndex, matchLength)), modifiedForeground, modifiedBackground));
			rightLine.TextSegments.Add(new TextSegment(CharactersToString(rightRange.GetRange(matchingIndex, matchLength)), modifiedForeground, modifiedBackground));

			if (leftRange.Count > matchIndex + matchLength && rightRange.Count > matchingIndex + matchLength)
			{
				HighlightCharacterMatches(leftLine, rightLine, leftRange.GetRange(matchIndex + matchLength, leftRange.Count - (matchIndex + matchLength)), rightRange.GetRange(matchingIndex + matchLength, rightRange.Count - (matchingIndex + matchLength)));
			}
			else if (leftRange.Count > matchIndex + matchLength)
			{
				leftLine.TextSegments.Add(new TextSegment(CharactersToString(leftRange.GetRange(matchIndex + matchLength, leftRange.Count - (matchIndex + matchLength))), deletedForeground, deletedBackground));
			}
			else if (rightRange.Count > matchingIndex + matchLength)
			{
				rightLine.TextSegments.Add(new TextSegment(CharactersToString(rightRange.GetRange(matchingIndex + matchLength, rightRange.Count - (matchingIndex + matchLength))), addedForeground, addedBackground));
			}
		}

		static string CharactersToString(List<object> characters)
		{
			var sb = new StringBuilder();
			foreach (var c in characters)
			{
				sb.Append(c);
			}
			return sb.ToString();
		}

		private int CountMatchingCharacters(List<object> leftRange, List<object> rightRange)
		{
			FindLongestMatch(leftRange, rightRange, out int matchIndex, out int matchingIndex, out int matchLength);

			if (matchLength == 0)
			{
				return 0;
			}

			if (matchIndex > 0 && matchingIndex > 0)
			{
				matchLength += CountMatchingCharacters(leftRange.GetRange(0, matchIndex), rightRange.GetRange(0, matchingIndex));
			}

			if (leftRange.Count > matchIndex + matchLength && rightRange.Count > matchingIndex + matchLength)
			{
				matchLength += CountMatchingCharacters(leftRange.GetRange(matchIndex + matchLength, leftRange.Count - (matchIndex + matchLength)), rightRange.GetRange(matchingIndex + matchLength, rightRange.Count - (matchingIndex + matchLength)));
			}

			return matchLength;
		}

		private void MatchLines(List<Line> leftRange, List<Line> rightRange)
		{
			FindLongestMatch(new List<object>(leftRange.ToArray()), new List<object>(rightRange.ToArray()), out int matchIndex, out int matchingIndex, out int matchLength);

			if (matchLength == 0)
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
						j += matchLength;
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

		#endregion

	}
}
