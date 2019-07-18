using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace FileDiff
{
	public class Line
	{

		#region Members

		private int hash;
		private int hashNoWhitespace;

		#endregion

		#region Constructor

		public Line()
		{

		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return $"{LineIndex}  {Text}  {MatchingLineIndex}";
		}

		public override int GetHashCode()
		{
			return AppSettings.IgnoreWhiteSpace ? hashNoWhitespace : hash;
		}

		#endregion

		#region Properties

		private string text = "";
		public string Text
		{
			get { return text; }
			set
			{
				text = value;
				TrimmedText = value.Trim();
				hash = value.GetHashCode();

				characters = null;
				trimmedCharacters = null;

				string textNoWhitespace = Regex.Replace(value, @"\s+", "");
				hashNoWhitespace = textNoWhitespace.GetHashCode();
				IsWhitespaceLine = textNoWhitespace == "";

				TextSegments.Clear();
				AddTextSegment(value, Type);
			}
		}

		public string TrimmedText { get; set; }

		public ObservableCollection<TextSegment> TextSegments { get; set; } = new ObservableCollection<TextSegment>();

		public bool IsWhitespaceLine { get; set; }

		List<char> characters;
		public List<char> Characters
		{
			get
			{
				if (characters == null)
				{
					characters = new List<char>();

					foreach (char c in text.ToCharArray())
					{
						characters.Add(c);
					}
				}
				return characters;
			}
		}

		List<char> trimmedCharacters;
		public List<char> TrimmedCharacters
		{
			get
			{
				if (trimmedCharacters == null)
				{
					trimmedCharacters = new List<char>();

					foreach (char c in TrimmedText.ToCharArray())
					{
						trimmedCharacters.Add(c);
					}
				}
				return trimmedCharacters;
			}
		}

		public int? LineIndex { get; set; }

		private TextState type;
		public TextState Type
		{
			get { return type; }
			set
			{
				type = value;
				TextSegments.Clear();
				AddTextSegment(Text, value);
			}
		}

		public SolidColorBrush BackgroundBrush
		{
			get
			{
				switch (type)
				{
					case TextState.Deleted:
						return AppSettings.DeletedBackground;
					case TextState.New:
						return AppSettings.NewBackground;
					case TextState.PartialMatch:
						return AppSettings.PartialMatchBackground;

					default:
						return AppSettings.FullMatchBackground;
				}
			}
		}

		public SolidColorBrush ForegroundBrush
		{
			get
			{
				switch (type)
				{
					case TextState.Deleted:
						return AppSettings.DeletedForeground;
					case TextState.New:
						return AppSettings.NewForeground;
					case TextState.PartialMatch:
						return AppSettings.PartialMatchForeground;

					default:
						return AppSettings.FullMatchForeground;
				}
			}
		}

		public int? MatchingLineIndex { get; set; }

		private GlyphRun RenderedText;
		private double renderedTextWidth;

		private int? renderedLineIndex;
		private Typeface renderedTypeface;
		private double renderedFontSize;
		private double renderedDpiScale;

		public GlyphRun GetRenderedLineIndexText(Typeface typeface, double fontSize, double dpiScale, out double runWidth)
		{
			if (renderedLineIndex != LineIndex || !typeface.Equals(renderedTypeface) || fontSize != renderedFontSize || dpiScale != renderedDpiScale)
			{
				RenderedText = TextUtils.CreateGlyphRun(LineIndex == -1 ? "+" : LineIndex.ToString(), typeface, fontSize, dpiScale, out renderedTextWidth);

				renderedLineIndex = LineIndex;
				renderedTypeface = typeface;
				renderedFontSize = fontSize;
				renderedDpiScale = dpiScale;
			}

			runWidth = renderedTextWidth;
			return RenderedText;
		}

		#endregion

		#region Methods

		public void AddTextSegment(string text, TextState state)
		{
			int segmentLength;

			do
			{
				segmentLength = Math.Min(1000, text.Length);
				if (segmentLength > 0 && char.IsHighSurrogate(text[segmentLength - 1]))
				{
					segmentLength--;
				}

				TextSegments.Add(new TextSegment(text.Substring(0, segmentLength), state));
				text = text.Remove(0, segmentLength);
			} while (text.Length > 0);
		}

		#endregion

	}
}
