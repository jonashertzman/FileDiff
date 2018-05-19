using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace FileDiff
{
	public class FileItem
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

			if (type != TextState.Filler)
			{
				FileInfo fi = new FileInfo(path);
				fileDate = fi.LastWriteTime;
				if (!isFolder)
				{
					fileSize = fi.Length;
				}
			}
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return $"{Name}  {Type.ToString()}";
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

		DateTime fileDate = DateTime.MinValue;
		public string Date
		{
			get
			{
				if (fileDate != DateTime.MinValue)
					return fileDate.ToString();

				return "";
			}
		}

		public int Level { get; set; }

		public bool IsFolder { get; set; }


		public TextState Type { get; set; }

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
				}
			}
		}

		public SolidColorBrush BackgroundBrush
		{
			get
			{
				switch (Type)
				{
					case TextState.Deleted:
						return AppSettings.DeletedBackground;
					case TextState.New:
						return AppSettings.NewBackground;
					case TextState.PartialMatch:
						return AppSettings.PartialMatchBackground;
					case TextState.Ignored:
						return AppSettings.IgnoredBackground;

					default:
						return AppSettings.FullMatchBackground;
				}
			}
		}

		public SolidColorBrush ForegroundBrush
		{
			get
			{
				switch (Type)
				{
					case TextState.Deleted:
						return AppSettings.DeletedForeground;
					case TextState.New:
						return AppSettings.NewForeground;
					case TextState.PartialMatch:
						return AppSettings.PartialMatchForeground;
					case TextState.Ignored:
						return AppSettings.IgnoredForeground;

					default:
						return AppSettings.FullMatchForeground;
				}
			}
		}

		public Visibility Visible { get { return Type == TextState.Filler ? Visibility.Hidden : Visibility.Visible; } }

		public bool ChildDiffExists
		{
			get
			{
				foreach (FileItem i in Children)
				{
					if (i.Type != TextState.FullMatch && i.Type != TextState.Ignored)
					{
						return true;
					}
				}
				return false;
			}
		}

		public string Checksum
		{
			get
			{
				StringBuilder s = new StringBuilder();

				using (MD5 md5 = MD5.Create())
				{
					try
					{
						using (FileStream stream = File.OpenRead(Path))
						{
							foreach (byte b in md5.ComputeHash(stream))
							{
								s.Append(b.ToString("x2"));
							}
						}
					}
					catch (IOException)
					{
						return "";
					}
					catch (Exception e)
					{
						MessageBox.Show(e.Message);
					}
				}

				return s.ToString();
			}
		}

		#endregion

	}
}