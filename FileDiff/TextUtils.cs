using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace FileDiff
{
	static class TextUtils
	{

		static Dictionary<Typeface, GlyphTypeface> glyphTypefaces = new Dictionary<Typeface, GlyphTypeface>();
		static readonly GlyphTypeface defaultGlyphTypeface;

		static TextUtils()
		{
			Typeface defaultTypface = new Typeface("Courier New");

			defaultTypface.TryGetGlyphTypeface(out defaultGlyphTypeface);
			glyphTypefaces.Add(defaultTypface, defaultGlyphTypeface);
		}

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

			glyphTypeface.CharacterToGlyphMap.TryGetValue(ReplaceGlyph('W'), out ushort wIndex);
			double characterWidth = Math.Ceiling(glyphTypeface.AdvanceWidths[wIndex] * fontSize / dpiScale) * dpiScale; ;

			ushort[] glyphIndexes = new ushort[text.Length];
			double[] advanceWidths = new double[text.Length];

			double totalWidth = 0;
			for (int n = 0; n < text.Length; n++)
			{
				glyphTypeface.CharacterToGlyphMap.TryGetValue(ReplaceGlyph(text[n]), out ushort glyphIndex);
				glyphIndexes[n] = glyphIndex;
				double width = text[n] == '\t' ? AppSettings.TabSize * characterWidth : Math.Ceiling(glyphTypeface.AdvanceWidths[glyphIndex] * fontSize / dpiScale) * dpiScale;
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

		private static char ReplaceGlyph(char c)
		{
			if (AppSettings.ShowWhiteSpaceCharacters)
			{
				if (c == '\t')
				{
					return '›';
				}
				else if (c == ' ')
				{
					return '·';
				}
			}
			else
			{
				if (c == '\t') // Why does the tab glyph render as a rectangle?
				{
					return ' ';
				}
			}

			return c;
		}

	}
}
