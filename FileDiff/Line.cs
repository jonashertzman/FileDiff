using System.ComponentModel;

namespace FileDiff
{
	public class Line : INotifyPropertyChanged
	{

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

		private int lineNumber;
		public int LineNumber
		{
			get { return lineNumber; }
			set { lineNumber = value; OnPropertyChanged(nameof(LineNumber)); }
		}

		public int Hash { get; private set; }

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}
}
