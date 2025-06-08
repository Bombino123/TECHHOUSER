using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common;

public struct FieldMetadata
{
	private readonly EdmMember _fieldType;

	private readonly int _ordinal;

	public EdmMember FieldType => _fieldType;

	public int Ordinal => _ordinal;

	public FieldMetadata(int ordinal, EdmMember fieldType)
	{
		if (ordinal < 0)
		{
			throw new ArgumentOutOfRangeException("ordinal");
		}
		Check.NotNull(fieldType, "fieldType");
		_fieldType = fieldType;
		_ordinal = ordinal;
	}
}
