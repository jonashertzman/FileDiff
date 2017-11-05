using System.Windows.Media;

namespace FileDiff
{
	public class SettingsData
	{

		public static bool IgnoreWhiteSpace { get; set; } = true;

		public static bool ShowLineChanges { get; set; } = true;

		// Minimum character length of substrings used when looking for similarities between two lines.
		public static int CharacterMatchThreshold { get; set; } = 4;

		// The percentage of matching characters two lines must have in common to be considered partially matched lines.
		public static float LineSimilarityThreshold { get; set; } = 0.4f;

		// Single lines shorter than this will not be considered for full line matches.
		public static int FullMatchLineLengthThreshold { get; internal set; } = 1;

		public static Brush DeletedForeground { get; set; } = new SolidColorBrush(Color.FromRgb(200, 0, 0));
		public static Brush DeletedBackground { get; set; } = new SolidColorBrush(Color.FromRgb(255, 220, 220));

		public static Brush AddedForeground { get; set; } = new SolidColorBrush(Color.FromRgb(0, 120, 0));
		public static Brush AddedBackground { get; set; } = new SolidColorBrush(Color.FromRgb(220, 255, 220));

		public static Brush ModifiedForeground { get; set; } = new SolidColorBrush(Color.FromRgb(0, 0, 0));
		public static Brush ModifiedBackground { get; set; } = new SolidColorBrush(Color.FromRgb(220, 220, 255));

	}
}
