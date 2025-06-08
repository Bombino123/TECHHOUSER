using System.Diagnostics;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[DebuggerDisplay("{Ordinal} {Name} {Options}")]
[ComVisible(true)]
public sealed class MethodExportInfo
{
	private MethodExportInfoOptions options;

	private ushort? ordinal;

	private string name;

	private const MethodExportInfoOptions DefaultOptions = MethodExportInfoOptions.FromUnmanaged;

	public ushort? Ordinal
	{
		get
		{
			return ordinal;
		}
		set
		{
			ordinal = value;
		}
	}

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public MethodExportInfoOptions Options
	{
		get
		{
			return options;
		}
		set
		{
			options = value;
		}
	}

	public MethodExportInfo()
	{
		options = MethodExportInfoOptions.FromUnmanaged;
	}

	public MethodExportInfo(string name)
	{
		options = MethodExportInfoOptions.FromUnmanaged;
		this.name = name;
	}

	public MethodExportInfo(ushort ordinal)
	{
		options = MethodExportInfoOptions.FromUnmanaged;
		this.ordinal = ordinal;
	}

	public MethodExportInfo(string name, ushort? ordinal)
	{
		options = MethodExportInfoOptions.FromUnmanaged;
		this.name = name;
		this.ordinal = ordinal;
	}

	public MethodExportInfo(string name, ushort? ordinal, MethodExportInfoOptions options)
	{
		this.options = options;
		this.name = name;
		this.ordinal = ordinal;
	}
}
