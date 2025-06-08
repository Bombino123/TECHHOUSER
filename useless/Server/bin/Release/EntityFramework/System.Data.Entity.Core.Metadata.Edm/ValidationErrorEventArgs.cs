namespace System.Data.Entity.Core.Metadata.Edm;

internal class ValidationErrorEventArgs : EventArgs
{
	private readonly EdmItemError _validationError;

	public EdmItemError ValidationError => _validationError;

	public ValidationErrorEventArgs(EdmItemError validationError)
	{
		_validationError = validationError;
	}
}
