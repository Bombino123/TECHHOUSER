using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

public abstract class MetadataItem
{
	[Flags]
	internal enum MetadataFlags
	{
		None = 0,
		CSpace = 1,
		OSpace = 2,
		OCSpace = 3,
		SSpace = 4,
		CSSpace = 5,
		DataSpace = 7,
		Readonly = 8,
		IsAbstract = 0x10,
		In = 0x200,
		Out = 0x400,
		InOut = 0x600,
		ReturnValue = 0x800,
		ParameterMode = 0xE00
	}

	private int _flags;

	private MetadataPropertyCollection _itemAttributes;

	private static readonly EdmType[] _builtInTypes;

	private static readonly ReadOnlyCollection<FacetDescription> _generalFacetDescriptions;

	private static readonly FacetDescription _nullableFacetDescription;

	private static readonly FacetDescription _defaultValueFacetDescription;

	private static readonly FacetDescription _collectionKindFacetDescription;

	internal virtual IEnumerable<MetadataProperty> Annotations => from p in GetMetadataProperties()
		where p.IsAnnotation
		select p;

	public abstract BuiltInTypeKind BuiltInTypeKind { get; }

	[MetadataProperty(BuiltInTypeKind.MetadataProperty, true)]
	public virtual ReadOnlyMetadataCollection<MetadataProperty> MetadataProperties => GetMetadataProperties().AsReadOnlyMetadataCollection();

	internal MetadataCollection<MetadataProperty> RawMetadataProperties => _itemAttributes;

	public Documentation Documentation { get; set; }

	internal abstract string Identity { get; }

	internal bool IsReadOnly => GetFlag(MetadataFlags.Readonly);

	internal static FacetDescription DefaultValueFacetDescription => _defaultValueFacetDescription;

	internal static FacetDescription CollectionKindFacetDescription => _collectionKindFacetDescription;

	internal static FacetDescription NullableFacetDescription => _nullableFacetDescription;

	internal static EdmProviderManifest EdmProviderManifest => EdmProviderManifest.Instance;

	internal MetadataItem()
	{
	}

	internal MetadataItem(MetadataFlags flags)
	{
		_flags = (int)flags;
	}

	internal MetadataPropertyCollection GetMetadataProperties()
	{
		if (_itemAttributes == null)
		{
			MetadataPropertyCollection metadataPropertyCollection = new MetadataPropertyCollection(this);
			if (IsReadOnly)
			{
				metadataPropertyCollection.SetReadOnly();
			}
			Interlocked.CompareExchange(ref _itemAttributes, metadataPropertyCollection, null);
		}
		return _itemAttributes;
	}

	public void AddAnnotation(string name, object value)
	{
		Check.NotEmpty(name, "name");
		MetadataProperty metadataProperty = Annotations.FirstOrDefault((MetadataProperty a) => a.Name == name);
		if (metadataProperty != null)
		{
			if (value == null)
			{
				RemoveAnnotation(name);
			}
			else
			{
				metadataProperty.Value = value;
			}
		}
		else if (value != null)
		{
			GetMetadataProperties().Add(MetadataProperty.CreateAnnotation(name, value));
		}
	}

	public bool RemoveAnnotation(string name)
	{
		Check.NotEmpty(name, "name");
		MetadataPropertyCollection metadataProperties = GetMetadataProperties();
		if (metadataProperties.TryGetValue(name, ignoreCase: false, out var item))
		{
			return metadataProperties.Remove(item);
		}
		return false;
	}

	internal virtual bool EdmEquals(MetadataItem item)
	{
		if (item != null)
		{
			if (this != item)
			{
				if (BuiltInTypeKind == item.BuiltInTypeKind)
				{
					return Identity == item.Identity;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	internal virtual void SetReadOnly()
	{
		if (!IsReadOnly)
		{
			if (_itemAttributes != null)
			{
				_itemAttributes.SetReadOnly();
			}
			SetFlag(MetadataFlags.Readonly, value: true);
		}
	}

	internal virtual void BuildIdentity(StringBuilder builder)
	{
		builder.Append(Identity);
	}

	internal void AddMetadataProperties(IEnumerable<MetadataProperty> metadataProperties)
	{
		GetMetadataProperties().AddRange(metadataProperties);
	}

	internal DataSpace GetDataSpace()
	{
		return (MetadataFlags)(_flags & 7) switch
		{
			MetadataFlags.CSpace => DataSpace.CSpace, 
			MetadataFlags.OSpace => DataSpace.OSpace, 
			MetadataFlags.SSpace => DataSpace.SSpace, 
			MetadataFlags.OCSpace => DataSpace.OCSpace, 
			MetadataFlags.CSSpace => DataSpace.CSSpace, 
			_ => (DataSpace)(-1), 
		};
	}

	internal void SetDataSpace(DataSpace space)
	{
		_flags = (_flags & -8) | (int)(MetadataFlags.DataSpace & Convert(space));
	}

	private static MetadataFlags Convert(DataSpace space)
	{
		return space switch
		{
			DataSpace.CSpace => MetadataFlags.CSpace, 
			DataSpace.OSpace => MetadataFlags.OSpace, 
			DataSpace.SSpace => MetadataFlags.SSpace, 
			DataSpace.OCSpace => MetadataFlags.OCSpace, 
			DataSpace.CSSpace => MetadataFlags.CSSpace, 
			_ => MetadataFlags.None, 
		};
	}

	internal ParameterMode GetParameterMode()
	{
		return (MetadataFlags)(_flags & 0xE00) switch
		{
			MetadataFlags.In => ParameterMode.In, 
			MetadataFlags.Out => ParameterMode.Out, 
			MetadataFlags.InOut => ParameterMode.InOut, 
			MetadataFlags.ReturnValue => ParameterMode.ReturnValue, 
			_ => (ParameterMode)(-1), 
		};
	}

	internal void SetParameterMode(ParameterMode mode)
	{
		_flags = (_flags & -3585) | (int)(MetadataFlags.ParameterMode & Convert(mode));
	}

	private static MetadataFlags Convert(ParameterMode mode)
	{
		return mode switch
		{
			ParameterMode.In => MetadataFlags.In, 
			ParameterMode.Out => MetadataFlags.Out, 
			ParameterMode.InOut => MetadataFlags.InOut, 
			ParameterMode.ReturnValue => MetadataFlags.ReturnValue, 
			_ => MetadataFlags.ParameterMode, 
		};
	}

	internal bool GetFlag(MetadataFlags flag)
	{
		return flag == (MetadataFlags)((uint)_flags & (uint)flag);
	}

	internal void SetFlag(MetadataFlags flag, bool value)
	{
		SpinWait spinWait = default(SpinWait);
		while (true)
		{
			int flags = _flags;
			int value2 = (value ? (flags | (int)flag) : (flags & (int)(~flag)));
			if ((flags & 8) == 8)
			{
				if ((flag & MetadataFlags.Readonly) == MetadataFlags.Readonly)
				{
					break;
				}
				throw new InvalidOperationException(Strings.OperationOnReadOnlyItem);
			}
			if (flags == Interlocked.CompareExchange(ref _flags, value2, flags))
			{
				break;
			}
			spinWait.SpinOnce();
		}
	}

	static MetadataItem()
	{
		_builtInTypes = new EdmType[40];
		_builtInTypes[0] = new ComplexType();
		_builtInTypes[2] = new ComplexType();
		_builtInTypes[1] = new ComplexType();
		_builtInTypes[3] = new ComplexType();
		_builtInTypes[3] = new ComplexType();
		_builtInTypes[7] = new EnumType();
		_builtInTypes[6] = new ComplexType();
		_builtInTypes[8] = new ComplexType();
		_builtInTypes[9] = new ComplexType();
		_builtInTypes[10] = new EnumType();
		_builtInTypes[11] = new ComplexType();
		_builtInTypes[12] = new ComplexType();
		_builtInTypes[13] = new ComplexType();
		_builtInTypes[14] = new ComplexType();
		_builtInTypes[4] = new ComplexType();
		_builtInTypes[5] = new ComplexType();
		_builtInTypes[15] = new ComplexType();
		_builtInTypes[16] = new ComplexType();
		_builtInTypes[17] = new ComplexType();
		_builtInTypes[18] = new ComplexType();
		_builtInTypes[19] = new ComplexType();
		_builtInTypes[20] = new ComplexType();
		_builtInTypes[21] = new ComplexType();
		_builtInTypes[22] = new ComplexType();
		_builtInTypes[23] = new ComplexType();
		_builtInTypes[24] = new ComplexType();
		_builtInTypes[25] = new EnumType();
		_builtInTypes[26] = new ComplexType();
		_builtInTypes[27] = new EnumType();
		_builtInTypes[28] = new ComplexType();
		_builtInTypes[29] = new ComplexType();
		_builtInTypes[30] = new ComplexType();
		_builtInTypes[31] = new ComplexType();
		_builtInTypes[32] = new ComplexType();
		_builtInTypes[33] = new EnumType();
		_builtInTypes[34] = new ComplexType();
		_builtInTypes[35] = new ComplexType();
		_builtInTypes[36] = new ComplexType();
		_builtInTypes[37] = new ComplexType();
		_builtInTypes[38] = new ComplexType();
		_builtInTypes[39] = new ComplexType();
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem), "ItemType", isAbstract: false, null);
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataProperty), "MetadataProperty", isAbstract: true, (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.GlobalItem), "GlobalItem", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.TypeUsage), "TypeUsage", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.EdmType), "EdmType", isAbstract: true, (ComplexType)GetBuiltInType(BuiltInTypeKind.GlobalItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.SimpleType), "SimpleType", isAbstract: true, (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmType));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.EnumType), "EnumType", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.SimpleType));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.PrimitiveType), "PrimitiveType", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.SimpleType));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.CollectionType), "CollectionType", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmType));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.RefType), "RefType", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmType));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.EdmMember), "EdmMember", isAbstract: true, (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.EdmProperty), "EdmProperty", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmMember));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.NavigationProperty), "NavigationProperty", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmMember));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.ProviderManifest), "ProviderManifest", isAbstract: true, (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.RelationshipEndMember), "RelationshipEnd", isAbstract: true, (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmMember));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.AssociationEndMember), "AssociationEnd", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.RelationshipEndMember));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.EnumMember), "EnumMember", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.ReferentialConstraint), "ReferentialConstraint", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.StructuralType), "StructuralType", isAbstract: true, (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmType));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.RowType), "RowType", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.StructuralType));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.ComplexType), "ComplexType", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.StructuralType));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.EntityTypeBase), "ElementType", isAbstract: true, (ComplexType)GetBuiltInType(BuiltInTypeKind.StructuralType));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.EntityType), "EntityType", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.EntityTypeBase));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.RelationshipType), "RelationshipType", isAbstract: true, (ComplexType)GetBuiltInType(BuiltInTypeKind.EntityTypeBase));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.AssociationType), "AssociationType", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.RelationshipType));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.Facet), "Facet", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.EntityContainer), "EntityContainerType", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.GlobalItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.EntitySetBase), "BaseEntitySetType", isAbstract: true, (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.EntitySet), "EntitySetType", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.EntitySetBase));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.RelationshipSet), "RelationshipSet", isAbstract: true, (ComplexType)GetBuiltInType(BuiltInTypeKind.EntitySetBase));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.AssociationSet), "AssociationSetType", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.RelationshipSet));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.AssociationSetEnd), "AssociationSetEndType", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.FunctionParameter), "FunctionParameter", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.EdmFunction), "EdmFunction", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmType));
		InitializeBuiltInTypes((ComplexType)GetBuiltInType(BuiltInTypeKind.Documentation), "Documentation", isAbstract: false, (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));
		InitializeEnumType(BuiltInTypeKind.OperationAction, "DeleteAction", new string[2] { "None", "Cascade" });
		InitializeEnumType(BuiltInTypeKind.RelationshipMultiplicity, "RelationshipMultiplicity", new string[3] { "One", "ZeroToOne", "Many" });
		InitializeEnumType(BuiltInTypeKind.ParameterMode, "ParameterMode", new string[3] { "In", "Out", "InOut" });
		InitializeEnumType(BuiltInTypeKind.CollectionKind, "CollectionKind", new string[3] { "None", "List", "Bag" });
		InitializeEnumType(BuiltInTypeKind.PrimitiveTypeKind, "PrimitiveTypeKind", Enum.GetNames(typeof(PrimitiveTypeKind)));
		FacetDescription[] array = new FacetDescription[2];
		_nullableFacetDescription = new FacetDescription("Nullable", EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean), null, null, true);
		array[0] = _nullableFacetDescription;
		_defaultValueFacetDescription = new FacetDescription("DefaultValue", GetBuiltInType(BuiltInTypeKind.EdmType), null, null, null);
		array[1] = _defaultValueFacetDescription;
		_generalFacetDescriptions = new ReadOnlyCollection<FacetDescription>(array);
		_collectionKindFacetDescription = new FacetDescription("CollectionKind", GetBuiltInType(BuiltInTypeKind.EnumType), null, null, null);
		TypeUsage typeUsage = TypeUsage.Create(EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.String));
		TypeUsage typeUsage2 = TypeUsage.Create(EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean));
		TypeUsage typeUsage3 = TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmType));
		TypeUsage typeUsage4 = TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.TypeUsage));
		TypeUsage typeUsage5 = TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.ComplexType));
		AddBuiltInTypeProperties(BuiltInTypeKind.MetadataProperty, new EdmProperty[3]
		{
			new EdmProperty("Name", typeUsage),
			new EdmProperty("TypeUsage", typeUsage4),
			new EdmProperty("Value", typeUsage5)
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.MetadataItem, new EdmProperty[2]
		{
			new EdmProperty("MetadataProperties", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.MetadataProperty).GetCollectionType())),
			new EdmProperty("Documentation", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.Documentation)))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.TypeUsage, new EdmProperty[2]
		{
			new EdmProperty("EdmType", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmType))),
			new EdmProperty("Facets", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.Facet)))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.EdmType, new EdmProperty[5]
		{
			new EdmProperty("Name", typeUsage),
			new EdmProperty("Namespace", typeUsage),
			new EdmProperty("Abstract", typeUsage2),
			new EdmProperty("Sealed", typeUsage2),
			new EdmProperty("BaseType", typeUsage5)
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.EnumType, new EdmProperty[1]
		{
			new EdmProperty("EnumMembers", typeUsage)
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.CollectionType, new EdmProperty[1]
		{
			new EdmProperty("TypeUsage", typeUsage4)
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.RefType, new EdmProperty[1]
		{
			new EdmProperty("EntityType", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EntityType)))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.EdmMember, new EdmProperty[2]
		{
			new EdmProperty("Name", typeUsage),
			new EdmProperty("TypeUsage", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.TypeUsage)))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.EdmProperty, new EdmProperty[2]
		{
			new EdmProperty("Nullable", typeUsage),
			new EdmProperty("DefaultValue", typeUsage5)
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.NavigationProperty, new EdmProperty[2]
		{
			new EdmProperty("RelationshipTypeName", typeUsage),
			new EdmProperty("ToEndMemberName", typeUsage)
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.RelationshipEndMember, new EdmProperty[2]
		{
			new EdmProperty("OperationBehaviors", typeUsage5),
			new EdmProperty("RelationshipMultiplicity", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EnumType)))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.EnumMember, new EdmProperty[1]
		{
			new EdmProperty("Name", typeUsage)
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.ReferentialConstraint, new EdmProperty[4]
		{
			new EdmProperty("ToRole", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.RelationshipEndMember))),
			new EdmProperty("FromRole", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.RelationshipEndMember))),
			new EdmProperty("ToProperties", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmProperty).GetCollectionType())),
			new EdmProperty("FromProperties", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmProperty).GetCollectionType()))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.StructuralType, new EdmProperty[1]
		{
			new EdmProperty("Members", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmMember)))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.EntityTypeBase, new EdmProperty[1]
		{
			new EdmProperty("KeyMembers", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmMember)))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.Facet, new EdmProperty[3]
		{
			new EdmProperty("Name", typeUsage),
			new EdmProperty("EdmType", typeUsage3),
			new EdmProperty("Value", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmType)))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.EntityContainer, new EdmProperty[2]
		{
			new EdmProperty("Name", typeUsage),
			new EdmProperty("EntitySets", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EntitySet)))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.EntitySetBase, new EdmProperty[4]
		{
			new EdmProperty("Name", typeUsage),
			new EdmProperty("EntityType", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EntityType))),
			new EdmProperty("Schema", typeUsage),
			new EdmProperty("Table", typeUsage)
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.AssociationSet, new EdmProperty[1]
		{
			new EdmProperty("AssociationSetEnds", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.AssociationSetEnd).GetCollectionType()))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.AssociationSetEnd, new EdmProperty[2]
		{
			new EdmProperty("Role", typeUsage),
			new EdmProperty("EntitySetType", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EntitySet)))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.FunctionParameter, new EdmProperty[3]
		{
			new EdmProperty("Name", typeUsage),
			new EdmProperty("Mode", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EnumType))),
			new EdmProperty("TypeUsage", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.TypeUsage)))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.EdmFunction, new EdmProperty[4]
		{
			new EdmProperty("Name", typeUsage),
			new EdmProperty("Namespace", typeUsage),
			new EdmProperty("ReturnParameter", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.FunctionParameter))),
			new EdmProperty("Parameters", TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.FunctionParameter).GetCollectionType()))
		});
		AddBuiltInTypeProperties(BuiltInTypeKind.Documentation, new EdmProperty[2]
		{
			new EdmProperty("Summary", typeUsage),
			new EdmProperty("LongDescription", typeUsage)
		});
		for (int i = 0; i < _builtInTypes.Length; i++)
		{
			_builtInTypes[i].SetReadOnly();
		}
	}

	public static EdmType GetBuiltInType(BuiltInTypeKind builtInTypeKind)
	{
		return _builtInTypes[(int)builtInTypeKind];
	}

	public static ReadOnlyCollection<FacetDescription> GetGeneralFacetDescriptions()
	{
		return _generalFacetDescriptions;
	}

	private static void InitializeBuiltInTypes(ComplexType builtInType, string name, bool isAbstract, ComplexType baseType)
	{
		EdmType.Initialize(builtInType, name, "Edm", DataSpace.CSpace, isAbstract, baseType);
	}

	private static void AddBuiltInTypeProperties(BuiltInTypeKind builtInTypeKind, EdmProperty[] properties)
	{
		ComplexType complexType = (ComplexType)GetBuiltInType(builtInTypeKind);
		if (properties != null)
		{
			for (int i = 0; i < properties.Length; i++)
			{
				complexType.AddMember(properties[i]);
			}
		}
	}

	private static void InitializeEnumType(BuiltInTypeKind builtInTypeKind, string name, string[] enumMemberNames)
	{
		EnumType enumType = (EnumType)GetBuiltInType(builtInTypeKind);
		EdmType.Initialize(enumType, name, "Edm", DataSpace.CSpace, isAbstract: false, null);
		for (int i = 0; i < enumMemberNames.Length; i++)
		{
			enumType.AddMember(new EnumMember(enumMemberNames[i], i));
		}
	}
}
