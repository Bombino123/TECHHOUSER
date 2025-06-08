using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class TypeEqualityComparer : IEqualityComparer<IType>, IEqualityComparer<ITypeDefOrRef>, IEqualityComparer<TypeRef>, IEqualityComparer<TypeDef>, IEqualityComparer<TypeSpec>, IEqualityComparer<TypeSig>, IEqualityComparer<ExportedType>
{
	private readonly SigComparerOptions options;

	public static readonly TypeEqualityComparer Instance = new TypeEqualityComparer((SigComparerOptions)0u);

	public static readonly TypeEqualityComparer CaseInsensitive = new TypeEqualityComparer(SigComparerOptions.CaseInsensitiveAll);

	public static readonly TypeEqualityComparer CompareReferenceInSameModule = new TypeEqualityComparer(SigComparerOptions.ReferenceCompareForMemberDefsInSameModule);

	public TypeEqualityComparer(SigComparerOptions options)
	{
		this.options = options;
	}

	public bool Equals(IType x, IType y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(IType obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(ITypeDefOrRef x, ITypeDefOrRef y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(ITypeDefOrRef obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(TypeRef x, TypeRef y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(TypeRef obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(TypeDef x, TypeDef y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(TypeDef obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(TypeSpec x, TypeSpec y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(TypeSpec obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(TypeSig x, TypeSig y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(TypeSig obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}

	public bool Equals(ExportedType x, ExportedType y)
	{
		return new SigComparer(options).Equals(x, y);
	}

	public int GetHashCode(ExportedType obj)
	{
		return new SigComparer(options).GetHashCode(obj);
	}
}
