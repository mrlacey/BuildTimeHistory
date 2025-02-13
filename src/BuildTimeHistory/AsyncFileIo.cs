using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildTimeHistory;

public  class AsyncFileIo
{
	public static async Task<string> ReadAllTextAsync(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException(nameof(path));
		}

		byte[] result;
		using (FileStream stream = File.Open(path, FileMode.Open))
		{
			result = new byte[stream.Length];
			await stream.ReadAsync(result, 0, (int)stream.Length);
		}

		return Encoding.UTF8.GetString(result);
	}

	public static async Task WriteAllTextAsync(string path, string contents)
	{
		byte[] encodedText = Encoding.UTF8.GetBytes(contents);

		using FileStream sourceStream = new(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
		await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
	}
}
