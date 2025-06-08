using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Text;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class LoadMessageLogger
{
	private readonly Action<string> _logLoadMessage;

	private readonly Dictionary<EdmType, StringBuilder> _messages = new Dictionary<EdmType, StringBuilder>();

	internal LoadMessageLogger(Action<string> logLoadMessage)
	{
		_logLoadMessage = logLoadMessage;
	}

	internal virtual void LogLoadMessage(string message, EdmType relatedType)
	{
		if (_logLoadMessage != null)
		{
			_logLoadMessage(message);
		}
		LogMessagesWithTypeInfo(message, relatedType);
	}

	internal virtual string CreateErrorMessageWithTypeSpecificLoadLogs(string errorMessage, EdmType relatedType)
	{
		return new StringBuilder(errorMessage).AppendLine(GetTypeRelatedLogMessage(relatedType)).ToString();
	}

	private string GetTypeRelatedLogMessage(EdmType relatedType)
	{
		if (_messages.ContainsKey(relatedType))
		{
			return new StringBuilder().AppendLine().AppendLine(Strings.ExtraInfo).AppendLine(_messages[relatedType].ToString())
				.ToString();
		}
		return string.Empty;
	}

	private void LogMessagesWithTypeInfo(string message, EdmType relatedType)
	{
		if (_messages.ContainsKey(relatedType))
		{
			_messages[relatedType].AppendLine(message);
		}
		else
		{
			_messages.Add(relatedType, new StringBuilder(message));
		}
	}
}
