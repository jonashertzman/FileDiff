using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace FileDiff
{
	static class TextUtils
	{

		#region Members

		static Dictionary<Typeface, GlyphTypeface> glyphTypefaces = new Dictionary<Typeface, GlyphTypeface>();
		static readonly GlyphTypeface defaultGlyphTypeface;

		#endregion

		#region Constructor

		static TextUtils()
		{
			Typeface defaultTypface = new Typeface("Courier New");

			defaultTypface.TryGetGlyphTypeface(out defaultGlyphTypeface);
			glyphTypefaces.Add(defaultTypface, defaultGlyphTypeface);
		}

		#endregion

		#region Methods

		internal static GlyphRun CreateGlyphRun(string text, Typeface typeface, double fontSize, double dpiScale, out double runWidth)
		{
			if (text.Length == 0)
			{
				runWidth = 0;
				return null;
			}


			GlyphTypeface glyphTypeface;

			if (!glyphTypefaces.ContainsKey(typeface))
			{
				if (typeface.TryGetGlyphTypeface(out glyphTypeface))
				{
					glyphTypefaces.Add(typeface, glyphTypeface);
				}
				else
				{
					glyphTypeface = defaultGlyphTypeface;
				}
			}
			else
			{
				glyphTypeface = glyphTypefaces[typeface];
			}

			ushort[] glyphIndexes = new ushort[text.Length];
			double[] advanceWidths = new double[text.Length];

			double totalWidth = 0;
			int codePoint;
			for (int n = 0; n < text.Length; n++)
			{
				if (char.IsHighSurrogate(text[n]))
				{
					codePoint = char.ConvertToUtf32(text, n);
				}
				else if (char.IsLowSurrogate(text[n]))
				{
					codePoint = '\u200B';
				}
				else
				{
					codePoint = text[n];
				}

				ushort glyphIndex = ReplaceGlyph(codePoint, glyphTypeface, fontSize, dpiScale, out double width);

				glyphIndexes[n] = glyphIndex;
				advanceWidths[n] = width;

				totalWidth += width;
			}

			GetTextBounds(glyphTypeface, fontSize, out double topDistance, out double bottomDistance);

			if (glyphTypeface.Baseline * fontSize > topDistance)
			{
				topDistance = glyphTypeface.Baseline * fontSize * glyphTypeface.Height;
			}

			topDistance = Math.Ceiling(topDistance / dpiScale) * dpiScale;

			GlyphRun run = new GlyphRun(
				glyphTypeface: glyphTypeface,
				bidiLevel: 0,
				isSideways: false,
				renderingEmSize: Math.Ceiling(fontSize / dpiScale) * dpiScale,
				glyphIndices: glyphIndexes,
				baselineOrigin: new Point(0, topDistance),
				advanceWidths: advanceWidths,
				glyphOffsets: null,
				characters: null,
				deviceFontName: null,
				clusterMap: null,
				caretStops: null,
				language: null);

			runWidth = totalWidth;
			return run;
		}

		private static ushort ReplaceGlyph(int codePoint, GlyphTypeface glyphTypeface, double fontSize, double dpiScale, out double width)
		{
			glyphTypeface.CharacterToGlyphMap.TryGetValue('W', out ushort wIndex);
			double characterWidth = Math.Ceiling(glyphTypeface.AdvanceWidths[wIndex] * fontSize / dpiScale) * dpiScale;

			int displayCodePoint = codePoint;
			ushort glyphIndex;

			if (codePoint == '\t')
			{
				displayCodePoint = AppSettings.ShowWhiteSpaceCharacters ? '›' : ' ';

				glyphTypeface.CharacterToGlyphMap.TryGetValue(displayCodePoint, out glyphIndex);
				width = AppSettings.TabSize * characterWidth;
				return glyphIndex;
			}
			else if (codePoint == ' ')
			{
				displayCodePoint = AppSettings.ShowWhiteSpaceCharacters ? '·' : ' ';

				glyphTypeface.CharacterToGlyphMap.TryGetValue(displayCodePoint, out glyphIndex);
				width = Math.Ceiling(glyphTypeface.AdvanceWidths[glyphIndex] * fontSize / dpiScale) * dpiScale;
				return glyphIndex;
			}
			else if (codePoint == '\u200B')
			{
				displayCodePoint = ' ';

				glyphTypeface.CharacterToGlyphMap.TryGetValue(displayCodePoint, out glyphIndex);
				width = 0;
				return glyphIndex;
			}
			else
			{
				glyphTypeface.CharacterToGlyphMap.TryGetValue(displayCodePoint, out glyphIndex);
				width = Math.Ceiling(glyphTypeface.AdvanceWidths[glyphIndex] * fontSize / dpiScale) * dpiScale;
				return glyphIndex;
			}
		}

		public static double FontHeight(Typeface typeface, double fontSize, double dpiScale)
		{
			if (!typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
			{
				glyphTypeface = defaultGlyphTypeface;
			}
			if (!GetTextBounds(glyphTypeface, fontSize, out double topDistance, out double bottomDistance))
			{
				return MeasureText("", typeface, fontSize).Height;
			}

			Debug.Print($"-----------------------------------------");
			Debug.Print($"{glyphTypeface.FamilyNames[new CultureInfo("en-us")]}");
			Debug.Print($"baseline: {glyphTypeface.Baseline}");
			Debug.Print($"height: {glyphTypeface.Height}");
			Debug.Print($"topDistance: { topDistance}");
			Debug.Print($"bottomDistance: {bottomDistance}");
			Debug.Print($"total distance: {bottomDistance + topDistance}");
			Debug.Print($"baseline * size: {glyphTypeface.Baseline * fontSize}");

			if (glyphTypeface.Baseline * fontSize > topDistance)
			{
				topDistance = glyphTypeface.Baseline * fontSize * glyphTypeface.Height;
			}

			return (Math.Ceiling(topDistance / dpiScale) * dpiScale) + (Math.Ceiling(bottomDistance / dpiScale) * dpiScale);
		}

		private static bool GetTextBounds(GlyphTypeface glyphTypeface, double fontSize, out double topDistance, out double bottomDistance)
		{
			string testCharacters = "ÅÄÖÃÂ_[]{}|ygf";

			topDistance = double.MaxValue;
			bottomDistance = double.MinValue;

			for (int n = 0; n < testCharacters.Length; n++)
			{
				if (glyphTypeface.CharacterToGlyphMap.TryGetValue(testCharacters[n], out ushort glyphIndex))
				{
					Geometry outline = glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, 0);
					topDistance = Math.Min(topDistance, outline.Bounds.Top);
					bottomDistance = Math.Max(bottomDistance, outline.Bounds.Bottom);
				}
			}

			if (bottomDistance == double.MinValue)
			{
				return false;
			}

			topDistance = Math.Abs(topDistance);
			return true;
		}

		private static Size MeasureText(string text, Typeface typeface, double fontSize)
		{
			FormattedText formattedText = new FormattedText(
				text,
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				typeface,
				fontSize,
				Brushes.Black,
				new NumberSubstitution(),
				TextFormattingMode.Display);

			return new Size(formattedText.Width, formattedText.Height);
		}

		private static string FindFont(int codePoint)
		{
			foreach (FontFamily family in Fonts.SystemFontFamilies)
			{
				var typefaces = family.GetTypefaces();
				foreach (Typeface typeface in typefaces)
				{
					typeface.TryGetGlyphTypeface(out GlyphTypeface glyph);
					if (glyph != null && glyph.CharacterToGlyphMap.TryGetValue(codePoint, out ushort glyphIndex))
					{
						if (family.FamilyNames.TryGetValue(XmlLanguage.GetLanguage("en-us"), out string familyName))
						{
							return familyName;
						}
					}
				}
			}
			return "";
		}

		#endregion

	}
}
