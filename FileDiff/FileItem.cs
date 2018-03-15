using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace FileDiff
{
	public class FileItem : INotifyPropertyChanged
	{

		#region Constructor

		public FileItem()
		{
		}

		public FileItem(string name, bool isFolder, TextState type, string path, int level)
		{
			Name = name;
			IsFolder = isFolder;
			Type = type;
			Path = path;
			Level = level;

			if (!isFolder && type != TextState.Filler)
			{
				fileSize = new FileInfo(path).Length;

			}
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

		public string Path { get; set; }

		public string Name { get; set; }

		private long fileSize = -1;

		public string Size
		{
			get
			{
				if (fileSize != -1)
					return fileSize.ToString();

				return "";
			}
		}

		public int Level { get; set; }

		public bool IsFolder { get; set; }

		public double NameWidth
		{
			get { return Math.Max(0, AppSettings.NameColumnWidth - (Level * 19)); } // TODO: Get the indentation length programmatically.
		}

		private double sizeWidth;
		public double SizeWidth
		{
			get { return sizeWidth; }
			set { sizeWidth = value; OnPropertyChanged(nameof(SizeWidth)); }
		}

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

		#region Methods 

		internal void UpdateWidth()
		{
			OnPropertyChanged(nameof(NameWidth));

			foreach (FileItem f in Children)
			{
				f.UpdateWidth();
			}
		}

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