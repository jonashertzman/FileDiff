using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace FileDiff
{
	public class Line : INotifyPropertyChanged
	{
		#region Members

		private int Hash;

		#endregion

		#region Constructor

		public Line()
		{
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return $"{LineIndex}  {Text}  {MatchingLineIndex}";
		}

		public override int GetHashCode()
		{
			return Hash;
		}

		#endregion

		#region Properties

		private string text;
		public string Text
		{
			get { return text; }
			set
			{
				text = value;
				Hash = value.GetHashCode();
				TextSegments.Clear();
				TextSegments.Add(new TextSegment() { Text = value });
				OnPropertyChanged(nameof(Text));
			}
		}

		private ObservableCollection<TextSegment> textSegments = new ObservableCollection<TextSegment>();
		public ObservableCollection<TextSegment> TextSegments
		{
			get { return textSegments; }
			set { textSegments = value; OnPropertyChanged(nameof(TextSegments)); }
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
			set
			{
				foreground = value;
				foreach (TextSegment t in TextSegments)
				{
					t.Foreground = value;
				}
				OnPropertyChanged(nameof(Foreground));
			}
		}

		private Brush background;
		public Brush Background
		{
			get { return background; }
			set
			{
				background = value;
				foreach (TextSegment t in TextSegments)
				{
					t.Background = value;
				}
				OnPropertyChanged(nameof(Background));
			}
		}

		private MatchType type;
		public MatchType Type
		{
			get { return type; }
			set
			{
				type = value;
				switch (value)
				{
					case MatchType.FullMatch:
						Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
						Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
						break;
					case MatchType.PartialMatch:
						Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
						Background = new SolidColorBrush(Color.FromRgb(220, 220, 255));
						break;
				}
			}
		}

		public int? MatchingLineIndex { get; set; }

		#endregion

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
