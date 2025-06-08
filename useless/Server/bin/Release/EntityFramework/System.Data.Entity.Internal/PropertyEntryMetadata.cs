using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Internal;

internal class PropertyEntryMetadata : MemberEntryMetadata
{
	private readonly bool _isMapped;

	private readonly bool _isComplex;

	public bool IsComplex => _isComplex;

	public override MemberEntryType MemberEntryType
	{
		get
		{
			if (!_isComplex)
			{
				return MemberEntryType.ScalarProperty;
			}
			return MemberEntryType.ComplexProperty;
		}
	}

	public bool IsMapped => _isMapped;

	public override Type MemberType => base.ElementType;

	public PropertyEntryMetadata(Type declaringType, Type propertyType, string propertyName, bool isMapped, bool isComplex)
		: base(declaringType, propertyType, propertyName)
	{
		_isMapped = isMapped;
		_isComplex = isComplex;
	}

	public static PropertyEntryMetadata ValidateNameAndGetMetadata(InternalContext internalContext, Type declaringType, Type requestedType, string propertyName)
	{
		DbHelpers.GetPropertyTypes(declaringType).TryGetValue(propertyName, out var value);
		MetadataWorkspace metadataWorkspace = internalContext.ObjectContext.MetadataWorkspace;
		StructuralType item = metadataWorkspace.GetItem<StructuralType>(declaringType.FullNameWithNesting(), DataSpace.OSpace);
		bool isMapped = false;
		bool isComplex = false;
		item.Members.TryGetValue(propertyName, ignoreCase: false, out var item2);
		if (item2 != null)
		{
			if (!(item2 is EdmProperty edmProperty))
			{
				return null;
			}
			if (value == null)
			{
				value = ((!(edmProperty.TypeUsage.EdmType is PrimitiveType primitiveType)) ? ((ObjectItemCollection)metadataWorkspace.GetItemCollection(DataSpace.OSpace)).GetClrType((StructuralType)edmProperty.TypeUsage.EdmType) : primitiveType.ClrEquivalentType);
			}
			isMapped = true;
			isComplex = edmProperty.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType;
		}
		else
		{
			IDictionary<string, Func<object, object>> propertyGetters = DbHelpers.GetPropertyGetters(declaringType);
			IDictionary<string, Action<object, object>> propertySetters = DbHelpers.GetPropertySetters(declaringType);
			if (!propertyGetters.ContainsKey(propertyName) && !propertySetters.ContainsKey(propertyName))
			{
				return null;
			}
		}
		if (!requestedType.IsAssignableFrom(value))
		{
			throw Error.DbEntityEntry_WrongGenericForProp(propertyName, declaringType.Name, requestedType.Name, value.Name);
		}
		return new PropertyEntryMetadata(declaringType, value, propertyName, isMapped, isComplex);
	}

	public override InternalMemberEntry CreateMemberEntry(InternalEntityEntry internalEntityEntry, InternalPropertyEntry parentPropertyEntry)
	{
		if (parentPropertyEntry != null)
		{
			return new InternalNestedPropertyEntry(parentPropertyEntry, this);
		}
		return new InternalEntityPropertyEntry(internalEntityEntry, this);
	}
}
