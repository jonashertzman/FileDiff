using System.ComponentModel;

namespace FileDiff
{
	public class DiffRange
	{

		public override string ToString()
		{
			return $"{Start}  {Length}";
		}

		public int Start { get; internal set; }

		public int Length { get; internal set; }

	}
}
