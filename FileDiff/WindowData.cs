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

		string leftFile;
		public string LeftFile
		{
			get { return leftFile; }
			set { leftFile = value; OnPropertyChanged(nameof(LeftFile)); }
		}

		string rightFile;
		public string RightFile
		{
			get { return rightFile; }
			set { rightFile = value; OnPropertyChanged(nameof(RightFile)); }
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
