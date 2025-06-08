using System.Runtime.InteropServices;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public sealed class Local : IVariable
{
	private TypeSig typeSig;

	private int index;

	private string name;

	private PdbLocalAttributes attributes;

	public TypeSig Type
	{
		get
		{
			return typeSig;
		}
		set
		{
			typeSig = value;
		}
	}

	public int Index
	{
		get
		{
			return index;
		}
		internal set
		{
			index = value;
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

	public PdbLocalAttributes Attributes
	{
		get
		{
			return attributes;
		}
		set
		{
			attributes = value;
		}
	}

	internal void SetName(string name)
	{
		this.name = name;
	}

	internal void SetAttributes(PdbLocalAttributes attributes)
	{
		this.attributes = attributes;
	}

	public Local(TypeSig typeSig)
	{
		this.typeSig = typeSig;
	}

	public Local(TypeSig typeSig, string name)
	{
		this.typeSig = typeSig;
		this.name = name;
	}

	public Local(TypeSig typeSig, string name, int index)
	{
		this.typeSig = typeSig;
		this.name = name;
		this.index = index;
	}

	public override string ToString()
	{
		string text = name;
		if (string.IsNullOrEmpty(text))
		{
			return $"V_{Index}";
		}
		return text;
	}
}
