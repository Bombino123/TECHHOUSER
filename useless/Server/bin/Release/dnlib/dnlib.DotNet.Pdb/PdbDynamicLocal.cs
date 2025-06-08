using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Pdb;

public sealed class PdbDynamicLocal
{
	private readonly IList<byte> flags;

	private string name;

	private Local local;

	public IList<byte> Flags => flags;

	public string Name
	{
		get
		{
			string text = name;
			if (text != null)
			{
				return text;
			}
			return local?.Name;
		}
		set
		{
			name = value;
		}
	}

	public bool IsConstant => Local == null;

	public bool IsVariable => Local != null;

	public Local Local
	{
		get
		{
			return local;
		}
		set
		{
			local = value;
		}
	}

	public PdbDynamicLocal()
	{
		flags = new List<byte>();
	}

	public PdbDynamicLocal(int capacity)
	{
		flags = new List<byte>(capacity);
	}
}
