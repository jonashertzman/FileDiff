namespace FileDiff
{
	public class DiffRange
	{

		#region Overrides 

		public override string ToString()
		{
			return $"{Start}-{End}+{Offset})";
		}

		#endregion

		#region Properies 

		public int Start { get; set; } = 0;

		public int Length { get; set; } = 1;

		public int Offset { get; set; } = 0;

		public int End
		{
			get
			{
				return Start + Length - 1;
			}
		}

		public int Top
		{
			get
			{
				if (Offset < 0)
				{
					return Start + Offset;
				}
				return Start;
			}
		}

		public int Bottom
		{
			get
			{
				if (Offset < 0)
				{
					return End;
				}
				return End + Offset;
			}
		}

		#endregion

	}
}
