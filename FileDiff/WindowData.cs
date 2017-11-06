using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

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

		public bool IgnoreWhiteSpace
		{
			get { return AppSettings.Settings.IgnoreWhiteSpace; }
			set { AppSettings.Settings.IgnoreWhiteSpace = value; OnPropertyChanged(nameof(IgnoreWhiteSpace)); }
		}

		public bool ShowLineChanges
		{
			get { return AppSettings.Settings.ShowLineChanges; }
			set { AppSettings.Settings.ShowLineChanges = value; OnPropertyChanged(nameof(ShowLineChanges)); }
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
