using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace FileDiff;

public class FileItem
{

	#region Constructor

	public FileItem()
	{

	}

	public FileItem(string path, int level, WIN32_FIND_DATA findData)
	{
		Name = findData.cFileName;
		Path = path;
		Level = level;

		IsFolder = (findData.dwFileAttributes & FileAttributes.Directory) != 0;

		if (!IsFolder)
		{
			Size = (long)Combine(findData.nFileSizeHigh, findData.nFileSizeLow);
		}

		Date = DateTime.FromFileTime((long)Combine(findData.ftLastWriteTime.dwHighDateTime, findData.ftLastWriteTime.dwLowDateTime));
	}

	#endregion

	#region Overrides

	public override string ToString()
	{
		return $"{Name}  {Type}";
	}

	#endregion

	#region Properties

	public FileItem Parent { get; set; }

	public ObservableCollection<FileItem> Children { get; set; } = [];

	public FileItem CorrespondingItem { get; set; }

	public string Path { get; set; } = "";

	public string Name { get; set; } = "";

	public string Key
	{
		get
		{
			if (IsFolder)
				return "*" + Name;

			return Name;
		}
	}

	public DateTime Date { get; set; }

	public long Size { get; set; }

	public int Level { get; set; }

	public bool IsFolder { get; set; }

	public TextState Type { get; set; }

	public bool IsExpanded
	{
		get;
		set
		{
			if (value != field)
			{
				field = value;
				CorrespondingItem.IsExpanded = value;
			}
		}
	}

	public Brush BackgroundBrush
	{
		get
		{
			return AppSettings.GetFolderBackground(Type);
		}
	}

	public Brush ForegroundBrush
	{
		get
		{
			return AppSettings.GetFolderForeground(Type);
		}
	}

	public Visibility Visible { get { return Type == TextState.Filler ? Visibility.Hidden : Visibility.Visible; } }

	public bool ChildDiffExists
	{
		get
		{
			foreach (FileItem i in Children)
			{
				if (i.Type != TextState.FullMatch && !(i.Type == TextState.Ignored || i.CorrespondingItem.Type == TextState.Ignored))
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
			if (IsFolder || Type == TextState.Filler)
				return "";

			StringBuilder s = new();

			using (MD5 md5 = MD5.Create())
			{
				try
				{
					using FileStream stream = File.OpenRead(Path);
					foreach (byte b in md5.ComputeHash(stream))
					{
						s.Append(b.ToString("x2"));
					}
				}
				catch (IOException)
				{
					return "";
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			return s.ToString();
		}
	}

	#endregion

	#region Methods

	private ulong Combine(uint highValue, uint lowValue)
	{
		return (ulong)highValue << 32 | lowValue;
	}

	#endregion

}
