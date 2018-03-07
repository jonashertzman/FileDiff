using System.ComponentModel;
using System.Windows.Media;

namespace FileDiff
{
	public class TextSegment : INotifyPropertyChanged
	{

		public TextSegment()
		{

		}

		public TextSegment(string text, TextState textState)
		{
			this.Text = text;
			this.type = textState;
		}

		public override string ToString()
		{
			return Text;
		}

		private TextState type;
		public TextState Type
		{
			get { return type; }
			set { type = value; OnPropertyChanged(nameof(Type)); }
		}

		private string text;
		public string Text
		{
			get { return text; }
			set { text = value; OnPropertyChanged(nameof(Text)); }
		}

		public GlyphRun RenderedText { get; set; }

		public SolidColorBrush BackgroundBrush
		{
			get
			{
				switch (type)
				{
					case TextState.Deleted:
						return AppSettings.DeletedBackground;
					case TextState.New:
						return AppSettings.NewBackground;
					case TextState.PartialMatch:
						return AppSettings.PartialMatchBackground;

					default:
						return AppSettings.FullMatchBackground;
				}
			}
		}

		public SolidColorBrush ForegroundBrush
		{
			get
			{
				switch (type)
				{
					case TextState.Deleted:
						return AppSettings.DeletedForeground;
					case TextState.New:
						return AppSettings.NewForeground;
					case TextState.PartialMatch:
						return AppSettings.PartialMatchForeground;

					default:
						return AppSettings.FullMatchForeground;
				}
			}
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}
}
