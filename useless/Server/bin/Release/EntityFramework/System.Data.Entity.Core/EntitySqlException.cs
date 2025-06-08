using System.Data.Entity.Core.Common.EntitySql;
using System.Data.Entity.Resources;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace System.Data.Entity.Core;

[Serializable]
public sealed class EntitySqlException : EntityException
{
	private const int HResultInvalidQuery = -2146232006;

	private readonly string _errorDescription;

	private readonly string _errorContext;

	public string ErrorDescription => _errorDescription ?? string.Empty;

	public string ErrorContext => _errorContext ?? string.Empty;

	public int Line { get; }

	public int Column { get; }

	public EntitySqlException()
		: this(Strings.GeneralQueryError)
	{
	}

	public EntitySqlException(string message)
		: base(message)
	{
		base.HResult = -2146232006;
	}

	public EntitySqlException(string message, Exception innerException)
		: base(message, innerException)
	{
		base.HResult = -2146232006;
	}

	private EntitySqlException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_errorDescription = info.GetString("ErrorDescription");
		_errorContext = info.GetString("ErrorContext");
		Line = info.GetInt32("Line");
		Column = info.GetInt32("Column");
	}

	internal static EntitySqlException Create(ErrorContext errCtx, string errorMessage, Exception innerException)
	{
		return Create(errCtx.CommandText, errorMessage, errCtx.InputPosition, errCtx.ErrorContextInfo, errCtx.UseContextInfoAsResourceIdentifier, innerException);
	}

	internal static EntitySqlException Create(string commandText, string errorDescription, int errorPosition, string errorContextInfo, bool loadErrorContextInfoFromResource, Exception innerException)
	{
		int lineNumber;
		int columnNumber;
		string errorContext = FormatErrorContext(commandText, errorPosition, errorContextInfo, loadErrorContextInfoFromResource, out lineNumber, out columnNumber);
		return new EntitySqlException(FormatQueryError(errorDescription, errorContext), errorDescription, errorContext, lineNumber, columnNumber, innerException);
	}

	private EntitySqlException(string message, string errorDescription, string errorContext, int line, int column, Exception innerException)
		: base(message, innerException)
	{
		_errorDescription = errorDescription;
		_errorContext = errorContext;
		Line = line;
		Column = column;
		base.HResult = -2146232006;
	}

	internal static string GetGenericErrorMessage(string commandText, int position)
	{
		int lineNumber = 0;
		int columnNumber = 0;
		return FormatErrorContext(commandText, position, "GenericSyntaxError", loadErrorContextInfoFromResource: true, out lineNumber, out columnNumber);
	}

	internal static string FormatErrorContext(string commandText, int errorPosition, string errorContextInfo, bool loadErrorContextInfoFromResource, out int lineNumber, out int columnNumber)
	{
		if (loadErrorContextInfoFromResource)
		{
			errorContextInfo = ((!string.IsNullOrEmpty(errorContextInfo)) ? EntityRes.GetString(errorContextInfo) : string.Empty);
		}
		StringBuilder stringBuilder = new StringBuilder(commandText.Length);
		for (int i = 0; i < commandText.Length; i++)
		{
			char c = commandText[i];
			if (CqlLexer.IsNewLine(c))
			{
				c = '\n';
			}
			else if ((char.IsControl(c) || char.IsWhiteSpace(c)) && '\r' != c)
			{
				c = ' ';
			}
			stringBuilder.Append(c);
		}
		commandText = stringBuilder.ToString().TrimEnd(new char[1] { '\n' });
		string[] array = commandText.Split(new char[1] { '\n' }, StringSplitOptions.None);
		lineNumber = 0;
		columnNumber = errorPosition;
		while (lineNumber < array.Length && columnNumber > array[lineNumber].Length)
		{
			columnNumber -= array[lineNumber].Length + 1;
			lineNumber++;
		}
		lineNumber++;
		columnNumber++;
		stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(errorContextInfo))
		{
			stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}, ", new object[1] { errorContextInfo });
		}
		if (errorPosition >= 0)
		{
			stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0} {1}, {2} {3}", Strings.LocalizedLine, lineNumber, Strings.LocalizedColumn, columnNumber);
		}
		return stringBuilder.ToString();
	}

	private static string FormatQueryError(string errorMessage, string errorContext)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(errorMessage);
		if (!string.IsNullOrEmpty(errorContext))
		{
			stringBuilder.AppendFormat(CultureInfo.CurrentCulture, " {0} {1}", new object[2]
			{
				Strings.LocalizedNear,
				errorContext
			});
		}
		return stringBuilder.Append(".").ToString();
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ErrorDescription", _errorDescription);
		info.AddValue("ErrorContext", _errorContext);
		info.AddValue("Line", Line);
		info.AddValue("Column", Column);
	}
}
