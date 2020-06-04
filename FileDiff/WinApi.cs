using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FileDiff
{

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

	public class WinApi
	{
		public const int MAX_PATH = 260;
		public const int MAX_ALTERNATE = 14;
		public const long MAXDWORD = 0xffffffff;
		public const int FIND_FIRST_EX_LARGE_FETCH = 2;
		public const int GWL_STYLE = -16;
		public const int WS_MAXIMIZEBOX = 0x10000;
		public const int WS_MINIMIZEBOX = 0x20000;

		[DllImport("kernel32", CharSet = CharSet.Auto)]
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
	}

}
