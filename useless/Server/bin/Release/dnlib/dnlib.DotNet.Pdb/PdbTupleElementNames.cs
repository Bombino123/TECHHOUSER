using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Pdb;

public sealed class PdbTupleElementNames
{
	private readonly IList<string> tupleElementNames;

	private string name;

	private Local local;

	private Instruction scopeStart;

	private Instruction scopeEnd;

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

	public bool IsConstant => local == null;

	public bool IsVariable => local != null;

	public Instruction ScopeStart
	{
		get
		{
			return scopeStart;
		}
		set
		{
			scopeStart = value;
		}
	}

	public Instruction ScopeEnd
	{
		get
		{
			return scopeEnd;
		}
		set
		{
			scopeEnd = value;
		}
	}

	public IList<string> TupleElementNames => tupleElementNames;

	public PdbTupleElementNames()
	{
		tupleElementNames = new List<string>();
	}

	public PdbTupleElementNames(int capacity)
	{
		tupleElementNames = new List<string>(capacity);
	}
}
