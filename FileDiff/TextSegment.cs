using System.Windows.Media;

namespace FileDiff;

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

	public string Text { get; set; }

	public bool IsWhiteSpace
	{
		get
		{
			return Text.Trim().Length == 0;
		}
	}

	public GlyphRun RenderedText { get; private set; }

	public SolidColorBrush BackgroundBrush
	{
		get
		{
			return AppSettings.GetFileBackground(Type);
		}
	}

	public SolidColorBrush ForegroundBrush
	{
		get
		{
			return AppSettings.GetFileForeground(Type);
		}
	}

	#endregion

	#region Methods

	private double renderedTextWidth;
	private Typeface renderedTypeface;
	private double renderedFontSize;
	private double renderedDpiScale;
	private int renderedTabSize;

	public GlyphRun GetRenderedText(Typeface typeface, double fontSize, double dpiScale, int tabSize, double startPosition, out double runWidth)
	{
		if (!typeface.Equals(renderedTypeface) || fontSize != renderedFontSize || dpiScale != renderedDpiScale || tabSize != renderedTabSize)
		{
			RenderedText = TextUtils.CreateGlyphRun(Text, typeface, fontSize, dpiScale, startPosition, out renderedTextWidth);

			renderedTypeface = typeface;
			renderedFontSize = fontSize;
			renderedDpiScale = dpiScale;
			renderedTabSize = tabSize;
		}

		runWidth = renderedTextWidth;
		return RenderedText;
	}

	#endregion

}
