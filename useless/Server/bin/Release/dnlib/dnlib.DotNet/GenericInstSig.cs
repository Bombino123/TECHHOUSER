using System.Collections.Generic;

namespace dnlib.DotNet;

public sealed class GenericInstSig : LeafSig
{
	private ClassOrValueTypeSig genericType;

	private readonly IList<TypeSig> genericArgs;

	public override ElementType ElementType => ElementType.GenericInst;

	public ClassOrValueTypeSig GenericType
	{
		get
		{
			return genericType;
		}
		set
		{
			genericType = value;
		}
	}

	public IList<TypeSig> GenericArguments => genericArgs;

	public GenericInstSig()
	{
		genericArgs = new List<TypeSig>();
	}

	public GenericInstSig(ClassOrValueTypeSig genericType)
	{
		this.genericType = genericType;
		genericArgs = new List<TypeSig>();
	}

	public GenericInstSig(ClassOrValueTypeSig genericType, uint genArgCount)
	{
		this.genericType = genericType;
		genericArgs = new List<TypeSig>((int)genArgCount);
	}

	public GenericInstSig(ClassOrValueTypeSig genericType, int genArgCount)
		: this(genericType, (uint)genArgCount)
	{
	}

	public GenericInstSig(ClassOrValueTypeSig genericType, TypeSig genArg1)
	{
		this.genericType = genericType;
		genericArgs = new List<TypeSig> { genArg1 };
	}

	public GenericInstSig(ClassOrValueTypeSig genericType, TypeSig genArg1, TypeSig genArg2)
	{
		this.genericType = genericType;
		genericArgs = new List<TypeSig> { genArg1, genArg2 };
	}

	public GenericInstSig(ClassOrValueTypeSig genericType, TypeSig genArg1, TypeSig genArg2, TypeSig genArg3)
	{
		this.genericType = genericType;
		genericArgs = new List<TypeSig> { genArg1, genArg2, genArg3 };
	}

	public GenericInstSig(ClassOrValueTypeSig genericType, params TypeSig[] genArgs)
	{
		this.genericType = genericType;
		genericArgs = new List<TypeSig>(genArgs);
	}

	public GenericInstSig(ClassOrValueTypeSig genericType, IList<TypeSig> genArgs)
	{
		this.genericType = genericType;
		genericArgs = new List<TypeSig>(genArgs);
	}
}
