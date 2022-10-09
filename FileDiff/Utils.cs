﻿using System.IO;
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

	public static SolidColorBrush ToBrush(this string HexColorString)
	{
		return new BrushConverter().ConvertFrom(HexColorString) as SolidColorBrush;
	}

}
