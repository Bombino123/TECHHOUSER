namespace System.Data.Entity.Validation;

[Serializable]
public class DbValidationError
{
	private readonly string _propertyName;

	private readonly string _errorMessage;

	public string PropertyName => _propertyName;

	public string ErrorMessage => _errorMessage;

	public DbValidationError(string propertyName, string errorMessage)
	{
		_propertyName = propertyName;
		_errorMessage = errorMessage;
	}
}
