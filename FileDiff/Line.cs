﻿using System.Collections.ObjectModel;
using System.Windows.Media;

namespace FileDiff;

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
		return $"{LineIndex}-{MatchingLineIndex} ({DiffId})".PadRight(12, ' ') + $" {Type.ToString().PadRight(10, ' ')}    {Text}";
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

			//string textNoWhitespace = Regex.Replace(value, @"\s+", "");
			string textNoWhitespace = value.Trim();
			hashNoWhitespace = textNoWhitespace.GetHashCode();
			IsWhitespaceLine = textNoWhitespace == "";

			characters = null;
			trimmedCharacters = null;

			TextSegments.Clear();
			AddTextSegment(value, Type);
		}
	}

	public string TrimmedText { get; private set; }

	public ObservableCollection<TextSegment> TextSegments { get; private set; } = [];

	public bool IsWhitespaceLine { get; private set; }

	private List<char> characters;
	public List<char> Characters
	{
		get
		{
			if (characters == null)
			{
				characters = [.. text.ToCharArray()];
			}
			return characters;
		}
	}

	private List<char> trimmedCharacters;
	public List<char> TrimmedCharacters
	{
		get
		{
			if (trimmedCharacters == null)
			{
				trimmedCharacters = [.. TrimmedText.ToCharArray()];
			}
			return trimmedCharacters;
		}
	}

	public int? LineIndex { get; set; }

	public int? MatchingLineIndex { get; set; }

	public Dictionary<int?, float> MatchFactions { get; } = [];

	private TextState type;
	public TextState Type
	{
		get { return type; }
		set
		{
			if (type != value)
			{
				type = value;
				TextSegments.Clear();
				AddTextSegment(Text, value);
			}
		}
	}

	public bool IsFiller
	{
		get
		{
			return type == TextState.Filler || type == TextState.MovedFiller;
		}
	}

	public SolidColorBrush BackgroundBrush
	{
		get
		{
			return AppSettings.GetFileBackground(type);
		}
	}

	public SolidColorBrush ForegroundBrush
	{
		get
		{
			return AppSettings.GetFileForeground(type);
		}
	}

	public int DisplayIndex { get; set; }

	public int DisplayOffset { get; set; }

	public int DiffId { get; set; } = -1;

	private GlyphRun renderedIndexText;
	private double renderedIndexTextWidth;

	private int? renderedLineIndex;
	private Typeface renderedTypeface;
	private double renderedFontSize;
	private double renderedDpiScale;

	public GlyphRun GetRenderedLineIndexText(Typeface typeface, double fontSize, double dpiScale, out double runWidth)
	{
		if (renderedLineIndex != LineIndex || !typeface.Equals(renderedTypeface) || fontSize != renderedFontSize || dpiScale != renderedDpiScale)
		{
			renderedIndexText = TextUtils.CreateGlyphRun(LineIndex == -1 ? "+" : LineIndex.ToString(), typeface, fontSize, dpiScale, 0, out renderedIndexTextWidth);

			renderedLineIndex = LineIndex;
			renderedTypeface = typeface;
			renderedFontSize = fontSize;
			renderedDpiScale = dpiScale;
		}

		runWidth = renderedIndexTextWidth;
		return renderedIndexText;
	}

	#endregion

	#region Methods

	public void AddTextSegment(string text, TextState state)
	{
		int start = 0;
		int segmentLength;

		do
		{
			segmentLength = Math.Min(1000, text.Length - start);
			if (segmentLength > 0 && char.IsHighSurrogate(text[start + segmentLength - 1]))
			{
				segmentLength--;
			}

			TextSegments.Add(new TextSegment(text.Substring(start, segmentLength), state));
			start += segmentLength;
		} while (text.Length > start);
	}

	#endregion

}
