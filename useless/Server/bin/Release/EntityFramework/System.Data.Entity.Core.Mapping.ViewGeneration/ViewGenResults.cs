using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal class ViewGenResults : InternalBase
{
	private readonly KeyToListMap<EntitySetBase, GeneratedView> m_views;

	private readonly ErrorLog m_errorLog;

	internal KeyToListMap<EntitySetBase, GeneratedView> Views => m_views;

	internal IEnumerable<EdmSchemaError> Errors => m_errorLog.Errors;

	internal bool HasErrors => m_errorLog.Count > 0;

	internal ViewGenResults()
	{
		m_views = new KeyToListMap<EntitySetBase, GeneratedView>(EqualityComparer<EntitySetBase>.Default);
		m_errorLog = new ErrorLog();
	}

	internal void AddErrors(ErrorLog errorLog)
	{
		m_errorLog.Merge(errorLog);
	}

	internal string ErrorsToString()
	{
		return m_errorLog.ToString();
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append(m_errorLog.Count);
		builder.Append(" ");
		m_errorLog.ToCompactString(builder);
	}
}
