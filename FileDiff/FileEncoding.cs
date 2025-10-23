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
		if (DetectBom(bytes))
		{
			HasBom = true;
		}

		// No bom found, check if data passes as a bom-less UTF-8 file.
		else if (CheckValidUtf8(bytes))
		{
			Type = Encoding.UTF8;
			HasBom = false;
		}

		// Check if data could be a bom-less UTF-16 or UTF-32 file.
		else if (DetectUtf16Utf32(bytes))
		{
			HasBom = false;
		}

		// No more sure ways to detect encoding, default to ANSI encoding with Latin 1 character set (ISO-8859-1).
		else
		{
			Type = Encoding.Latin1;
			HasBom = false;
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
		else if (Type == Encoding.Latin1)
		{
			name = $"ANSI ({Type.WebName})";
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

	private Encoding Type = Encoding.Default;

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

			return Encoding.Latin1;
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

	private bool DetectBom(byte[] bytes)
	{
		if (bytes.Length > 2 && bytes[0..3].SequenceEqual(UTF8_BOM))
		{
			Type = Encoding.UTF8;
		}
		else if (bytes.Length > 3 && bytes[0..4].SequenceEqual(UTF32LE_BOM)) // Must check this before UTF16 since the first 2 bytes are the same as an UTF16 little endian BOM.
		{
			Type = new UTF32Encoding(false, true);
		}
		else if (bytes.Length > 3 && bytes[0..4].SequenceEqual(UTF32BE_BOM))
		{
			Type = new UTF32Encoding(true, true);
		}
		else if (bytes.Length > 1 && bytes[0..2].SequenceEqual(UTF16LE_BOM))
		{
			Type = Encoding.Unicode;
		}
		else if (bytes.Length > 1 && bytes[0..2].SequenceEqual(UTF16BE_BOM))
		{
			Type = Encoding.BigEndianUnicode;
		}
		else
		{
			return false;
		}

		return true;
	}

	private bool DetectUtf16Utf32(byte[] bytes)
	{
		// Check if the file has null bytes, if so we assume it is a bom-less UTF-16 or UTF-32 file
		// since they encode white space, punctuation, numbers and English characters as ascii 
		// padded with 1 or 3 null bytes respectively, either before for big endian or after the
		// ascii character for little endian.
		for (int i = 0; i < bytes.Length; i++)
		{
			if (bytes[i] == 0)
			{
				if (i % 2 == 1) // Little endian since the null byte IS NOT on a multiple of 2 or 4.
				{
					if (i < bytes.Length && bytes[i + 1] == 0) // UTF-16 cannot have 2 consecutive null bytes, must be UTF-32.
					{
						Type = new UTF32Encoding(false, false);
						return true;
					}
					else
					{
						Type = Encoding.Unicode;
						return true;
					}
				}
				else // Big endian since the null byte IS on a multiple of 2 or 4.
				{
					if (i < bytes.Length && bytes[i + 1] == 0) // UTF-16 cannot have 2 consecutive null bytes, must be UTF-32.
					{
						Type = new UTF32Encoding(true, false);
						return true;
					}
					else
					{
						Type = Encoding.BigEndianUnicode;
						return true;
					}
				}
			}
		}

		return false;
	}

	private bool CheckValidUtf8(byte[] bytes)
	{
		int i = 0;

		if (bytes.StartsWith(UTF8_BOM))
		{
			i += UTF8_BOM.Length;
		}

		while (i < bytes.Length)
		{
			// 1 byte character
			if (bytes[i] >= 0x00 && bytes[i] <= 0x7F)
			{
				i++;
				continue;
			}

			// 2 byte character
			if (i + 1 < bytes.Length)
			{
				if (bytes[i] >= 0xC2 && bytes[i] <= 0xDF)
				{
					if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
					{
						i += 2;
						continue;
					}
				}
			}

			// 3 byte character
			if (i + 2 < bytes.Length)
			{
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
			}

			// 4 byte character
			if (i + 3 < bytes.Length)
			{
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
			}

			return false;
		}

		return true;
	}

	#endregion

}
