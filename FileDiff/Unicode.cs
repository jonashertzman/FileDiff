using System.IO;
using System.Text;

namespace FileDiff
{
	static class Unicode
	{

		public static FileEncoding GetEncoding(string path)
		{
			var bytes = new byte[10000];
			int bytesRead = 0;
			NewlineMode newlineMode = NewlineMode.Windows;

			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				bytesRead = fileStream.Read(bytes, 0, bytes.Length);
			}

			// Check what type of newline characters the file uses
			for (int i = 0; i < bytesRead; i++)
			{
				if (bytes[i] == '\n')
				{
					newlineMode = NewlineMode.Unix;
					break;
				}
				else if (bytes[i] == '\r')
				{
					if (i < bytesRead - 1 && bytes[i + 1] == '\n')
					{
						newlineMode = NewlineMode.Windows;
						break;
					}
					newlineMode = NewlineMode.Mac;
					break;
				}
			}

			// Check if the file has a BOM
			if (bytes[0] == 0x2B && bytes[1] == 0x2F && bytes[2] == 0x76)
				return new FileEncoding(Encoding.UTF7, true, newlineMode);
			if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
				return new FileEncoding(Encoding.UTF8, true, newlineMode);
			if (bytes[0] == 0xFF && bytes[1] == 0xFE)
				return new FileEncoding(Encoding.Unicode, true, newlineMode);
			if (bytes[0] == 0xFE && bytes[1] == 0xFF)
				return new FileEncoding(Encoding.BigEndianUnicode, true, newlineMode);
			if (bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
				return new FileEncoding(new UTF32Encoding(true, true), true, newlineMode);

			// Check if data passes as a bom-less UTF-8 file
			if (ValidUtf8(bytes, bytesRead))
			{
				return new FileEncoding(Encoding.UTF8, false, newlineMode);
			}

			// If null bytes exists we assume it is bom-less UTF-16
			for (int i = 0; i < bytesRead; i++)
			{
				if (bytes[i] == 0)
				{
					return new FileEncoding(Encoding.Unicode, false, newlineMode);
				}
			}

			// Otherwise, use windows default
			return new FileEncoding(Encoding.Default, false, newlineMode);
		}

		public static bool ValidUtf8(byte[] bytes, int length)
		{
			int i = 0;
			while (i < length - 4)
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

				if (bytes[i] >= 0xEE && bytes[i] <= 0xFF)
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

	}
}
