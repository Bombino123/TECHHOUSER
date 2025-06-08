using System.Data.Entity.Resources;
using System.Globalization;

namespace System.Data.Entity.Core.Metadata.Edm;

[Serializable]
public sealed class EdmSchemaError : EdmError
{
	private int _errorCode;

	private EdmSchemaErrorSeverity _severity;

	private string _schemaLocation;

	private int _line = -1;

	private int _column = -1;

	private string _stackTrace = string.Empty;

	public int ErrorCode => _errorCode;

	public EdmSchemaErrorSeverity Severity
	{
		get
		{
			return _severity;
		}
		set
		{
			_severity = value;
		}
	}

	public int Line => _line;

	public int Column => _column;

	public string SchemaLocation => _schemaLocation;

	public string SchemaName => GetNameFromSchemaLocation(SchemaLocation);

	public string StackTrace => _stackTrace;

	public EdmSchemaError(string message, int errorCode, EdmSchemaErrorSeverity severity)
		: this(message, errorCode, severity, null)
	{
	}

	internal EdmSchemaError(string message, int errorCode, EdmSchemaErrorSeverity severity, Exception exception)
		: base(message)
	{
		Initialize(errorCode, severity, null, -1, -1, exception);
	}

	internal EdmSchemaError(string message, int errorCode, EdmSchemaErrorSeverity severity, string schemaLocation, int line, int column)
		: this(message, errorCode, severity, schemaLocation, line, column, null)
	{
	}

	internal EdmSchemaError(string message, int errorCode, EdmSchemaErrorSeverity severity, string schemaLocation, int line, int column, Exception exception)
		: base(message)
	{
		if (severity < EdmSchemaErrorSeverity.Warning || severity > EdmSchemaErrorSeverity.Error)
		{
			throw new ArgumentOutOfRangeException("severity", severity, Strings.ArgumentOutOfRange(severity));
		}
		Initialize(errorCode, severity, schemaLocation, line, column, exception);
	}

	private void Initialize(int errorCode, EdmSchemaErrorSeverity severity, string schemaLocation, int line, int column, Exception exception)
	{
		if (errorCode < 0)
		{
			throw new ArgumentOutOfRangeException("errorCode", errorCode, Strings.ArgumentOutOfRangeExpectedPostiveNumber(errorCode));
		}
		_errorCode = errorCode;
		_severity = severity;
		_schemaLocation = schemaLocation;
		_line = line;
		_column = column;
		if (exception != null)
		{
			_stackTrace = exception.StackTrace;
		}
	}

	public override string ToString()
	{
		string text = Severity switch
		{
			EdmSchemaErrorSeverity.Error => Strings.GeneratorErrorSeverityError, 
			EdmSchemaErrorSeverity.Warning => Strings.GeneratorErrorSeverityWarning, 
			_ => Strings.GeneratorErrorSeverityUnknown, 
		};
		if (string.IsNullOrEmpty(SchemaName) && Line < 0 && Column < 0)
		{
			return string.Format(CultureInfo.CurrentCulture, "{0} {1:0000}: {2}", new object[3] { text, ErrorCode, base.Message });
		}
		return string.Format(CultureInfo.CurrentCulture, "{0}({1},{2}) : {3} {4:0000}: {5}", (SchemaName == null) ? Strings.SourceUriUnknown : SchemaName, Line, Column, text, ErrorCode, base.Message);
	}

	private static string GetNameFromSchemaLocation(string schemaLocation)
	{
		if (string.IsNullOrEmpty(schemaLocation))
		{
			return schemaLocation;
		}
		int num = Math.Max(schemaLocation.LastIndexOf('/'), schemaLocation.LastIndexOf('\\'));
		int num2 = num + 1;
		if (num < 0)
		{
			return schemaLocation;
		}
		if (num2 >= schemaLocation.Length)
		{
			return string.Empty;
		}
		return schemaLocation.Substring(num2);
	}
}
