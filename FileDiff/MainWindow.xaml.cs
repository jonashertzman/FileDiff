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
				if (l.MatchingLineIndex == null)
				{
					l.Foreground = new SolidColorBrush(Color.FromRgb(200, 0, 0));
					l.Background = new SolidColorBrush(Color.FromRgb(255, 220, 220));
				}
			}

			foreach (Line l in windowData.RightSide)
			{
				if (l.MatchingLineIndex == null)
				{
					l.Foreground = new SolidColorBrush(Color.FromRgb(0, 120, 0));
					l.Background = new SolidColorBrush(Color.FromRgb(220, 255, 220));
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
				MatchLines();
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

		private void MatchLines()
		{
			MatchRange(LS, RS);

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
			foreach (Line leftLine in leftRange)
			{
				foreach (Line rightLine in rightRange)
				{

				}
			}
		}

		private void MatchRange(List<Line> leftRange, List<Line> rightRange)
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
				rightRange[matchingIndex + i].MatchingLineIndex = leftRange[matchIndex + i].LineIndex;
			}

			if (matchIndex > 0 && matchingIndex > 0)
			{
				MatchRange(leftRange.GetRange(0, matchIndex), rightRange.GetRange(0, matchingIndex));
			}

			if (leftRange.Count > matchIndex + matchLength && rightRange.Count > matchingIndex + matchLength)
			{
				MatchRange(leftRange.GetRange(matchIndex + matchLength, leftRange.Count - (matchIndex + matchLength)), rightRange.GetRange(matchingIndex + matchLength, rightRange.Count - (matchingIndex + matchLength)));
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
