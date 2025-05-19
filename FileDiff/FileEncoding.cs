using System.IO;
using System.Text;

namespace FileDiff;

public class FileEncoding
{

	#region Members

	readonly byte[] UTF8_BOM = [0xEF, 0xBB, 0xBF];
	readonly byte[] UTF32LE_BOM = [0xFF, 0xFE, 0x00, 0x00];
	readonly byte[] UTF32BE_BOM = [0x00, 0x00, 0xFE, 0xFF];
	readonly byte[] UTF16LE_BOM = [0xFF, 0xFE];
	readonly byte[] UTF16BE_BOM = [0xFE, 0xFF];

	#endregion

	#region Constructor

	public FileEncoding(string path)
	{
		byte[] bytes = File.ReadAllBytes(path);

		// Check if the file has a BOM
		if (bytes.Length > 2 && bytes[0..3].SequenceEqual(UTF8_BOM))
		{
			Type = Encoding.UTF8;
			HasBom = true;
		}
		else if (bytes.Length > 3 && bytes[0..4].SequenceEqual(UTF32LE_BOM)) // Check this before UTF16 since the first 2 bytes are the same as an UTF16 little endian BOM.
		{
			Type = new UTF32Encoding(false, true);
			HasBom = true;
		}
		else if (bytes.Length > 3 && bytes[0..4].SequenceEqual(UTF32BE_BOM))
		{
			Type = new UTF32Encoding(true, true);
			HasBom = true;
		}
		else if (bytes.Length > 1 && bytes[0..2].SequenceEqual(UTF16LE_BOM))
		{
			Type = Encoding.Unicode;
			HasBom = true;
		}
		else if (bytes.Length > 1 && bytes[0..2].SequenceEqual(UTF16BE_BOM))
		{
			Type = Encoding.BigEndianUnicode;
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
		else if (Type is UTF32Encoding)
		{
			name = "UTF-32";
			if (Type.WebName == "utf-32BE")
			{
				name += " BE";
			}
			else
			{
				name += " LE";
			}
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

	public NewlineMode SaveNewline { get { return Newline == NewlineMode.Mixed ? NewlineMode.Windows : Newline; } }

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
			else if (Type.WebName == "utf-32")
			{
				return new UTF32Encoding(false, HasBom);
			}
			else if (Type.WebName == "utf-32BE")
			{
				return new UTF32Encoding(true, HasBom);
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
