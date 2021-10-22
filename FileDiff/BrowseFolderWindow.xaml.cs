using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileDiff;

public partial class BrowseFolderWindow : Window
{

	#region Constructor

	public BrowseFolderWindow()
	{
		InitializeComponent();

		Utils.HideMinimizeAndMaximizeButtons(this);

		foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
		{
			FolderTree.Items.Add(CreateTreeItem(driveInfo));
		}

		FolderTree.Focus();
	}

	#endregion

	#region Properties

	public string SelectedPath { get; set; } = "";

	#endregion

	#region Methods

	private static TreeViewItem CreateTreeItem(object source)
	{
		TreeViewItem item = new TreeViewItem { Tag = source };

		switch (source)
		{
			case DriveInfo drive:
				item.Header = drive.Name.TrimEnd('\\');
				item.Items.Add("Loading...");
				break;

			case DirectoryInfo direcory:
				item.Header = direcory.Name;
				item.Items.Add("Loading...");
				break;

			case FileInfo file:
				item.Header = file.Name;
				break;
		}

		return item;
	}

	private void ExpandAndSelect(string path)
	{
		ItemCollection parent = FolderTree.Items;
		string[] substrings = path.Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

		for (int i = 0; i < substrings.Length; i++)
		{
			foreach (TreeViewItem item in parent)
			{
				if (((string)item.Header).Equals(substrings[i], StringComparison.OrdinalIgnoreCase))
				{
					item.IsSelected = true;
					item.BringIntoView();
					item.Focus();

					if (i < substrings.Length - 1)
					{
						item.IsExpanded = true;
					}
					parent = item.Items;
					break;
				}
			}
		}
	}

	private static string GetItemPath(TreeViewItem item)
	{
		switch (item.Tag)
		{
			case DriveInfo driveInfo:
				return driveInfo.ToString();

			case DirectoryInfo directoryInfo:
				return directoryInfo.FullName;

			case FileInfo fileInfo:
				return fileInfo.FullName;
		}
		return null;
	}

	#endregion

	#region Events

	public void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
	{
		Mouse.OverrideCursor = Cursors.Wait;

		TreeViewItem item = e.Source as TreeViewItem;

		if ((item.Items.Count == 1) && (item.Items[0] is string))
		{
			item.Items.Clear();

			DirectoryInfo expandedDir = item.Tag is DriveInfo ? (item.Tag as DriveInfo).RootDirectory : (item.Tag as DirectoryInfo);

			try
			{
				foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
				{
					if (Utils.DirectoryAllowed(subDir.FullName))
					{
						item.Items.Add(CreateTreeItem(subDir));
					}
				}
				foreach (FileInfo file in expandedDir.GetFiles())
				{
					item.Items.Add(CreateTreeItem(file));
				}
			}
			catch (Exception exception)
			{
				Debug.Print(exception.Message);
			}
		}

		Mouse.OverrideCursor = null;
	}

	private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		SelectedPath = GetItemPath(e.NewValue as TreeViewItem);
	}

	private void ButtonOk_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = true;
	}

	private void Window_ContentRendered(object sender, EventArgs e)
	{
		if (SelectedPath != null)
		{
			ExpandAndSelect(SelectedPath);
		}
	}

	private void ButtonDesktop_Click(object sender, RoutedEventArgs e)
	{
		ExpandAndSelect(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
	}

	private void ButtonDocuments_Click(object sender, RoutedEventArgs e)
	{
		ExpandAndSelect(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
	}

	private void ButtonDownloads_Click(object sender, RoutedEventArgs e)
	{
		ExpandAndSelect(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
	}

	#endregion

}
