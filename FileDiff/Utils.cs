using System;
using System.IO;
using System.Windows;

namespace FileDiff
{
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
			window.SourceInitialized += (s, e) =>
			{
				IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
				int style = WinApi.GetWindowLong(hwnd, WinApi.GWL_STYLE);

				_ = WinApi.SetWindowLong(hwnd, WinApi.GWL_STYLE, style & ~WinApi.WS_MAXIMIZEBOX & ~WinApi.WS_MINIMIZEBOX);
			};
		}

	}
}
