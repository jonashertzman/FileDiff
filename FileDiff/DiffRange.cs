namespace FileDiff
{
	public class DiffRange
	{

		public override string ToString()
		{
			return $"{Start} - {End}  -  {Offset}";
		}

		public int Start { get; set; } = 0;

		public int Length { get; set; } = 1;

		public int End
		{
			get
			{
				return Start + Length - 1;
			}
		}

		public int Offset { get; set; } = 0;

	}
}
