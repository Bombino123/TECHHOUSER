using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common;

public class EntityRecordInfo : DataRecordInfo
{
	private readonly EntityKey _entityKey;

	public EntityKey EntityKey => _entityKey;

	public EntityRecordInfo(EntityType metadata, IEnumerable<EdmMember> memberInfo, EntityKey entityKey, EntitySet entitySet)
		: base(TypeUsage.Create(metadata), memberInfo)
	{
		Check.NotNull(entityKey, "entityKey");
		Check.NotNull(entitySet, "entitySet");
		_entityKey = entityKey;
		ValidateEntityType(entitySet);
	}

	internal EntityRecordInfo(EntityType metadata, EntityKey entityKey, EntitySet entitySet)
		: base(TypeUsage.Create(metadata))
	{
		_entityKey = entityKey;
	}

	internal EntityRecordInfo(DataRecordInfo info, EntityKey entityKey, EntitySet entitySet)
		: base(info)
	{
		_entityKey = entityKey;
	}

	private void ValidateEntityType(EntitySetBase entitySet)
	{
		if (RecordType.EdmType != null && (object)_entityKey != EntityKey.EntityNotValidKey && (object)_entityKey != EntityKey.NoEntitySetKey && RecordType.EdmType != entitySet.ElementType && !entitySet.ElementType.IsBaseTypeOf(RecordType.EdmType))
		{
			throw new ArgumentException(Strings.EntityTypesDoNotAgree);
		}
	}
}
