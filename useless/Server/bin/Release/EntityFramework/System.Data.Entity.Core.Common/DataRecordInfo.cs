using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common;

public class DataRecordInfo
{
	private readonly ReadOnlyCollection<FieldMetadata> _fieldMetadata;

	private readonly TypeUsage _metadata;

	public ReadOnlyCollection<FieldMetadata> FieldMetadata => _fieldMetadata;

	public virtual TypeUsage RecordType => _metadata;

	internal DataRecordInfo()
	{
	}

	public DataRecordInfo(TypeUsage metadata, IEnumerable<EdmMember> memberInfo)
	{
		Check.NotNull(metadata, "metadata");
		IBaseList<EdmMember> allStructuralMembers = TypeHelpers.GetAllStructuralMembers(metadata.EdmType);
		List<FieldMetadata> list = new List<FieldMetadata>(allStructuralMembers.Count);
		if (memberInfo != null)
		{
			foreach (EdmMember item in memberInfo)
			{
				if (item != null && 0 <= allStructuralMembers.IndexOf(item) && (BuiltInTypeKind.EdmProperty == item.BuiltInTypeKind || item.BuiltInTypeKind == BuiltInTypeKind.AssociationEndMember))
				{
					if (item.DeclaringType != metadata.EdmType && !item.DeclaringType.IsBaseTypeOf(metadata.EdmType))
					{
						throw new ArgumentException(Strings.EdmMembersDefiningTypeDoNotAgreeWithMetadataType);
					}
					list.Add(new FieldMetadata(list.Count, item));
					continue;
				}
				throw Error.InvalidEdmMemberInstance();
			}
		}
		if (Helper.IsStructuralType(metadata.EdmType) == 0 < list.Count)
		{
			_fieldMetadata = new ReadOnlyCollection<FieldMetadata>(list);
			_metadata = metadata;
			return;
		}
		throw Error.InvalidEdmMemberInstance();
	}

	internal DataRecordInfo(TypeUsage metadata)
	{
		IBaseList<EdmMember> allStructuralMembers = TypeHelpers.GetAllStructuralMembers(metadata);
		FieldMetadata[] array = new FieldMetadata[allStructuralMembers.Count];
		for (int i = 0; i < array.Length; i++)
		{
			EdmMember fieldType = allStructuralMembers[i];
			array[i] = new FieldMetadata(i, fieldType);
		}
		_fieldMetadata = new ReadOnlyCollection<FieldMetadata>(array);
		_metadata = metadata;
	}

	internal DataRecordInfo(DataRecordInfo recordInfo)
	{
		_fieldMetadata = recordInfo._fieldMetadata;
		_metadata = recordInfo._metadata;
	}
}
