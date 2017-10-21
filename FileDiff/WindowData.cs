using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FileDiff
{
	public class WindowData : INotifyPropertyChanged
	{

		ObservableCollection<Line> leftSide = new ObservableCollection<Line>();
		public ObservableCollection<Line> LeftSide
		{
			get { return leftSide; }
			set { leftSide = value; OnPropertyChanged(nameof(LeftSide)); }
		}

		ObservableCollection<Line> rightSide = new ObservableCollection<Line>();
		public ObservableCollection<Line> RightSide
		{
			get { return rightSide; }
			set { rightSide = value; OnPropertyChanged(nameof(RightSide)); }
		}

		string leftPath;
		public string LeftPath
		{
			get { return leftPath; }
			set { leftPath = value; OnPropertyChanged(nameof(LeftPath)); }
		}

		string rightPath;
		public string RightPath
		{
			get { return rightPath; }
			set { rightPath = value; OnPropertyChanged(nameof(RightPath)); }
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
