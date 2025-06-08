using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Text;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

public abstract class EdmType : GlobalItem, INamedDataModelItem
{
	private CollectionType _collectionType;

	private string _name;

	private string _namespace;

	private EdmType _baseType;

	internal string CacheIdentity { get; private set; }

	string INamedDataModelItem.Identity => Identity;

	internal override string Identity
	{
		get
		{
			if (CacheIdentity == null)
			{
				StringBuilder stringBuilder = new StringBuilder(50);
				BuildIdentity(stringBuilder);
				CacheIdentity = stringBuilder.ToString();
			}
			return CacheIdentity;
		}
	}

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public virtual string Name
	{
		get
		{
			return _name;
		}
		internal set
		{
			Util.ThrowIfReadOnly(this);
			_name = value;
		}
	}

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public virtual string NamespaceName
	{
		get
		{
			return _namespace;
		}
		internal set
		{
			Util.ThrowIfReadOnly(this);
			_namespace = value;
		}
	}

	[MetadataProperty(PrimitiveTypeKind.Boolean, false)]
	public bool Abstract
	{
		get
		{
			return GetFlag(MetadataFlags.IsAbstract);
		}
		internal set
		{
			Util.ThrowIfReadOnly(this);
			SetFlag(MetadataFlags.IsAbstract, value);
		}
	}

	[MetadataProperty(BuiltInTypeKind.EdmType, false)]
	public virtual EdmType BaseType
	{
		get
		{
			return _baseType;
		}
		internal set
		{
			Util.ThrowIfReadOnly(this);
			CheckBaseType(value);
			_baseType = value;
		}
	}

	public virtual string FullName => Identity;

	internal virtual Type ClrType => null;

	internal static IEnumerable<T> SafeTraverseHierarchy<T>(T startFrom) where T : EdmType
	{
		HashSet<T> visitedTypes = new HashSet<T>();
		T thisType = startFrom;
		while (thisType != null && !visitedTypes.Contains(thisType))
		{
			visitedTypes.Add(thisType);
			yield return thisType;
			thisType = thisType.BaseType as T;
		}
	}

	internal EdmType()
	{
	}

	internal EdmType(string name, string namespaceName, DataSpace dataSpace)
	{
		Check.NotNull(name, "name");
		Check.NotNull(namespaceName, "namespaceName");
		Initialize(this, name, namespaceName, dataSpace, isAbstract: false, null);
	}

	private void CheckBaseType(EdmType baseType)
	{
		for (EdmType edmType = baseType; edmType != null; edmType = edmType.BaseType)
		{
			if (edmType == this)
			{
				throw new ArgumentException(Strings.CannotSetBaseTypeCyclicInheritance(baseType.Name, Name));
			}
		}
		if (baseType != null && Helper.IsEntityTypeBase(this) && ((EntityTypeBase)baseType).KeyMembers.Count != 0 && ((EntityTypeBase)this).KeyMembers.Count != 0)
		{
			throw new ArgumentException(Strings.CannotDefineKeysOnBothBaseAndDerivedTypes);
		}
	}

	internal override void BuildIdentity(StringBuilder builder)
	{
		if (CacheIdentity != null)
		{
			builder.Append(CacheIdentity);
		}
		else
		{
			builder.Append(CreateEdmTypeIdentity(NamespaceName, Name));
		}
	}

	internal static string CreateEdmTypeIdentity(string namespaceName, string name)
	{
		string text = string.Empty;
		if (!string.IsNullOrEmpty(namespaceName))
		{
			text = namespaceName + ".";
		}
		return text + name;
	}

	internal static void Initialize(EdmType type, string name, string namespaceName, DataSpace dataSpace, bool isAbstract, EdmType baseType)
	{
		type._baseType = baseType;
		type._name = name;
		type._namespace = namespaceName;
		type.DataSpace = dataSpace;
		type.Abstract = isAbstract;
	}

	public override string ToString()
	{
		return FullName;
	}

	public CollectionType GetCollectionType()
	{
		if (_collectionType == null)
		{
			Interlocked.CompareExchange(ref _collectionType, new CollectionType(this), null);
		}
		return _collectionType;
	}

	internal virtual bool IsSubtypeOf(EdmType otherType)
	{
		return Helper.IsSubtypeOf(this, otherType);
	}

	internal virtual bool IsBaseTypeOf(EdmType otherType)
	{
		return otherType?.IsSubtypeOf(this) ?? false;
	}

	internal virtual bool IsAssignableFrom(EdmType otherType)
	{
		return Helper.IsAssignableFrom(this, otherType);
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly)
		{
			base.SetReadOnly();
			BaseType?.SetReadOnly();
		}
	}

	internal virtual IEnumerable<FacetDescription> GetAssociatedFacetDescriptions()
	{
		return MetadataItem.GetGeneralFacetDescriptions();
	}
}
