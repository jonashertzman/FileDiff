using System.ComponentModel;

namespace FileDiff
{
	public class DiffRange : INotifyPropertyChanged
	{

		public override string ToString()
		{
			return $"{Start}  {Length}";
		}

		public int Start { get; internal set; }

		public int Length { get; internal set; }

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}
}
