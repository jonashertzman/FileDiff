using System.Data;

namespace FileDiff
{
	public class DiffRange
	{

		#region Overrides 

		public override string ToString()
		{
			return $"{Start} - {End}  -  {Offset}";
		}

		#endregion

		#region Properies 

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

		#endregion

		#region Methods

		public bool Includes(int lineIndex)
		{
			return (lineIndex >= Start && lineIndex <= End) || (lineIndex >= Start + Offset && lineIndex <= End + Offset);
		}

		public bool MovedPast(int lineIndex)
		{
			if (Offset > 0)
			{
				return lineIndex > Start && lineIndex < End + Offset;
			}
			else if (Offset < 0)
			{
				return lineIndex < End && lineIndex > Start + Offset;
			}
			return false;
		}

		#endregion

	}
}
