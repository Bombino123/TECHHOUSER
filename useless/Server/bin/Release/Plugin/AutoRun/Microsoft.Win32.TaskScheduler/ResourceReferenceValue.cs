using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

[PublicAPI]
[ComVisible(true)]
public class ResourceReferenceValue
{
	public string ResourceFilePath { get; set; }

	public int ResourceIdentifier { get; set; }

	public ResourceReferenceValue([NotNull] string dllPath, int resourceId)
	{
		ResourceFilePath = dllPath;
		ResourceIdentifier = resourceId;
	}

	public static implicit operator string(ResourceReferenceValue value)
	{
		return value.ToString();
	}

	[NotNull]
	public static ResourceReferenceValue Parse([NotNull] string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!TryParse(value, out var resourceRef))
		{
			throw new FormatException();
		}
		return resourceRef;
	}

	public static bool TryParse(string value, out ResourceReferenceValue resourceRef)
	{
		if (!string.IsNullOrEmpty(value))
		{
			Match match = Regex.Match(value, "^\\$\\(\\@ (?<x>[^,]+), (?<i>-?\\d+)\\)$");
			if (match.Success)
			{
				resourceRef = new ResourceReferenceValue(match.Groups["x"].Value, int.Parse(match.Groups["i"].Value));
				return true;
			}
		}
		resourceRef = null;
		return false;
	}

	[NotNull]
	public string GetResolvedString()
	{
		if (!File.Exists(ResourceFilePath))
		{
			throw new FileNotFoundException("Invalid resource file path.", ResourceFilePath);
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = NativeMethods.LoadLibrary(ResourceFilePath);
			if (intPtr == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			StringBuilder stringBuilder = new StringBuilder(8192);
			int num = LoadString(intPtr, ResourceIdentifier, stringBuilder, stringBuilder.Capacity);
			if (num == 0)
			{
				throw new Win32Exception();
			}
			return stringBuilder.ToString(0, num);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				NativeMethods.FreeLibrary(intPtr);
			}
		}
	}

	public override string ToString()
	{
		return $"$(@ {ResourceFilePath}, {ResourceIdentifier})";
	}

	[DllImport("user32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
	private static extern int LoadString(IntPtr hInstance, int wID, StringBuilder lpBuffer, int nBufferMax);
}
