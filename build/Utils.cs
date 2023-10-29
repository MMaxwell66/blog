using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Markdig.Helpers;

internal static class Utils
{
	// A simple key-value pair yaml parser
	public static List<ValueTuple<StringSlice, StringSlice>> ParseYaml(StringLineGroup lineGroup)
	{
		var result = new List<ValueTuple<StringSlice, StringSlice>>(lineGroup.Count);
		for (var i = 0; i < lineGroup.Count; i++)
		{
			var slice = lineGroup.Lines[i].Slice;
			var idx = slice.IndexOf(':');
			if (idx < 0)
				ThrowArgumentException("Yaml front matter contains line without key-value pair.");

			var key = new StringSlice(slice.Text, slice.Start, idx - 1);
			var value = new StringSlice(slice.Text, idx + 1, slice.End);
			key.Trim();
			value.Trim();

			result.Add(new(key, value));
		}
		return result;
	}

	public static ReadOnlyMemory<char> AsMemory(this StringSlice slice)
	{
		if (slice.Text is null || (ulong)(uint)slice.Start + (uint)slice.Length > (uint)slice.Text.Length)
		{
			return default;
		}

		return slice.Text.AsMemory(slice.Start, slice.Length);
	}

	public static bool IsCI = bool.TryParse(Environment.GetEnvironmentVariable("CI"), out var ci) && ci;

	public static Task<string> RunCommandAndGetOutput(string name, string args)
	{
		using var proc = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = name,
				Arguments = args,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				CreateNoWindow = true
			}
		};

		proc.Start();
		return proc.StandardOutput.ReadToEndAsync();
	}

	[DoesNotReturn]
	public static void ThrowArgumentException(string msg) => throw new ArgumentException(msg);
}
