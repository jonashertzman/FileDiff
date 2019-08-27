using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace FileDiff
{
	public static class BackgroundCompare
	{

		#region Members

		private static int progress;
		public static IProgress<int> progressHandler;
		private static DateTime startTime;
		private static string currentRoot;

		#endregion

		#region Properties

		public static bool CompareCancelled { get; private set; } = false;

		#endregion

		#region Methods

		public static void Cancel()
		{
			CompareCancelled = true;
		}

		public static Tuple<List<Line>, List<Line>, TimeSpan> CompareFiles(List<Line> leftLines, List<Line> rightLines)
		{
			CompareCancelled = false;
			progress = 0;

			startTime = DateTime.UtcNow;

			MatchLines(leftLines, rightLines);

			AddFillerLines(ref leftLines, ref rightLines);

			return new Tuple<List<Line>, List<Line>, TimeSpan>(leftLines, rightLines, DateTime.UtcNow.Subtract(startTime));
		}

		public static Tuple<ObservableCollection<FileItem>, ObservableCollection<FileItem>, TimeSpan> CompareDirectories(string leftPath, string rightPath)
		{
			CompareCancelled = false;
			progress = 0;
			currentRoot = leftPath + "\\";

			startTime = DateTime.UtcNow;

			ObservableCollection<FileItem> leftItems = new ObservableCollection<FileItem>();
			ObservableCollection<FileItem> rightItems = new ObservableCollection<FileItem>();

			MatchDirectories(leftPath, leftItems, rightPath, rightItems, 1);

			return new Tuple<ObservableCollection<FileItem>, ObservableCollection<FileItem>, TimeSpan>(leftItems, rightItems, DateTime.UtcNow.Subtract(startTime));
		}

		private static void MatchLines(List<Line> leftRange, List<Line> rightRange)
		{
			if (CompareCancelled)
				return;

			FindLongestMatch(leftRange, rightRange, out int matchIndex, out int matchingIndex, out int matchLength);

			// Single line matches and ranges containing only whitespace are in most cases false positives.
			if (matchLength < 2 || WhitespaceRange(leftRange.GetRange(matchIndex, matchLength)))
			{
				MatchPartialLines(leftRange, rightRange);
				return;
			}

			//Thread.Sleep(500);
			IncreaseProgress(matchLength * 2);

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

		private static void MatchPartialLines(List<Line> leftRange, List<Line> rightRange)
		{
			int matchingCharacters = 0;
			float bestMatchFraction = 0;
			float matchFraction = 0;
			int bestLeft = 0;
			int bestRight = 0;

			bool lastLine = leftRange.Count == 1 || rightRange.Count == 1;

			for (int leftIndex = 0; leftIndex < leftRange.Count; leftIndex++)
			{
				if (bestMatchFraction == 1)
				{
					break;
				}

				if (leftRange[leftIndex].IsWhitespaceLine)
				{
					continue;
				}

				if (leftRange[leftIndex].TrimmedCharacters.Count > bestMatchFraction)
				{
					for (int rightIndex = 0; rightIndex < rightRange.Count; rightIndex++)
					{
						if (rightRange[rightIndex].IsWhitespaceLine)
						{
							continue;
						}

						matchingCharacters = CountMatchingCharacters(leftRange[leftIndex].TrimmedCharacters, rightRange[rightIndex].TrimmedCharacters, lastLine);
						matchFraction = (float)matchingCharacters * 2 / (leftRange[leftIndex].TrimmedCharacters.Count + rightRange[rightIndex].TrimmedCharacters.Count);
						if (matchFraction > bestMatchFraction)
						{
							bestMatchFraction = matchFraction;
							bestLeft = leftIndex;
							bestRight = rightIndex;
							if (bestMatchFraction == 1)
							{
								break;
							}
						}
					}
				}
			}

			if (bestMatchFraction > AppSettings.LineSimilarityThreshold || leftRange[bestLeft].IsWhitespaceLine || rightRange[bestRight].IsWhitespaceLine || leftRange[bestLeft].TrimmedText == rightRange[bestRight].TrimmedText)
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

		private static void HighlightCharacterMatches(Line leftLine, Line rightLine, List<char> leftRange, List<char> rightRange)
		{
			if (CompareCancelled)
				return;

			IncreaseProgress(2);

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
				leftLine.AddTextSegment(CharactersToString(leftRange), TextState.Deleted);
				rightLine.AddTextSegment(CharactersToString(rightRange), TextState.New);
				return;
			}

			if (matchIndex > 0 && matchingIndex > 0)
			{
				HighlightCharacterMatches(leftLine, rightLine, leftRange.GetRange(0, matchIndex), rightRange.GetRange(0, matchingIndex));
			}
			else if (matchIndex > 0)
			{
				leftLine.AddTextSegment(CharactersToString(leftRange.GetRange(0, matchIndex)), TextState.Deleted);
			}
			else if (matchingIndex > 0)
			{
				rightLine.AddTextSegment(CharactersToString(rightRange.GetRange(0, matchingIndex)), TextState.New);
			}

			leftLine.AddTextSegment(CharactersToString(leftRange.GetRange(matchIndex, matchLength)), TextState.PartialMatch);
			rightLine.AddTextSegment(CharactersToString(rightRange.GetRange(matchingIndex, matchLength)), TextState.PartialMatch);

			if (leftRange.Count > matchIndex + matchLength && rightRange.Count > matchingIndex + matchLength)
			{
				HighlightCharacterMatches(leftLine, rightLine, leftRange.GetRange(matchIndex + matchLength, leftRange.Count - (matchIndex + matchLength)), rightRange.GetRange(matchingIndex + matchLength, rightRange.Count - (matchingIndex + matchLength)));
			}
			else if (leftRange.Count > matchIndex + matchLength)
			{
				leftLine.AddTextSegment(CharactersToString(leftRange.GetRange(matchIndex + matchLength, leftRange.Count - (matchIndex + matchLength))), TextState.Deleted);
			}
			else if (rightRange.Count > matchingIndex + matchLength)
			{
				rightLine.AddTextSegment(CharactersToString(rightRange.GetRange(matchingIndex + matchLength, rightRange.Count - (matchingIndex + matchLength))), TextState.New);
			}
		}

		private static void MatchDirectories(string leftPath, ObservableCollection<FileItem> leftItems, string rightPath, ObservableCollection<FileItem> rightItems, int level)
		{
			if (CompareCancelled)
			{
				return;
			}

			if (level == 2)
			{
				progressHandler.Report(Char.ToUpper((leftPath ?? rightPath)[currentRoot.Length]) - 'A');
			}

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

			if (leftPath != null)
			{
				foreach (FileItem f in SearchDirectory(leftPath, level))
				{
					f.Type = TextState.Deleted;
					allItems.Add(f.Key, new FileItemPair(f, new FileItem() { IsFolder = f.IsFolder, Type = TextState.Filler, Level = level }));
				}
			}

			if (rightPath != null)
			{
				foreach (FileItem f in SearchDirectory(rightPath, level))
				{
					if (!allItems.ContainsKey(f.Key))
					{
						f.Type = TextState.New;
						allItems.Add(f.Key, new FileItemPair(new FileItem() { IsFolder = f.IsFolder, Type = TextState.Filler, Level = level }, f));
					}
					else
					{
						allItems[f.Key].RightItem = f;
						allItems[f.Key].LeftItem.Type = TextState.FullMatch;
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

					// TODO: Refactor this
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
						MatchDirectories(leftItem.Name == "" ? null : Path.Combine(FixRootPath(leftPath), leftItem.Name), leftItem.Children, rightItem.Name == "" ? null : Path.Combine(FixRootPath(rightPath), rightItem.Name), rightItem.Children, level + 1);
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
					// TODO: Refactor this
					if (FileIsIgnored(leftItem.Name) || FileIsIgnored(rightItem.Name))
					{
						if (FileIsIgnored(leftItem.Name))
						{
							leftItem.Type = TextState.Ignored;
						}
						if (FileIsIgnored(rightItem.Name))
						{
							rightItem.Type = TextState.Ignored;
						}
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

		private static List<FileItem> SearchDirectory(string path, int level)
		{
			List<FileItem> items = new List<FileItem>();

			IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
			IntPtr findHandle = WinApi.FindFirstFile(Path.Combine(path, "*"), out WIN32_FIND_DATA findData);

			string newPath;

			if (findHandle != INVALID_HANDLE_VALUE)
			{
				do
				{
					if (findData.cFileName != "." && findData.cFileName != "..")
					{
						newPath = Path.Combine(path, findData.cFileName);
						items.Add(new FileItem(newPath, level, findData));
					}
				}
				while (WinApi.FindNextFile(findHandle, out findData));
			}

			WinApi.FindClose(findHandle);

			return items;
		}

		private static int CountMatchingCharacters(List<char> leftRange, List<char> rightRange, bool lastLine)
		{
			if (CompareCancelled)
				return 0;

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

		private static void AddFillerLines(ref List<Line> leftFile, ref List<Line> rightFile)
		{
			int rightIndex = 0;

			List<Line> newLeft = new List<Line>();
			List<Line> newRight = new List<Line>();

			for (int leftIndex = 0; leftIndex < leftFile.Count; leftIndex++)
			{
				if (leftFile[leftIndex].MatchingLineIndex == null)
				{
					newLeft.Add(leftFile[leftIndex]);
					newRight.Add(new Line() { Type = TextState.Filler });
				}
				else
				{
					while (rightIndex < leftFile[leftIndex].MatchingLineIndex)
					{
						newLeft.Add(new Line() { Type = TextState.Filler });
						newRight.Add(rightFile[rightIndex]);
						rightIndex++;
					}
					newLeft.Add(leftFile[leftIndex]);
					newRight.Add(rightFile[rightIndex]);
					rightIndex++;
				}
			}
			while (rightIndex < rightFile.Count)
			{
				newLeft.Add(new Line() { Type = TextState.Filler });
				newRight.Add(rightFile[rightIndex]);
				rightIndex++;
			}

			leftFile = newLeft;
			rightFile = newRight;
		}

		private static bool WhitespaceRange(List<Line> range)
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

		private static void FindLongestMatch<T>(List<T> leftRange, List<T> rightRange, out int longestMatchIndex, out int longestMatchingIndex, out int longestMatchLength)
		{
			longestMatchIndex = 0;
			longestMatchingIndex = 0;
			longestMatchLength = 0;

			int matchLength;

			for (int i = 0; i < leftRange.Count - longestMatchLength; i++)
			{
				if (CompareCancelled)
				{
					break;
				}
				for (int j = 0; j < rightRange.Count - longestMatchLength; j++)
				{
					matchLength = 0;
					while (leftRange[i + matchLength].GetHashCode() == rightRange[j + matchLength].GetHashCode())
					{
						matchLength++;

						if (i + matchLength >= leftRange.Count || j + matchLength >= rightRange.Count)
						{
							break;
						}
					}

					if (matchLength > 0)
					{
						if (matchLength > longestMatchLength)
						{
							longestMatchIndex = i;
							longestMatchingIndex = j;
							longestMatchLength = matchLength;
						}
					}
				}
			}
		}

		private static string CharactersToString(List<char> characters)
		{
			var sb = new StringBuilder();
			foreach (char c in characters)
			{
				sb.Append(c);
			}
			return sb.ToString();
		}

		private static void IncreaseProgress(int amount)
		{
			progress += amount;
			progressHandler.Report(progress);
		}

		private static string FixRootPath(string path)
		{
			// Directory.GetDirectories, Directory.GetFiles and Path.Combine does not work on root paths without trailing backslashes.
			if (path.EndsWith(":"))
			{
				return path += "\\";
			}
			return path;
		}

		private static bool DirectoryIsIgnored(string directory)
		{
			foreach (TextAttribute a in AppSettings.IgnoredFolders)
			{
				if (WildcardCompare(directory, a.Text, true))
				{
					return true;
				}
			}
			return false;
		}

		private static bool FileIsIgnored(string directory)
		{
			foreach (TextAttribute a in AppSettings.IgnoredFiles)
			{
				if (WildcardCompare(directory, a.Text, true))
				{
					return true;
				}
			}
			return false;
		}

		private static bool WildcardCompare(string compare, string wildString, bool ignoreCase)
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

		#endregion

	}
}
