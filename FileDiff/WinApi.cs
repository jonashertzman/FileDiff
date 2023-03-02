﻿using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace FileDiff;

public enum FINDEX_INFO_LEVELS
{
	FindExInfoStandard = 0,
	FindExInfoBasic = 1
}

public enum FINDEX_SEARCH_OPS
{
	FindExSearchNameMatch = 0,
	FindExSearchLimitToDirectories = 1,
	FindExSearchLimitToDevices = 2
}

[StructLayout(LayoutKind.Sequential)]
public struct FILETIME
{
	public uint dwLowDateTime;
	public uint dwHighDateTime;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct WIN32_FIND_DATA
{
	public FileAttributes dwFileAttributes;
	public FILETIME ftCreationTime;
	public FILETIME ftLastAccessTime;
	public FILETIME ftLastWriteTime;
	public uint nFileSizeHigh;
	public uint nFileSizeLow;
	public uint dwReserved0;
	public uint dwReserved1;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = WinApi.MAX_PATH)]
	public string cFileName;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = WinApi.MAX_ALTERNATE)]
	public string cAlternate;
}

internal class WinApi
{
	public const int MAX_PATH = 260;
	public const int MAX_ALTERNATE = 14;
	public const long MAXDWORD = 0xffffffff;
	public const int FIND_FIRST_EX_LARGE_FETCH = 2;
	public const int GWL_STYLE = -16;
	public const int WS_MAXIMIZEBOX = 0x10000;
	public const int WS_MINIMIZEBOX = 0x20000;
	private const uint CF_UNICODETEXT = 13;


	[DllImport("kernel32", CharSet = CharSet.Unicode)]
	public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern IntPtr FindFirstFileEx(string lpFileName, FINDEX_INFO_LEVELS fInfoLevelId, out WIN32_FIND_DATA lpFindFileData, FINDEX_SEARCH_OPS fSearchOp, IntPtr lpSearchFilter, int dwAdditionalFlags);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

	[DllImport("kernel32.dll")]
	public static extern bool FindClose(IntPtr hFindFile);

	[DllImport("user32.dll")]
	extern public static int GetWindowLong(IntPtr hwnd, int index);

	[DllImport("user32.dll")]
	extern public static int SetWindowLong(IntPtr hwnd, int index, int value);

	[DllImport("user32.dll")]
	private static extern bool OpenClipboard(IntPtr hWndNewOwner);

	[DllImport("user32.dll")]
	private static extern bool CloseClipboard();

	[DllImport("user32.dll")]
	private static extern bool SetClipboardData(uint uFormat, IntPtr data);

	[DllImport("user32.dll")]
	private static extern IntPtr GetClipboardData(uint uFormat);

	[DllImport("User32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool IsClipboardFormatAvailable(uint format);

	[DllImport("Kernel32.dll", SetLastError = true)]
	private static extern IntPtr GlobalLock(IntPtr hMem);

	[DllImport("Kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GlobalUnlock(IntPtr hMem);

	[DllImport("Kernel32.dll", SetLastError = true)]
	private static extern int GlobalSize(IntPtr hMem);

	[DllImport("user32.dll")]
	static extern IntPtr GetOpenClipboardWindow();

	[DllImport("user32.dll")]
	static extern int GetWindowText(int hwnd, StringBuilder text, int count);


	public static bool CopyTextToClipboard(string text)
	{
		if (!OpenClipboard(IntPtr.Zero))
		{
			MessageBox.Show($"OpenClipboard failed {getOpenClipboardWindowText()}");
			return false;
		}

		var global = Marshal.StringToHGlobalUni(text);

		if (!SetClipboardData(CF_UNICODETEXT, global))
		{
			MessageBox.Show($"SetClipboardData failed {getOpenClipboardWindowText()}");
			return false;
		}

		if (!CloseClipboard())
		{
			MessageBox.Show($"CloseClipboard failed {getOpenClipboardWindowText()}");
			return false;
		}

		return true;
	}

	public static string GetTextFromClipboard()
	{
		if (!IsClipboardFormatAvailable(CF_UNICODETEXT))
		{
			MessageBox.Show("clipboard not available");
			return null;
		}

		try
		{
			if (!OpenClipboard(IntPtr.Zero))
			{
				MessageBox.Show("OpenClipboard failed");
				return null;
			}

			IntPtr handle = GetClipboardData(CF_UNICODETEXT);
			if (handle == IntPtr.Zero)
				return null;

			IntPtr pointer = IntPtr.Zero;

			try
			{
				pointer = GlobalLock(handle);
				if (pointer == IntPtr.Zero)
					return null;

				int size = GlobalSize(handle);
				byte[] buff = new byte[size];

				Marshal.Copy(pointer, buff, 0, size);

				return Encoding.Unicode.GetString(buff).TrimEnd('\0');
			}
			finally
			{
				if (pointer != IntPtr.Zero)
					GlobalUnlock(handle);
			}
		}
		finally
		{
			CloseClipboard();
		}
	}

	private static string getOpenClipboardWindowText()
	{
		IntPtr hwnd = GetOpenClipboardWindow();
		StringBuilder sb = new StringBuilder(501);
		GetWindowText(hwnd.ToInt32(), sb, 500);
		return sb.ToString();
	}

}
