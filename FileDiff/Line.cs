using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace FileDiff
{
	public class Line : INotifyPropertyChanged
	{

		public Line()
		{
			Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
			Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
			Type = MatchType.NoMatch;
		}

		public override string ToString()
		{
			return $"{LineIndex}  {Text}  {MatchingLineIndex}";
		}

		public override int GetHashCode()
		{
			return Hash;
		}

		private string text;
		public string Text
		{
			get { return text; }
			set
			{
				text = value;
				Hash = value.GetHashCode();
				OnPropertyChanged(nameof(Text));
			}
		}

		public List<object> Characters
		{
			get
			{
				List<object> list = new List<object>();

				foreach (char c in text.ToCharArray())
				{
					list.Add(c);
				}
				return list;
			}
		}

		private int? lineindex;
		public int? LineIndex
		{
			get { return lineindex; }
			set { lineindex = value; OnPropertyChanged(nameof(LineIndex)); }
		}

		private Brush foreground;
		public Brush Foreground
		{
			get { return foreground; }
			set { foreground = value; OnPropertyChanged(nameof(Foreground)); }
		}

		private Brush background;
		public Brush Background
		{
			get { return background; }
			set { background = value; OnPropertyChanged(nameof(Background)); }
		}

		public MatchType Type { get; set; }

		public int? MatchingLineIndex { get; set; }

		private int Hash;

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}

	public enum MatchType
	{
		FullMatch,
		PartialMatch,
		NoMatch
	}

}
