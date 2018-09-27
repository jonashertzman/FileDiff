using System.Windows;
using System.Windows.Media;

namespace FileDiff
{
	public class TextSegment
	{

		#region Constructor

		public TextSegment()
		{

		}

		public TextSegment(string text, TextState textState)
		{
			this.Text = text;
			this.Type = textState;
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return Text;
		}

		#endregion

		#region Properties

		public TextState Type { get; set; }

		private string Text { get; set; }

		public GlyphRun RenderedText { get; private set; }

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

		#endregion

		#region Methods

		private double renderedTextWidth;
		private Typeface renderedTypeface;
		private double renderedFontSize;
		private double renderedDpiScale;
		private bool renderedWhiteSpace;
		private int renderedTabSize;

		public GlyphRun GetRenderedText(Typeface typeface, double fontSize, double dpiScale, bool whiteSpace, int tabSize, out double runWidth)
		{
			if (!typeface.Equals(renderedTypeface) || fontSize != renderedFontSize || dpiScale != renderedDpiScale || whiteSpace != renderedWhiteSpace || tabSize != renderedTabSize)
			{
				RenderedText = TextUtils.CreateGlyphRun(Text, typeface, fontSize, dpiScale, out renderedTextWidth);

				renderedTypeface = typeface;
				renderedFontSize = fontSize;
				renderedDpiScale = dpiScale;
				renderedWhiteSpace = whiteSpace;
				renderedTabSize = tabSize;
			}

			runWidth = renderedTextWidth;
			return RenderedText;
		}

		#endregion

	}
}
