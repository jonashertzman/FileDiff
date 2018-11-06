using System.Windows.Media;

namespace FileDiff
{
	class FontData
	{

		public FontData(GlyphTypeface glyphTypeface, double topDistance, double bottomDistance)
		{
			GlyphTypeface = glyphTypeface;
			TopDistance = topDistance;
			BottomDistance = bottomDistance;
		}

		public GlyphTypeface GlyphTypeface { get; internal set; }
		public double TopDistance { get; internal set; }
		public double BottomDistance { get; internal set; }
	}
}
