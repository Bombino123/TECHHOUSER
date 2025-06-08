namespace System.Data.Entity.Core.Metadata.Edm;

internal class DataModelValidator
{
	public event EventHandler<DataModelErrorEventArgs> OnError;

	public void Validate(EdmModel model, bool validateSyntax)
	{
		EdmModelValidationContext edmModelValidationContext = new EdmModelValidationContext(model, validateSyntax);
		edmModelValidationContext.OnError += this.OnError;
		new EdmModelValidationVisitor(edmModelValidationContext, EdmModelRuleSet.CreateEdmModelRuleSet(model.SchemaVersion, validateSyntax)).Visit(model);
	}
}
