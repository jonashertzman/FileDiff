using System.IO;
using System.Text;

namespace FileDiff
{
	static class Unicode
	{
		public static Encoding GetEncoding(string path)
		{
			var bytes = new byte[1000];
			int bytesRead = 0;
			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				bytesRead = fileStream.Read(bytes, 0, bytes.Length);
			}

			// Check for BOM
			if (bytes[0] == 0x2b && bytes[1] == 0x2f && bytes[2] == 0x76)
				return Encoding.UTF7;
			if (bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf)
				return Encoding.UTF8;
			if (bytes[0] == 0xff && bytes[1] == 0xfe)
				return Encoding.Unicode;
			if (bytes[0] == 0xfe && bytes[1] == 0xff)
				return Encoding.BigEndianUnicode;
			if (bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0xfe && bytes[3] == 0xff)
				return Encoding.UTF32;

			if (Unicode.ValidUtf8(bytes, bytesRead))
			{
				return Encoding.UTF8;
			}

			// If null bytes exists we assume it is UTF-16
			for (int i = 0; i < bytesRead; i++)
			{
				if (bytes[i] == 0)
				{
					return Encoding.Unicode;
				}
			}

			return Encoding.Default;
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
