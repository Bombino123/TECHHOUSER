using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.EntitySql;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Utils;

internal static class ExternalCalls
{
	internal static bool IsReservedKeyword(string name)
	{
		return CqlLexer.IsReservedKeyword(name);
	}

	internal static DbCommandTree CompileView(string viewDef, StorageMappingItemCollection mappingItemCollection, ParserOptions.CompilationMode compilationMode)
	{
		Perspective perspective = new TargetPerspective(mappingItemCollection.Workspace);
		ParserOptions parserOptions = new ParserOptions();
		parserOptions.ParserCompilationMode = compilationMode;
		return CqlQuery.Compile(viewDef, perspective, parserOptions, null).CommandTree;
	}

	internal static DbExpression CompileFunctionView(string viewDef, StorageMappingItemCollection mappingItemCollection, ParserOptions.CompilationMode compilationMode, IEnumerable<DbParameterReferenceExpression> parameters)
	{
		Perspective perspective = new TargetPerspective(mappingItemCollection.Workspace);
		ParserOptions parserOptions = new ParserOptions();
		parserOptions.ParserCompilationMode = compilationMode;
		return CqlQuery.CompileQueryCommandLambda(viewDef, perspective, parserOptions, null, parameters.Select((DbParameterReferenceExpression pInfo) => pInfo.ResultType.Variable(pInfo.ParameterName))).Invoke(parameters);
	}

	internal static DbLambda CompileFunctionDefinition(string functionDefinition, IList<FunctionParameter> functionParameters, EdmItemCollection edmItemCollection)
	{
		ModelPerspective perspective = new ModelPerspective(new MetadataWorkspace(() => edmItemCollection, () => (StoreItemCollection)null, () => (StorageMappingItemCollection)null));
		return CqlQuery.CompileQueryCommandLambda(functionDefinition, perspective, null, null, functionParameters.Select((FunctionParameter pInfo) => pInfo.TypeUsage.Variable(pInfo.Name)));
	}
}
