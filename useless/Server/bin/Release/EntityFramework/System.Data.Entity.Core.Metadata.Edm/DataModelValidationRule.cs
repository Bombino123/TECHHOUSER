namespace System.Data.Entity.Core.Metadata.Edm;

internal abstract class DataModelValidationRule
{
	internal abstract Type ValidatedType { get; }

	internal abstract void Evaluate(EdmModelValidationContext context, MetadataItem item);
}
internal abstract class DataModelValidationRule<TItem> : DataModelValidationRule where TItem : class
{
	protected Action<EdmModelValidationContext, TItem> _validate;

	internal override Type ValidatedType => typeof(TItem);

	internal DataModelValidationRule(Action<EdmModelValidationContext, TItem> validate)
	{
		_validate = validate;
	}

	internal override void Evaluate(EdmModelValidationContext context, MetadataItem item)
	{
		_validate(context, item as TItem);
	}
}
