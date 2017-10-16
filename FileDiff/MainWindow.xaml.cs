using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
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

		private void LoadFiles()
		{
			LS.Clear();
			int i = 0;
			foreach (string s in File.ReadAllLines(windowData.LeftFile))
			{
				LS.Add(new Line() { Text = s, LineNumber = i++ });
			}

			RS.Clear();
			i = 0;
			foreach (string s in File.ReadAllLines(windowData.RightFile))
			{
				RS.Add(new Line() { Text = s, LineNumber = i++ });
			}
		}

		private void CompareFiles()
		{
			if (File.Exists(windowData.LeftFile) && File.Exists(windowData.RightFile))
			{
				LoadFiles();
				CompareRange(0, LS.Count - 1, 0, RS.Count - 1);

				int rightIndex = 0;

				for (int leftIndex = 0; leftIndex < LS.Count; leftIndex++)
				{
					if (LS[leftIndex].MatchingLineNumber == null)
					{
						windowData.LeftSide.Add(LS[leftIndex]);
						windowData.RightSide.Add(new Line());
					}
					else
					{
						while (rightIndex < LS[leftIndex].MatchingLineNumber)
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

				SetColors();

			}
		}

		private void SetColors()
		{
			foreach (Line l in windowData.LeftSide)
			{
				if (l.MatchingLineNumber == null)
				{
					l.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
					l.Background = new SolidColorBrush(Color.FromRgb(255, 220, 220));
				}
			}

			foreach (Line l in windowData.RightSide)
			{
				if (l.MatchingLineNumber == null)
				{
					l.Foreground = new SolidColorBrush(Color.FromRgb(0, 150, 0));
					l.Background = new SolidColorBrush(Color.FromRgb(220, 255, 220));
				}
			}
		}

		private void CompareRange(int leftStart, int leftEnd, int rightStart, int rightEnd)
		{
			FindLongestMatch(leftStart, leftEnd, rightStart, rightEnd, out int matchIndex, out int matchingIndex, out int matchLength);

			if (matchLength == 0)
			{
				return;
			}

			for (int i = 0; i < matchLength; i++)
			{
				LS[matchIndex + i].MatchingLineNumber = matchingIndex + i;
				RS[matchingIndex + i].MatchingLineNumber = matchIndex + i;
			}

			if (matchIndex > leftStart && matchingIndex > rightStart)
			{
				CompareRange(leftStart, matchIndex - 1, rightStart, matchingIndex - 1);
			}

			if (leftEnd > matchIndex + matchLength && rightEnd > matchingIndex + matchLength)
			{
				CompareRange(matchIndex + matchLength, leftEnd, matchingIndex + matchLength, rightEnd);
			}

		}

		private void FindLongestMatch(int leftStart, int leftEnd, int rightStart, int rightEnd, out int longestMatchIndex, out int longestMatchingIndex, out int longestMatchLength)
		{
			int matchIndex = -1;

			longestMatchIndex = 0;
			longestMatchingIndex = 0;
			longestMatchLength = 0;

			for (int i = leftStart; i <= leftEnd; i++)
			{
				int matchLength = 0;
				int matchingIndex = 0;

				for (int j = rightStart; j <= rightEnd; j++)
				{
					while (LS[i + matchLength].Hash == RS[j + matchLength].Hash)
					{
						if (matchIndex == -1)
						{
							matchIndex = i;
							matchingIndex = j;
						}
						matchLength++;

						if (i + matchLength > leftEnd || j + matchLength > rightEnd)
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

		#endregion

	}
}
