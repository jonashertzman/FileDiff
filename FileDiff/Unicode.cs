using System.IO;
using System.Text;

namespace FileDiff
{
	static class Unicode
	{

		public static FileEncoding GetEncoding(string path)
		{
			var bytes = new byte[1000];
			int bytesRead = 0;
			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				bytesRead = fileStream.Read(bytes, 0, bytes.Length);
			}

			// Check for BOM
			if (bytes[0] == 0x2B && bytes[1] == 0x2F && bytes[2] == 0x76)
				return new FileEncoding(Encoding.UTF7, true);
			if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
				return new FileEncoding(Encoding.UTF8, true);
			if (bytes[0] == 0xFF && bytes[1] == 0xFE)
				return new FileEncoding(Encoding.Unicode, true);
			if (bytes[0] == 0xFE && bytes[1] == 0xFF)
				return new FileEncoding(Encoding.BigEndianUnicode, true);
			if (bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
				return new FileEncoding(new UTF32Encoding(true, true), true);

			// Check if data passes as a bom-less UTF-8 file
			if (ValidUtf8(bytes, bytesRead))
			{
				return new FileEncoding(Encoding.UTF8, false);
			}

			// If null bytes exists we assume it is bom-less UTF-16
			for (int i = 0; i < bytesRead; i++)
			{
				if (bytes[i] == 0)
				{
					return new FileEncoding(Encoding.Unicode, false);
				}
			}

			// Otherwise, use windows default
			return new FileEncoding(Encoding.Default, false);
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
