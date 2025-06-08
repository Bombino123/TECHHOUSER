using System.ComponentModel;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core;

[AttributeUsage(AttributeTargets.All)]
internal sealed class EntityResDescriptionAttribute : DescriptionAttribute
{
	private bool _replaced;

	public override string Description
	{
		get
		{
			if (!_replaced)
			{
				_replaced = true;
				base.DescriptionValue = EntityRes.GetString(base.Description);
			}
			return base.Description;
		}
	}

	public EntityResDescriptionAttribute(string description)
		: base(description)
	{
	}
}
