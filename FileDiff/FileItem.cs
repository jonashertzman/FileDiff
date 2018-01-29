using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace FileDiff
{
	public class FileItem : INotifyPropertyChanged
	{

		#region Constructor

		public FileItem()
		{
		}

		public FileItem(string name, bool isFolder)
		{
			Name = name;
			IsFolder = isFolder;
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return $"{Name}  {IsFolder}";
		}

		#endregion

		#region Properties

		public ObservableCollection<FileItem> Children { get; set; } = new ObservableCollection<FileItem>();

		public FileItem CorrespondingItem { get; set; }

		public string Name { get; set; }

		public bool IsFolder { get; set; }

		private bool isSelected;
		public bool IsSelected
		{
			get { return this.isSelected; }
			set
			{
				if (value != this.isSelected)
				{
					this.isSelected = value;
					CorrespondingItem.IsSelected = value;
					NotifyPropertyChanged(nameof(IsSelected));
				}
			}
		}

		private bool isExpanded;
		public bool IsExpanded
		{
			get { return this.isExpanded; }
			set
			{
				if (value != this.isExpanded)
				{
					Debug.Print($"{this.Name} expanded = {value}");
					this.isExpanded = value;
					CorrespondingItem.IsExpanded = value;
					NotifyPropertyChanged(nameof(IsExpanded));
				}
			}
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string propName)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		#endregion

	}
}
