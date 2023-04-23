using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FileDiff;

public partial class BrowseFolderWindow : Window
{

	#region Members

	private readonly DispatcherTimer renderTimer = new DispatcherTimer();

	ScrollViewer folderTreeScrollViewer;

	#endregion

	#region Constructor

	public BrowseFolderWindow()
	{
		InitializeComponent();

		Utils.HideMinimizeAndMaximizeButtons(this);

		renderTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
		renderTimer.Tick += RenderTimer_Tick;

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
				string label = "";
				if (drive.IsReady)
				{
					if (drive.VolumeLabel == "")
					{
						label = "Local Disk ";
					}
					else
					{
						label = $"{drive.VolumeLabel} ";
					}
				}
				item.Header = $"{label}({drive.Name.TrimEnd('\\')})";
				item.Items.Add("Loading...");
				break;

			case DirectoryInfo directory:
				item.Header = directory.Name;
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
				string subpath = item.Tag is DriveInfo ? ((DriveInfo)item.Tag).Name.TrimEnd('\\') : (string)item.Header;

				if (subpath.Equals(substrings[i], StringComparison.OrdinalIgnoreCase))
				{
					item.IsExpanded = true;

					if (i == substrings.Length - 1)
					{
						item.IsSelected = true;

						// We cannot scroll to the selected item until a render of the tree view control has occurred,
						// instead we set a timer long enough so the scroll happens after the next render. 
						renderTimer.Start();
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

	private ScrollViewer GetScrollViewer(UIElement element)
	{
		if (element == null) return null;

		ScrollViewer retour = null;
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element) && retour == null; i++)
		{
			if (VisualTreeHelper.GetChild(element, i) is ScrollViewer viewer)
			{
				retour = viewer;
			}
			else
			{
				retour = GetScrollViewer(VisualTreeHelper.GetChild(element, i) as UIElement);
			}
		}
		return retour;
	}

	#endregion

	#region Events

	private void RenderTimer_Tick(object sender, EventArgs e)
	{
		renderTimer.Stop();

		folderTreeScrollViewer.ScrollToVerticalOffset(double.MaxValue);

		TreeViewItem tvi = FolderTree.SelectedItem as TreeViewItem;
		tvi.BringIntoView();
		tvi.Focus();
	}

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
		folderTreeScrollViewer = GetScrollViewer(FolderTree);

		if (!string.IsNullOrWhiteSpace(SelectedPath))
		{
			SelectedPath = Path.GetFullPath(SelectedPath);
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
