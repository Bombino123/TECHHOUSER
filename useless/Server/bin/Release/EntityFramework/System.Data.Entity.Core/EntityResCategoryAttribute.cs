using System.ComponentModel;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core;

[AttributeUsage(AttributeTargets.All)]
internal sealed class EntityResCategoryAttribute : CategoryAttribute
{
	public EntityResCategoryAttribute(string category)
		: base(category)
	{
	}

	protected override string GetLocalizedString(string value)
	{
		return EntityRes.GetString(value);
	}
}
