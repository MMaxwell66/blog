using System.Runtime.CompilerServices;

internal static class Log
{
	public static Verbosity level;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteLine(string msg)
	{
		if (level >= Verbosity.Normal) Console.WriteLine(msg);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DiagWriteLine(string msg)
	{
		if (level >= Verbosity.Diagnostic) Console.WriteLine(msg);
	}
}
