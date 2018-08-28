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

				string textNoWhitespace = Regex.Replace(value, @"\s+", "");
				hashNoWhitespace = textNoWhitespace.GetHashCode();
				IsWhitespaceLine = textNoWhitespace == "";

				TextSegments.Clear();
				TextSegments.Add(new TextSegment(value, Type));
			}
		}

		public string TrimmedText { get; set; }

		public ObservableCollection<TextSegment> TextSegments { get; set; } = new ObservableCollection<TextSegment>();

		public bool IsWhitespaceLine { get; set; }

		public List<char> Characters
		{
			get
			{
				List<char> list = new List<char>();

				foreach (char c in text.ToCharArray())
				{
					list.Add(c);
				}
				return list;
			}
		}

		public List<char> TrimmedCharacters
		{
			get
			{
				List<char> list = new List<char>();

				foreach (char c in TrimmedText.ToCharArray())
				{
					list.Add(c);
				}
				return list;
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
				TextSegments.Add(new TextSegment(Text, value));
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

		public GlyphRun RenderedText { get; private set; }
		private double renderedTextWidth;

		private FontFamily renderedFontFamily;
		private FontStyle renderedFontStyle;
		private FontWeight renderedFontWeight;
		private FontStretch renderedFontStretch;
		private double renderedFontSize;
		private double renderedDpiScale;

		public GlyphRun GetRenderedLineIndexText(FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize, double dpiScale, out double runWidth)
		{
			if (!fontFamily.Equals(renderedFontFamily) || !fontStyle.Equals(renderedFontStyle) || !fontWeight.Equals(renderedFontWeight) || !fontStretch.Equals(renderedFontStretch) || fontSize != renderedFontSize || dpiScale != renderedDpiScale)
			{
				System.Diagnostics.Debug.Print(LineIndex.ToString());
				RenderedText = TextUtils.CreateGlyphRun(LineIndex == -1 ? "+" : LineIndex.ToString(), fontFamily, fontStyle, fontWeight, fontStretch, fontSize, dpiScale, out renderedTextWidth);

				renderedFontFamily = fontFamily;
				renderedFontStyle = fontStyle;
				renderedFontWeight = fontWeight;
				renderedFontStretch = fontStretch;
				renderedFontSize = fontSize;
				renderedDpiScale = dpiScale;
			}

			runWidth = renderedTextWidth;
			return RenderedText;
		}

		#endregion

	}
}
