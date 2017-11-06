using System.ComponentModel;
using System.Windows.Media;

namespace FileDiff
{

	public class SettingsData : INotifyPropertyChanged
	{

		public bool IgnoreWhiteSpace { get; set; } = true;

		public bool ShowLineChanges { get; set; } = true;

		// Minimum character length of substrings used when looking for similarities between two lines.
		public int CharacterMatchThreshold { get; set; } = 4;

		// The percentage of matching characters two lines must have in common to be considered partially matched lines.
		public float LineSimilarityThreshold { get; set; } = 0.4f;

		// Single lines shorter than this will not be considered for full line matches.
		public int FullMatchLineLengthThreshold { get; internal set; } = 1;

		public Color FullMatchForeground { get; set; } = Colors.Black;
		public Color FullMatchBackground { get; set; } = Colors.White;

		public Color PartialMatchForeground { get; set; } = Colors.Black;
		public Color PartialMatchBackground { get; set; } = Color.FromRgb(220, 220, 255);

		public Color DeletedForeground { get; set; } = Color.FromRgb(200, 0, 0);
		public Color DeletedBackground { get; set; } = Color.FromRgb(255, 220, 220);

		public Color NewForeground { get; set; } = Color.FromRgb(0, 120, 0);
		public Color NewBackground { get; set; } = Color.FromRgb(220, 255, 220);

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}
}
