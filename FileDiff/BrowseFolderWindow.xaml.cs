using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FileDiff
{
	public partial class BrowseFolderWindow : Window
	{

		#region Constructor

		public BrowseFolderWindow()
		{
			InitializeComponent();

			foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
			{
				FolderTree.Items.Add(CreateTreeItem(driveInfo));
			}
		}

		#endregion

		#region Properties

		public string SelectedPath { get; set; }

		#endregion

		#region Methods

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

		#endregion

		#region Events

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

		private string GetItemPath(TreeViewItem item)
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

		private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			SelectedPath = GetItemPath(e.NewValue as TreeViewItem);
		}

		private void ButtonOk_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			ItemCollection parent = FolderTree.Items;

			foreach (string s in SelectedPath.Split('\\'))
			{
				foreach (TreeViewItem item in parent)
				{
					if (item.Header.Equals(s + (parent == FolderTree.Items ? "\\" : "")))
					{
						item.IsSelected = true;
						item.IsExpanded = true;
						item.BringIntoView();
						parent = item.Items;
						break;
					}
				}
			}
		}

		#endregion

	}
}
