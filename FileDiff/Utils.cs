using System.IO;
using System.Windows;
using System.Windows.Media;

namespace FileDiff;

static class Utils
{

	public static bool DirectoryAllowed(string path)
	{
		try
		{
			Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
		}
		catch
		{
			return false;
		}
		return true;
	}

	public static void HideMinimizeAndMaximizeButtons(Window window)
	{
		window.SourceInitialized += (sender, eventArgs) =>
		{
			IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
			int style = WinApi.GetWindowLong(hwnd, WinApi.GWL_STYLE);

			_ = WinApi.SetWindowLong(hwnd, WinApi.GWL_STYLE, style & ~WinApi.WS_MAXIMIZEBOX & ~WinApi.WS_MINIMIZEBOX);
		};
	}

	public static string TimeSpanToShortString(TimeSpan timeSpan)
	{
		if (timeSpan.TotalHours >= 1)
		{
			return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
		}
		if (timeSpan.Minutes > 0)
		{
			return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
		}
		return $"{timeSpan.Seconds}.{timeSpan.Milliseconds.ToString().PadLeft(3, '0')}s";
	}

	public static SolidColorBrush ToBrush(this string colorString)
	{
		try
		{
			return new BrushConverter().ConvertFrom(colorString) as SolidColorBrush;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static string FixRootPath(string path)
	{
		// Directory.GetDirectories, Directory.GetFiles and Path.Combine does not work on root paths without trailing backslashes.
		if (path.EndsWith(":"))
		{
			return path += "\\";
		}
		return path;
	}

	#region Extention Methods

	public static bool In<T>(this T item, params T[] list)
	{
		return list.Contains(item);
	}

	public static bool NotIn<T>(this T item, params T[] list)
	{
		return !list.Contains(item);
	}

	#endregion

}
