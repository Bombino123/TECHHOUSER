namespace System.Data.Entity.Core.Metadata.Edm;

internal class EdmModelValidationRule<TItem> : DataModelValidationRule<TItem> where TItem : class
{
	internal EdmModelValidationRule(Action<EdmModelValidationContext, TItem> validate)
		: base(validate)
	{
	}
}
