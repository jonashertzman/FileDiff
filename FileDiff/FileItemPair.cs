namespace FileDiff
{
	public class FileItemPair
	{

		#region Constructor

		public FileItemPair(FileItem leftItem, FileItem rightItem)
		{
			LeftItem = leftItem;
			RightItem = rightItem;
		}

		#endregion

		#region Properties

		public FileItem LeftItem { get; set; }

		public FileItem RightItem { get; set; }

		#endregion

	}
}
