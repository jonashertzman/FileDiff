using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FileDiff
{
	public partial class BrowseFolderWindow : Window
	{

		public BrowseFolderWindow()
		{
			InitializeComponent();

			foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
			{
				FolderTree.Items.Add(CreateTreeItem(driveInfo));
			}
		}

		public void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
		{
			TreeViewItem item = e.Source as TreeViewItem;

			if ((item.Items.Count == 1) && (item.Items[0] is string))
			{
				item.Items.Clear();

				DirectoryInfo expandedDir = item.Tag is DriveInfo ? (item.Tag as DriveInfo).RootDirectory : (item.Tag as DirectoryInfo);

				try
				{
					foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
					{
						item.Items.Add(CreateTreeItem(subDir));
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
		}

		private TreeViewItem CreateTreeItem(object source)
		{
			TreeViewItem item = new TreeViewItem();
			item.Header = source.ToString();
			item.Tag = source;
			if (source is DriveInfo || source is DirectoryInfo)
			{
				item.Items.Add("Loading...");
			}
			return item;
		}

	}
}
