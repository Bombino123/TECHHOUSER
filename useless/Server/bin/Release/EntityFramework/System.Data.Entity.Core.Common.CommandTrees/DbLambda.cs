using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.Core.Common.CommandTrees;

public sealed class DbLambda
{
	private readonly ReadOnlyCollection<DbVariableReferenceExpression> _variables;

	private readonly DbExpression _body;

	public DbExpression Body => _body;

	public IList<DbVariableReferenceExpression> Variables => _variables;

	internal DbLambda(ReadOnlyCollection<DbVariableReferenceExpression> variables, DbExpression bodyExp)
	{
		_variables = variables;
		_body = bodyExp;
	}

	public static DbLambda Create(DbExpression body, IEnumerable<DbVariableReferenceExpression> variables)
	{
		return DbExpressionBuilder.Lambda(body, variables);
	}

	public static DbLambda Create(DbExpression body, params DbVariableReferenceExpression[] variables)
	{
		return DbExpressionBuilder.Lambda(body, variables);
	}

	public static DbLambda Create(TypeUsage argument1Type, Func<DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, Func<DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, Func<DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(argument5Type, "argument5Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3], array[4]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type, TypeUsage argument6Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(argument5Type, "argument5Type");
		Check.NotNull(argument6Type, "argument6Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3], array[4], array[5]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type, TypeUsage argument6Type, TypeUsage argument7Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(argument5Type, "argument5Type");
		Check.NotNull(argument6Type, "argument6Type");
		Check.NotNull(argument7Type, "argument7Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type, argument7Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3], array[4], array[5], array[6]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type, TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(argument5Type, "argument5Type");
		Check.NotNull(argument6Type, "argument6Type");
		Check.NotNull(argument7Type, "argument7Type");
		Check.NotNull(argument8Type, "argument8Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type, argument7Type, argument8Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type, TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(argument5Type, "argument5Type");
		Check.NotNull(argument6Type, "argument6Type");
		Check.NotNull(argument7Type, "argument7Type");
		Check.NotNull(argument8Type, "argument8Type");
		Check.NotNull(argument9Type, "argument9Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type, argument7Type, argument8Type, argument9Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7], array[8]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type, TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(argument5Type, "argument5Type");
		Check.NotNull(argument6Type, "argument6Type");
		Check.NotNull(argument7Type, "argument7Type");
		Check.NotNull(argument8Type, "argument8Type");
		Check.NotNull(argument9Type, "argument9Type");
		Check.NotNull(argument10Type, "argument10Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type, argument7Type, argument8Type, argument9Type, argument10Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7], array[8], array[9]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type, TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type, TypeUsage argument11Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(argument5Type, "argument5Type");
		Check.NotNull(argument6Type, "argument6Type");
		Check.NotNull(argument7Type, "argument7Type");
		Check.NotNull(argument8Type, "argument8Type");
		Check.NotNull(argument9Type, "argument9Type");
		Check.NotNull(argument10Type, "argument10Type");
		Check.NotNull(argument11Type, "argument11Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type, argument7Type, argument8Type, argument9Type, argument10Type, argument11Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7], array[8], array[9], array[10]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type, TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type, TypeUsage argument11Type, TypeUsage argument12Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(argument5Type, "argument5Type");
		Check.NotNull(argument6Type, "argument6Type");
		Check.NotNull(argument7Type, "argument7Type");
		Check.NotNull(argument8Type, "argument8Type");
		Check.NotNull(argument9Type, "argument9Type");
		Check.NotNull(argument10Type, "argument10Type");
		Check.NotNull(argument11Type, "argument11Type");
		Check.NotNull(argument12Type, "argument12Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type, argument7Type, argument8Type, argument9Type, argument10Type, argument11Type, argument12Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7], array[8], array[9], array[10], array[11]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type, TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type, TypeUsage argument11Type, TypeUsage argument12Type, TypeUsage argument13Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(argument5Type, "argument5Type");
		Check.NotNull(argument6Type, "argument6Type");
		Check.NotNull(argument7Type, "argument7Type");
		Check.NotNull(argument8Type, "argument8Type");
		Check.NotNull(argument9Type, "argument9Type");
		Check.NotNull(argument10Type, "argument10Type");
		Check.NotNull(argument11Type, "argument11Type");
		Check.NotNull(argument12Type, "argument12Type");
		Check.NotNull(argument13Type, "argument13Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type, argument7Type, argument8Type, argument9Type, argument10Type, argument11Type, argument12Type, argument13Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7], array[8], array[9], array[10], array[11], array[12]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type, TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type, TypeUsage argument11Type, TypeUsage argument12Type, TypeUsage argument13Type, TypeUsage argument14Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(argument5Type, "argument5Type");
		Check.NotNull(argument6Type, "argument6Type");
		Check.NotNull(argument7Type, "argument7Type");
		Check.NotNull(argument8Type, "argument8Type");
		Check.NotNull(argument9Type, "argument9Type");
		Check.NotNull(argument10Type, "argument10Type");
		Check.NotNull(argument11Type, "argument11Type");
		Check.NotNull(argument12Type, "argument12Type");
		Check.NotNull(argument13Type, "argument13Type");
		Check.NotNull(argument14Type, "argument14Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type, argument7Type, argument8Type, argument9Type, argument10Type, argument11Type, argument12Type, argument13Type, argument14Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7], array[8], array[9], array[10], array[11], array[12], array[13]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type, TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type, TypeUsage argument11Type, TypeUsage argument12Type, TypeUsage argument13Type, TypeUsage argument14Type, TypeUsage argument15Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(argument5Type, "argument5Type");
		Check.NotNull(argument6Type, "argument6Type");
		Check.NotNull(argument7Type, "argument7Type");
		Check.NotNull(argument8Type, "argument8Type");
		Check.NotNull(argument9Type, "argument9Type");
		Check.NotNull(argument10Type, "argument10Type");
		Check.NotNull(argument11Type, "argument11Type");
		Check.NotNull(argument12Type, "argument12Type");
		Check.NotNull(argument13Type, "argument13Type");
		Check.NotNull(argument14Type, "argument14Type");
		Check.NotNull(argument15Type, "argument15Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type, argument7Type, argument8Type, argument9Type, argument10Type, argument11Type, argument12Type, argument13Type, argument14Type, argument15Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7], array[8], array[9], array[10], array[11], array[12], array[13], array[14]), array);
	}

	public static DbLambda Create(TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type, TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type, TypeUsage argument11Type, TypeUsage argument12Type, TypeUsage argument13Type, TypeUsage argument14Type, TypeUsage argument15Type, TypeUsage argument16Type, Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
	{
		Check.NotNull(argument1Type, "argument1Type");
		Check.NotNull(argument2Type, "argument2Type");
		Check.NotNull(argument3Type, "argument3Type");
		Check.NotNull(argument4Type, "argument4Type");
		Check.NotNull(argument5Type, "argument5Type");
		Check.NotNull(argument6Type, "argument6Type");
		Check.NotNull(argument7Type, "argument7Type");
		Check.NotNull(argument8Type, "argument8Type");
		Check.NotNull(argument9Type, "argument9Type");
		Check.NotNull(argument10Type, "argument10Type");
		Check.NotNull(argument11Type, "argument11Type");
		Check.NotNull(argument12Type, "argument12Type");
		Check.NotNull(argument13Type, "argument13Type");
		Check.NotNull(argument14Type, "argument14Type");
		Check.NotNull(argument15Type, "argument15Type");
		Check.NotNull(argument16Type, "argument16Type");
		Check.NotNull(lambdaFunction, "lambdaFunction");
		DbVariableReferenceExpression[] array = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type, argument7Type, argument8Type, argument9Type, argument10Type, argument11Type, argument12Type, argument13Type, argument14Type, argument15Type, argument16Type);
		return DbExpressionBuilder.Lambda(lambdaFunction(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7], array[8], array[9], array[10], array[11], array[12], array[13], array[14], array[15]), array);
	}

	private static DbVariableReferenceExpression[] CreateVariables(MethodInfo lambdaMethod, params TypeUsage[] argumentTypes)
	{
		string[] array = DbExpressionBuilder.ExtractAliases(lambdaMethod);
		DbVariableReferenceExpression[] array2 = new DbVariableReferenceExpression[argumentTypes.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = argumentTypes[i].Variable(array[i]);
		}
		return array2;
	}
}
