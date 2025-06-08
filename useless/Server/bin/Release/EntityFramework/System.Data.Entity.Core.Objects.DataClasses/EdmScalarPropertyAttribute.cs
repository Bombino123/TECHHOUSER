namespace System.Data.Entity.Core.Objects.DataClasses;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EdmScalarPropertyAttribute : EdmPropertyAttribute
{
	private bool _isNullable = true;

	public bool IsNullable
	{
		get
		{
			return _isNullable;
		}
		set
		{
			_isNullable = value;
		}
	}

	public bool EntityKeyProperty { get; set; }
}
