using System.IO;
using System.Text;

namespace FileDiff;

public class FileEncoding
{

	#region Constructor

	public FileEncoding(string path)
	{
		byte[] bytes = File.ReadAllBytes(path);

		// Check if the file has a BOM
		if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
		{
			Type = Encoding.UTF8;
			HasBom = true;
		}
		else if (bytes[0] == 0xFF && bytes[1] == 0xFE)
		{
			Type = Encoding.Unicode;
			HasBom = true;
		}
		else if (bytes[0] == 0xFE && bytes[1] == 0xFF)
		{
			Type = Encoding.BigEndianUnicode;
			HasBom = true;
		}
		else if (bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
		{
			Type = new UTF32Encoding(true, true);
			HasBom = true;
		}

		// No bom found, check if data passes as a bom-less UTF-8 file
		else if (CheckValidUtf8(bytes))
		{
			Type = Encoding.UTF8;
		}

		// Check if the file has null bytes, if so we assume it is a bom-less UTF-16 file
		else
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] == 0)
				{
					Type = Encoding.Unicode;
					break;
				}
			}
		}
	}

	#endregion

	#region Overrides

	public override string ToString()
	{
		string name = "";

		if (Type == Encoding.UTF8)
		{
			name = "UTF-8";
		}
		else if (Type == Encoding.Unicode)
		{
			name = "UTF-16 LE";
		}
		else if (Type == Encoding.BigEndianUnicode)
		{
			name = "UTF-16 BE";
		}
		else if (Type == Encoding.UTF32)
		{
			name = "UTF-32";
		}
		else if (Type == Encoding.Default)
		{
			name = Type.WebName;
		}

		if (HasBom)
		{
			name += " BOM";
		}

		name += $" ({Newline})";

		return name;
	}

	#endregion

	#region Properties

	public Encoding Type { get; private set; } = Encoding.Default;

	public bool HasBom { get; private set; }

	public NewlineMode Newline { get; set; } = NewlineMode.Windows;

	public Encoding GetEncoding
	{
		get
		{
			if (Type == Encoding.UTF8)
			{
				return new UTF8Encoding(HasBom);
			}
			else if (Type == Encoding.Unicode)
			{
				return new UnicodeEncoding(false, HasBom);
			}
			else if (Type == Encoding.BigEndianUnicode)
			{
				return new UnicodeEncoding(true, HasBom);
			}
			else if (Type == Encoding.UTF32)
			{
				return new UTF32Encoding(false, HasBom);
			}

			return Encoding.Default;
		}
	}

	public string NewlineString
	{
		get
		{
			if (Newline == NewlineMode.Mac)
			{
				return "\r";
			}
			else if (Newline == NewlineMode.Unix)
			{
				return "\n";
			}
			return "\r\n";
		}
	}

	public static NewlineMode GetNewlineMode(string newlineString)
	{
		return newlineString switch
		{
			"\n" => NewlineMode.Unix,
			"\r" => NewlineMode.Mac,
			_ => NewlineMode.Windows,
		};
	}

	#endregion

	#region Methods 

	private static bool CheckValidUtf8(byte[] bytes)
	{
		int i = 0;
		while (i < bytes.Length - 4)
		{
			// 1 byte character
			if (bytes[i] >= 0x00 && bytes[i] <= 0x7F)
			{
				i++;
				continue;
			}

			// 2 byte character
			if (bytes[i] >= 0xC2 && bytes[i] <= 0xDF)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					i += 2;
					continue;
				}
			}

			// 3 byte character
			if (bytes[i] == 0xE0)
			{
				if (bytes[i + 1] >= 0xA0 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						i += 3;
						continue;
					}
				}
			}

			if (bytes[i] >= 0xE1 && bytes[i] <= 0xEC)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						i += 3;
						continue;
					}
				}
			}

			if (bytes[i] == 0xED)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0x9F)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						i += 3;
						continue;
					}
				}
			}

			if (bytes[i] >= 0xEE && bytes[i] <= 0xEF)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						i += 3;
						continue;
					}
				}
			}

			// 4 byte character
			if (bytes[i] == 0xF0)
			{
				if (bytes[i + 1] >= 0x90 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
						{
							i += 4;
							continue;
						}
					}
				}
			}

			if (bytes[i] >= 0xF1 && bytes[i] <= 0xF3)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
						{
							i += 4;
							continue;
						}
					}
				}
			}

			if (bytes[i] == 0xF4)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0x8F)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
						{
							i += 4;
							continue;
						}
					}
				}
			}

			return false;
		}
		return true;
	}

	#endregion

}
