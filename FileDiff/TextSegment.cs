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

		public GlyphRun Run { get; set; }

		public SolidColorBrush BackgroundBrush
		{
			get
			{
				switch (type)
				{
					case TextState.Deleted:
						return AppSettings.deletedBackgroundBrush;
					case TextState.New:
						return AppSettings.newBackgrounBrush;
					case TextState.PartialMatch:
						return AppSettings.partialMatchBackgroundBrush;

					default:
						return AppSettings.fullMatchBackgroundBrush;
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
						return AppSettings.deletedForegroundBrush;
					case TextState.New:
						return AppSettings.newForegroundBrush;
					case TextState.PartialMatch:
						return AppSettings.partialMatchForegroundBrush;

					default:
						return AppSettings.fullMatchForegroundBrush;
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
