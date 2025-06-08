using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal sealed class DbExpressionValidator : DbExpressionRebinder
{
	private readonly DataSpace requiredSpace;

	private readonly DataSpace[] allowedMetadataSpaces;

	private readonly DataSpace[] allowedFunctionSpaces;

	private readonly Dictionary<string, DbParameterReferenceExpression> paramMappings = new Dictionary<string, DbParameterReferenceExpression>();

	private readonly Stack<Dictionary<string, TypeUsage>> variableScopes = new Stack<Dictionary<string, TypeUsage>>();

	private string expressionArgumentName;

	internal Dictionary<string, DbParameterReferenceExpression> Parameters => paramMappings;

	internal DbExpressionValidator(MetadataWorkspace metadata, DataSpace expectedDataSpace)
		: base(metadata)
	{
		requiredSpace = expectedDataSpace;
		allowedFunctionSpaces = new DataSpace[2]
		{
			DataSpace.CSpace,
			DataSpace.SSpace
		};
		if (expectedDataSpace == DataSpace.SSpace)
		{
			allowedMetadataSpaces = new DataSpace[2]
			{
				DataSpace.SSpace,
				DataSpace.CSpace
			};
		}
		else
		{
			allowedMetadataSpaces = new DataSpace[1] { DataSpace.CSpace };
		}
	}

	internal void ValidateExpression(DbExpression expression, string argumentName)
	{
		expressionArgumentName = argumentName;
		VisitExpression(expression);
		expressionArgumentName = null;
	}

	protected override EntitySetBase VisitEntitySet(EntitySetBase entitySet)
	{
		return ValidateMetadata(entitySet, base.VisitEntitySet, (EntitySetBase es) => es.EntityContainer.DataSpace, allowedMetadataSpaces);
	}

	protected override EdmFunction VisitFunction(EdmFunction function)
	{
		return ValidateMetadata(function, base.VisitFunction, (EdmFunction func) => func.DataSpace, allowedFunctionSpaces);
	}

	protected override EdmType VisitType(EdmType type)
	{
		return ValidateMetadata(type, base.VisitType, (EdmType et) => et.DataSpace, allowedMetadataSpaces);
	}

	protected override TypeUsage VisitTypeUsage(TypeUsage type)
	{
		return ValidateMetadata(type, base.VisitTypeUsage, (TypeUsage tu) => tu.EdmType.DataSpace, allowedMetadataSpaces);
	}

	protected override void OnEnterScope(IEnumerable<DbVariableReferenceExpression> scopeVariables)
	{
		Dictionary<string, TypeUsage> item = scopeVariables.ToDictionary<DbVariableReferenceExpression, string, TypeUsage>((DbVariableReferenceExpression var) => var.VariableName, (DbVariableReferenceExpression var) => var.ResultType, StringComparer.Ordinal);
		variableScopes.Push(item);
	}

	protected override void OnExitScope()
	{
		variableScopes.Pop();
	}

	public override DbExpression Visit(DbVariableReferenceExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = base.Visit(expression);
		if (dbExpression.ExpressionKind == DbExpressionKind.VariableReference)
		{
			DbVariableReferenceExpression dbVariableReferenceExpression = (DbVariableReferenceExpression)dbExpression;
			TypeUsage value = null;
			using (Stack<Dictionary<string, TypeUsage>>.Enumerator enumerator = variableScopes.GetEnumerator())
			{
				while (enumerator.MoveNext() && !enumerator.Current.TryGetValue(dbVariableReferenceExpression.VariableName, out value))
				{
				}
			}
			if (value == null)
			{
				ThrowInvalid(Strings.Cqt_Validator_VarRefInvalid(dbVariableReferenceExpression.VariableName));
			}
			if (!TypeSemantics.IsEqual(dbVariableReferenceExpression.ResultType, value))
			{
				ThrowInvalid(Strings.Cqt_Validator_VarRefTypeMismatch(dbVariableReferenceExpression.VariableName));
			}
		}
		return dbExpression;
	}

	public override DbExpression Visit(DbParameterReferenceExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = base.Visit(expression);
		if (dbExpression.ExpressionKind == DbExpressionKind.ParameterReference)
		{
			DbParameterReferenceExpression dbParameterReferenceExpression = dbExpression as DbParameterReferenceExpression;
			if (paramMappings.TryGetValue(dbParameterReferenceExpression.ParameterName, out var value))
			{
				if (!TypeSemantics.IsEqual(dbParameterReferenceExpression.ResultType, value.ResultType))
				{
					ThrowInvalid(Strings.Cqt_Validator_InvalidIncompatibleParameterReferences(dbParameterReferenceExpression.ParameterName));
				}
			}
			else
			{
				paramMappings.Add(dbParameterReferenceExpression.ParameterName, dbParameterReferenceExpression);
			}
		}
		return dbExpression;
	}

	private TMetadata ValidateMetadata<TMetadata>(TMetadata metadata, Func<TMetadata, TMetadata> map, Func<TMetadata, DataSpace> getDataSpace, DataSpace[] allowedSpaces)
	{
		TMetadata val = map(metadata);
		if ((object)metadata != (object)val)
		{
			ThrowInvalidMetadata<TMetadata>();
		}
		DataSpace resultSpace = getDataSpace(val);
		if (!allowedSpaces.Any((DataSpace ds) => ds == resultSpace))
		{
			ThrowInvalidSpace<TMetadata>();
		}
		return val;
	}

	private void ThrowInvalidMetadata<TMetadata>()
	{
		ThrowInvalid(Strings.Cqt_Validator_InvalidOtherWorkspaceMetadata(typeof(TMetadata).Name));
	}

	private void ThrowInvalidSpace<TMetadata>()
	{
		ThrowInvalid(Strings.Cqt_Validator_InvalidIncorrectDataSpaceMetadata(typeof(TMetadata).Name, Enum.GetName(typeof(DataSpace), requiredSpace)));
	}

	private void ThrowInvalid(string message)
	{
		throw new ArgumentException(message, expressionArgumentName);
	}
}
