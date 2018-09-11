using System.Windows;
using System.Windows.Media;

namespace FileDiff
{
	public class TextSegment
	{

		public TextSegment()
		{

		}

		public TextSegment(string text, TextState textState)
		{
			this.Text = text;
			this.Type = textState;
		}

		public override string ToString()
		{
			return Text;
		}

		public TextState Type { get; set; }

		public string Text { get; private set; }

		public GlyphRun RenderedText { get; private set; }
		private double renderedTextWidth;

		private FontFamily renderedFontFamily;
		private FontStyle renderedFontStyle;
		private FontWeight renderedFontWeight;
		private FontStretch renderedFontStretch;
		private double renderedFontSize;
		private double renderedDpiScale;
		private bool renderedWhiteSpace;
		private int renderedTabSize;

		public GlyphRun GetRenderedText(FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize, double dpiScale, bool whiteSpace, int tabSize, out double runWidth)
		{
			if (!fontFamily.Equals(renderedFontFamily) || !fontStyle.Equals(renderedFontStyle) || !fontWeight.Equals(renderedFontWeight) || !fontStretch.Equals(renderedFontStretch) || fontSize != renderedFontSize || dpiScale != renderedDpiScale || whiteSpace != renderedWhiteSpace || tabSize != renderedTabSize)
			{
				RenderedText = TextUtils.CreateGlyphRun(Text, fontFamily, fontStyle, fontWeight, fontStretch, fontSize, dpiScale, out renderedTextWidth);

				renderedFontFamily = fontFamily;
				renderedFontStyle = fontStyle;
				renderedFontWeight = fontWeight;
				renderedFontStretch = fontStretch;
				renderedFontSize = fontSize;
				renderedDpiScale = dpiScale;
				renderedWhiteSpace = whiteSpace;
				renderedTabSize = tabSize;
			}

			runWidth = renderedTextWidth;
			return RenderedText;
		}

		public SolidColorBrush BackgroundBrush
		{
			get
			{
				switch (Type)
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
				switch (Type)
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

	}
}
