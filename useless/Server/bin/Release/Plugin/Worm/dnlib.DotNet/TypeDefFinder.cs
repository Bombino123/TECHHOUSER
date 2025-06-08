using System;
using System.Collections.Generic;
using System.Text;
using dnlib.Threading;

namespace dnlib.DotNet;

internal sealed class TypeDefFinder : ITypeDefFinder, IDisposable
{
	private const SigComparerOptions TypeComparerOptions = SigComparerOptions.DontCompareTypeScope | SigComparerOptions.TypeRefCanReferenceGlobalType;

	private bool isCacheEnabled;

	private readonly bool includeNestedTypes;

	private Dictionary<ITypeDefOrRef, TypeDef> typeRefCache = new Dictionary<ITypeDefOrRef, TypeDef>(new TypeEqualityComparer(SigComparerOptions.DontCompareTypeScope | SigComparerOptions.TypeRefCanReferenceGlobalType));

	private Dictionary<string, TypeDef> normalNameCache = new Dictionary<string, TypeDef>(StringComparer.Ordinal);

	private Dictionary<string, TypeDef> reflectionNameCache = new Dictionary<string, TypeDef>(StringComparer.Ordinal);

	private readonly StringBuilder sb = new StringBuilder();

	private IEnumerator<TypeDef> typeEnumerator;

	private readonly IEnumerable<TypeDef> rootTypes;

	private readonly Lock theLock = Lock.Create();

	public bool IsCacheEnabled
	{
		get
		{
			theLock.EnterReadLock();
			try
			{
				return IsCacheEnabled_NoLock;
			}
			finally
			{
				theLock.ExitReadLock();
			}
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				IsCacheEnabled_NoLock = value;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	private bool IsCacheEnabled_NoLock
	{
		get
		{
			return isCacheEnabled;
		}
		set
		{
			if (isCacheEnabled != value)
			{
				if (typeEnumerator != null)
				{
					typeEnumerator.Dispose();
					typeEnumerator = null;
				}
				typeRefCache.Clear();
				normalNameCache.Clear();
				reflectionNameCache.Clear();
				if (value)
				{
					InitializeTypeEnumerator();
				}
				isCacheEnabled = value;
			}
		}
	}

	public TypeDefFinder(IEnumerable<TypeDef> rootTypes)
		: this(rootTypes, includeNestedTypes: true)
	{
	}

	public TypeDefFinder(IEnumerable<TypeDef> rootTypes, bool includeNestedTypes)
	{
		this.rootTypes = rootTypes ?? throw new ArgumentNullException("rootTypes");
		this.includeNestedTypes = includeNestedTypes;
	}

	private void InitializeTypeEnumerator()
	{
		if (typeEnumerator != null)
		{
			typeEnumerator.Dispose();
			typeEnumerator = null;
		}
		typeEnumerator = (includeNestedTypes ? AllTypesHelper.Types(rootTypes) : rootTypes).GetEnumerator();
	}

	public void ResetCache()
	{
		theLock.EnterWriteLock();
		try
		{
			bool isCacheEnabled_NoLock = IsCacheEnabled_NoLock;
			IsCacheEnabled_NoLock = false;
			IsCacheEnabled_NoLock = isCacheEnabled_NoLock;
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	public TypeDef Find(string fullName, bool isReflectionName)
	{
		if (fullName == null)
		{
			return null;
		}
		theLock.EnterWriteLock();
		try
		{
			if (isCacheEnabled)
			{
				return isReflectionName ? FindCacheReflection(fullName) : FindCacheNormal(fullName);
			}
			return isReflectionName ? FindSlowReflection(fullName) : FindSlowNormal(fullName);
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	public TypeDef Find(TypeRef typeRef)
	{
		if (typeRef == null)
		{
			return null;
		}
		theLock.EnterWriteLock();
		try
		{
			return isCacheEnabled ? FindCache(typeRef) : FindSlow(typeRef);
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	private TypeDef FindCache(TypeRef typeRef)
	{
		if (typeRefCache.TryGetValue(typeRef, out var value))
		{
			return value;
		}
		SigComparer sigComparer = new SigComparer(SigComparerOptions.DontCompareTypeScope | SigComparerOptions.TypeRefCanReferenceGlobalType);
		do
		{
			value = GetNextTypeDefCache();
		}
		while (value != null && !sigComparer.Equals(value, typeRef));
		return value;
	}

	private TypeDef FindCacheReflection(string fullName)
	{
		if (reflectionNameCache.TryGetValue(fullName, out var value))
		{
			return value;
		}
		do
		{
			value = GetNextTypeDefCache();
			if (value == null)
			{
				return value;
			}
			sb.Length = 0;
		}
		while (!(FullNameFactory.FullName(value, isReflection: true, null, sb) == fullName));
		return value;
	}

	private TypeDef FindCacheNormal(string fullName)
	{
		if (normalNameCache.TryGetValue(fullName, out var value))
		{
			return value;
		}
		do
		{
			value = GetNextTypeDefCache();
			if (value == null)
			{
				return value;
			}
			sb.Length = 0;
		}
		while (!(FullNameFactory.FullName(value, isReflection: false, null, sb) == fullName));
		return value;
	}

	private TypeDef FindSlow(TypeRef typeRef)
	{
		InitializeTypeEnumerator();
		SigComparer sigComparer = new SigComparer(SigComparerOptions.DontCompareTypeScope | SigComparerOptions.TypeRefCanReferenceGlobalType);
		TypeDef nextTypeDef;
		do
		{
			nextTypeDef = GetNextTypeDef();
		}
		while (nextTypeDef != null && !sigComparer.Equals(nextTypeDef, typeRef));
		return nextTypeDef;
	}

	private TypeDef FindSlowReflection(string fullName)
	{
		InitializeTypeEnumerator();
		TypeDef nextTypeDef;
		do
		{
			nextTypeDef = GetNextTypeDef();
			if (nextTypeDef == null)
			{
				return nextTypeDef;
			}
			sb.Length = 0;
		}
		while (!(FullNameFactory.FullName(nextTypeDef, isReflection: true, null, sb) == fullName));
		return nextTypeDef;
	}

	private TypeDef FindSlowNormal(string fullName)
	{
		InitializeTypeEnumerator();
		TypeDef nextTypeDef;
		do
		{
			nextTypeDef = GetNextTypeDef();
			if (nextTypeDef == null)
			{
				return nextTypeDef;
			}
			sb.Length = 0;
		}
		while (!(FullNameFactory.FullName(nextTypeDef, isReflection: false, null, sb) == fullName));
		return nextTypeDef;
	}

	private TypeDef GetNextTypeDef()
	{
		while (typeEnumerator.MoveNext())
		{
			TypeDef current = typeEnumerator.Current;
			if (current != null)
			{
				return current;
			}
		}
		return null;
	}

	private TypeDef GetNextTypeDefCache()
	{
		TypeDef nextTypeDef = GetNextTypeDef();
		if (nextTypeDef == null)
		{
			return null;
		}
		if (!typeRefCache.ContainsKey(nextTypeDef))
		{
			typeRefCache[nextTypeDef] = nextTypeDef;
		}
		sb.Length = 0;
		string key;
		if (!normalNameCache.ContainsKey(key = FullNameFactory.FullName(nextTypeDef, isReflection: false, null, sb)))
		{
			normalNameCache[key] = nextTypeDef;
		}
		sb.Length = 0;
		if (!reflectionNameCache.ContainsKey(key = FullNameFactory.FullName(nextTypeDef, isReflection: true, null, sb)))
		{
			reflectionNameCache[key] = nextTypeDef;
		}
		return nextTypeDef;
	}

	public void Dispose()
	{
		theLock.EnterWriteLock();
		try
		{
			if (typeEnumerator != null)
			{
				typeEnumerator.Dispose();
			}
			typeEnumerator = null;
			typeRefCache = null;
			normalNameCache = null;
			reflectionNameCache = null;
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}
}
