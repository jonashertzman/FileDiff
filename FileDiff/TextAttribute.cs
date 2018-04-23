using System.ComponentModel;

namespace FileDiff
{
	public class TextAttribute : INotifyPropertyChanged
	{

		public TextAttribute()
		{

		}

		string text;
		public string Text
		{
			get { return text; }
			set { text = value; OnPropertyChanged(nameof(Text)); }
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
