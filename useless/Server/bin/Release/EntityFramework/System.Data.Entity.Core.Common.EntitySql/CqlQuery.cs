using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.EntitySql.AST;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.EntitySql;

internal static class CqlQuery
{
	internal static ParseResult Compile(string commandText, Perspective perspective, ParserOptions parserOptions, IEnumerable<DbParameterReferenceExpression> parameters)
	{
		return CompileCommon(commandText, parserOptions, (Node astCommand, ParserOptions validatedParserOptions) => AnalyzeCommandSemantics(astCommand, perspective, validatedParserOptions, parameters));
	}

	internal static DbLambda CompileQueryCommandLambda(string queryCommandText, Perspective perspective, ParserOptions parserOptions, IEnumerable<DbParameterReferenceExpression> parameters, IEnumerable<DbVariableReferenceExpression> variables)
	{
		return CompileCommon(queryCommandText, parserOptions, (Node astCommand, ParserOptions validatedParserOptions) => AnalyzeQueryExpressionSemantics(astCommand, perspective, validatedParserOptions, parameters, variables));
	}

	private static Node Parse(string commandText, ParserOptions parserOptions)
	{
		Check.NotEmpty(commandText, "commandText");
		Node node = new CqlParser(parserOptions, debug: true).Parse(commandText);
		if (node == null)
		{
			throw EntitySqlException.Create(commandText, Strings.InvalidEmptyQuery, 0, null, loadErrorContextInfoFromResource: false, null);
		}
		return node;
	}

	private static TResult CompileCommon<TResult>(string commandText, ParserOptions parserOptions, Func<Node, ParserOptions, TResult> compilationFunction) where TResult : class
	{
		parserOptions = parserOptions ?? new ParserOptions();
		return compilationFunction(Parse(commandText, parserOptions), parserOptions);
	}

	private static ParseResult AnalyzeCommandSemantics(Node astExpr, Perspective perspective, ParserOptions parserOptions, IEnumerable<DbParameterReferenceExpression> parameters)
	{
		return AnalyzeSemanticsCommon(astExpr, perspective, parserOptions, parameters, null, (SemanticAnalyzer analyzer, Node astExpression) => analyzer.AnalyzeCommand(astExpression));
	}

	private static DbLambda AnalyzeQueryExpressionSemantics(Node astQueryCommand, Perspective perspective, ParserOptions parserOptions, IEnumerable<DbParameterReferenceExpression> parameters, IEnumerable<DbVariableReferenceExpression> variables)
	{
		return AnalyzeSemanticsCommon(astQueryCommand, perspective, parserOptions, parameters, variables, (SemanticAnalyzer analyzer, Node astExpr) => analyzer.AnalyzeQueryCommand(astExpr));
	}

	private static TResult AnalyzeSemanticsCommon<TResult>(Node astExpr, Perspective perspective, ParserOptions parserOptions, IEnumerable<DbParameterReferenceExpression> parameters, IEnumerable<DbVariableReferenceExpression> variables, Func<SemanticAnalyzer, Node, TResult> analysisFunction) where TResult : class
	{
		TResult val = null;
		try
		{
			SemanticAnalyzer arg = new SemanticAnalyzer(SemanticResolver.Create(perspective, parserOptions, parameters, variables));
			return analysisFunction(arg, astExpr);
		}
		catch (MetadataException innerException)
		{
			throw new EntitySqlException(Strings.GeneralExceptionAsQueryInnerException("Metadata"), innerException);
		}
		catch (MappingException innerException2)
		{
			throw new EntitySqlException(Strings.GeneralExceptionAsQueryInnerException("Mapping"), innerException2);
		}
	}
}
