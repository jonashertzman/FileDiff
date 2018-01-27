using System.Collections.Generic;
using System.ComponentModel;

namespace FileDiff
{
	public class FileItem
	{

		public FileItem(string path, bool folder)
		{
			Path = path;
			Folder = folder;
		}

		public override string ToString()
		{
			return $"{Path}  {Folder}";
		}

		public List<FileItem> Children { get; set; } = new List<FileItem>();

		public string Path { get; set; }

		public bool Folder { get; set; }

		private bool isSelected;
		public bool IsSelected
		{
			get { return this.isSelected; }
			set
			{
				if (value != this.isSelected)
				{
					this.isSelected = value;
					NotifyPropertyChanged("IsSelected");
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
					NotifyPropertyChanged("IsExpanded");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string propName)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

	}
}
