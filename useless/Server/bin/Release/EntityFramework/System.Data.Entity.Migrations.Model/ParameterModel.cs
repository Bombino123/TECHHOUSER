using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Migrations.Model;

public class ParameterModel : PropertyModel
{
	public bool IsOutParameter { get; set; }

	public ParameterModel(PrimitiveTypeKind type)
		: this(type, null)
	{
	}

	public ParameterModel(PrimitiveTypeKind type, TypeUsage typeUsage)
		: base(type, typeUsage)
	{
	}
}
