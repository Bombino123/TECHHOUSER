using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal abstract class PropertyRef
{
	internal virtual PropertyRef CreateNestedPropertyRef(PropertyRef p)
	{
		return new NestedPropertyRef(p, this);
	}

	internal PropertyRef CreateNestedPropertyRef(EdmMember p)
	{
		return CreateNestedPropertyRef(new SimplePropertyRef(p));
	}

	internal PropertyRef CreateNestedPropertyRef(RelProperty p)
	{
		return CreateNestedPropertyRef(new RelPropertyRef(p));
	}

	public override string ToString()
	{
		return "";
	}
}
