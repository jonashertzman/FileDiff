using System.ComponentModel;
using System.Windows.Media;

namespace FileDiff
{
	public class TextSegment
	{

		public TextSegment()
		{

		}

		public TextSegment(string text, TextState textState)
		{
			this.Text = text;
			this.Type = textState;
		}

		public override string ToString()
		{
			return Text;
		}

		public TextState Type { get; set; }

		public string Text { get; set; }

		public GlyphRun RenderedText { get; set; }

		public SolidColorBrush BackgroundBrush
		{
			get
			{
				switch (Type)
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
				switch (Type)
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

	}
}
