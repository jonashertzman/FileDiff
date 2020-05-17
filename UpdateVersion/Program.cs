using System;
using System.IO;

namespace UpdateVersion
{
	class Program
	{
		static void Main(string[] args)
		{
			DateTime buildDate = DateTime.Now;
			string buildNumber = $"{buildDate:yy}{buildDate.DayOfYear:D3}";

			Console.WriteLine($"Updating version to {buildNumber}");

			File.WriteAllText(@"..\..\..\docs\download\version.txt", buildNumber);
		}
	}
}
