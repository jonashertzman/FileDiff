namespace FileDiff
{

	public enum TextState
	{
		FullMatch,
		PartialMatch,
		Deleted,
		New,
		Filler
	}

	public enum CompareMode
	{
		File,
		Folder,
		TwoLevel
	}

}
