using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class EdmModelValidationContext
{
	private readonly EdmModel _model;

	private readonly bool _validateSyntax;

	public bool ValidateSyntax => _validateSyntax;

	public EdmModel Model => _model;

	public bool IsCSpace => _model.Containers.First().DataSpace == DataSpace.CSpace;

	public event EventHandler<DataModelErrorEventArgs> OnError;

	public EdmModelValidationContext(EdmModel model, bool validateSyntax)
	{
		_model = model;
		_validateSyntax = validateSyntax;
	}

	public void AddError(MetadataItem item, string propertyName, string errorMessage)
	{
		RaiseDataModelValidationEvent(new DataModelErrorEventArgs
		{
			ErrorMessage = errorMessage,
			Item = item,
			PropertyName = propertyName
		});
	}

	private void RaiseDataModelValidationEvent(DataModelErrorEventArgs error)
	{
		if (this.OnError != null)
		{
			this.OnError(this, error);
		}
	}
}
