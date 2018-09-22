using System;
using System.Collections.Generic;
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

		internal static GlyphRun CreateGlyphRun(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize, double dpiScale, out double runWidth)
		{
			if (text.Length == 0)
			{
				runWidth = 0;
				return null;
			}

			Typeface t = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);

			GlyphTypeface glyphTypeface;

			if (!glyphTypefaces.ContainsKey(t))
			{
				if (t.TryGetGlyphTypeface(out glyphTypeface))
				{
					glyphTypefaces.Add(t, glyphTypeface);
				}
				else
				{
					glyphTypeface = defaultGlyphTypeface;
				}
			}
			else
			{
				glyphTypeface = glyphTypefaces[t];
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

			GlyphRun run = new GlyphRun(
				glyphTypeface: glyphTypeface,
				bidiLevel: 0,
				isSideways: false,
				renderingEmSize: Math.Ceiling((fontSize) / dpiScale) * dpiScale,
				glyphIndices: glyphIndexes,
				baselineOrigin: new Point(0, Math.Ceiling((fontSize * glyphTypeface.Baseline) / dpiScale) * dpiScale),
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

		private static string FindFont(int codePoint)
		{
			ICollection<FontFamily> fontFamilies = Fonts.GetFontFamilies(@"C:\Windows\Fonts\");

			foreach (FontFamily family in fontFamilies)
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
