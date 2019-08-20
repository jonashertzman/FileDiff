using System;
using System.Collections.Generic;
using System.Text;

namespace FileDiff
{
	public static class BackgroundCompare
	{

		#region Members

		public static bool CompareCancelled { get; private set; } = false;
		public static bool experimentalMatching;

		private static int progress;
		public static IProgress<int> progressHandler;

		private static DateTime startTime;

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
			if (experimentalMatching)
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
			else
			{
				int matchingCharacters = 0;
				int bestMatchingCharacters = 0;
				int bestLeft = 0;
				int bestRight = 0;

				bool lastLine = leftRange.Count == 1 || rightRange.Count == 1;

				for (int leftIndex = 0; leftIndex < leftRange.Count; leftIndex++)
				{
					if (CompareCancelled)
						return;

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
			foreach (var c in characters)
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

		#endregion

	}
}
