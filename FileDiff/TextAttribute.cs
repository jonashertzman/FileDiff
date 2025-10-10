using System.ComponentModel;

namespace FileDiff;

public class TextAttribute : INotifyPropertyChanged
{

	public TextAttribute()
	{

	}

	public string Text
	{
		get;
		set { field = value; OnPropertyChanged(nameof(Text)); }
	}

	#region INotifyPropertyChanged

	public event PropertyChangedEventHandler PropertyChanged;

	public void OnPropertyChanged(string name)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	#endregion

}
