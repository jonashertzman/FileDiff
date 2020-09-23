namespace FileDiff
{

	public enum TextState
	{
		FullMatch,
		PartialMatch,
		Deleted,
		New,
		Filler,
		Ignored,
		MovedFrom1,
		MovedFrom2,
		MovedTo,
		MovedToFiller,
		MovedFromFiller
	}

	public enum CompareMode
	{
		File,
		Folder
	}

	public enum NewlineMode
	{
		Windows,
		Unix,
		Mac
	}

}
