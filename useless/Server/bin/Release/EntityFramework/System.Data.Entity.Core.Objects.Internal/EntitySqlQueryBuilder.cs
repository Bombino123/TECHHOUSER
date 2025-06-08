using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Text;

namespace System.Data.Entity.Core.Objects.Internal;

internal static class EntitySqlQueryBuilder
{
	private const string _setOpEpilog = "\r\n)";

	private const string _setOpProlog = "(\r\n";

	private const string _fromOp = "\r\nFROM (\r\n";

	private const string _asOp = "\r\n) AS ";

	private const string _distinctProlog = "SET(\r\n";

	private const string _distinctEpilog = "\r\n)";

	private const string _exceptOp = "\r\n) EXCEPT (\r\n";

	private const string _groupByOp = "\r\nGROUP BY\r\n";

	private const string _intersectOp = "\r\n) INTERSECT (\r\n";

	private const string _ofTypeProlog = "OFTYPE(\r\n(\r\n";

	private const string _ofTypeInfix = "\r\n),\r\n[";

	private const string _ofTypeInfix2 = "].[";

	private const string _ofTypeEpilog = "]\r\n)";

	private const string _orderByOp = "\r\nORDER BY\r\n";

	private const string _selectOp = "SELECT ";

	private const string _selectValueOp = "SELECT VALUE ";

	private const string _skipOp = "\r\nSKIP\r\n";

	private const string _limitOp = "\r\nLIMIT\r\n";

	private const string _topOp = "SELECT VALUE TOP(\r\n";

	private const string _topInfix = "\r\n) ";

	private const string _unionOp = "\r\n) UNION (\r\n";

	private const string _unionAllOp = "\r\n) UNION ALL (\r\n";

	private const string _whereOp = "\r\nWHERE\r\n";

	private static string GetCommandText(ObjectQueryState query)
	{
		string commandText = null;
		if (!query.TryGetCommandText(out commandText))
		{
			throw new NotSupportedException(Strings.ObjectQuery_QueryBuilder_NotSupportedLinqSource);
		}
		return commandText;
	}

	private static ObjectParameterCollection MergeParameters(ObjectContext context, ObjectParameterCollection sourceQueryParams, ObjectParameter[] builderMethodParams)
	{
		if (sourceQueryParams == null && builderMethodParams.Length == 0)
		{
			return null;
		}
		ObjectParameterCollection objectParameterCollection = ObjectParameterCollection.DeepCopy(sourceQueryParams);
		if (objectParameterCollection == null)
		{
			objectParameterCollection = new ObjectParameterCollection(context.Perspective);
		}
		foreach (ObjectParameter item in builderMethodParams)
		{
			objectParameterCollection.Add(item);
		}
		return objectParameterCollection;
	}

	private static ObjectParameterCollection MergeParameters(ObjectParameterCollection query1Params, ObjectParameterCollection query2Params)
	{
		if (query1Params == null && query2Params == null)
		{
			return null;
		}
		ObjectParameterCollection objectParameterCollection;
		ObjectParameterCollection objectParameterCollection2;
		if (query1Params != null)
		{
			objectParameterCollection = ObjectParameterCollection.DeepCopy(query1Params);
			objectParameterCollection2 = query2Params;
		}
		else
		{
			objectParameterCollection = ObjectParameterCollection.DeepCopy(query2Params);
			objectParameterCollection2 = query1Params;
		}
		if (objectParameterCollection2 != null)
		{
			foreach (ObjectParameter item in objectParameterCollection2)
			{
				objectParameterCollection.Add(item.ShallowCopy());
			}
		}
		return objectParameterCollection;
	}

	private static ObjectQueryState NewBuilderQuery(ObjectQueryState sourceQuery, Type elementType, StringBuilder queryText, Span newSpan, IEnumerable<ObjectParameter> enumerableParams)
	{
		return NewBuilderQuery(sourceQuery, elementType, queryText, allowsLimit: false, newSpan, enumerableParams);
	}

	private static ObjectQueryState NewBuilderQuery(ObjectQueryState sourceQuery, Type elementType, StringBuilder queryText, bool allowsLimit, Span newSpan, IEnumerable<ObjectParameter> enumerableParams)
	{
		ObjectParameterCollection objectParameterCollection = enumerableParams as ObjectParameterCollection;
		if (objectParameterCollection == null && enumerableParams != null)
		{
			objectParameterCollection = new ObjectParameterCollection(sourceQuery.ObjectContext.Perspective);
			foreach (ObjectParameter enumerableParam in enumerableParams)
			{
				objectParameterCollection.Add(enumerableParam);
			}
		}
		EntitySqlQueryState entitySqlQueryState = new EntitySqlQueryState(elementType, queryText.ToString(), allowsLimit, sourceQuery.ObjectContext, objectParameterCollection, newSpan);
		sourceQuery.ApplySettingsTo(entitySqlQueryState);
		return entitySqlQueryState;
	}

	private static ObjectQueryState BuildSetOp(ObjectQueryState leftQuery, ObjectQueryState rightQuery, Span newSpan, string setOp)
	{
		string commandText = GetCommandText(leftQuery);
		string commandText2 = GetCommandText(rightQuery);
		if (leftQuery.ObjectContext != rightQuery.ObjectContext)
		{
			throw new ArgumentException(Strings.ObjectQuery_QueryBuilder_InvalidQueryArgument, "query");
		}
		StringBuilder stringBuilder = new StringBuilder("(\r\n".Length + commandText.Length + setOp.Length + commandText2.Length + "\r\n)".Length);
		stringBuilder.Append("(\r\n");
		stringBuilder.Append(commandText);
		stringBuilder.Append(setOp);
		stringBuilder.Append(commandText2);
		stringBuilder.Append("\r\n)");
		return NewBuilderQuery(leftQuery, leftQuery.ElementType, stringBuilder, newSpan, MergeParameters(leftQuery.Parameters, rightQuery.Parameters));
	}

	private static ObjectQueryState BuildSelectOrSelectValue(ObjectQueryState query, string alias, string projection, ObjectParameter[] parameters, string projectOp, Type elementType)
	{
		string commandText = GetCommandText(query);
		StringBuilder stringBuilder = new StringBuilder(projectOp.Length + projection.Length + "\r\nFROM (\r\n".Length + commandText.Length + "\r\n) AS ".Length + alias.Length);
		stringBuilder.Append(projectOp);
		stringBuilder.Append(projection);
		stringBuilder.Append("\r\nFROM (\r\n");
		stringBuilder.Append(commandText);
		stringBuilder.Append("\r\n) AS ");
		stringBuilder.Append(alias);
		return NewBuilderQuery(query, elementType, stringBuilder, null, MergeParameters(query.ObjectContext, query.Parameters, parameters));
	}

	private static ObjectQueryState BuildOrderByOrWhere(ObjectQueryState query, string alias, string predicateOrKeys, ObjectParameter[] parameters, string op, string skipCount, bool allowsLimit)
	{
		string commandText = GetCommandText(query);
		int num = "SELECT VALUE ".Length + alias.Length + "\r\nFROM (\r\n".Length + commandText.Length + "\r\n) AS ".Length + alias.Length + op.Length + predicateOrKeys.Length;
		if (skipCount != null)
		{
			num += "\r\nSKIP\r\n".Length + skipCount.Length;
		}
		StringBuilder stringBuilder = new StringBuilder(num);
		stringBuilder.Append("SELECT VALUE ");
		stringBuilder.Append(alias);
		stringBuilder.Append("\r\nFROM (\r\n");
		stringBuilder.Append(commandText);
		stringBuilder.Append("\r\n) AS ");
		stringBuilder.Append(alias);
		stringBuilder.Append(op);
		stringBuilder.Append(predicateOrKeys);
		if (skipCount != null)
		{
			stringBuilder.Append("\r\nSKIP\r\n");
			stringBuilder.Append(skipCount);
		}
		return NewBuilderQuery(query, query.ElementType, stringBuilder, allowsLimit, query.Span, MergeParameters(query.ObjectContext, query.Parameters, parameters));
	}

	internal static ObjectQueryState Distinct(ObjectQueryState query)
	{
		string commandText = GetCommandText(query);
		StringBuilder stringBuilder = new StringBuilder("SET(\r\n".Length + commandText.Length + "\r\n)".Length);
		stringBuilder.Append("SET(\r\n");
		stringBuilder.Append(commandText);
		stringBuilder.Append("\r\n)");
		return NewBuilderQuery(query, query.ElementType, stringBuilder, query.Span, ObjectParameterCollection.DeepCopy(query.Parameters));
	}

	internal static ObjectQueryState Except(ObjectQueryState leftQuery, ObjectQueryState rightQuery)
	{
		return BuildSetOp(leftQuery, rightQuery, leftQuery.Span, "\r\n) EXCEPT (\r\n");
	}

	internal static ObjectQueryState GroupBy(ObjectQueryState query, string alias, string keys, string projection, ObjectParameter[] parameters)
	{
		string commandText = GetCommandText(query);
		StringBuilder stringBuilder = new StringBuilder("SELECT ".Length + projection.Length + "\r\nFROM (\r\n".Length + commandText.Length + "\r\n) AS ".Length + alias.Length + "\r\nGROUP BY\r\n".Length + keys.Length);
		stringBuilder.Append("SELECT ");
		stringBuilder.Append(projection);
		stringBuilder.Append("\r\nFROM (\r\n");
		stringBuilder.Append(commandText);
		stringBuilder.Append("\r\n) AS ");
		stringBuilder.Append(alias);
		stringBuilder.Append("\r\nGROUP BY\r\n");
		stringBuilder.Append(keys);
		return NewBuilderQuery(query, typeof(DbDataRecord), stringBuilder, null, MergeParameters(query.ObjectContext, query.Parameters, parameters));
	}

	internal static ObjectQueryState Intersect(ObjectQueryState leftQuery, ObjectQueryState rightQuery)
	{
		Span newSpan = Span.CopyUnion(leftQuery.Span, rightQuery.Span);
		return BuildSetOp(leftQuery, rightQuery, newSpan, "\r\n) INTERSECT (\r\n");
	}

	internal static ObjectQueryState OfType(ObjectQueryState query, EdmType newType, Type clrOfType)
	{
		string commandText = GetCommandText(query);
		StringBuilder stringBuilder = new StringBuilder("OFTYPE(\r\n(\r\n".Length + commandText.Length + "\r\n),\r\n[".Length + newType.NamespaceName.Length + ((!string.IsNullOrEmpty(newType.NamespaceName)) ? "].[".Length : 0) + newType.Name.Length + "]\r\n)".Length);
		stringBuilder.Append("OFTYPE(\r\n(\r\n");
		stringBuilder.Append(commandText);
		stringBuilder.Append("\r\n),\r\n[");
		if (!string.IsNullOrEmpty(newType.NamespaceName))
		{
			stringBuilder.Append(newType.NamespaceName);
			stringBuilder.Append("].[");
		}
		stringBuilder.Append(newType.Name);
		stringBuilder.Append("]\r\n)");
		return NewBuilderQuery(query, clrOfType, stringBuilder, query.Span, ObjectParameterCollection.DeepCopy(query.Parameters));
	}

	internal static ObjectQueryState OrderBy(ObjectQueryState query, string alias, string keys, ObjectParameter[] parameters)
	{
		return BuildOrderByOrWhere(query, alias, keys, parameters, "\r\nORDER BY\r\n", null, allowsLimit: true);
	}

	internal static ObjectQueryState Select(ObjectQueryState query, string alias, string projection, ObjectParameter[] parameters)
	{
		return BuildSelectOrSelectValue(query, alias, projection, parameters, "SELECT ", typeof(DbDataRecord));
	}

	internal static ObjectQueryState SelectValue(ObjectQueryState query, string alias, string projection, ObjectParameter[] parameters, Type projectedType)
	{
		return BuildSelectOrSelectValue(query, alias, projection, parameters, "SELECT VALUE ", projectedType);
	}

	internal static ObjectQueryState Skip(ObjectQueryState query, string alias, string keys, string count, ObjectParameter[] parameters)
	{
		return BuildOrderByOrWhere(query, alias, keys, parameters, "\r\nORDER BY\r\n", count, allowsLimit: true);
	}

	internal static ObjectQueryState Top(ObjectQueryState query, string alias, string count, ObjectParameter[] parameters)
	{
		int length = count.Length;
		string commandText = GetCommandText(query);
		bool allowsLimitSubclause = ((EntitySqlQueryState)query).AllowsLimitSubclause;
		length = ((!allowsLimitSubclause) ? (length + ("SELECT VALUE TOP(\r\n".Length + "\r\n) ".Length + alias.Length + "\r\nFROM (\r\n".Length + commandText.Length + "\r\n) AS ".Length + alias.Length)) : (length + (commandText.Length + "\r\nLIMIT\r\n".Length)));
		StringBuilder stringBuilder = new StringBuilder(length);
		if (allowsLimitSubclause)
		{
			stringBuilder.Append(commandText);
			stringBuilder.Append("\r\nLIMIT\r\n");
			stringBuilder.Append(count);
		}
		else
		{
			stringBuilder.Append("SELECT VALUE TOP(\r\n");
			stringBuilder.Append(count);
			stringBuilder.Append("\r\n) ");
			stringBuilder.Append(alias);
			stringBuilder.Append("\r\nFROM (\r\n");
			stringBuilder.Append(commandText);
			stringBuilder.Append("\r\n) AS ");
			stringBuilder.Append(alias);
		}
		return NewBuilderQuery(query, query.ElementType, stringBuilder, query.Span, MergeParameters(query.ObjectContext, query.Parameters, parameters));
	}

	internal static ObjectQueryState Union(ObjectQueryState leftQuery, ObjectQueryState rightQuery)
	{
		Span newSpan = Span.CopyUnion(leftQuery.Span, rightQuery.Span);
		return BuildSetOp(leftQuery, rightQuery, newSpan, "\r\n) UNION (\r\n");
	}

	internal static ObjectQueryState UnionAll(ObjectQueryState leftQuery, ObjectQueryState rightQuery)
	{
		Span newSpan = Span.CopyUnion(leftQuery.Span, rightQuery.Span);
		return BuildSetOp(leftQuery, rightQuery, newSpan, "\r\n) UNION ALL (\r\n");
	}

	internal static ObjectQueryState Where(ObjectQueryState query, string alias, string predicate, ObjectParameter[] parameters)
	{
		return BuildOrderByOrWhere(query, alias, predicate, parameters, "\r\nWHERE\r\n", null, allowsLimit: false);
	}
}
