namespace FileDiff;

public enum TextState
{
	FullMatch,
	PartialMatch,
	Deleted,
	New,
	Filler,
	Ignored,
	MovedFrom,
	MovedTo,
	MovedFiller,
}

public enum CompareMode
{
	File,
	Folder,
}

public enum NewlineMode
{
	Windows,
	Unix,
	Mac,
}
