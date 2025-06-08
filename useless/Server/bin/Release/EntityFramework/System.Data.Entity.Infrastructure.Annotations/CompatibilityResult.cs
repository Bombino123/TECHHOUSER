using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Annotations;

public sealed class CompatibilityResult
{
	private readonly bool _isCompatible;

	private readonly string _errorMessage;

	public bool IsCompatible => _isCompatible;

	public string ErrorMessage => _errorMessage;

	public CompatibilityResult(bool isCompatible, string errorMessage)
	{
		_isCompatible = isCompatible;
		_errorMessage = errorMessage;
		if (!isCompatible)
		{
			Check.NotEmpty(errorMessage, "errorMessage");
		}
	}

	public static implicit operator bool(CompatibilityResult result)
	{
		Check.NotNull(result, "result");
		return result._isCompatible;
	}
}
