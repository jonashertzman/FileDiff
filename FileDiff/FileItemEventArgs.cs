namespace FileDiff;

public class FileItemEventArgs : EventArgs
{
	public FileItem SelectedItem { get; }

	public FileItemEventArgs(FileItem fileItem)
	{
		SelectedItem = fileItem;
	}
}
