using System.IO;
using System.Text;

namespace FileDiff
{
	static class Unicode
	{

		public static FileEncoding GetEncoding(string path)
		{
			Encoding encoding = Encoding.Default;
			bool bom = false;
			NewlineMode newlineMode = NewlineMode.Windows;

			var bytes = new byte[10000];
			int bytesRead = 0;

			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				bytesRead = fileStream.Read(bytes, 0, bytes.Length);
			}

			// Check if the file has a BOM
			if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
			{
				encoding = Encoding.UTF8;
				bom = true;
			}
			else if (bytes[0] == 0xFF && bytes[1] == 0xFE)
			{
				encoding = Encoding.Unicode;
				bom = true;
			}
			else if (bytes[0] == 0xFE && bytes[1] == 0xFF)
			{
				encoding = Encoding.BigEndianUnicode;
				bom = true;
			}
			else if (bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
			{
				encoding = new UTF32Encoding(true, true);
				bom = true;
			}

			// No bom found, check if data passes as a bom-less UTF-8 file
			else if (ValidUtf8(bytes, bytesRead))
			{
				encoding = Encoding.UTF8;
			}

			// Check if the file has null bytes, if so we assume it's a bom-less UTF-16
			else
			{
				for (int i = 0; i < bytesRead; i++)
				{
					if (bytes[i] == 0)
					{
						encoding = Encoding.Unicode;
						break;
					}
				}
			}


			// Check what newline characters are used
			for (int i = 0; i < bytesRead; i++)
			{
				if (bytes[i] == '\n')
				{
					newlineMode = NewlineMode.Unix;
					break;
				}
				else if (bytes[i] == '\r')
				{
					int newLineBytes = encoding == Encoding.Unicode || encoding == Encoding.BigEndianUnicode ? 2 : 1;
					if (i < bytesRead - newLineBytes && bytes[i + newLineBytes] == '\n')
					{
						newlineMode = NewlineMode.Windows;
						break;
					}
					newlineMode = NewlineMode.Mac;
					break;
				}
			}

			return new FileEncoding(encoding, bom, newlineMode);
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

	}
}
