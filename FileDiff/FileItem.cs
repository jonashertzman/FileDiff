using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace FileDiff
{
	public class FileItem : INotifyPropertyChanged
	{

		#region Constructor

		public FileItem()
		{
		}

		public FileItem(string name, bool isFolder, TextState type)
		{
			Name = name;
			IsFolder = isFolder;
			Type = type;
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


		private TextState type;
		public TextState Type
		{
			get { return type; }
			set
			{
				type = value;
				OnPropertyChanged(nameof(Type));
			}
		}
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
					OnPropertyChanged(nameof(IsSelected));
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
					this.isExpanded = value;
					CorrespondingItem.IsExpanded = value;
					OnPropertyChanged(nameof(IsExpanded));
				}
			}
		}

		public Visibility Visible { get { return type == TextState.Filler ? Visibility.Hidden : Visibility.Visible; } }

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propName)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		#endregion

	}
}
