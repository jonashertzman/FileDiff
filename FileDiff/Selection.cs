namespace FileDiff
{
	class Selection
	{

		#region Members

		public int StartLine;
		public int StartCharacter;
		public int EndLine;
		public int EndCharacter;

		#endregion

		#region Properties

		public int TopLine
		{
			get
			{
				return StartLine <= EndLine ? StartLine : EndLine;
			}
		}

		public int BottomLine
		{
			get
			{
				return StartLine <= EndLine ? EndLine : StartLine;
			}
		}

		public int TopCharacter
		{
			get
			{
				if (StartLine == EndLine)
				{
					return StartCharacter <= EndCharacter ? StartCharacter : EndCharacter;
				}
				return StartLine < EndLine ? StartCharacter : EndCharacter;
			}
		}

		public int BottomCharacter
		{
			get
			{
				if (StartLine == EndLine)
				{
					return StartCharacter <= EndCharacter ? EndCharacter : StartCharacter;
				}
				return StartLine < EndLine ? EndCharacter : StartCharacter;
			}
		}

		#endregion

	}
}
