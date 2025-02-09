using System.IO;
using System.Windows;

namespace FileDiff;

internal static class Log
{

	#region Properties

	public static Window OwnerWindow { get; set; }

	#endregion

	#region Methods

	public static void DisplayException(Exception exception, string source)
	{
		LogException(exception, source);

		ExceptionWindow exceptionWindow = new()
		{
			Owner = OwnerWindow,
			ExceptionType = exception.GetType().Name,
			ExceptionMessage = exception.Message,
			Source = source,
			StackTrace = exception.StackTrace
		};

		exceptionWindow.ShowDialog();
	}

	public static void LogException(Exception exception, string source)
	{
		string errorText = $"{DateTime.UtcNow} - {source}\nException:  {exception.GetType().Name}\nMessage:    {exception.Message}\n{exception.StackTrace}\n\n";

		Directory.CreateDirectory(Path.GetDirectoryName(AppSettings.LogPath));
		File.AppendAllText(AppSettings.LogPath, errorText);
	}

	#endregion

}
