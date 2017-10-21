using System.ComponentModel;
using System.Windows.Media;

namespace FileDiff
{
	public class TextSegment : INotifyPropertyChanged
	{

		public TextSegment()
		{

		}

		public TextSegment(string text, Brush foreground, Brush background)
		{
			this.Text = text;
			this.Foreground = foreground;
			this.Background = background;
		}

		public override string ToString()
		{
			return Text;
		}

		private string text;
		public string Text
		{
			get { return text; }
			set { text = value; OnPropertyChanged(nameof(Text)); }
		}

		private Brush foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
		public Brush Foreground
		{
			get { return foreground; }
			set { foreground = value; OnPropertyChanged(nameof(Foreground)); }
		}

		private Brush background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
		public Brush Background
		{
			get { return background; }
			set { background = value; OnPropertyChanged(nameof(Background)); }
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
