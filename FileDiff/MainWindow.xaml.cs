using System;
using System.Collections.Generic;
using System.IO;
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

		WindowData windowData = new WindowData();

		List<Line> LS = new List<Line>();
		List<Line> RS = new List<Line>();

		#endregion

		#region Constructor

		public MainWindow()
		{
			InitializeComponent();

			this.DataContext = this.windowData;
		}

		#endregion

		private void SetColors()
		{
			foreach (Line l in windowData.LeftSide)
			{
				if (l.Type == MatchType.NoMatch)
				{
					l.Foreground = new SolidColorBrush(Color.FromRgb(200, 0, 0));
					l.Background = new SolidColorBrush(Color.FromRgb(255, 220, 220));
				}
				else if (l.Type == MatchType.PartialMatch)
				{
					l.Background = new SolidColorBrush(Color.FromRgb(220, 220, 255));
				}
			}

			foreach (Line l in windowData.RightSide)
			{
				if (l.Type == MatchType.NoMatch)
				{
					l.Foreground = new SolidColorBrush(Color.FromRgb(0, 120, 0));
					l.Background = new SolidColorBrush(Color.FromRgb(220, 255, 220));
				}
				else if (l.Type == MatchType.PartialMatch)
				{
					l.Background = new SolidColorBrush(Color.FromRgb(220, 220, 255));
				}
			}
		}

		private void CompareFiles()
		{
			windowData.LeftSide.Clear();
			windowData.RightSide.Clear();

			if (File.Exists(windowData.LeftFile) && File.Exists(windowData.RightFile))
			{
				Mouse.OverrideCursor = Cursors.Wait;

				LoadFiles();
				DisplayLines();
				SetColors();

				Mouse.OverrideCursor = null;
			}
		}

		private void LoadFiles()
		{
			LS.Clear();
			RS.Clear();

			try
			{
				int i = 0;
				foreach (string s in File.ReadAllLines(windowData.LeftFile))
				{
					LS.Add(new Line() { Text = s, LineIndex = i++ });
				}

				i = 0;
				foreach (string s in File.ReadAllLines(windowData.RightFile))
				{
					RS.Add(new Line() { Text = s, LineIndex = i++ });
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		private void DisplayLines()
		{
			MatchLines(LS, RS);

			int rightIndex = 0;

			for (int leftIndex = 0; leftIndex < LS.Count; leftIndex++)
			{
				if (LS[leftIndex].MatchingLineIndex == null)
				{
					windowData.LeftSide.Add(LS[leftIndex]);
					windowData.RightSide.Add(new Line());
				}
				else
				{
					while (rightIndex < LS[leftIndex].MatchingLineIndex)
					{
						windowData.LeftSide.Add(new Line());
						windowData.RightSide.Add(RS[rightIndex]);
						rightIndex++;
					}
					windowData.LeftSide.Add(LS[leftIndex]);
					windowData.RightSide.Add(RS[rightIndex]);
					rightIndex++;
				}
			}
			while (rightIndex < RS.Count)
			{
				windowData.LeftSide.Add(new Line());
				windowData.RightSide.Add(RS[rightIndex]);
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
				windowData.LeftFile = Environment.GetCommandLineArgs()[1];
				windowData.RightFile = Environment.GetCommandLineArgs()[2];
				CompareFiles();
			}
		}

		private void CommandCompare_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			CompareFiles();
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
