using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.EntitySql;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Resources;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Data.Entity.Core.Objects.ELinq;

internal sealed class ExpressionConverter
{
	private class ParameterReferenceRemover : DefaultExpressionVisitor
	{
		private readonly ObjectParameterCollection objectParameters;

		internal static DbExpression RemoveParameterReferences(DbExpression expression, ObjectParameterCollection availableParameters)
		{
			return new ParameterReferenceRemover(availableParameters).VisitExpression(expression);
		}

		private ParameterReferenceRemover(ObjectParameterCollection availableParams)
		{
			objectParameters = availableParams;
		}

		public override DbExpression Visit(DbParameterReferenceExpression expression)
		{
			Check.NotNull(expression, "expression");
			if (objectParameters.Contains(expression.ParameterName))
			{
				ObjectParameter objectParameter = objectParameters[expression.ParameterName];
				if (objectParameter.Value == null)
				{
					return expression.ResultType.Null();
				}
				return expression.ResultType.Constant(objectParameter.Value);
			}
			return expression;
		}
	}

	private enum EqualsPattern
	{
		Store,
		PositiveNullEqualityNonComposable,
		PositiveNullEqualityComposable
	}

	internal sealed class MethodCallTranslator : TypedTranslator<MethodCallExpression>
	{
		internal sealed class HierarchyIdMethodCallTranslator : CallTranslator
		{
			private static readonly Dictionary<MethodInfo, string> _methodFunctionRenames = GetRenamedMethodFunctions();

			internal HierarchyIdMethodCallTranslator()
				: base(GetSupportedMethods())
			{
			}

			private static MethodInfo GetStaticMethod<TResult>(Expression<Func<TResult>> lambda)
			{
				return ((MethodCallExpression)lambda.Body).Method;
			}

			private static MethodInfo GetInstanceMethod<T, TResult>(Expression<Func<T, TResult>> lambda)
			{
				return ((MethodCallExpression)lambda.Body).Method;
			}

			private static IEnumerable<MethodInfo> GetSupportedMethods()
			{
				yield return GetStaticMethod(() => HierarchyId.GetRoot());
				yield return GetStaticMethod(() => HierarchyId.Parse(null));
				yield return GetInstanceMethod((HierarchyId h) => h.GetAncestor(0));
				yield return GetInstanceMethod((HierarchyId h) => h.GetDescendant(null, null));
				yield return GetInstanceMethod((HierarchyId h) => h.GetLevel());
				yield return GetInstanceMethod((HierarchyId h) => h.IsDescendantOf(null));
				yield return GetInstanceMethod((HierarchyId h) => h.GetReparentedValue(null, null));
			}

			private static Dictionary<MethodInfo, string> GetRenamedMethodFunctions()
			{
				Dictionary<MethodInfo, string> dictionary = new Dictionary<MethodInfo, string>();
				dictionary.Add(GetStaticMethod(() => HierarchyId.GetRoot()), "HierarchyIdGetRoot");
				dictionary.Add(GetStaticMethod(() => HierarchyId.Parse(null)), "HierarchyIdParse");
				dictionary.Add(GetInstanceMethod((HierarchyId h) => h.GetAncestor(0)), "GetAncestor");
				dictionary.Add(GetInstanceMethod((HierarchyId h) => h.GetDescendant(null, null)), "GetDescendant");
				dictionary.Add(GetInstanceMethod((HierarchyId h) => h.GetLevel()), "GetLevel");
				dictionary.Add(GetInstanceMethod((HierarchyId h) => h.IsDescendantOf(null)), "IsDescendantOf");
				dictionary.Add(GetInstanceMethod((HierarchyId h) => h.GetReparentedValue(null, null)), "GetReparentedValue");
				return dictionary;
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				MethodInfo method = call.Method;
				if (!_methodFunctionRenames.TryGetValue(method, out var value))
				{
					value = method.Name;
				}
				return parent.TranslateIntoCanonicalFunction(linqArguments: (!method.IsStatic) ? new Expression[1] { call.Object }.Concat(call.Arguments).ToArray() : call.Arguments.ToArray(), functionName: value, Expression: call);
			}
		}

		internal abstract class CallTranslator
		{
			private readonly IEnumerable<MethodInfo> _methods;

			internal IEnumerable<MethodInfo> Methods => _methods;

			protected CallTranslator(params MethodInfo[] methods)
			{
				_methods = methods;
			}

			protected CallTranslator(IEnumerable<MethodInfo> methods)
			{
				_methods = methods;
			}

			internal abstract DbExpression Translate(ExpressionConverter parent, MethodCallExpression call);

			public override string ToString()
			{
				return GetType().Name;
			}
		}

		private abstract class ObjectQueryCallTranslator : CallTranslator
		{
			private readonly string _methodName;

			internal string MethodName => _methodName;

			internal static bool IsCandidateMethod(MethodInfo method)
			{
				Type declaringType = method.DeclaringType;
				if ((method.IsPublic || (method.IsAssembly && (method.Name == "MergeAs" || method.Name == "IncludeSpan"))) && null != declaringType && declaringType.IsGenericType())
				{
					return typeof(ObjectQuery<>) == declaringType.GetGenericTypeDefinition();
				}
				return false;
			}

			internal static Expression RemoveConvertToObjectQuery(Expression queryExpression)
			{
				if (queryExpression.NodeType == ExpressionType.Convert)
				{
					UnaryExpression unaryExpression = (UnaryExpression)queryExpression;
					Type type = unaryExpression.Operand.Type;
					if (type.IsGenericType() && (typeof(IQueryable<>) == type.GetGenericTypeDefinition() || typeof(IOrderedQueryable<>) == type.GetGenericTypeDefinition()))
					{
						queryExpression = unaryExpression.Operand;
					}
				}
				return queryExpression;
			}

			protected ObjectQueryCallTranslator(string methodName)
			{
				_methodName = methodName;
			}
		}

		private abstract class ObjectQueryBuilderCallTranslator : ObjectQueryCallTranslator
		{
			private readonly SequenceMethodTranslator _translator;

			protected ObjectQueryBuilderCallTranslator(string methodName, SequenceMethod sequenceEquivalent)
				: base(methodName)
			{
				_sequenceTranslators.TryGetValue(sequenceEquivalent, out _translator);
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return _translator.Translate(parent, call);
			}
		}

		private sealed class ObjectQueryBuilderUnionTranslator : ObjectQueryBuilderCallTranslator
		{
			internal ObjectQueryBuilderUnionTranslator()
				: base("Union", SequenceMethod.Union)
			{
			}
		}

		private sealed class ObjectQueryBuilderIntersectTranslator : ObjectQueryBuilderCallTranslator
		{
			internal ObjectQueryBuilderIntersectTranslator()
				: base("Intersect", SequenceMethod.Intersect)
			{
			}
		}

		private sealed class ObjectQueryBuilderExceptTranslator : ObjectQueryBuilderCallTranslator
		{
			internal ObjectQueryBuilderExceptTranslator()
				: base("Except", SequenceMethod.Except)
			{
			}
		}

		private sealed class ObjectQueryBuilderDistinctTranslator : ObjectQueryBuilderCallTranslator
		{
			internal ObjectQueryBuilderDistinctTranslator()
				: base("Distinct", SequenceMethod.Distinct)
			{
			}
		}

		private sealed class ObjectQueryBuilderOfTypeTranslator : ObjectQueryBuilderCallTranslator
		{
			internal ObjectQueryBuilderOfTypeTranslator()
				: base("OfType", SequenceMethod.OfType)
			{
			}
		}

		private sealed class ObjectQueryBuilderFirstTranslator : ObjectQueryBuilderCallTranslator
		{
			internal ObjectQueryBuilderFirstTranslator()
				: base("First", SequenceMethod.First)
			{
			}
		}

		private sealed class ObjectQueryBuilderToListTranslator : ObjectQueryBuilderCallTranslator
		{
			internal ObjectQueryBuilderToListTranslator()
				: base("ToList", SequenceMethod.ToList)
			{
			}
		}

		private sealed class ObjectQueryIncludeTranslator : ObjectQueryCallTranslator
		{
			internal ObjectQueryIncludeTranslator()
				: base("Include")
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression expression = parent.TranslateExpression(call.Object);
				if (!parent.TryGetSpan(expression, out var span))
				{
					span = null;
				}
				DbExpression dbExpression = parent.TranslateExpression(call.Arguments[0]);
				string text = null;
				if (dbExpression.ExpressionKind == DbExpressionKind.Constant)
				{
					text = (string)((DbConstantExpression)dbExpression).Value;
					if (parent.CanIncludeSpanInfo())
					{
						span = Span.IncludeIn(span, text);
					}
					return parent.AddSpanMapping(expression, span);
				}
				throw new NotSupportedException(Strings.ELinq_UnsupportedInclude);
			}
		}

		private sealed class ObjectQueryMergeAsTranslator : ObjectQueryCallTranslator
		{
			internal ObjectQueryMergeAsTranslator()
				: base("MergeAs")
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				if (call.Arguments[0].NodeType != ExpressionType.Constant)
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedMergeAs);
				}
				MergeOption mergeOption = (MergeOption)((ConstantExpression)call.Arguments[0]).Value;
				EntityUtil.CheckArgumentMergeOption(mergeOption);
				parent.NotifyMergeOption(mergeOption);
				Expression linq = ObjectQueryCallTranslator.RemoveConvertToObjectQuery(call.Object);
				DbExpression expression = parent.TranslateExpression(linq);
				if (!parent.TryGetSpan(expression, out var span))
				{
					span = null;
				}
				return parent.AddSpanMapping(expression, span);
			}
		}

		private sealed class ObjectQueryIncludeSpanTranslator : ObjectQueryCallTranslator
		{
			internal ObjectQueryIncludeSpanTranslator()
				: base("IncludeSpan")
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				Span span = (Span)((ConstantExpression)call.Arguments[0]).Value;
				Expression linq = ObjectQueryCallTranslator.RemoveConvertToObjectQuery(call.Object);
				DbExpression expression = parent.TranslateExpression(linq);
				if (!parent.CanIncludeSpanInfo())
				{
					span = null;
				}
				return parent.AddSpanMapping(expression, span);
			}
		}

		internal sealed class DefaultTranslator : CallTranslator
		{
			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				MethodInfo method = call.Method;
				if (method.DeclaringType.Assembly().FullName == "Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" && method.Name == "Mid" && new Type[2]
				{
					typeof(string),
					typeof(int)
				}.SequenceEqual(from p in method.GetParameters()
					select p.ParameterType))
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedMethodSuggestedAlternative(method, "System.String Mid(System.String, Int32, Int32)"));
				}
				throw new NotSupportedException(Strings.ELinq_UnsupportedMethod(method));
			}
		}

		private sealed class FunctionCallTranslator
		{
			internal DbExpression TranslateFunctionCall(ExpressionConverter parent, MethodCallExpression call, DbFunctionAttribute functionAttribute)
			{
				List<DbExpression> list = (from a in call.Arguments
					select UnwrapNoOpConverts(a) into b
					select NormalizeAllSetSources(parent, parent.TranslateExpression(b))).ToList();
				List<TypeUsage> argumentTypes = list.Select((DbExpression a) => a.ResultType).ToList();
				EdmFunction edmFunction = parent.FindFunction(functionAttribute.NamespaceName, functionAttribute.FunctionName, argumentTypes, isGroupAggregateFunction: false, call);
				if (!edmFunction.IsComposableAttribute)
				{
					throw new NotSupportedException(Strings.CannotCallNoncomposableFunction(edmFunction.FullName));
				}
				DbExpression dbExpression = edmFunction.Invoke(list);
				return ValidateReturnType(dbExpression, dbExpression.ResultType, parent, call, call.Type, isElementOfCollection: false);
			}

			private DbExpression NormalizeAllSetSources(ExpressionConverter parent, DbExpression argumentExpr)
			{
				DbExpression dbExpression = null;
				switch (argumentExpr.ResultType.EdmType.BuiltInTypeKind)
				{
				case BuiltInTypeKind.CollectionType:
				{
					DbExpressionBinding dbExpressionBinding = argumentExpr.BindAs(parent.AliasGenerator.Next());
					DbExpression dbExpression2 = NormalizeAllSetSources(parent, dbExpressionBinding.Variable);
					if (dbExpression2 != dbExpressionBinding.Variable)
					{
						dbExpression = dbExpressionBinding.Project(dbExpression2);
					}
					break;
				}
				case BuiltInTypeKind.RowType:
				{
					List<KeyValuePair<string, DbExpression>> list = new List<KeyValuePair<string, DbExpression>>();
					RowType obj = argumentExpr.ResultType.EdmType as RowType;
					bool flag = false;
					foreach (EdmProperty property in obj.Properties)
					{
						DbPropertyExpression dbPropertyExpression = argumentExpr.Property(property);
						dbExpression = NormalizeAllSetSources(parent, dbPropertyExpression);
						if (dbExpression != dbPropertyExpression)
						{
							flag = true;
							list.Add(new KeyValuePair<string, DbExpression>(dbPropertyExpression.Property.Name, dbExpression));
						}
						else
						{
							list.Add(new KeyValuePair<string, DbExpression>(dbPropertyExpression.Property.Name, dbPropertyExpression));
						}
					}
					dbExpression = ((!flag) ? argumentExpr : DbExpressionBuilder.NewRow(list));
					break;
				}
				}
				if (dbExpression != null && dbExpression != argumentExpr)
				{
					return parent.NormalizeSetSource(dbExpression);
				}
				return parent.NormalizeSetSource(argumentExpr);
			}

			private Expression UnwrapNoOpConverts(Expression expression)
			{
				if (expression.NodeType == ExpressionType.Convert)
				{
					UnaryExpression unaryExpression = (UnaryExpression)expression;
					Expression expression2 = UnwrapNoOpConverts(unaryExpression.Operand);
					if (expression.Type.IsAssignableFrom(expression2.Type))
					{
						return expression2;
					}
				}
				return expression;
			}

			private DbExpression ValidateReturnType(DbExpression result, TypeUsage actualReturnType, ExpressionConverter parent, MethodCallExpression call, Type clrReturnType, bool isElementOfCollection)
			{
				switch (actualReturnType.EdmType.BuiltInTypeKind)
				{
				case BuiltInTypeKind.CollectionType:
				{
					if (!clrReturnType.IsGenericType())
					{
						throw new NotSupportedException(Strings.ELinq_DbFunctionAttributedFunctionWithWrongReturnType(call.Method, call.Method.DeclaringType));
					}
					Type genericTypeDefinition = clrReturnType.GetGenericTypeDefinition();
					if (genericTypeDefinition != typeof(IEnumerable<>) && genericTypeDefinition != typeof(IQueryable<>))
					{
						throw new NotSupportedException(Strings.ELinq_DbFunctionAttributedFunctionWithWrongReturnType(call.Method, call.Method.DeclaringType));
					}
					Type clrReturnType2 = clrReturnType.GetGenericArguments()[0];
					result = ValidateReturnType(result, TypeHelpers.GetElementTypeUsage(actualReturnType), parent, call, clrReturnType2, isElementOfCollection: true);
					break;
				}
				case BuiltInTypeKind.RowType:
					if (clrReturnType != typeof(DbDataRecord))
					{
						throw new NotSupportedException(Strings.ELinq_DbFunctionAttributedFunctionWithWrongReturnType(call.Method, call.Method.DeclaringType));
					}
					break;
				case BuiltInTypeKind.RefType:
					if (clrReturnType != typeof(EntityKey))
					{
						throw new NotSupportedException(Strings.ELinq_DbFunctionAttributedFunctionWithWrongReturnType(call.Method, call.Method.DeclaringType));
					}
					break;
				default:
				{
					if (isElementOfCollection && parent.GetCastTargetType(actualReturnType, clrReturnType, null, preserveCastForDateTime: false) != null)
					{
						throw new NotSupportedException(Strings.ELinq_DbFunctionAttributedFunctionWithWrongReturnType(call.Method, call.Method.DeclaringType));
					}
					TypeUsage valueLayerType = parent.GetValueLayerType(clrReturnType);
					if (!TypeSemantics.IsPromotableTo(actualReturnType, valueLayerType))
					{
						throw new NotSupportedException(Strings.ELinq_DbFunctionAttributedFunctionWithWrongReturnType(call.Method, call.Method.DeclaringType));
					}
					if (!isElementOfCollection)
					{
						result = parent.AlignTypes(result, clrReturnType);
					}
					break;
				}
				}
				return result;
			}
		}

		internal sealed class CanonicalFunctionDefaultTranslator : CallTranslator
		{
			internal CanonicalFunctionDefaultTranslator()
				: base(GetMethods())
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				List<MethodInfo> list = new List<MethodInfo>();
				list.Add(typeof(Math).GetDeclaredMethod("Ceiling", typeof(decimal)));
				list.Add(typeof(Math).GetDeclaredMethod("Ceiling", typeof(double)));
				list.Add(typeof(Math).GetDeclaredMethod("Floor", typeof(decimal)));
				list.Add(typeof(Math).GetDeclaredMethod("Floor", typeof(double)));
				list.Add(typeof(Math).GetDeclaredMethod("Round", typeof(decimal)));
				list.Add(typeof(Math).GetDeclaredMethod("Round", typeof(double)));
				list.Add(typeof(Math).GetDeclaredMethod("Round", typeof(decimal), typeof(int)));
				list.Add(typeof(Math).GetDeclaredMethod("Round", typeof(double), typeof(int)));
				list.Add(typeof(decimal).GetDeclaredMethod("Floor", typeof(decimal)));
				list.Add(typeof(decimal).GetDeclaredMethod("Ceiling", typeof(decimal)));
				list.Add(typeof(decimal).GetDeclaredMethod("Round", typeof(decimal)));
				list.Add(typeof(decimal).GetDeclaredMethod("Round", typeof(decimal), typeof(int)));
				list.Add(typeof(string).GetDeclaredMethod("Replace", typeof(string), typeof(string)));
				list.Add(typeof(string).GetDeclaredMethod("ToLower"));
				list.Add(typeof(string).GetDeclaredMethod("ToUpper"));
				list.Add(typeof(string).GetDeclaredMethod("Trim"));
				List<MethodInfo> list2 = list;
				list2.AddRange(new Type[7]
				{
					typeof(decimal),
					typeof(double),
					typeof(float),
					typeof(int),
					typeof(long),
					typeof(sbyte),
					typeof(short)
				}.Select((Type a) => typeof(Math).GetDeclaredMethod("Abs", a)));
				return list2;
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				Expression[] linqArguments;
				if (!call.Method.IsStatic)
				{
					List<Expression> list = new List<Expression>(call.Arguments.Count + 1);
					list.Add(call.Object);
					list.AddRange(call.Arguments);
					linqArguments = list.ToArray();
				}
				else
				{
					linqArguments = call.Arguments.ToArray();
				}
				return parent.TranslateIntoCanonicalFunction(call.Method.Name, call, linqArguments);
			}
		}

		internal sealed class LikeFunctionTranslator : CallTranslator
		{
			internal LikeFunctionTranslator()
				: base(GetMethods())
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(DbFunctions).GetDeclaredMethod("Like", typeof(string), typeof(string));
				yield return typeof(DbFunctions).GetDeclaredMethod("Like", typeof(string), typeof(string), typeof(string));
				yield return typeof(EntityFunctions).GetDeclaredMethod("Like", typeof(string), typeof(string));
				yield return typeof(EntityFunctions).GetDeclaredMethod("Like", typeof(string), typeof(string), typeof(string));
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return parent.TranslateLike(call);
			}
		}

		internal abstract class AsUnicodeNonUnicodeBaseFunctionTranslator : CallTranslator
		{
			private readonly bool _isUnicode;

			protected AsUnicodeNonUnicodeBaseFunctionTranslator(IEnumerable<MethodInfo> methods, bool isUnicode)
				: base(methods)
			{
				_isUnicode = isUnicode;
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression dbExpression = parent.TranslateExpression(call.Arguments[0]);
				TypeUsage typeUsage = dbExpression.ResultType.ShallowCopy(new FacetValues
				{
					Unicode = _isUnicode
				});
				return dbExpression.ExpressionKind switch
				{
					DbExpressionKind.Constant => typeUsage.Constant(((DbConstantExpression)dbExpression).Value), 
					DbExpressionKind.ParameterReference => typeUsage.Parameter(((DbParameterReferenceExpression)dbExpression).ParameterName), 
					DbExpressionKind.Null => typeUsage.Null(), 
					_ => throw new NotSupportedException(Strings.ELinq_UnsupportedAsUnicodeAndAsNonUnicode(call.Method)), 
				};
			}
		}

		internal sealed class AsUnicodeFunctionTranslator : AsUnicodeNonUnicodeBaseFunctionTranslator
		{
			internal AsUnicodeFunctionTranslator()
				: base(GetMethods(), isUnicode: true)
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(DbFunctions).GetDeclaredMethod("AsUnicode", typeof(string));
				yield return typeof(EntityFunctions).GetDeclaredMethod("AsUnicode", typeof(string));
			}
		}

		internal sealed class AsNonUnicodeFunctionTranslator : AsUnicodeNonUnicodeBaseFunctionTranslator
		{
			internal AsNonUnicodeFunctionTranslator()
				: base(GetMethods(), isUnicode: false)
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(DbFunctions).GetDeclaredMethod("AsNonUnicode", typeof(string));
				yield return typeof(EntityFunctions).GetDeclaredMethod("AsNonUnicode", typeof(string));
			}
		}

		internal sealed class HasFlagTranslator : CallTranslator
		{
			private static readonly MethodInfo _hasFlagMethod = typeof(Enum).GetDeclaredMethod("HasFlag", typeof(Enum));

			internal HasFlagTranslator()
				: base(_hasFlagMethod)
			{
			}

			private static DbExpression TranslateHasFlag(ExpressionConverter parent, Expression sourceExpression, Expression valueExpression)
			{
				if (valueExpression.NodeType == ExpressionType.Constant && ((ConstantExpression)valueExpression).Value == null)
				{
					throw new ArgumentNullException("flag");
				}
				DbExpression dbExpression = parent.TranslateExpression(valueExpression);
				DbExpression dbExpression2 = parent.TranslateExpression(sourceExpression);
				if (dbExpression2.ResultType.EdmType != dbExpression.ResultType.EdmType)
				{
					throw new NotSupportedException(Strings.ELinq_HasFlagArgumentAndSourceTypeMismatch(dbExpression.ResultType.EdmType.Name, dbExpression2.ResultType.EdmType.Name));
				}
				TypeUsage toType = TypeHelpers.CreateEnumUnderlyingTypeUsage(dbExpression2.ResultType);
				DbCastExpression dbCastExpression = dbExpression.CastTo(toType);
				return dbExpression2.CastTo(toType).BitwiseAnd(dbCastExpression).Equal(dbCastExpression);
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return TranslateHasFlag(parent, call.Object, call.Arguments[0]);
			}
		}

		internal sealed class MathTruncateTranslator : CallTranslator
		{
			internal MathTruncateTranslator()
				: base(typeof(Math).GetDeclaredMethod("Truncate", typeof(decimal)), typeof(Math).GetDeclaredMethod("Truncate", typeof(double)))
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression value = parent.TranslateExpression(call.Arguments[0]);
				DbConstantExpression digits = DbExpressionBuilder.Constant(0);
				return value.Truncate(digits);
			}
		}

		internal sealed class MathPowerTranslator : CallTranslator
		{
			internal MathPowerTranslator()
				: base(typeof(Math).GetDeclaredMethod("Pow", typeof(double), typeof(double)))
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression baseArgument = parent.TranslateExpression(call.Arguments[0]);
				DbExpression exponent = parent.TranslateExpression(call.Arguments[1]);
				return baseArgument.Power(exponent);
			}
		}

		internal sealed class GuidNewGuidTranslator : CallTranslator
		{
			internal GuidNewGuidTranslator()
				: base(typeof(Guid).GetDeclaredMethod("NewGuid"))
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return EdmFunctions.NewGuid();
			}
		}

		internal sealed class StringContainsTranslator : CallTranslator
		{
			internal StringContainsTranslator()
				: base(GetMethods())
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(string).GetDeclaredMethod("Contains", typeof(string));
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return parent.TranslateFunctionIntoLike(call, insertPercentAtStart: true, insertPercentAtEnd: true, CreateDefaultTranslation);
			}

			private static DbExpression CreateDefaultTranslation(ExpressionConverter parent, MethodCallExpression call, DbExpression patternExpression, DbExpression inputExpression)
			{
				return parent.CreateCanonicalFunction("IndexOf", call, patternExpression, inputExpression).GreaterThan(DbExpressionBuilder.Constant(0));
			}
		}

		internal sealed class IndexOfTranslator : CallTranslator
		{
			internal IndexOfTranslator()
				: base(GetMethods())
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(string).GetDeclaredMethod("IndexOf", typeof(string));
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return parent.TranslateIntoCanonicalFunction("IndexOf", call, call.Arguments[0], call.Object).Minus(DbExpressionBuilder.Constant(1));
			}
		}

		internal sealed class StartsWithTranslator : CallTranslator
		{
			internal StartsWithTranslator()
				: base(GetMethods())
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(string).GetDeclaredMethod("StartsWith", typeof(string));
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return parent.TranslateFunctionIntoLike(call, insertPercentAtStart: false, insertPercentAtEnd: true, CreateDefaultTranslation);
			}

			private static DbExpression CreateDefaultTranslation(ExpressionConverter parent, MethodCallExpression call, DbExpression patternExpression, DbExpression inputExpression)
			{
				return parent.CreateCanonicalFunction("IndexOf", call, patternExpression, inputExpression).Equal(DbExpressionBuilder.Constant(1));
			}
		}

		internal sealed class EndsWithTranslator : CallTranslator
		{
			internal EndsWithTranslator()
				: base(GetMethods())
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(string).GetDeclaredMethod("EndsWith", typeof(string));
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return parent.TranslateFunctionIntoLike(call, insertPercentAtStart: true, insertPercentAtEnd: false, CreateDefaultTranslation);
			}

			private static DbExpression CreateDefaultTranslation(ExpressionConverter parent, MethodCallExpression call, DbExpression patternExpression, DbExpression inputExpression)
			{
				DbFunctionExpression dbFunctionExpression = parent.CreateCanonicalFunction("Reverse", call, patternExpression);
				DbFunctionExpression dbFunctionExpression2 = parent.CreateCanonicalFunction("Reverse", call, inputExpression);
				return parent.CreateCanonicalFunction("IndexOf", call, dbFunctionExpression, dbFunctionExpression2).Equal(DbExpressionBuilder.Constant(1));
			}
		}

		internal sealed class SubstringTranslator : CallTranslator
		{
			internal SubstringTranslator()
				: base(GetMethods())
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(string).GetDeclaredMethod("Substring", typeof(int));
				yield return typeof(string).GetDeclaredMethod("Substring", typeof(int), typeof(int));
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression dbExpression = parent.TranslateExpression(call.Arguments[0]);
				DbExpression dbExpression2 = parent.TranslateExpression(call.Object);
				DbExpression dbExpression3 = dbExpression.Plus(DbExpressionBuilder.Constant(1));
				DbExpression dbExpression4 = ((call.Arguments.Count != 1) ? parent.TranslateExpression(call.Arguments[1]) : parent.CreateCanonicalFunction("Length", call, dbExpression2).Minus(dbExpression));
				return parent.CreateCanonicalFunction("Substring", call, dbExpression2, dbExpression3, dbExpression4);
			}
		}

		internal sealed class RemoveTranslator : CallTranslator
		{
			internal RemoveTranslator()
				: base(GetMethods())
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(string).GetDeclaredMethod("Remove", typeof(int));
				yield return typeof(string).GetDeclaredMethod("Remove", typeof(int), typeof(int));
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression dbExpression = parent.TranslateExpression(call.Object);
				DbExpression dbExpression2 = parent.TranslateExpression(call.Arguments[0]);
				DbExpression dbExpression3 = parent.CreateCanonicalFunction("Substring", call, dbExpression, DbExpressionBuilder.Constant(1), dbExpression2);
				if (call.Arguments.Count == 2)
				{
					DbExpression dbExpression4 = parent.TranslateExpression(call.Arguments[1]);
					if (!IsNonNegativeIntegerConstant(dbExpression4))
					{
						throw new NotSupportedException(Strings.ELinq_UnsupportedStringRemoveCase(call.Method, call.Method.GetParameters()[1].Name));
					}
					DbExpression dbExpression5 = dbExpression2.Plus(dbExpression4).Plus(DbExpressionBuilder.Constant(1));
					DbExpression dbExpression6 = parent.CreateCanonicalFunction("Length", call, dbExpression).Minus(dbExpression2.Plus(dbExpression4));
					DbExpression dbExpression7 = parent.CreateCanonicalFunction("Substring", call, dbExpression, dbExpression5, dbExpression6);
					dbExpression3 = parent.CreateCanonicalFunction("Concat", call, dbExpression3, dbExpression7);
				}
				return dbExpression3;
			}

			private static bool IsNonNegativeIntegerConstant(DbExpression argument)
			{
				if (argument.ExpressionKind != DbExpressionKind.Constant || !TypeSemantics.IsPrimitiveType(argument.ResultType, PrimitiveTypeKind.Int32))
				{
					return false;
				}
				if ((int)((DbConstantExpression)argument).Value < 0)
				{
					return false;
				}
				return true;
			}
		}

		internal sealed class InsertTranslator : CallTranslator
		{
			internal InsertTranslator()
				: base(GetMethods())
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(string).GetDeclaredMethod("Insert", typeof(int), typeof(string));
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression dbExpression = parent.TranslateExpression(call.Object);
				DbExpression dbExpression2 = parent.TranslateExpression(call.Arguments[0]);
				DbExpression dbExpression3 = parent.CreateCanonicalFunction("Substring", call, dbExpression, DbExpressionBuilder.Constant(1), dbExpression2);
				DbExpression dbExpression4 = parent.CreateCanonicalFunction("Substring", call, dbExpression, dbExpression2.Plus(DbExpressionBuilder.Constant(1)), parent.CreateCanonicalFunction("Length", call, dbExpression).Minus(dbExpression2));
				DbExpression dbExpression5 = parent.TranslateExpression(call.Arguments[1]);
				return parent.CreateCanonicalFunction("Concat", call, parent.CreateCanonicalFunction("Concat", call, dbExpression3, dbExpression5), dbExpression4);
			}
		}

		internal sealed class IsNullOrEmptyTranslator : CallTranslator
		{
			internal IsNullOrEmptyTranslator()
				: base(GetMethods())
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(string).GetDeclaredMethod("IsNullOrEmpty", typeof(string));
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression dbExpression = parent.TranslateExpression(call.Arguments[0]);
				DbExpression left = dbExpression.IsNull();
				DbExpression right = parent.CreateCanonicalFunction("Length", call, dbExpression).Equal(DbExpressionBuilder.Constant(0));
				return left.Or(right);
			}
		}

		internal sealed class StringConcatTranslator : CallTranslator
		{
			internal StringConcatTranslator()
				: base(GetMethods())
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(string).GetDeclaredMethod("Concat", typeof(string), typeof(string));
				yield return typeof(string).GetDeclaredMethod("Concat", typeof(string), typeof(string), typeof(string));
				yield return typeof(string).GetDeclaredMethod("Concat", typeof(string), typeof(string), typeof(string), typeof(string));
				yield return typeof(string).GetDeclaredMethod("Concat", typeof(object), typeof(object));
				yield return typeof(string).GetDeclaredMethod("Concat", typeof(object), typeof(object), typeof(object));
				yield return typeof(string).GetDeclaredMethod("Concat", typeof(object), typeof(object), typeof(object), typeof(object));
				yield return typeof(string).GetDeclaredMethod("Concat", typeof(object[]));
				yield return typeof(string).GetDeclaredMethod("Concat", typeof(string[]));
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				Expression[] linqArgs;
				if (call.Arguments.Count == 1 && (call.Arguments.First().Type == typeof(object[]) || call.Arguments.First().Type == typeof(string[])))
				{
					if (call.Arguments[0] is NewArrayExpression)
					{
						linqArgs = ((NewArrayExpression)call.Arguments[0]).Expressions.ToArray();
					}
					else
					{
						ConstantExpression constantExpression = (ConstantExpression)call.Arguments[0];
						if (constantExpression.Value == null)
						{
							throw new ArgumentNullException((constantExpression.Type == typeof(object[])) ? "args" : "values");
						}
						Expression[] array = ((object[])constantExpression.Value).Select((object v) => Expression.Constant(v)).ToArray();
						linqArgs = array;
					}
				}
				else
				{
					linqArgs = call.Arguments.ToArray();
				}
				return StringTranslatorUtil.ConcatArgs(parent, call, linqArgs);
			}
		}

		internal sealed class ToStringTranslator : CallTranslator
		{
			private static readonly MethodInfo[] _methods = new MethodInfo[15]
			{
				typeof(string).GetDeclaredMethod("ToString"),
				typeof(byte).GetDeclaredMethod("ToString"),
				typeof(sbyte).GetDeclaredMethod("ToString"),
				typeof(short).GetDeclaredMethod("ToString"),
				typeof(int).GetDeclaredMethod("ToString"),
				typeof(long).GetDeclaredMethod("ToString"),
				typeof(double).GetDeclaredMethod("ToString"),
				typeof(float).GetDeclaredMethod("ToString"),
				typeof(Guid).GetDeclaredMethod("ToString"),
				typeof(DateTime).GetDeclaredMethod("ToString"),
				typeof(DateTimeOffset).GetDeclaredMethod("ToString"),
				typeof(TimeSpan).GetDeclaredMethod("ToString"),
				typeof(decimal).GetDeclaredMethod("ToString"),
				typeof(bool).GetDeclaredMethod("ToString"),
				typeof(object).GetDeclaredMethod("ToString")
			};

			internal ToStringTranslator()
				: base(_methods)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return StringTranslatorUtil.ConvertToString(parent, call.Object);
			}
		}

		internal abstract class TrimBaseTranslator : CallTranslator
		{
			private readonly string _canonicalFunctionName;

			protected TrimBaseTranslator(IEnumerable<MethodInfo> methods, string canonicalFunctionName)
				: base(methods)
			{
				_canonicalFunctionName = canonicalFunctionName;
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				if (call.Arguments.Count != 0 && !IsEmptyArray(call.Arguments[0]))
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedTrimStartTrimEndCase(call.Method));
				}
				return parent.TranslateIntoCanonicalFunction(_canonicalFunctionName, call, call.Object);
			}

			internal static bool IsEmptyArray(Expression expression)
			{
				NewArrayExpression newArrayExpression = expression as NewArrayExpression;
				if (expression.NodeType == ExpressionType.NewArrayInit)
				{
					if (newArrayExpression.Expressions.Count == 0)
					{
						return true;
					}
				}
				else if (expression.NodeType == ExpressionType.NewArrayBounds && newArrayExpression.Expressions.Count == 1 && newArrayExpression.Expressions[0].NodeType == ExpressionType.Constant)
				{
					return object.Equals(((ConstantExpression)newArrayExpression.Expressions[0]).Value, 0);
				}
				return false;
			}
		}

		internal sealed class TrimTranslator : TrimBaseTranslator
		{
			internal TrimTranslator()
				: base(GetMethods(), "Trim")
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(string).GetDeclaredMethod("Trim", typeof(char[]));
			}
		}

		internal sealed class TrimStartTranslator : TrimBaseTranslator
		{
			internal TrimStartTranslator()
				: base(GetMethods(), "LTrim")
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(string).GetDeclaredMethod("TrimStart", typeof(char[]));
			}
		}

		internal sealed class TrimEndTranslator : TrimBaseTranslator
		{
			internal TrimEndTranslator()
				: base(GetMethods(), "RTrim")
			{
			}

			private static IEnumerable<MethodInfo> GetMethods()
			{
				yield return typeof(string).GetDeclaredMethod("TrimEnd", typeof(char[]));
			}
		}

		internal sealed class VBCanonicalFunctionDefaultTranslator : CallTranslator
		{
			private const string s_stringsTypeFullName = "Microsoft.VisualBasic.Strings";

			private const string s_dateAndTimeTypeFullName = "Microsoft.VisualBasic.DateAndTime";

			internal VBCanonicalFunctionDefaultTranslator(Assembly vbAssembly)
				: base(GetMethods(vbAssembly))
			{
			}

			private static IEnumerable<MethodInfo> GetMethods(Assembly vbAssembly)
			{
				Type stringsType = vbAssembly.GetType("Microsoft.VisualBasic.Strings");
				yield return stringsType.GetDeclaredMethod("Trim", typeof(string));
				yield return stringsType.GetDeclaredMethod("LTrim", typeof(string));
				yield return stringsType.GetDeclaredMethod("RTrim", typeof(string));
				yield return stringsType.GetDeclaredMethod("Left", typeof(string), typeof(int));
				yield return stringsType.GetDeclaredMethod("Right", typeof(string), typeof(int));
				Type dateTimeType = vbAssembly.GetType("Microsoft.VisualBasic.DateAndTime");
				yield return dateTimeType.GetDeclaredMethod("Year", typeof(DateTime));
				yield return dateTimeType.GetDeclaredMethod("Month", typeof(DateTime));
				yield return dateTimeType.GetDeclaredMethod("Day", typeof(DateTime));
				yield return dateTimeType.GetDeclaredMethod("Hour", typeof(DateTime));
				yield return dateTimeType.GetDeclaredMethod("Minute", typeof(DateTime));
				yield return dateTimeType.GetDeclaredMethod("Second", typeof(DateTime));
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return parent.TranslateIntoCanonicalFunction(call.Method.Name, call, call.Arguments.ToArray());
			}
		}

		internal sealed class VBCanonicalFunctionRenameTranslator : CallTranslator
		{
			private const string s_stringsTypeFullName = "Microsoft.VisualBasic.Strings";

			private static readonly Dictionary<MethodInfo, string> s_methodNameMap = new Dictionary<MethodInfo, string>(4);

			internal VBCanonicalFunctionRenameTranslator(Assembly vbAssembly)
				: base(GetMethods(vbAssembly).ToArray())
			{
			}

			private static IEnumerable<MethodInfo> GetMethods(Assembly vbAssembly)
			{
				Type stringsType = vbAssembly.GetType("Microsoft.VisualBasic.Strings");
				yield return GetMethodInfo(stringsType, "Len", "Length", new Type[1] { typeof(string) });
				yield return GetMethodInfo(stringsType, "Mid", "Substring", new Type[3]
				{
					typeof(string),
					typeof(int),
					typeof(int)
				});
				yield return GetMethodInfo(stringsType, "UCase", "ToUpper", new Type[1] { typeof(string) });
				yield return GetMethodInfo(stringsType, "LCase", "ToLower", new Type[1] { typeof(string) });
			}

			private static MethodInfo GetMethodInfo(Type declaringType, string methodName, string canonicalFunctionName, Type[] argumentTypes)
			{
				MethodInfo declaredMethod = declaringType.GetDeclaredMethod(methodName, argumentTypes);
				s_methodNameMap.Add(declaredMethod, canonicalFunctionName);
				return declaredMethod;
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return parent.TranslateIntoCanonicalFunction(s_methodNameMap[call.Method], call, call.Arguments.ToArray());
			}
		}

		internal sealed class VBDatePartTranslator : CallTranslator
		{
			private const string s_dateAndTimeTypeFullName = "Microsoft.VisualBasic.DateAndTime";

			private const string s_DateIntervalFullName = "Microsoft.VisualBasic.DateInterval";

			private const string s_FirstDayOfWeekFullName = "Microsoft.VisualBasic.FirstDayOfWeek";

			private const string s_FirstWeekOfYearFullName = "Microsoft.VisualBasic.FirstWeekOfYear";

			private static readonly HashSet<string> _supportedIntervals = new HashSet<string> { "Year", "Month", "Day", "Hour", "Minute", "Second" };

			internal VBDatePartTranslator(Assembly vbAssembly)
				: base(GetMethods(vbAssembly))
			{
			}

			private static IEnumerable<MethodInfo> GetMethods(Assembly vbAssembly)
			{
				Type type = vbAssembly.GetType("Microsoft.VisualBasic.DateAndTime");
				Type type2 = vbAssembly.GetType("Microsoft.VisualBasic.DateInterval");
				Type type3 = vbAssembly.GetType("Microsoft.VisualBasic.FirstDayOfWeek");
				Type type4 = vbAssembly.GetType("Microsoft.VisualBasic.FirstWeekOfYear");
				yield return type.GetDeclaredMethod("DatePart", type2, typeof(DateTime), type3, type4);
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				string text = ((call.Arguments[0] as ConstantExpression) ?? throw new NotSupportedException(Strings.ELinq_UnsupportedVBDatePartNonConstantInterval(call.Method, call.Method.GetParameters()[0].Name))).Value.ToString();
				if (!_supportedIntervals.Contains(text))
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedVBDatePartInvalidInterval(call.Method, call.Method.GetParameters()[0].Name, text));
				}
				return parent.TranslateIntoCanonicalFunction(text, call, call.Arguments[1]);
			}
		}

		private abstract class SequenceMethodTranslator
		{
			private readonly IEnumerable<SequenceMethod> _methods;

			internal IEnumerable<SequenceMethod> Methods => _methods;

			protected SequenceMethodTranslator(params SequenceMethod[] methods)
			{
				_methods = methods;
			}

			internal virtual DbExpression Translate(ExpressionConverter parent, MethodCallExpression call, SequenceMethod sequenceMethod)
			{
				return Translate(parent, call);
			}

			internal abstract DbExpression Translate(ExpressionConverter parent, MethodCallExpression call);

			public override string ToString()
			{
				return GetType().Name;
			}
		}

		private abstract class PagingTranslator : UnarySequenceMethodTranslator
		{
			protected PagingTranslator(params SequenceMethod[] methods)
				: base(methods)
			{
			}

			protected override DbExpression TranslateUnary(ExpressionConverter parent, DbExpression operand, MethodCallExpression call)
			{
				Expression linq = call.Arguments[1];
				DbExpression count = parent.TranslateExpression(linq);
				return TranslatePagingOperator(parent, operand, count);
			}

			protected abstract DbExpression TranslatePagingOperator(ExpressionConverter parent, DbExpression operand, DbExpression count);
		}

		private sealed class TakeTranslator : PagingTranslator
		{
			internal TakeTranslator()
				: base(SequenceMethod.Take)
			{
			}

			protected override DbExpression TranslatePagingOperator(ExpressionConverter parent, DbExpression operand, DbExpression count)
			{
				if (count is DbConstantExpression dbConstantExpression && dbConstantExpression.Value.Equals(0))
				{
					return parent.Filter(operand.BindAs(parent.AliasGenerator.Next()), DbExpressionBuilder.False);
				}
				return parent.Limit(operand, count);
			}
		}

		private sealed class SkipTranslator : PagingTranslator
		{
			internal SkipTranslator()
				: base(SequenceMethod.Skip)
			{
			}

			protected override DbExpression TranslatePagingOperator(ExpressionConverter parent, DbExpression operand, DbExpression count)
			{
				return parent.Skip(operand.BindAs(parent.AliasGenerator.Next()), count);
			}
		}

		private sealed class JoinTranslator : SequenceMethodTranslator
		{
			internal JoinTranslator()
				: base(SequenceMethod.Join)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression input = parent.TranslateSet(call.Arguments[0]);
				DbExpression input2 = parent.TranslateSet(call.Arguments[1]);
				LambdaExpression lambdaExpression = parent.GetLambdaExpression(call, 2);
				LambdaExpression lambdaExpression2 = parent.GetLambdaExpression(call, 3);
				LambdaExpression lambdaExpression3 = parent.GetLambdaExpression(call, 4);
				string leftName;
				string rightName;
				InitializerMetadata initializerMetadata;
				bool num = IsTrivialRename(lambdaExpression3, parent, out leftName, out rightName, out initializerMetadata);
				DbExpressionBinding binding;
				DbExpression dbExpression = (num ? parent.TranslateLambda(lambdaExpression, input, leftName, out binding) : parent.TranslateLambda(lambdaExpression, input, out binding));
				DbExpressionBinding binding2;
				DbExpression dbExpression2 = (num ? parent.TranslateLambda(lambdaExpression2, input2, rightName, out binding2) : parent.TranslateLambda(lambdaExpression2, input2, out binding2));
				if (!TypeSemantics.IsEqualComparable(dbExpression.ResultType) || !TypeSemantics.IsEqualComparable(dbExpression2.ResultType))
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedKeySelector(call.Method.Name));
				}
				DbExpression dbExpression3 = parent.CreateEqualsExpression(dbExpression, dbExpression2, EqualsPattern.PositiveNullEqualityNonComposable, lambdaExpression.Body.Type, lambdaExpression2.Body.Type);
				if (num)
				{
					TypeUsage elementType = TypeUsage.Create(TypeHelpers.CreateRowType(new List<KeyValuePair<string, TypeUsage>>
					{
						new KeyValuePair<string, TypeUsage>(binding.VariableName, binding.VariableType),
						new KeyValuePair<string, TypeUsage>(binding2.VariableName, binding2.VariableType)
					}, initializerMetadata));
					return new DbJoinExpression(DbExpressionKind.InnerJoin, TypeUsage.Create(TypeHelpers.CreateCollectionType(elementType)), binding, binding2, dbExpression3);
				}
				DbExpressionBinding dbExpressionBinding = binding.InnerJoin(binding2, dbExpression3).BindAs(parent.AliasGenerator.Next());
				DbPropertyExpression cqtExpression = dbExpressionBinding.Variable.Property(binding.VariableName);
				DbPropertyExpression cqtExpression2 = dbExpressionBinding.Variable.Property(binding2.VariableName);
				parent._bindingContext.PushBindingScope(new Binding(lambdaExpression3.Parameters[0], cqtExpression));
				parent._bindingContext.PushBindingScope(new Binding(lambdaExpression3.Parameters[1], cqtExpression2));
				DbExpression projection = parent.TranslateExpression(lambdaExpression3.Body);
				parent._bindingContext.PopBindingScope();
				parent._bindingContext.PopBindingScope();
				return dbExpressionBinding.Project(projection);
			}
		}

		private abstract class BinarySequenceMethodTranslator : SequenceMethodTranslator
		{
			protected BinarySequenceMethodTranslator(params SequenceMethod[] methods)
				: base(methods)
			{
			}

			private static DbExpression TranslateLeft(ExpressionConverter parent, Expression expr)
			{
				return parent.TranslateSet(expr);
			}

			protected virtual DbExpression TranslateRight(ExpressionConverter parent, Expression expr)
			{
				return parent.TranslateSet(expr);
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				if (call.Object != null)
				{
					DbExpression left = TranslateLeft(parent, call.Object);
					DbExpression right = TranslateRight(parent, call.Arguments[0]);
					return TranslateBinary(parent, left, right);
				}
				DbExpression left2 = TranslateLeft(parent, call.Arguments[0]);
				DbExpression right2 = TranslateRight(parent, call.Arguments[1]);
				return TranslateBinary(parent, left2, right2);
			}

			protected abstract DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right);
		}

		private class ConcatTranslator : BinarySequenceMethodTranslator
		{
			internal ConcatTranslator()
				: base(SequenceMethod.Concat)
			{
			}

			protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right)
			{
				return parent.UnionAll(left, right);
			}
		}

		private sealed class UnionTranslator : BinarySequenceMethodTranslator
		{
			internal UnionTranslator()
				: base(SequenceMethod.Union)
			{
			}

			protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right)
			{
				return parent.Distinct(parent.UnionAll(left, right));
			}
		}

		private sealed class IntersectTranslator : BinarySequenceMethodTranslator
		{
			internal IntersectTranslator()
				: base(SequenceMethod.Intersect)
			{
			}

			protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right)
			{
				return parent.Intersect(left, right);
			}
		}

		private sealed class ExceptTranslator : BinarySequenceMethodTranslator
		{
			internal ExceptTranslator()
				: base(SequenceMethod.Except)
			{
			}

			protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right)
			{
				return parent.Except(left, right);
			}

			protected override DbExpression TranslateRight(ExpressionConverter parent, Expression expr)
			{
				parent.IgnoreInclude++;
				DbExpression result = base.TranslateRight(parent, expr);
				parent.IgnoreInclude--;
				return result;
			}
		}

		private abstract class AggregateTranslator : SequenceMethodTranslator
		{
			private readonly string _functionName;

			private readonly bool _takesPredicate;

			protected AggregateTranslator(string functionName, bool takesPredicate, params SequenceMethod[] methods)
				: base(methods)
			{
				_takesPredicate = takesPredicate;
				_functionName = functionName;
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				bool num = 1 == call.Arguments.Count;
				DbExpression dbExpression = parent.TranslateSet(call.Arguments[0]);
				if (!num)
				{
					LambdaExpression lambdaExpression = parent.GetLambdaExpression(call, 1);
					DbExpressionBinding binding;
					DbExpression dbExpression2 = parent.TranslateLambda(lambdaExpression, dbExpression, out binding);
					dbExpression = ((!_takesPredicate) ? binding.Project(dbExpression2) : parent.Filter(binding, dbExpression2));
				}
				TypeUsage returnType = GetReturnType(parent, call);
				EdmFunction function = FindFunction(parent, call, returnType);
				dbExpression = WrapCollectionOperand(parent, dbExpression, returnType);
				DbExpression cqt = function.Invoke(new List<DbExpression>(1) { dbExpression });
				return parent.AlignTypes(cqt, call.Type);
			}

			protected virtual TypeUsage GetReturnType(ExpressionConverter parent, MethodCallExpression call)
			{
				return parent.GetValueLayerType(call.Type);
			}

			protected virtual DbExpression WrapCollectionOperand(ExpressionConverter parent, DbExpression operand, TypeUsage returnType)
			{
				if (!TypeUsageEquals(returnType, ((CollectionType)operand.ResultType.EdmType).TypeUsage))
				{
					DbExpressionBinding dbExpressionBinding = operand.BindAs(parent.AliasGenerator.Next());
					operand = dbExpressionBinding.Project(dbExpressionBinding.Variable.CastTo(returnType));
				}
				return operand;
			}

			protected virtual DbExpression WrapNonCollectionOperand(ExpressionConverter parent, DbExpression operand, TypeUsage returnType)
			{
				if (!TypeUsageEquals(returnType, operand.ResultType))
				{
					operand = operand.CastTo(returnType);
				}
				return operand;
			}

			protected virtual EdmFunction FindFunction(ExpressionConverter parent, MethodCallExpression call, TypeUsage argumentType)
			{
				List<TypeUsage> list = new List<TypeUsage>(1);
				list.Add(argumentType);
				return parent.FindCanonicalFunction(_functionName, list, isGroupAggregateFunction: true, call);
			}
		}

		private sealed class MaxTranslator : AggregateTranslator
		{
			internal MaxTranslator()
				: base("Max", false, SequenceMethod.Max, SequenceMethod.MaxSelector, SequenceMethod.MaxInt, SequenceMethod.MaxIntSelector, SequenceMethod.MaxDecimal, SequenceMethod.MaxDecimalSelector, SequenceMethod.MaxDouble, SequenceMethod.MaxDoubleSelector, SequenceMethod.MaxLong, SequenceMethod.MaxLongSelector, SequenceMethod.MaxSingle, SequenceMethod.MaxSingleSelector, SequenceMethod.MaxNullableDecimal, SequenceMethod.MaxNullableDecimalSelector, SequenceMethod.MaxNullableDouble, SequenceMethod.MaxNullableDoubleSelector, SequenceMethod.MaxNullableInt, SequenceMethod.MaxNullableIntSelector, SequenceMethod.MaxNullableLong, SequenceMethod.MaxNullableLongSelector, SequenceMethod.MaxNullableSingle, SequenceMethod.MaxNullableSingleSelector)
			{
			}

			protected override TypeUsage GetReturnType(ExpressionConverter parent, MethodCallExpression call)
			{
				TypeUsage returnType = base.GetReturnType(parent, call);
				if (!TypeSemantics.IsEnumerationType(returnType))
				{
					return returnType;
				}
				return TypeUsage.Create(Helper.GetUnderlyingEdmTypeForEnumType(returnType.EdmType), returnType.Facets);
			}
		}

		private sealed class MinTranslator : AggregateTranslator
		{
			internal MinTranslator()
				: base("Min", false, SequenceMethod.Min, SequenceMethod.MinSelector, SequenceMethod.MinDecimal, SequenceMethod.MinDecimalSelector, SequenceMethod.MinDouble, SequenceMethod.MinDoubleSelector, SequenceMethod.MinInt, SequenceMethod.MinIntSelector, SequenceMethod.MinLong, SequenceMethod.MinLongSelector, SequenceMethod.MinNullableDecimal, SequenceMethod.MinSingle, SequenceMethod.MinSingleSelector, SequenceMethod.MinNullableDecimalSelector, SequenceMethod.MinNullableDouble, SequenceMethod.MinNullableDoubleSelector, SequenceMethod.MinNullableInt, SequenceMethod.MinNullableIntSelector, SequenceMethod.MinNullableLong, SequenceMethod.MinNullableLongSelector, SequenceMethod.MinNullableSingle, SequenceMethod.MinNullableSingleSelector)
			{
			}

			protected override TypeUsage GetReturnType(ExpressionConverter parent, MethodCallExpression call)
			{
				TypeUsage returnType = base.GetReturnType(parent, call);
				if (!TypeSemantics.IsEnumerationType(returnType))
				{
					return returnType;
				}
				return TypeUsage.Create(Helper.GetUnderlyingEdmTypeForEnumType(returnType.EdmType), returnType.Facets);
			}
		}

		private sealed class AverageTranslator : AggregateTranslator
		{
			internal AverageTranslator()
				: base("Avg", false, SequenceMethod.AverageDecimal, SequenceMethod.AverageDecimalSelector, SequenceMethod.AverageDouble, SequenceMethod.AverageDoubleSelector, SequenceMethod.AverageInt, SequenceMethod.AverageIntSelector, SequenceMethod.AverageLong, SequenceMethod.AverageLongSelector, SequenceMethod.AverageSingle, SequenceMethod.AverageSingleSelector, SequenceMethod.AverageNullableDecimal, SequenceMethod.AverageNullableDecimalSelector, SequenceMethod.AverageNullableDouble, SequenceMethod.AverageNullableDoubleSelector, SequenceMethod.AverageNullableInt, SequenceMethod.AverageNullableIntSelector, SequenceMethod.AverageNullableLong, SequenceMethod.AverageNullableLongSelector, SequenceMethod.AverageNullableSingle, SequenceMethod.AverageNullableSingleSelector)
			{
			}
		}

		private sealed class SumTranslator : AggregateTranslator
		{
			internal SumTranslator()
				: base("Sum", false, SequenceMethod.SumDecimal, SequenceMethod.SumDecimalSelector, SequenceMethod.SumDouble, SequenceMethod.SumDoubleSelector, SequenceMethod.SumInt, SequenceMethod.SumIntSelector, SequenceMethod.SumLong, SequenceMethod.SumLongSelector, SequenceMethod.SumSingle, SequenceMethod.SumSingleSelector, SequenceMethod.SumNullableDecimal, SequenceMethod.SumNullableDecimalSelector, SequenceMethod.SumNullableDouble, SequenceMethod.SumNullableDoubleSelector, SequenceMethod.SumNullableInt, SequenceMethod.SumNullableIntSelector, SequenceMethod.SumNullableLong, SequenceMethod.SumNullableLongSelector, SequenceMethod.SumNullableSingle, SequenceMethod.SumNullableSingleSelector)
			{
			}
		}

		private abstract class CountTranslatorBase : AggregateTranslator
		{
			protected CountTranslatorBase(string functionName, params SequenceMethod[] methods)
				: base(functionName, takesPredicate: true, methods)
			{
			}

			protected override DbExpression WrapCollectionOperand(ExpressionConverter parent, DbExpression operand, TypeUsage returnType)
			{
				return operand.BindAs(parent.AliasGenerator.Next()).Project(DbExpressionBuilder.Constant(1));
			}

			protected override DbExpression WrapNonCollectionOperand(ExpressionConverter parent, DbExpression operand, TypeUsage returnType)
			{
				DbExpression dbExpression = DbExpressionBuilder.Constant(1);
				if (!TypeUsageEquals(dbExpression.ResultType, returnType))
				{
					dbExpression = dbExpression.CastTo(returnType);
				}
				return dbExpression;
			}

			protected override EdmFunction FindFunction(ExpressionConverter parent, MethodCallExpression call, TypeUsage argumentType)
			{
				TypeUsage argumentType2 = TypeUsage.CreateDefaultTypeUsage(EdmProviderManifest.Instance.GetPrimitiveType(PrimitiveTypeKind.Int32));
				return base.FindFunction(parent, call, argumentType2);
			}
		}

		private sealed class CountTranslator : CountTranslatorBase
		{
			internal CountTranslator()
				: base("Count", SequenceMethod.Count, SequenceMethod.CountPredicate)
			{
			}
		}

		private sealed class LongCountTranslator : CountTranslatorBase
		{
			internal LongCountTranslator()
				: base("BigCount", SequenceMethod.LongCount, SequenceMethod.LongCountPredicate)
			{
			}
		}

		private abstract class UnarySequenceMethodTranslator : SequenceMethodTranslator
		{
			protected UnarySequenceMethodTranslator(params SequenceMethod[] methods)
				: base(methods)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				if (call.Object != null)
				{
					DbExpression operand = parent.TranslateSet(call.Object);
					return TranslateUnary(parent, operand, call);
				}
				DbExpression operand2 = parent.TranslateSet(call.Arguments[0]);
				return TranslateUnary(parent, operand2, call);
			}

			protected abstract DbExpression TranslateUnary(ExpressionConverter parent, DbExpression operand, MethodCallExpression call);
		}

		private sealed class PassthroughTranslator : UnarySequenceMethodTranslator
		{
			internal PassthroughTranslator()
				: base(SequenceMethod.AsQueryableGeneric, SequenceMethod.AsQueryable, SequenceMethod.AsEnumerable, SequenceMethod.ToList)
			{
			}

			protected override DbExpression TranslateUnary(ExpressionConverter parent, DbExpression operand, MethodCallExpression call)
			{
				if (TypeSemantics.IsCollectionType(operand.ResultType))
				{
					return operand;
				}
				throw new NotSupportedException(Strings.ELinq_UnsupportedPassthrough(call.Method.Name, operand.ResultType.EdmType.Name));
			}
		}

		private sealed class OfTypeTranslator : UnarySequenceMethodTranslator
		{
			internal OfTypeTranslator()
				: base(SequenceMethod.OfType)
			{
			}

			protected override DbExpression TranslateUnary(ExpressionConverter parent, DbExpression operand, MethodCallExpression call)
			{
				Type type = call.Method.GetGenericArguments()[0];
				if (!parent.TryGetValueLayerType(type, out var type2) || (!TypeSemantics.IsEntityType(type2) && !TypeSemantics.IsComplexType(type2)))
				{
					throw new NotSupportedException(Strings.ELinq_InvalidOfTypeResult(DescribeClrType(type)));
				}
				return parent.OfType(operand, type2);
			}
		}

		private sealed class DistinctTranslator : UnarySequenceMethodTranslator
		{
			internal DistinctTranslator()
				: base(SequenceMethod.Distinct)
			{
			}

			protected override DbExpression TranslateUnary(ExpressionConverter parent, DbExpression operand, MethodCallExpression call)
			{
				return parent.Distinct(operand);
			}
		}

		private sealed class AnyTranslator : UnarySequenceMethodTranslator
		{
			internal AnyTranslator()
				: base(SequenceMethod.Any)
			{
			}

			protected override DbExpression TranslateUnary(ExpressionConverter parent, DbExpression operand, MethodCallExpression call)
			{
				return operand.IsEmpty().Not();
			}
		}

		private abstract class OneLambdaTranslator : SequenceMethodTranslator
		{
			internal OneLambdaTranslator(params SequenceMethod[] methods)
				: base(methods)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression source;
				DbExpressionBinding sourceBinding;
				DbExpression lambda;
				return Translate(parent, call, out source, out sourceBinding, out lambda);
			}

			protected DbExpression Translate(ExpressionConverter parent, MethodCallExpression call, out DbExpression source, out DbExpressionBinding sourceBinding, out DbExpression lambda)
			{
				source = parent.TranslateExpression(call.Arguments[0]);
				LambdaExpression lambdaExpression = parent.GetLambdaExpression(call, 1);
				lambda = parent.TranslateLambda(lambdaExpression, source, out sourceBinding);
				return TranslateOneLambda(parent, sourceBinding, lambda);
			}

			protected abstract DbExpression TranslateOneLambda(ExpressionConverter parent, DbExpressionBinding sourceBinding, DbExpression lambda);
		}

		private sealed class AnyPredicateTranslator : OneLambdaTranslator
		{
			internal AnyPredicateTranslator()
				: base(SequenceMethod.AnyPredicate)
			{
			}

			protected override DbExpression TranslateOneLambda(ExpressionConverter parent, DbExpressionBinding sourceBinding, DbExpression lambda)
			{
				return sourceBinding.Any(lambda);
			}
		}

		private sealed class AllTranslator : OneLambdaTranslator
		{
			internal AllTranslator()
				: base(SequenceMethod.All)
			{
			}

			protected override DbExpression TranslateOneLambda(ExpressionConverter parent, DbExpressionBinding sourceBinding, DbExpression lambda)
			{
				return sourceBinding.All(lambda);
			}
		}

		private sealed class WhereTranslator : OneLambdaTranslator
		{
			internal WhereTranslator()
				: base(default(SequenceMethod))
			{
			}

			protected override DbExpression TranslateOneLambda(ExpressionConverter parent, DbExpressionBinding sourceBinding, DbExpression lambda)
			{
				return parent.Filter(sourceBinding, lambda);
			}
		}

		private sealed class SelectTranslator : OneLambdaTranslator
		{
			internal SelectTranslator()
				: base(SequenceMethod.Select)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression source;
				DbExpressionBinding sourceBinding;
				DbExpression lambda;
				return Translate(parent, call, out source, out sourceBinding, out lambda);
			}

			protected override DbExpression TranslateOneLambda(ExpressionConverter parent, DbExpressionBinding sourceBinding, DbExpression lambda)
			{
				return parent.Project(sourceBinding, lambda);
			}
		}

		private sealed class DefaultIfEmptyTranslator : SequenceMethodTranslator
		{
			internal DefaultIfEmptyTranslator()
				: base(SequenceMethod.DefaultIfEmpty, SequenceMethod.DefaultIfEmptyValue)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression dbExpression = parent.TranslateSet(call.Arguments[0]);
				DbExpression dbExpression2 = ((call.Arguments.Count == 2) ? parent.TranslateExpression(call.Arguments[1]) : GetDefaultValue(parent, call.Type));
				DbExpressionBinding left = DbExpressionBuilder.NewCollection((byte)1).BindAs(parent.AliasGenerator.Next());
				bool flag = dbExpression2 != null && dbExpression2.ExpressionKind != DbExpressionKind.Null;
				if (flag)
				{
					DbExpressionBinding dbExpressionBinding = dbExpression.BindAs(parent.AliasGenerator.Next());
					dbExpression = dbExpressionBinding.Project(new Row(DbExpressionBuilder.As((byte)1, "sentinel"), dbExpressionBinding.Variable.As("value")));
				}
				DbExpressionBinding dbExpressionBinding2 = dbExpression.BindAs(parent.AliasGenerator.Next());
				DbExpressionBinding dbExpressionBinding3 = left.LeftOuterJoin(dbExpressionBinding2, true).BindAs(parent.AliasGenerator.Next());
				DbExpression dbExpression3 = dbExpressionBinding3.Variable.Property(dbExpressionBinding2.VariableName);
				if (flag)
				{
					dbExpression3 = DbExpressionBuilder.Case(new DbIsNullExpression[1] { dbExpression3.Property("sentinel").IsNull() }, new DbExpression[1] { dbExpression2 }, dbExpression3.Property("value"));
				}
				DbExpression dbExpression4 = dbExpressionBinding3.Project(dbExpression3);
				parent.ApplySpanMapping(dbExpression, dbExpression4);
				return dbExpression4;
			}

			private static DbExpression GetDefaultValue(ExpressionConverter parent, Type resultType)
			{
				Type elementType = TypeSystem.GetElementType(resultType);
				object defaultValue = TypeSystem.GetDefaultValue(elementType);
				if (defaultValue != null)
				{
					return parent.TranslateExpression(Expression.Constant(defaultValue, elementType));
				}
				return null;
			}
		}

		private sealed class ContainsTranslator : SequenceMethodTranslator
		{
			internal ContainsTranslator()
				: base(SequenceMethod.Contains)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return TranslateContains(parent, call.Arguments[0], call.Arguments[1]);
			}

			private static DbExpression TranslateContainsHelper(ExpressionConverter parent, DbExpression left, IEnumerable<DbExpression> rightList, EqualsPattern pattern, Type leftType, Type rightType)
			{
				return Helpers.BuildBalancedTreeInPlace(new List<DbExpression>(rightList.Select((DbExpression argument) => parent.CreateEqualsExpression(left, argument, pattern, leftType, rightType))), (DbExpression prev, DbExpression next) => prev.Or(next));
			}

			internal static DbExpression TranslateContains(ExpressionConverter parent, Expression sourceExpression, Expression valueExpression)
			{
				DbExpression dbExpression = parent.NormalizeSetSource(parent.TranslateExpression(sourceExpression));
				DbExpression dbExpression2 = parent.TranslateExpression(valueExpression);
				Type elementType = TypeSystem.GetElementType(sourceExpression.Type);
				if (dbExpression.ExpressionKind == DbExpressionKind.NewInstance)
				{
					IList<DbExpression> arguments = ((DbNewInstanceExpression)dbExpression).Arguments;
					if (arguments.Count > 0)
					{
						bool useCSharpNullComparisonBehavior = parent._funcletizer.RootContext.ContextOptions.UseCSharpNullComparisonBehavior;
						bool flag = parent.ProviderManifest.SupportsInExpression();
						if (!useCSharpNullComparisonBehavior && !flag)
						{
							return TranslateContainsHelper(parent, dbExpression2, arguments, EqualsPattern.Store, elementType, valueExpression.Type);
						}
						List<DbExpression> list = new List<DbExpression>();
						List<DbExpression> list2 = new List<DbExpression>();
						foreach (DbExpression item in arguments)
						{
							((item.ExpressionKind == DbExpressionKind.Constant) ? list : list2).Add(item);
						}
						DbExpression dbExpression3 = null;
						if (list.Count > 0)
						{
							EqualsPattern pattern = (useCSharpNullComparisonBehavior ? EqualsPattern.PositiveNullEqualityNonComposable : EqualsPattern.Store);
							dbExpression3 = (flag ? DbExpressionBuilder.CreateInExpression(dbExpression2, list) : TranslateContainsHelper(parent, dbExpression2, list, pattern, elementType, valueExpression.Type));
							if (useCSharpNullComparisonBehavior)
							{
								dbExpression3 = dbExpression3.And(dbExpression2.IsNull().Not());
							}
						}
						DbExpression dbExpression4 = null;
						if (list2.Count > 0)
						{
							EqualsPattern pattern2 = (useCSharpNullComparisonBehavior ? EqualsPattern.PositiveNullEqualityComposable : EqualsPattern.Store);
							dbExpression4 = TranslateContainsHelper(parent, dbExpression2, list2, pattern2, elementType, valueExpression.Type);
						}
						if (dbExpression3 == null)
						{
							return dbExpression4;
						}
						if (dbExpression4 == null)
						{
							return dbExpression3;
						}
						return dbExpression3.Or(dbExpression4);
					}
					return false;
				}
				DbExpressionBinding dbExpressionBinding = dbExpression.BindAs(parent.AliasGenerator.Next());
				EqualsPattern pattern3 = EqualsPattern.Store;
				if (parent._funcletizer.RootContext.ContextOptions.UseCSharpNullComparisonBehavior)
				{
					pattern3 = EqualsPattern.PositiveNullEqualityComposable;
				}
				return dbExpressionBinding.Filter(parent.CreateEqualsExpression(dbExpressionBinding.Variable, dbExpression2, pattern3, elementType, valueExpression.Type)).Exists();
			}
		}

		private abstract class FirstTranslatorBase : UnarySequenceMethodTranslator
		{
			protected FirstTranslatorBase(params SequenceMethod[] methods)
				: base(methods)
			{
			}

			protected virtual DbExpression LimitResult(ExpressionConverter parent, DbExpression expression)
			{
				return parent.Limit(expression, DbExpressionBuilder.Constant(1));
			}

			protected override DbExpression TranslateUnary(ExpressionConverter parent, DbExpression operand, MethodCallExpression call)
			{
				DbExpression dbExpression = LimitResult(parent, operand);
				if (!parent.IsQueryRoot(call))
				{
					dbExpression = dbExpression.Element();
					dbExpression = AddDefaultCase(dbExpression, call.Type);
				}
				Span span = null;
				if (parent.TryGetSpan(operand, out span))
				{
					parent.AddSpanMapping(dbExpression, span);
				}
				return dbExpression;
			}

			internal static DbExpression AddDefaultCase(DbExpression element, Type elementType)
			{
				object defaultValue = TypeSystem.GetDefaultValue(elementType);
				if (defaultValue == null)
				{
					return element;
				}
				return DbExpressionBuilder.Case(new List<DbExpression>(1) { CreateIsNullExpression(element, elementType) }, new List<DbExpression>(1) { element.ResultType.Constant(defaultValue) }, element);
			}
		}

		private sealed class FirstTranslator : FirstTranslatorBase
		{
			internal FirstTranslator()
				: base(SequenceMethod.First)
			{
			}

			protected override DbExpression TranslateUnary(ExpressionConverter parent, DbExpression operand, MethodCallExpression call)
			{
				if (!parent.IsQueryRoot(call))
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedNestedFirst);
				}
				return base.TranslateUnary(parent, operand, call);
			}
		}

		private sealed class FirstOrDefaultTranslator : FirstTranslatorBase
		{
			internal FirstOrDefaultTranslator()
				: base(SequenceMethod.FirstOrDefault)
			{
			}
		}

		private abstract class SingleTranslatorBase : FirstTranslatorBase
		{
			protected SingleTranslatorBase(params SequenceMethod[] methods)
				: base(methods)
			{
			}

			protected override DbExpression TranslateUnary(ExpressionConverter parent, DbExpression operand, MethodCallExpression call)
			{
				if (!parent.IsQueryRoot(call))
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedNestedSingle);
				}
				return base.TranslateUnary(parent, operand, call);
			}

			protected override DbExpression LimitResult(ExpressionConverter parent, DbExpression expression)
			{
				return parent.Limit(expression, DbExpressionBuilder.Constant(2));
			}
		}

		private sealed class SingleTranslator : SingleTranslatorBase
		{
			internal SingleTranslator()
				: base(SequenceMethod.Single)
			{
			}
		}

		private sealed class SingleOrDefaultTranslator : SingleTranslatorBase
		{
			internal SingleOrDefaultTranslator()
				: base(SequenceMethod.SingleOrDefault)
			{
			}
		}

		private abstract class FirstPredicateTranslatorBase : OneLambdaTranslator
		{
			protected FirstPredicateTranslatorBase(params SequenceMethod[] methods)
				: base(methods)
			{
			}

			protected virtual DbExpression RestrictResult(ExpressionConverter parent, DbExpression expression)
			{
				return parent.Limit(expression, DbExpressionBuilder.Constant(1));
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression expression = base.Translate(parent, call);
				if (parent.IsQueryRoot(call))
				{
					return RestrictResult(parent, expression);
				}
				expression = RestrictResult(parent, expression);
				DbExpression element = expression.Element();
				element = FirstTranslatorBase.AddDefaultCase(element, call.Type);
				Span span = null;
				if (parent.TryGetSpan(expression, out span))
				{
					parent.AddSpanMapping(element, span);
				}
				return element;
			}

			protected override DbExpression TranslateOneLambda(ExpressionConverter parent, DbExpressionBinding sourceBinding, DbExpression lambda)
			{
				return parent.Filter(sourceBinding, lambda);
			}
		}

		private sealed class FirstPredicateTranslator : FirstPredicateTranslatorBase
		{
			internal FirstPredicateTranslator()
				: base(SequenceMethod.FirstPredicate)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				if (!parent.IsQueryRoot(call))
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedNestedFirst);
				}
				return base.Translate(parent, call);
			}
		}

		private sealed class FirstOrDefaultPredicateTranslator : FirstPredicateTranslatorBase
		{
			internal FirstOrDefaultPredicateTranslator()
				: base(SequenceMethod.FirstOrDefaultPredicate)
			{
			}
		}

		private abstract class SinglePredicateTranslatorBase : FirstPredicateTranslatorBase
		{
			protected SinglePredicateTranslatorBase(params SequenceMethod[] methods)
				: base(methods)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				if (!parent.IsQueryRoot(call))
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedNestedSingle);
				}
				return base.Translate(parent, call);
			}

			protected override DbExpression RestrictResult(ExpressionConverter parent, DbExpression expression)
			{
				return parent.Limit(expression, DbExpressionBuilder.Constant(2));
			}
		}

		private sealed class SinglePredicateTranslator : SinglePredicateTranslatorBase
		{
			internal SinglePredicateTranslator()
				: base(SequenceMethod.SinglePredicate)
			{
			}
		}

		private sealed class SingleOrDefaultPredicateTranslator : SinglePredicateTranslatorBase
		{
			internal SingleOrDefaultPredicateTranslator()
				: base(SequenceMethod.SingleOrDefaultPredicate)
			{
			}
		}

		private sealed class SelectManyTranslator : OneLambdaTranslator
		{
			internal SelectManyTranslator()
				: base(SequenceMethod.SelectMany, SequenceMethod.SelectManyResultSelector)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				LambdaExpression lambdaExpression = ((call.Arguments.Count == 3) ? parent.GetLambdaExpression(call, 2) : null);
				DbExpression dbExpression = base.Translate(parent, call);
				if (IsLeftOuterJoin(dbExpression, out var crossApplyInput, out var lojRightInput))
				{
					if (lambdaExpression != null && IsTrivialRename(lambdaExpression, parent, out var leftName, out var rightName, out var initializerMetadata))
					{
						DbExpressionBinding dbExpressionBinding = crossApplyInput.Expression.BindAs(leftName);
						DbExpressionBinding dbExpressionBinding2 = dbExpressionBinding.Variable.Property(lojRightInput.Name).BindAs(rightName);
						TypeUsage elementType = TypeUsage.Create(TypeHelpers.CreateRowType(new List<KeyValuePair<string, TypeUsage>>
						{
							new KeyValuePair<string, TypeUsage>(dbExpressionBinding.VariableName, dbExpressionBinding.VariableType),
							new KeyValuePair<string, TypeUsage>(dbExpressionBinding2.VariableName, dbExpressionBinding2.VariableType)
						}, initializerMetadata));
						return new DbApplyExpression(DbExpressionKind.OuterApply, TypeUsage.Create(TypeHelpers.CreateCollectionType(elementType)), dbExpressionBinding, dbExpressionBinding2);
					}
					dbExpression = crossApplyInput.OuterApply(crossApplyInput.Variable.Property(lojRightInput).BindAs(parent.AliasGenerator.Next()));
				}
				DbExpressionBinding dbExpressionBinding3 = dbExpression.BindAs(parent.AliasGenerator.Next());
				RowType rowType = (RowType)dbExpressionBinding3.Variable.ResultType.EdmType;
				DbExpression dbExpression2 = dbExpressionBinding3.Variable.Property(rowType.Properties[1]);
				DbExpression projection;
				if (lambdaExpression != null)
				{
					DbExpression cqtExpression = dbExpressionBinding3.Variable.Property(rowType.Properties[0]);
					parent._bindingContext.PushBindingScope(new Binding(lambdaExpression.Parameters[0], cqtExpression));
					parent._bindingContext.PushBindingScope(new Binding(lambdaExpression.Parameters[1], dbExpression2));
					projection = parent.TranslateSet(lambdaExpression.Body);
					parent._bindingContext.PopBindingScope();
					parent._bindingContext.PopBindingScope();
				}
				else
				{
					projection = dbExpression2;
				}
				return dbExpressionBinding3.Project(projection);
			}

			private static bool IsLeftOuterJoin(DbExpression cqtExpression, out DbExpressionBinding crossApplyInput, out EdmProperty lojRightInput)
			{
				crossApplyInput = null;
				lojRightInput = null;
				if (cqtExpression.ExpressionKind != DbExpressionKind.CrossApply)
				{
					return false;
				}
				DbApplyExpression dbApplyExpression = (DbApplyExpression)cqtExpression;
				if (dbApplyExpression.Input.VariableType.EdmType.BuiltInTypeKind != BuiltInTypeKind.RowType)
				{
					return false;
				}
				RowType rowType = (RowType)dbApplyExpression.Input.VariableType.EdmType;
				if (dbApplyExpression.Apply.Expression.ExpressionKind != DbExpressionKind.Project)
				{
					return false;
				}
				DbProjectExpression dbProjectExpression = (DbProjectExpression)dbApplyExpression.Apply.Expression;
				if (dbProjectExpression.Input.Expression.ExpressionKind != DbExpressionKind.LeftOuterJoin)
				{
					return false;
				}
				DbJoinExpression dbJoinExpression = (DbJoinExpression)dbProjectExpression.Input.Expression;
				if (dbProjectExpression.Projection.ExpressionKind != DbExpressionKind.Property)
				{
					return false;
				}
				DbPropertyExpression dbPropertyExpression = (DbPropertyExpression)dbProjectExpression.Projection;
				if (dbPropertyExpression.Instance != dbProjectExpression.Input.Variable || dbPropertyExpression.Property.Name != dbJoinExpression.Right.VariableName || dbJoinExpression.JoinCondition.ExpressionKind != DbExpressionKind.Constant)
				{
					return false;
				}
				DbConstantExpression dbConstantExpression = (DbConstantExpression)dbJoinExpression.JoinCondition;
				if (!(dbConstantExpression.Value is bool) || !(bool)dbConstantExpression.Value)
				{
					return false;
				}
				if (dbJoinExpression.Left.Expression.ExpressionKind != DbExpressionKind.NewInstance)
				{
					return false;
				}
				DbNewInstanceExpression dbNewInstanceExpression = (DbNewInstanceExpression)dbJoinExpression.Left.Expression;
				if (dbNewInstanceExpression.Arguments.Count != 1 || dbNewInstanceExpression.Arguments[0].ExpressionKind != DbExpressionKind.Constant)
				{
					return false;
				}
				if (dbJoinExpression.Right.Expression.ExpressionKind != DbExpressionKind.Property)
				{
					return false;
				}
				DbPropertyExpression lojRight = (DbPropertyExpression)dbJoinExpression.Right.Expression;
				if (lojRight.Instance != dbApplyExpression.Input.Variable)
				{
					return false;
				}
				EdmProperty edmProperty = rowType.Properties.SingleOrDefault((EdmProperty p) => p.Name == lojRight.Property.Name);
				if (edmProperty == null)
				{
					return false;
				}
				crossApplyInput = dbApplyExpression.Input;
				lojRightInput = edmProperty;
				return true;
			}

			protected override DbExpression TranslateOneLambda(ExpressionConverter parent, DbExpressionBinding sourceBinding, DbExpression lambda)
			{
				lambda = parent.NormalizeSetSource(lambda);
				DbExpressionBinding apply = lambda.BindAs(parent.AliasGenerator.Next());
				return sourceBinding.CrossApply(apply);
			}
		}

		private sealed class CastMethodTranslator : SequenceMethodTranslator
		{
			internal CastMethodTranslator()
				: base(SequenceMethod.Cast)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression input = parent.TranslateSet(call.Arguments[0]);
				Type elementType = TypeSystem.GetElementType(call.Type);
				Type elementType2 = TypeSystem.GetElementType(call.Arguments[0].Type);
				DbExpressionBinding dbExpressionBinding = input.BindAs(parent.AliasGenerator.Next());
				DbExpression projection = parent.CreateCastExpression(dbExpressionBinding.Variable, elementType, elementType2);
				return parent.Project(dbExpressionBinding, projection);
			}
		}

		private sealed class GroupByTranslator : SequenceMethodTranslator
		{
			internal GroupByTranslator()
				: base(SequenceMethod.GroupBy, SequenceMethod.GroupByElementSelector, SequenceMethod.GroupByElementSelectorResultSelector, SequenceMethod.GroupByResultSelector)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call, SequenceMethod sequenceMethod)
			{
				DbExpression input = parent.TranslateSet(call.Arguments[0]);
				LambdaExpression lambdaExpression = parent.GetLambdaExpression(call, 1);
				DbGroupExpressionBinding binding;
				DbExpression dbExpression = parent.TranslateLambda(lambdaExpression, input, out binding);
				if (!TypeSemantics.IsEqualComparable(dbExpression.ResultType))
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedKeySelector(call.Method.Name));
				}
				List<KeyValuePair<string, DbExpression>> list = new List<KeyValuePair<string, DbExpression>>();
				List<KeyValuePair<string, DbAggregate>> list2 = new List<KeyValuePair<string, DbAggregate>>();
				list.Add(new KeyValuePair<string, DbExpression>("Key", dbExpression));
				list2.Add(new KeyValuePair<string, DbAggregate>("Group", binding.GroupAggregate));
				DbExpressionBinding dbExpressionBinding = binding.GroupBy(list, list2).BindAs(parent.AliasGenerator.Next());
				DbExpression dbExpression2 = dbExpressionBinding.Variable.Property("Group");
				if (sequenceMethod == SequenceMethod.GroupByElementSelector || sequenceMethod == SequenceMethod.GroupByElementSelectorResultSelector)
				{
					LambdaExpression lambdaExpression2 = parent.GetLambdaExpression(call, 2);
					DbExpressionBinding binding2;
					DbExpression projection = parent.TranslateLambda(lambdaExpression2, dbExpression2, out binding2);
					dbExpression2 = binding2.Project(projection);
				}
				DbExpression[] array = new DbExpression[2]
				{
					dbExpressionBinding.Variable.Property("Key"),
					dbExpression2
				};
				List<EdmProperty> properties = new List<EdmProperty>(2)
				{
					new EdmProperty("Key", array[0].ResultType),
					new EdmProperty("Group", array[1].ResultType)
				};
				InitializerMetadata initializerMetadata = InitializerMetadata.CreateGroupingInitializer(parent.EdmItemCollection, TypeSystem.GetElementType(call.Type));
				TypeUsage instanceType = TypeUsage.Create(new RowType(properties, initializerMetadata));
				DbExpression dbExpression3 = dbExpressionBinding.Project(instanceType.New(array));
				DbExpression result = dbExpression3;
				return ProcessResultSelector(parent, call, sequenceMethod, dbExpression3, result);
			}

			private static DbExpression ProcessResultSelector(ExpressionConverter parent, MethodCallExpression call, SequenceMethod sequenceMethod, DbExpression topLevelProject, DbExpression result)
			{
				LambdaExpression lambdaExpression = null;
				switch (sequenceMethod)
				{
				case SequenceMethod.GroupByResultSelector:
					lambdaExpression = parent.GetLambdaExpression(call, 2);
					break;
				case SequenceMethod.GroupByElementSelectorResultSelector:
					lambdaExpression = parent.GetLambdaExpression(call, 3);
					break;
				}
				if (lambdaExpression != null)
				{
					DbExpressionBinding dbExpressionBinding = topLevelProject.BindAs(parent.AliasGenerator.Next());
					DbPropertyExpression cqtExpression = dbExpressionBinding.Variable.Property("Key");
					DbPropertyExpression cqtExpression2 = dbExpressionBinding.Variable.Property("Group");
					parent._bindingContext.PushBindingScope(new Binding(lambdaExpression.Parameters[0], cqtExpression));
					parent._bindingContext.PushBindingScope(new Binding(lambdaExpression.Parameters[1], cqtExpression2));
					DbExpression projection = parent.TranslateExpression(lambdaExpression.Body);
					result = dbExpressionBinding.Project(projection);
					parent._bindingContext.PopBindingScope();
					parent._bindingContext.PopBindingScope();
				}
				return result;
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				return null;
			}
		}

		private sealed class GroupJoinTranslator : SequenceMethodTranslator
		{
			internal GroupJoinTranslator()
				: base(SequenceMethod.GroupJoin)
			{
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression input = parent.TranslateSet(call.Arguments[0]);
				DbExpression input2 = parent.TranslateSet(call.Arguments[1]);
				LambdaExpression lambdaExpression = parent.GetLambdaExpression(call, 2);
				LambdaExpression lambdaExpression2 = parent.GetLambdaExpression(call, 3);
				DbExpressionBinding binding;
				DbExpression dbExpression = parent.TranslateLambda(lambdaExpression, input, out binding);
				DbExpressionBinding binding2;
				DbExpression dbExpression2 = parent.TranslateLambda(lambdaExpression2, input2, out binding2);
				if (!TypeSemantics.IsEqualComparable(dbExpression.ResultType) || !TypeSemantics.IsEqualComparable(dbExpression2.ResultType))
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedKeySelector(call.Method.Name));
				}
				DbExpression value = parent.Filter(binding2, parent.CreateEqualsExpression(dbExpression, dbExpression2, EqualsPattern.PositiveNullEqualityNonComposable, lambdaExpression.Body.Type, lambdaExpression2.Body.Type));
				DbExpression projection = DbExpressionBuilder.NewRow(new List<KeyValuePair<string, DbExpression>>(2)
				{
					new KeyValuePair<string, DbExpression>("o", binding.Variable),
					new KeyValuePair<string, DbExpression>("i", value)
				});
				DbExpressionBinding dbExpressionBinding = binding.Project(projection).BindAs(parent.AliasGenerator.Next());
				DbExpression cqtExpression = dbExpressionBinding.Variable.Property("o");
				DbExpression cqtExpression2 = dbExpressionBinding.Variable.Property("i");
				LambdaExpression lambdaExpression3 = parent.GetLambdaExpression(call, 4);
				parent._bindingContext.PushBindingScope(new Binding(lambdaExpression3.Parameters[0], cqtExpression));
				parent._bindingContext.PushBindingScope(new Binding(lambdaExpression3.Parameters[1], cqtExpression2));
				DbExpression projection2 = parent.TranslateExpression(lambdaExpression3.Body);
				parent._bindingContext.PopBindingScope();
				parent._bindingContext.PopBindingScope();
				return CollapseTrivialRenamingProjection(dbExpressionBinding.Project(projection2));
			}

			private static DbExpression CollapseTrivialRenamingProjection(DbExpression cqtExpression)
			{
				if (cqtExpression.ExpressionKind != DbExpressionKind.Project)
				{
					return cqtExpression;
				}
				DbProjectExpression dbProjectExpression = (DbProjectExpression)cqtExpression;
				if (dbProjectExpression.Projection.ExpressionKind != DbExpressionKind.NewInstance || dbProjectExpression.Projection.ResultType.EdmType.BuiltInTypeKind != BuiltInTypeKind.RowType)
				{
					return cqtExpression;
				}
				DbNewInstanceExpression dbNewInstanceExpression = (DbNewInstanceExpression)dbProjectExpression.Projection;
				RowType rowType = (RowType)dbNewInstanceExpression.ResultType.EdmType;
				List<Tuple<EdmProperty, string>> list = new List<Tuple<EdmProperty, string>>();
				for (int i = 0; i < dbNewInstanceExpression.Arguments.Count; i++)
				{
					if (dbNewInstanceExpression.Arguments[i].ExpressionKind != DbExpressionKind.Property)
					{
						return cqtExpression;
					}
					DbPropertyExpression dbPropertyExpression = (DbPropertyExpression)dbNewInstanceExpression.Arguments[i];
					if (dbPropertyExpression.Instance != dbProjectExpression.Input.Variable)
					{
						return cqtExpression;
					}
					list.Add(Tuple.Create((EdmProperty)dbPropertyExpression.Property, rowType.Properties[i].Name));
				}
				if (dbProjectExpression.Input.Expression.ExpressionKind != DbExpressionKind.Project)
				{
					return cqtExpression;
				}
				DbProjectExpression dbProjectExpression2 = (DbProjectExpression)dbProjectExpression.Input.Expression;
				if (dbProjectExpression2.Projection.ExpressionKind != DbExpressionKind.NewInstance || dbProjectExpression2.Projection.ResultType.EdmType.BuiltInTypeKind != BuiltInTypeKind.RowType)
				{
					return cqtExpression;
				}
				DbNewInstanceExpression dbNewInstanceExpression2 = (DbNewInstanceExpression)dbProjectExpression2.Projection;
				RowType rowType2 = (RowType)dbNewInstanceExpression2.ResultType.EdmType;
				List<DbExpression> list2 = new List<DbExpression>();
				foreach (Tuple<EdmProperty, string> item in list)
				{
					int index = rowType2.Properties.IndexOf(item.Item1);
					list2.Add(dbNewInstanceExpression2.Arguments[index]);
				}
				DbNewInstanceExpression projection = dbNewInstanceExpression.ResultType.New(list2);
				return dbProjectExpression2.Input.Project(projection);
			}
		}

		private abstract class OrderByTranslatorBase : OneLambdaTranslator
		{
			private readonly bool _ascending;

			protected OrderByTranslatorBase(bool ascending, params SequenceMethod[] methods)
				: base(methods)
			{
				_ascending = ascending;
			}

			protected override DbExpression TranslateOneLambda(ExpressionConverter parent, DbExpressionBinding sourceBinding, DbExpression lambda)
			{
				List<DbSortClause> list = new List<DbSortClause>(1);
				DbSortClause item = (_ascending ? lambda.ToSortClause() : lambda.ToSortClauseDescending());
				list.Add(item);
				return parent.Sort(sourceBinding, list);
			}
		}

		private sealed class OrderByTranslator : OrderByTranslatorBase
		{
			internal OrderByTranslator()
				: base(true, SequenceMethod.OrderBy)
			{
			}
		}

		private sealed class OrderByDescendingTranslator : OrderByTranslatorBase
		{
			internal OrderByDescendingTranslator()
				: base(false, SequenceMethod.OrderByDescending)
			{
			}
		}

		private abstract class ThenByTranslatorBase : SequenceMethodTranslator
		{
			private readonly bool _ascending;

			protected ThenByTranslatorBase(bool ascending, params SequenceMethod[] methods)
				: base(methods)
			{
				_ascending = ascending;
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				DbExpression dbExpression = parent.TranslateSet(call.Arguments[0]);
				if (DbExpressionKind.Sort != dbExpression.ExpressionKind)
				{
					throw new InvalidOperationException(Strings.ELinq_ThenByDoesNotFollowOrderBy);
				}
				DbSortExpression obj = (DbSortExpression)dbExpression;
				DbExpressionBinding input = obj.Input;
				LambdaExpression lambdaExpression = parent.GetLambdaExpression(call, 1);
				ParameterExpression linqExpression = lambdaExpression.Parameters[0];
				parent._bindingContext.PushBindingScope(new Binding(linqExpression, input.Variable));
				DbExpression key = parent.TranslateExpression(lambdaExpression.Body);
				parent._bindingContext.PopBindingScope();
				List<DbSortClause> list = new List<DbSortClause>(obj.SortOrder);
				list.Add(new DbSortClause(key, _ascending, null));
				return parent.Sort(input, list);
			}
		}

		private sealed class ThenByTranslator : ThenByTranslatorBase
		{
			internal ThenByTranslator()
				: base(true, SequenceMethod.ThenBy)
			{
			}
		}

		private sealed class ThenByDescendingTranslator : ThenByTranslatorBase
		{
			internal ThenByDescendingTranslator()
				: base(false, SequenceMethod.ThenByDescending)
			{
			}
		}

		private sealed class SpatialMethodCallTranslator : CallTranslator
		{
			private static readonly Dictionary<MethodInfo, string> _methodFunctionRenames = GetRenamedMethodFunctions();

			internal SpatialMethodCallTranslator()
				: base(GetSupportedMethods())
			{
			}

			private static MethodInfo GetStaticMethod<TResult>(Expression<Func<TResult>> lambda)
			{
				return ((MethodCallExpression)lambda.Body).Method;
			}

			private static MethodInfo GetInstanceMethod<T, TResult>(Expression<Func<T, TResult>> lambda)
			{
				return ((MethodCallExpression)lambda.Body).Method;
			}

			private static IEnumerable<MethodInfo> GetSupportedMethods()
			{
				yield return GetStaticMethod(() => DbGeography.FromText(null));
				yield return GetStaticMethod(() => DbGeography.FromText(null, 0));
				yield return GetStaticMethod(() => DbGeography.PointFromText(null, 0));
				yield return GetStaticMethod(() => DbGeography.LineFromText(null, 0));
				yield return GetStaticMethod(() => DbGeography.PolygonFromText(null, 0));
				yield return GetStaticMethod(() => DbGeography.MultiPointFromText(null, 0));
				yield return GetStaticMethod(() => DbGeography.MultiLineFromText(null, 0));
				yield return GetStaticMethod(() => DbGeography.MultiPolygonFromText(null, 0));
				yield return GetStaticMethod(() => DbGeography.GeographyCollectionFromText(null, 0));
				yield return GetStaticMethod(() => DbGeography.FromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeography.FromBinary(null));
				yield return GetStaticMethod(() => DbGeography.PointFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeography.LineFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeography.PolygonFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeography.MultiPointFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeography.MultiLineFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeography.MultiPolygonFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeography.GeographyCollectionFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeography.FromGml(null));
				yield return GetStaticMethod(() => DbGeography.FromGml(null, 0));
				yield return GetInstanceMethod((DbGeography geo) => geo.AsBinary());
				yield return GetInstanceMethod((DbGeography geo) => geo.AsGml());
				yield return GetInstanceMethod((DbGeography geo) => geo.AsText());
				yield return GetInstanceMethod((DbGeography geo) => geo.SpatialEquals(null));
				yield return GetInstanceMethod((DbGeography geo) => geo.Disjoint(null));
				yield return GetInstanceMethod((DbGeography geo) => geo.Intersects(null));
				yield return GetInstanceMethod((DbGeography geo) => geo.Buffer(0.0));
				yield return GetInstanceMethod((DbGeography geo) => geo.Distance(null));
				yield return GetInstanceMethod((DbGeography geo) => geo.Intersection(null));
				yield return GetInstanceMethod((DbGeography geo) => geo.Union(null));
				yield return GetInstanceMethod((DbGeography geo) => geo.Difference(null));
				yield return GetInstanceMethod((DbGeography geo) => geo.SymmetricDifference(null));
				yield return GetInstanceMethod((DbGeography geo) => geo.ElementAt(0));
				yield return GetInstanceMethod((DbGeography geo) => geo.PointAt(0));
				yield return GetStaticMethod(() => DbGeometry.FromText(null));
				yield return GetStaticMethod(() => DbGeometry.FromText(null, 0));
				yield return GetStaticMethod(() => DbGeometry.PointFromText(null, 0));
				yield return GetStaticMethod(() => DbGeometry.LineFromText(null, 0));
				yield return GetStaticMethod(() => DbGeometry.PolygonFromText(null, 0));
				yield return GetStaticMethod(() => DbGeometry.MultiPointFromText(null, 0));
				yield return GetStaticMethod(() => DbGeometry.MultiLineFromText(null, 0));
				yield return GetStaticMethod(() => DbGeometry.MultiPolygonFromText(null, 0));
				yield return GetStaticMethod(() => DbGeometry.GeometryCollectionFromText(null, 0));
				yield return GetStaticMethod(() => DbGeometry.FromBinary(null));
				yield return GetStaticMethod(() => DbGeometry.FromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeometry.PointFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeometry.LineFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeometry.PolygonFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeometry.MultiPointFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeometry.MultiLineFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeometry.MultiPolygonFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeometry.GeometryCollectionFromBinary(null, 0));
				yield return GetStaticMethod(() => DbGeometry.FromGml(null));
				yield return GetStaticMethod(() => DbGeometry.FromGml(null, 0));
				yield return GetInstanceMethod((DbGeometry geo) => geo.AsBinary());
				yield return GetInstanceMethod((DbGeometry geo) => geo.AsGml());
				yield return GetInstanceMethod((DbGeometry geo) => geo.AsText());
				yield return GetInstanceMethod((DbGeometry geo) => geo.SpatialEquals(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Disjoint(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Intersects(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Touches(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Crosses(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Within(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Contains(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Overlaps(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Relate(null, null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Buffer(0.0));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Distance(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Intersection(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Union(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.Difference(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.SymmetricDifference(null));
				yield return GetInstanceMethod((DbGeometry geo) => geo.ElementAt(0));
				yield return GetInstanceMethod((DbGeometry geo) => geo.PointAt(0));
				yield return GetInstanceMethod((DbGeometry geo) => geo.InteriorRingAt(0));
			}

			private static Dictionary<MethodInfo, string> GetRenamedMethodFunctions()
			{
				Dictionary<MethodInfo, string> dictionary = new Dictionary<MethodInfo, string>();
				dictionary.Add(GetStaticMethod(() => DbGeography.FromText(null)), "GeographyFromText");
				dictionary.Add(GetStaticMethod(() => DbGeography.FromText(null, 0)), "GeographyFromText");
				dictionary.Add(GetStaticMethod(() => DbGeography.PointFromText(null, 0)), "GeographyPointFromText");
				dictionary.Add(GetStaticMethod(() => DbGeography.LineFromText(null, 0)), "GeographyLineFromText");
				dictionary.Add(GetStaticMethod(() => DbGeography.PolygonFromText(null, 0)), "GeographyPolygonFromText");
				dictionary.Add(GetStaticMethod(() => DbGeography.MultiPointFromText(null, 0)), "GeographyMultiPointFromText");
				dictionary.Add(GetStaticMethod(() => DbGeography.MultiLineFromText(null, 0)), "GeographyMultiLineFromText");
				dictionary.Add(GetStaticMethod(() => DbGeography.MultiPolygonFromText(null, 0)), "GeographyMultiPolygonFromText");
				dictionary.Add(GetStaticMethod(() => DbGeography.GeographyCollectionFromText(null, 0)), "GeographyCollectionFromText");
				dictionary.Add(GetStaticMethod(() => DbGeography.FromBinary(null, 0)), "GeographyFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeography.FromBinary(null)), "GeographyFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeography.PointFromBinary(null, 0)), "GeographyPointFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeography.LineFromBinary(null, 0)), "GeographyLineFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeography.PolygonFromBinary(null, 0)), "GeographyPolygonFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeography.MultiPointFromBinary(null, 0)), "GeographyMultiPointFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeography.MultiLineFromBinary(null, 0)), "GeographyMultiLineFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeography.MultiPolygonFromBinary(null, 0)), "GeographyMultiPolygonFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeography.GeographyCollectionFromBinary(null, 0)), "GeographyCollectionFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeography.FromGml(null)), "GeographyFromGml");
				dictionary.Add(GetStaticMethod(() => DbGeography.FromGml(null, 0)), "GeographyFromGml");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.AsBinary()), "AsBinary");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.AsGml()), "AsGml");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.AsText()), "AsText");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.SpatialEquals(null)), "SpatialEquals");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.Disjoint(null)), "SpatialDisjoint");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.Intersects(null)), "SpatialIntersects");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.Buffer(0.0)), "SpatialBuffer");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.Distance(null)), "Distance");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.Intersection(null)), "SpatialIntersection");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.Union(null)), "SpatialUnion");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.Difference(null)), "SpatialDifference");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.SymmetricDifference(null)), "SpatialSymmetricDifference");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.ElementAt(0)), "SpatialElementAt");
				dictionary.Add(GetInstanceMethod((DbGeography geo) => geo.PointAt(0)), "PointAt");
				dictionary.Add(GetStaticMethod(() => DbGeometry.FromText(null)), "GeometryFromText");
				dictionary.Add(GetStaticMethod(() => DbGeometry.FromText(null, 0)), "GeometryFromText");
				dictionary.Add(GetStaticMethod(() => DbGeometry.PointFromText(null, 0)), "GeometryPointFromText");
				dictionary.Add(GetStaticMethod(() => DbGeometry.LineFromText(null, 0)), "GeometryLineFromText");
				dictionary.Add(GetStaticMethod(() => DbGeometry.PolygonFromText(null, 0)), "GeometryPolygonFromText");
				dictionary.Add(GetStaticMethod(() => DbGeometry.MultiPointFromText(null, 0)), "GeometryMultiPointFromText");
				dictionary.Add(GetStaticMethod(() => DbGeometry.MultiLineFromText(null, 0)), "GeometryMultiLineFromText");
				dictionary.Add(GetStaticMethod(() => DbGeometry.MultiPolygonFromText(null, 0)), "GeometryMultiPolygonFromText");
				dictionary.Add(GetStaticMethod(() => DbGeometry.GeometryCollectionFromText(null, 0)), "GeometryCollectionFromText");
				dictionary.Add(GetStaticMethod(() => DbGeometry.FromBinary(null)), "GeometryFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeometry.FromBinary(null, 0)), "GeometryFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeometry.PointFromBinary(null, 0)), "GeometryPointFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeometry.LineFromBinary(null, 0)), "GeometryLineFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeometry.PolygonFromBinary(null, 0)), "GeometryPolygonFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeometry.MultiPointFromBinary(null, 0)), "GeometryMultiPointFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeometry.MultiLineFromBinary(null, 0)), "GeometryMultiLineFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeometry.MultiPolygonFromBinary(null, 0)), "GeometryMultiPolygonFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeometry.GeometryCollectionFromBinary(null, 0)), "GeometryCollectionFromBinary");
				dictionary.Add(GetStaticMethod(() => DbGeometry.FromGml(null)), "GeometryFromGml");
				dictionary.Add(GetStaticMethod(() => DbGeometry.FromGml(null, 0)), "GeometryFromGml");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.AsBinary()), "AsBinary");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.AsGml()), "AsGml");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.AsText()), "AsText");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.SpatialEquals(null)), "SpatialEquals");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Disjoint(null)), "SpatialDisjoint");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Intersects(null)), "SpatialIntersects");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Touches(null)), "SpatialTouches");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Crosses(null)), "SpatialCrosses");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Within(null)), "SpatialWithin");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Contains(null)), "SpatialContains");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Overlaps(null)), "SpatialOverlaps");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Relate(null, null)), "SpatialRelate");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Buffer(0.0)), "SpatialBuffer");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Distance(null)), "Distance");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Intersection(null)), "SpatialIntersection");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Union(null)), "SpatialUnion");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.Difference(null)), "SpatialDifference");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.SymmetricDifference(null)), "SpatialSymmetricDifference");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.ElementAt(0)), "SpatialElementAt");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.PointAt(0)), "PointAt");
				dictionary.Add(GetInstanceMethod((DbGeometry geo) => geo.InteriorRingAt(0)), "InteriorRingAt");
				return dictionary;
			}

			internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
			{
				MethodInfo method = call.Method;
				if (!_methodFunctionRenames.TryGetValue(method, out var value))
				{
					value = "ST" + method.Name;
				}
				return parent.TranslateIntoCanonicalFunction(linqArguments: (!method.IsStatic) ? new Expression[1] { call.Object }.Concat(call.Arguments).ToArray() : call.Arguments.ToArray(), functionName: value, Expression: call);
			}
		}

		private const string s_stringsTypeFullName = "Microsoft.VisualBasic.Strings";

		private static readonly CallTranslator _defaultTranslator = new DefaultTranslator();

		private static readonly FunctionCallTranslator _functionCallTranslator = new FunctionCallTranslator();

		private static readonly Dictionary<MethodInfo, CallTranslator> _methodTranslators = InitializeMethodTranslators();

		private static readonly Dictionary<SequenceMethod, SequenceMethodTranslator> _sequenceTranslators = InitializeSequenceMethodTranslators();

		private static readonly Dictionary<string, ObjectQueryCallTranslator> _objectQueryTranslators = InitializeObjectQueryTranslators();

		private static bool s_vbMethodsInitialized;

		private static readonly object _vbInitializerLock = new object();

		internal MethodCallTranslator()
			: base(new ExpressionType[1] { ExpressionType.Call })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, MethodCallExpression linq)
		{
			if (ReflectionUtil.TryIdentifySequenceMethod(linq.Method, out var sequenceMethod) && _sequenceTranslators.TryGetValue(sequenceMethod, out var value))
			{
				return value.Translate(parent, linq, sequenceMethod);
			}
			if (TryGetCallTranslator(linq.Method, out var callTranslator))
			{
				return callTranslator.Translate(parent, linq);
			}
			if (ObjectQueryCallTranslator.IsCandidateMethod(linq.Method) && _objectQueryTranslators.TryGetValue(linq.Method.Name, out var value2))
			{
				return value2.Translate(parent, linq);
			}
			DbFunctionAttribute dbFunctionAttribute = linq.Method.GetCustomAttributes<DbFunctionAttribute>(inherit: false).FirstOrDefault();
			if (dbFunctionAttribute != null)
			{
				return _functionCallTranslator.TranslateFunctionCall(parent, linq, dbFunctionAttribute);
			}
			string name = linq.Method.Name;
			if (name != null && name == "Contains" && linq.Method.GetParameters().Count() == 1 && linq.Method.ReturnType.Equals(typeof(bool)) && linq.Method.IsImplementationOfGenericInterfaceMethod(typeof(ICollection<>), out var _))
			{
				return ContainsTranslator.TranslateContains(parent, linq.Object, linq.Arguments[0]);
			}
			return _defaultTranslator.Translate(parent, linq);
		}

		private static Dictionary<MethodInfo, CallTranslator> InitializeMethodTranslators()
		{
			Dictionary<MethodInfo, CallTranslator> dictionary = new Dictionary<MethodInfo, CallTranslator>();
			foreach (CallTranslator callTranslator in GetCallTranslators())
			{
				foreach (MethodInfo method in callTranslator.Methods)
				{
					dictionary.Add(method, callTranslator);
				}
			}
			return dictionary;
		}

		private static Dictionary<SequenceMethod, SequenceMethodTranslator> InitializeSequenceMethodTranslators()
		{
			Dictionary<SequenceMethod, SequenceMethodTranslator> dictionary = new Dictionary<SequenceMethod, SequenceMethodTranslator>();
			foreach (SequenceMethodTranslator sequenceMethodTranslator in GetSequenceMethodTranslators())
			{
				foreach (SequenceMethod method in sequenceMethodTranslator.Methods)
				{
					dictionary.Add(method, sequenceMethodTranslator);
				}
			}
			return dictionary;
		}

		private static Dictionary<string, ObjectQueryCallTranslator> InitializeObjectQueryTranslators()
		{
			Dictionary<string, ObjectQueryCallTranslator> dictionary = new Dictionary<string, ObjectQueryCallTranslator>(StringComparer.Ordinal);
			foreach (ObjectQueryCallTranslator objectQueryCallTranslator in GetObjectQueryCallTranslators())
			{
				dictionary[objectQueryCallTranslator.MethodName] = objectQueryCallTranslator;
			}
			return dictionary;
		}

		private static bool TryGetCallTranslator(MethodInfo methodInfo, out CallTranslator callTranslator)
		{
			if (_methodTranslators.TryGetValue(methodInfo, out callTranslator))
			{
				return true;
			}
			if ("Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" == methodInfo.DeclaringType.Assembly().FullName)
			{
				lock (_vbInitializerLock)
				{
					if (!s_vbMethodsInitialized)
					{
						InitializeVBMethods(methodInfo.DeclaringType.Assembly());
						s_vbMethodsInitialized = true;
					}
					return _methodTranslators.TryGetValue(methodInfo, out callTranslator);
				}
			}
			callTranslator = null;
			return false;
		}

		private static void InitializeVBMethods(Assembly vbAssembly)
		{
			foreach (CallTranslator visualBasicCallTranslator in GetVisualBasicCallTranslators(vbAssembly))
			{
				foreach (MethodInfo method in visualBasicCallTranslator.Methods)
				{
					_methodTranslators.Add(method, visualBasicCallTranslator);
				}
			}
		}

		private static IEnumerable<CallTranslator> GetVisualBasicCallTranslators(Assembly vbAssembly)
		{
			yield return new VBCanonicalFunctionDefaultTranslator(vbAssembly);
			yield return new VBCanonicalFunctionRenameTranslator(vbAssembly);
			yield return new VBDatePartTranslator(vbAssembly);
		}

		private static IEnumerable<CallTranslator> GetCallTranslators()
		{
			return new CallTranslator[23]
			{
				new CanonicalFunctionDefaultTranslator(),
				new AsUnicodeFunctionTranslator(),
				new AsNonUnicodeFunctionTranslator(),
				new MathTruncateTranslator(),
				new MathPowerTranslator(),
				new GuidNewGuidTranslator(),
				new LikeFunctionTranslator(),
				new StringContainsTranslator(),
				new StartsWithTranslator(),
				new EndsWithTranslator(),
				new IndexOfTranslator(),
				new SubstringTranslator(),
				new RemoveTranslator(),
				new InsertTranslator(),
				new IsNullOrEmptyTranslator(),
				new StringConcatTranslator(),
				new TrimTranslator(),
				new TrimStartTranslator(),
				new TrimEndTranslator(),
				new SpatialMethodCallTranslator(),
				new HierarchyIdMethodCallTranslator(),
				new HasFlagTranslator(),
				new ToStringTranslator()
			};
		}

		private static IEnumerable<SequenceMethodTranslator> GetSequenceMethodTranslators()
		{
			yield return new ConcatTranslator();
			yield return new UnionTranslator();
			yield return new IntersectTranslator();
			yield return new ExceptTranslator();
			yield return new DistinctTranslator();
			yield return new WhereTranslator();
			yield return new SelectTranslator();
			yield return new OrderByTranslator();
			yield return new OrderByDescendingTranslator();
			yield return new ThenByTranslator();
			yield return new ThenByDescendingTranslator();
			yield return new SelectManyTranslator();
			yield return new AnyTranslator();
			yield return new AnyPredicateTranslator();
			yield return new AllTranslator();
			yield return new JoinTranslator();
			yield return new GroupByTranslator();
			yield return new MaxTranslator();
			yield return new MinTranslator();
			yield return new AverageTranslator();
			yield return new SumTranslator();
			yield return new CountTranslator();
			yield return new LongCountTranslator();
			yield return new CastMethodTranslator();
			yield return new GroupJoinTranslator();
			yield return new OfTypeTranslator();
			yield return new PassthroughTranslator();
			yield return new DefaultIfEmptyTranslator();
			yield return new FirstTranslator();
			yield return new FirstPredicateTranslator();
			yield return new FirstOrDefaultTranslator();
			yield return new FirstOrDefaultPredicateTranslator();
			yield return new TakeTranslator();
			yield return new SkipTranslator();
			yield return new SingleTranslator();
			yield return new SinglePredicateTranslator();
			yield return new SingleOrDefaultTranslator();
			yield return new SingleOrDefaultPredicateTranslator();
			yield return new ContainsTranslator();
		}

		private static IEnumerable<ObjectQueryCallTranslator> GetObjectQueryCallTranslators()
		{
			yield return new ObjectQueryBuilderDistinctTranslator();
			yield return new ObjectQueryBuilderExceptTranslator();
			yield return new ObjectQueryBuilderFirstTranslator();
			yield return new ObjectQueryBuilderToListTranslator();
			yield return new ObjectQueryIncludeTranslator();
			yield return new ObjectQueryBuilderIntersectTranslator();
			yield return new ObjectQueryBuilderOfTypeTranslator();
			yield return new ObjectQueryBuilderUnionTranslator();
			yield return new ObjectQueryMergeAsTranslator();
			yield return new ObjectQueryIncludeSpanTranslator();
		}

		private static bool IsTrivialRename(LambdaExpression selectorLambda, ExpressionConverter converter, out string leftName, out string rightName, out InitializerMetadata initializerMetadata)
		{
			leftName = null;
			rightName = null;
			initializerMetadata = null;
			if (selectorLambda.Parameters.Count != 2 || selectorLambda.Body.NodeType != ExpressionType.New)
			{
				return false;
			}
			NewExpression newExpression = (NewExpression)selectorLambda.Body;
			if (newExpression.Arguments.Count != 2)
			{
				return false;
			}
			if (newExpression.Arguments[0] != selectorLambda.Parameters[0] || newExpression.Arguments[1] != selectorLambda.Parameters[1])
			{
				return false;
			}
			leftName = newExpression.Members[0].Name;
			rightName = newExpression.Members[1].Name;
			initializerMetadata = InitializerMetadata.CreateProjectionInitializer(converter.EdmItemCollection, newExpression);
			converter.ValidateInitializerMetadata(initializerMetadata);
			return true;
		}
	}

	private sealed class OrderByLifter
	{
		private abstract class OrderByLifterBase
		{
			protected readonly DbExpression _root;

			protected readonly AliasGenerator _aliasGenerator;

			protected OrderByLifterBase(DbExpression root, AliasGenerator aliasGenerator)
			{
				_root = root;
				_aliasGenerator = aliasGenerator;
			}

			internal static OrderByLifterBase GetLifter(DbExpression source, AliasGenerator aliasGenerator)
			{
				if (source.ExpressionKind == DbExpressionKind.Sort)
				{
					return new SortLifter((DbSortExpression)source, aliasGenerator);
				}
				if (source.ExpressionKind == DbExpressionKind.Project)
				{
					DbProjectExpression dbProjectExpression = (DbProjectExpression)source;
					DbExpression expression = dbProjectExpression.Input.Expression;
					if (expression.ExpressionKind == DbExpressionKind.Sort)
					{
						return new ProjectSortLifter(dbProjectExpression, (DbSortExpression)expression, aliasGenerator);
					}
					if (expression.ExpressionKind == DbExpressionKind.Skip)
					{
						return new ProjectSkipLifter(dbProjectExpression, (DbSkipExpression)expression, aliasGenerator);
					}
					if (expression.ExpressionKind == DbExpressionKind.Limit)
					{
						DbLimitExpression dbLimitExpression = (DbLimitExpression)expression;
						DbExpression argument = dbLimitExpression.Argument;
						if (argument.ExpressionKind == DbExpressionKind.Sort)
						{
							return new ProjectLimitSortLifter(dbProjectExpression, dbLimitExpression, (DbSortExpression)argument, aliasGenerator);
						}
						if (argument.ExpressionKind == DbExpressionKind.Skip)
						{
							return new ProjectLimitSkipLifter(dbProjectExpression, dbLimitExpression, (DbSkipExpression)argument, aliasGenerator);
						}
					}
				}
				if (source.ExpressionKind == DbExpressionKind.Skip)
				{
					return new SkipLifter((DbSkipExpression)source, aliasGenerator);
				}
				if (source.ExpressionKind == DbExpressionKind.Limit)
				{
					DbLimitExpression dbLimitExpression2 = (DbLimitExpression)source;
					DbExpression argument2 = dbLimitExpression2.Argument;
					if (argument2.ExpressionKind == DbExpressionKind.Sort)
					{
						return new LimitSortLifter(dbLimitExpression2, (DbSortExpression)argument2, aliasGenerator);
					}
					if (argument2.ExpressionKind == DbExpressionKind.Skip)
					{
						return new LimitSkipLifter(dbLimitExpression2, (DbSkipExpression)argument2, aliasGenerator);
					}
					if (argument2.ExpressionKind == DbExpressionKind.Project)
					{
						DbProjectExpression dbProjectExpression2 = (DbProjectExpression)argument2;
						DbExpression expression2 = dbProjectExpression2.Input.Expression;
						if (expression2.ExpressionKind == DbExpressionKind.Sort)
						{
							return new ProjectLimitSortLifter(dbProjectExpression2, dbLimitExpression2, (DbSortExpression)expression2, aliasGenerator);
						}
						if (expression2.ExpressionKind == DbExpressionKind.Skip)
						{
							return new ProjectLimitSkipLifter(dbProjectExpression2, dbLimitExpression2, (DbSkipExpression)expression2, aliasGenerator);
						}
					}
				}
				return new PassthroughOrderByLifter(source, aliasGenerator);
			}

			internal abstract DbExpression Project(DbProjectExpression project);

			internal abstract DbExpression Filter(DbFilterExpression filter);

			internal virtual DbExpression OfType(TypeUsage type)
			{
				DbExpressionBinding dbExpressionBinding = _root.BindAs(_aliasGenerator.Next());
				DbExpression dbExpression = Filter(dbExpressionBinding.Filter(dbExpressionBinding.Variable.IsOf(type)));
				OrderByLifterBase lifter = GetLifter(dbExpression, _aliasGenerator);
				DbExpressionBinding dbExpressionBinding2 = dbExpression.BindAs(_aliasGenerator.Next());
				return lifter.Project(dbExpressionBinding2.Project(dbExpressionBinding2.Variable.TreatAs(type)));
			}

			internal abstract DbExpression Limit(DbExpression k);

			internal abstract DbExpression Skip(DbExpression k);

			protected static DbProjectExpression ComposeProject(DbExpression input, DbProjectExpression first, DbProjectExpression second)
			{
				DbLambda lambda = DbExpressionBuilder.Lambda(second.Projection, second.Input.Variable);
				DbProjectExpression project = first.Input.Project(lambda.Invoke(first.Projection));
				return RebindProject(input, project);
			}

			protected static DbFilterExpression ComposeFilter(DbExpression input, DbProjectExpression first, DbFilterExpression second)
			{
				DbLambda lambda = DbExpressionBuilder.Lambda(second.Predicate, second.Input.Variable);
				DbFilterExpression filter = first.Input.Filter(lambda.Invoke(first.Projection));
				return RebindFilter(input, filter);
			}

			protected static DbSkipExpression AddToSkip(DbExpression input, DbSkipExpression skip, DbExpression plusK)
			{
				DbExpression k = CombineIntegers(skip.Count, plusK, (int l, int r) => l + r);
				return RebindSkip(input, skip, k);
			}

			protected static DbLimitExpression SubtractFromLimit(DbExpression input, DbLimitExpression limit, DbExpression minusK)
			{
				DbExpression count = CombineIntegers(limit.Limit, minusK, (int l, int r) => (r <= l) ? (l - r) : 0);
				return input.Limit(count);
			}

			protected static DbLimitExpression MinimumLimit(DbExpression input, DbLimitExpression limit, DbExpression k)
			{
				DbExpression count = CombineIntegers(limit.Limit, k, Math.Min);
				return input.Limit(count);
			}

			private static DbExpression CombineIntegers(DbExpression left, DbExpression right, Func<int, int, int> combineConstants)
			{
				if (left.ExpressionKind == DbExpressionKind.Constant && right.ExpressionKind == DbExpressionKind.Constant)
				{
					object value = ((DbConstantExpression)left).Value;
					object value2 = ((DbConstantExpression)right).Value;
					if (value is int && value2 is int)
					{
						return left.ResultType.Constant(combineConstants((int)value, (int)value2));
					}
				}
				throw new InvalidOperationException(Strings.ADP_InternalProviderError(1025));
			}

			protected static DbProjectExpression RebindProject(DbExpression input, DbProjectExpression project)
			{
				return input.BindAs(project.Input.VariableName).Project(project.Projection);
			}

			protected static DbFilterExpression RebindFilter(DbExpression input, DbFilterExpression filter)
			{
				return input.BindAs(filter.Input.VariableName).Filter(filter.Predicate);
			}

			protected static DbSortExpression RebindSort(DbExpression input, DbSortExpression sort)
			{
				return input.BindAs(sort.Input.VariableName).Sort(sort.SortOrder);
			}

			protected static DbSortExpression ApplySkipOrderToSort(DbExpression input, DbSkipExpression sortSpec)
			{
				return input.BindAs(sortSpec.Input.VariableName).Sort(sortSpec.SortOrder);
			}

			protected static DbSkipExpression ApplySortOrderToSkip(DbExpression input, DbSortExpression sort, DbExpression k)
			{
				return input.BindAs(sort.Input.VariableName).Skip(sort.SortOrder, k);
			}

			protected static DbSkipExpression RebindSkip(DbExpression input, DbSkipExpression skip, DbExpression k)
			{
				return input.BindAs(skip.Input.VariableName).Skip(skip.SortOrder, k);
			}
		}

		private class LimitSkipLifter : OrderByLifterBase
		{
			private readonly DbLimitExpression _limit;

			private readonly DbSkipExpression _skip;

			internal LimitSkipLifter(DbLimitExpression limit, DbSkipExpression skip, AliasGenerator aliasGenerator)
				: base(limit, aliasGenerator)
			{
				_limit = limit;
				_skip = skip;
			}

			internal override DbExpression Filter(DbFilterExpression filter)
			{
				return OrderByLifterBase.ApplySkipOrderToSort(filter, _skip);
			}

			internal override DbExpression Project(DbProjectExpression project)
			{
				return project;
			}

			internal override DbExpression Limit(DbExpression k)
			{
				if (_limit.Limit.ExpressionKind == DbExpressionKind.Constant && k.ExpressionKind == DbExpressionKind.Constant)
				{
					return OrderByLifterBase.MinimumLimit(_skip, _limit, k);
				}
				return OrderByLifterBase.ApplySkipOrderToSort(_limit, _skip).Limit(k);
			}

			internal override DbExpression Skip(DbExpression k)
			{
				return OrderByLifterBase.RebindSkip(_limit, _skip, k);
			}
		}

		private class LimitSortLifter : OrderByLifterBase
		{
			private readonly DbLimitExpression _limit;

			private readonly DbSortExpression _sort;

			internal LimitSortLifter(DbLimitExpression limit, DbSortExpression sort, AliasGenerator aliasGenerator)
				: base(limit, aliasGenerator)
			{
				_limit = limit;
				_sort = sort;
			}

			internal override DbExpression Filter(DbFilterExpression filter)
			{
				return OrderByLifterBase.RebindSort(filter, _sort);
			}

			internal override DbExpression Project(DbProjectExpression project)
			{
				return project;
			}

			internal override DbExpression Limit(DbExpression k)
			{
				if (_limit.Limit.ExpressionKind == DbExpressionKind.Constant && k.ExpressionKind == DbExpressionKind.Constant)
				{
					return OrderByLifterBase.MinimumLimit(_sort, _limit, k);
				}
				return OrderByLifterBase.RebindSort(_limit, _sort).Limit(k);
			}

			internal override DbExpression Skip(DbExpression k)
			{
				return OrderByLifterBase.ApplySortOrderToSkip(_limit, _sort, k);
			}
		}

		private class ProjectLimitSkipLifter : OrderByLifterBase
		{
			private readonly DbProjectExpression _project;

			private readonly DbLimitExpression _limit;

			private readonly DbSkipExpression _skip;

			private readonly DbExpression _source;

			internal ProjectLimitSkipLifter(DbProjectExpression project, DbLimitExpression limit, DbSkipExpression skip, AliasGenerator aliasGenerator)
				: base(project, aliasGenerator)
			{
				_project = project;
				_limit = limit;
				_skip = skip;
				_source = skip.Input.Expression;
			}

			internal override DbExpression Filter(DbFilterExpression filter)
			{
				return OrderByLifterBase.RebindProject(OrderByLifterBase.ApplySkipOrderToSort(OrderByLifterBase.ComposeFilter(_skip.Limit(_limit.Limit), _project, filter), _skip), _project);
			}

			internal override DbExpression Project(DbProjectExpression project)
			{
				return OrderByLifterBase.ComposeProject(_skip.Limit(_limit.Limit), _project, project);
			}

			internal override DbExpression Limit(DbExpression k)
			{
				if (_limit.Limit.ExpressionKind == DbExpressionKind.Constant && k.ExpressionKind == DbExpressionKind.Constant)
				{
					return OrderByLifterBase.RebindProject(OrderByLifterBase.MinimumLimit(_skip, _limit, k), _project);
				}
				return OrderByLifterBase.RebindProject(OrderByLifterBase.ApplySkipOrderToSort(_skip.Limit(_limit.Limit), _skip).Limit(k), _project);
			}

			internal override DbExpression Skip(DbExpression k)
			{
				if (_skip.Count.ExpressionKind == DbExpressionKind.Constant && _limit.Limit.ExpressionKind == DbExpressionKind.Constant && k.ExpressionKind == DbExpressionKind.Constant)
				{
					return OrderByLifterBase.RebindProject(OrderByLifterBase.SubtractFromLimit(OrderByLifterBase.AddToSkip(_source, _skip, k), _limit, k), _project);
				}
				return OrderByLifterBase.RebindProject(OrderByLifterBase.RebindSkip(_skip.Limit(_limit.Limit), _skip, k), _project);
			}
		}

		private class ProjectLimitSortLifter : OrderByLifterBase
		{
			private readonly DbProjectExpression _project;

			private readonly DbLimitExpression _limit;

			private readonly DbSortExpression _sort;

			internal ProjectLimitSortLifter(DbProjectExpression project, DbLimitExpression limit, DbSortExpression sort, AliasGenerator aliasGenerator)
				: base(project, aliasGenerator)
			{
				_project = project;
				_limit = limit;
				_sort = sort;
			}

			internal override DbExpression Filter(DbFilterExpression filter)
			{
				return OrderByLifterBase.RebindProject(OrderByLifterBase.RebindSort(OrderByLifterBase.ComposeFilter(_sort.Limit(_limit.Limit), _project, filter), _sort), _project);
			}

			internal override DbExpression Project(DbProjectExpression project)
			{
				return OrderByLifterBase.ComposeProject(_sort.Limit(_limit.Limit), _project, project);
			}

			internal override DbExpression Limit(DbExpression k)
			{
				if (_limit.Limit.ExpressionKind == DbExpressionKind.Constant && k.ExpressionKind == DbExpressionKind.Constant)
				{
					return OrderByLifterBase.RebindProject(OrderByLifterBase.MinimumLimit(_sort, _limit, k), _project);
				}
				return OrderByLifterBase.RebindProject(OrderByLifterBase.RebindSort(_sort.Limit(_limit.Limit), _sort).Limit(k), _project);
			}

			internal override DbExpression Skip(DbExpression k)
			{
				return OrderByLifterBase.RebindProject(OrderByLifterBase.ApplySortOrderToSkip(_sort.Limit(_limit.Limit), _sort, k), _project);
			}
		}

		private class ProjectSkipLifter : OrderByLifterBase
		{
			private readonly DbProjectExpression _project;

			private readonly DbSkipExpression _skip;

			private readonly DbExpression _source;

			internal ProjectSkipLifter(DbProjectExpression project, DbSkipExpression skip, AliasGenerator aliasGenerator)
				: base(project, aliasGenerator)
			{
				_project = project;
				_skip = skip;
				_source = _skip.Input.Expression;
			}

			internal override DbExpression Filter(DbFilterExpression filter)
			{
				return OrderByLifterBase.RebindProject(OrderByLifterBase.ApplySkipOrderToSort(OrderByLifterBase.ComposeFilter(_skip, _project, filter), _skip), _project);
			}

			internal override DbExpression Limit(DbExpression k)
			{
				return _root.Limit(k);
			}

			internal override DbExpression Project(DbProjectExpression project)
			{
				return OrderByLifterBase.ComposeProject(_skip, _project, project);
			}

			internal override DbExpression Skip(DbExpression k)
			{
				if (_skip.Count.ExpressionKind == DbExpressionKind.Constant && k.ExpressionKind == DbExpressionKind.Constant)
				{
					return OrderByLifterBase.RebindProject(OrderByLifterBase.AddToSkip(_source, _skip, k), _project);
				}
				return OrderByLifterBase.RebindProject(OrderByLifterBase.RebindSkip(_skip, _skip, k), _project);
			}
		}

		private class SkipLifter : OrderByLifterBase
		{
			private readonly DbSkipExpression _skip;

			private readonly DbExpression _source;

			internal SkipLifter(DbSkipExpression skip, AliasGenerator aliasGenerator)
				: base(skip, aliasGenerator)
			{
				_skip = skip;
				_source = skip.Input.Expression;
			}

			internal override DbExpression Filter(DbFilterExpression filter)
			{
				return OrderByLifterBase.ApplySkipOrderToSort(filter, _skip);
			}

			internal override DbExpression Project(DbProjectExpression project)
			{
				return project;
			}

			internal override DbExpression Limit(DbExpression k)
			{
				return _root.Limit(k);
			}

			internal override DbExpression Skip(DbExpression k)
			{
				if (_skip.Count.ExpressionKind == DbExpressionKind.Constant && k.ExpressionKind == DbExpressionKind.Constant)
				{
					return OrderByLifterBase.AddToSkip(_source, _skip, k);
				}
				return OrderByLifterBase.RebindSkip(_skip, _skip, k);
			}
		}

		private class ProjectSortLifter : OrderByLifterBase
		{
			private readonly DbProjectExpression _project;

			private readonly DbSortExpression _sort;

			private readonly DbExpression _source;

			internal ProjectSortLifter(DbProjectExpression project, DbSortExpression sort, AliasGenerator aliasGenerator)
				: base(project, aliasGenerator)
			{
				_project = project;
				_sort = sort;
				_source = sort.Input.Expression;
			}

			internal override DbExpression Project(DbProjectExpression project)
			{
				return OrderByLifterBase.ComposeProject(_sort, _project, project);
			}

			internal override DbExpression Filter(DbFilterExpression filter)
			{
				return OrderByLifterBase.RebindProject(OrderByLifterBase.RebindSort(OrderByLifterBase.ComposeFilter(_source, _project, filter), _sort), _project);
			}

			internal override DbExpression Limit(DbExpression k)
			{
				return _root.Limit(k);
			}

			internal override DbExpression Skip(DbExpression k)
			{
				return OrderByLifterBase.RebindProject(OrderByLifterBase.ApplySortOrderToSkip(_source, _sort, k), _project);
			}
		}

		private class SortLifter : OrderByLifterBase
		{
			private readonly DbSortExpression _sort;

			private readonly DbExpression _source;

			internal SortLifter(DbSortExpression sort, AliasGenerator aliasGenerator)
				: base(sort, aliasGenerator)
			{
				_sort = sort;
				_source = sort.Input.Expression;
			}

			internal override DbExpression Project(DbProjectExpression project)
			{
				return project;
			}

			internal override DbExpression Filter(DbFilterExpression filter)
			{
				return OrderByLifterBase.RebindSort(OrderByLifterBase.RebindFilter(_source, filter), _sort);
			}

			internal override DbExpression Limit(DbExpression k)
			{
				return _root.Limit(k);
			}

			internal override DbExpression Skip(DbExpression k)
			{
				return OrderByLifterBase.ApplySortOrderToSkip(_source, _sort, k);
			}
		}

		private class PassthroughOrderByLifter : OrderByLifterBase
		{
			internal PassthroughOrderByLifter(DbExpression source, AliasGenerator aliasGenerator)
				: base(source, aliasGenerator)
			{
			}

			internal override DbExpression Project(DbProjectExpression project)
			{
				return project;
			}

			internal override DbExpression Filter(DbFilterExpression filter)
			{
				return filter;
			}

			internal override DbExpression OfType(TypeUsage type)
			{
				return _root.OfType(type);
			}

			internal override DbExpression Limit(DbExpression k)
			{
				return _root.Limit(k);
			}

			internal override DbExpression Skip(DbExpression k)
			{
				throw new NotSupportedException(Strings.ELinq_SkipWithoutOrder);
			}
		}

		private readonly AliasGenerator _aliasGenerator;

		internal OrderByLifter(AliasGenerator aliasGenerator)
		{
			_aliasGenerator = aliasGenerator;
		}

		internal DbExpression Project(DbExpressionBinding input, DbExpression projection)
		{
			return GetLifter(input.Expression).Project(input.Project(projection));
		}

		internal DbExpression Filter(DbExpressionBinding input, DbExpression predicate)
		{
			return GetLifter(input.Expression).Filter(input.Filter(predicate));
		}

		internal DbExpression OfType(DbExpression argument, TypeUsage type)
		{
			return GetLifter(argument).OfType(type);
		}

		internal DbExpression Skip(DbExpressionBinding input, DbExpression skipCount)
		{
			return GetLifter(input.Expression).Skip(skipCount);
		}

		internal DbExpression Limit(DbExpression argument, DbExpression limit)
		{
			return GetLifter(argument).Limit(limit);
		}

		private OrderByLifterBase GetLifter(DbExpression root)
		{
			return OrderByLifterBase.GetLifter(root, _aliasGenerator);
		}
	}

	internal sealed class MemberAccessTranslator : TypedTranslator<MemberExpression>
	{
		private sealed class SpatialPropertyTranslator : PropertyTranslator
		{
			private readonly Dictionary<PropertyInfo, string> propertyFunctionRenames = GetRenamedPropertyFunctions();

			internal SpatialPropertyTranslator()
				: base(GetSupportedProperties())
			{
			}

			private static PropertyInfo GetProperty<T, TResult>(Expression<Func<T, TResult>> lambda)
			{
				return (PropertyInfo)((MemberExpression)lambda.Body).Member;
			}

			private static IEnumerable<PropertyInfo> GetSupportedProperties()
			{
				yield return GetProperty((DbGeography geo) => geo.CoordinateSystemId);
				yield return GetProperty((DbGeography geo) => geo.SpatialTypeName);
				yield return GetProperty((DbGeography geo) => geo.Dimension);
				yield return GetProperty((DbGeography geo) => geo.IsEmpty);
				yield return GetProperty((DbGeography geo) => geo.ElementCount);
				yield return GetProperty((DbGeography geo) => geo.Latitude);
				yield return GetProperty((DbGeography geo) => geo.Longitude);
				yield return GetProperty((DbGeography geo) => geo.Elevation);
				yield return GetProperty((DbGeography geo) => geo.Measure);
				yield return GetProperty((DbGeography geo) => geo.Length);
				yield return GetProperty((DbGeography geo) => geo.StartPoint);
				yield return GetProperty((DbGeography geo) => geo.EndPoint);
				yield return GetProperty((DbGeography geo) => geo.IsClosed);
				yield return GetProperty((DbGeography geo) => geo.PointCount);
				yield return GetProperty((DbGeography geo) => geo.Area);
				yield return GetProperty((DbGeometry geo) => geo.CoordinateSystemId);
				yield return GetProperty((DbGeometry geo) => geo.SpatialTypeName);
				yield return GetProperty((DbGeometry geo) => geo.Dimension);
				yield return GetProperty((DbGeometry geo) => geo.Envelope);
				yield return GetProperty((DbGeometry geo) => geo.IsEmpty);
				yield return GetProperty((DbGeometry geo) => geo.IsSimple);
				yield return GetProperty((DbGeometry geo) => geo.Boundary);
				yield return GetProperty((DbGeometry geo) => geo.IsValid);
				yield return GetProperty((DbGeometry geo) => geo.ConvexHull);
				yield return GetProperty((DbGeometry geo) => geo.ElementCount);
				yield return GetProperty((DbGeometry geo) => geo.XCoordinate);
				yield return GetProperty((DbGeometry geo) => geo.YCoordinate);
				yield return GetProperty((DbGeometry geo) => geo.Elevation);
				yield return GetProperty((DbGeometry geo) => geo.Measure);
				yield return GetProperty((DbGeometry geo) => geo.Length);
				yield return GetProperty((DbGeometry geo) => geo.StartPoint);
				yield return GetProperty((DbGeometry geo) => geo.EndPoint);
				yield return GetProperty((DbGeometry geo) => geo.IsClosed);
				yield return GetProperty((DbGeometry geo) => geo.IsRing);
				yield return GetProperty((DbGeometry geo) => geo.PointCount);
				yield return GetProperty((DbGeometry geo) => geo.Area);
				yield return GetProperty((DbGeometry geo) => geo.Centroid);
				yield return GetProperty((DbGeometry geo) => geo.PointOnSurface);
				yield return GetProperty((DbGeometry geo) => geo.ExteriorRing);
				yield return GetProperty((DbGeometry geo) => geo.InteriorRingCount);
			}

			private static Dictionary<PropertyInfo, string> GetRenamedPropertyFunctions()
			{
				Dictionary<PropertyInfo, string> dictionary = new Dictionary<PropertyInfo, string>();
				dictionary.Add(GetProperty((DbGeography geo) => geo.CoordinateSystemId), "CoordinateSystemId");
				dictionary.Add(GetProperty((DbGeography geo) => geo.SpatialTypeName), "SpatialTypeName");
				dictionary.Add(GetProperty((DbGeography geo) => geo.Dimension), "SpatialDimension");
				dictionary.Add(GetProperty((DbGeography geo) => geo.IsEmpty), "IsEmptySpatial");
				dictionary.Add(GetProperty((DbGeography geo) => geo.ElementCount), "SpatialElementCount");
				dictionary.Add(GetProperty((DbGeography geo) => geo.Latitude), "Latitude");
				dictionary.Add(GetProperty((DbGeography geo) => geo.Longitude), "Longitude");
				dictionary.Add(GetProperty((DbGeography geo) => geo.Elevation), "Elevation");
				dictionary.Add(GetProperty((DbGeography geo) => geo.Measure), "Measure");
				dictionary.Add(GetProperty((DbGeography geo) => geo.Length), "SpatialLength");
				dictionary.Add(GetProperty((DbGeography geo) => geo.StartPoint), "StartPoint");
				dictionary.Add(GetProperty((DbGeography geo) => geo.EndPoint), "EndPoint");
				dictionary.Add(GetProperty((DbGeography geo) => geo.IsClosed), "IsClosedSpatial");
				dictionary.Add(GetProperty((DbGeography geo) => geo.PointCount), "PointCount");
				dictionary.Add(GetProperty((DbGeography geo) => geo.Area), "Area");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.CoordinateSystemId), "CoordinateSystemId");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.SpatialTypeName), "SpatialTypeName");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.Dimension), "SpatialDimension");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.Envelope), "SpatialEnvelope");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.IsEmpty), "IsEmptySpatial");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.IsSimple), "IsSimpleGeometry");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.Boundary), "SpatialBoundary");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.IsValid), "IsValidGeometry");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.ConvexHull), "SpatialConvexHull");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.ElementCount), "SpatialElementCount");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.XCoordinate), "XCoordinate");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.YCoordinate), "YCoordinate");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.Elevation), "Elevation");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.Measure), "Measure");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.Length), "SpatialLength");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.StartPoint), "StartPoint");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.EndPoint), "EndPoint");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.IsClosed), "IsClosedSpatial");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.IsRing), "IsRing");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.PointCount), "PointCount");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.Area), "Area");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.Centroid), "Centroid");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.PointOnSurface), "PointOnSurface");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.ExteriorRing), "ExteriorRing");
				dictionary.Add(GetProperty((DbGeometry geo) => geo.InteriorRingCount), "InteriorRingCount");
				return dictionary;
			}

			internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
			{
				PropertyInfo propertyInfo = (PropertyInfo)call.Member;
				if (!propertyFunctionRenames.TryGetValue(propertyInfo, out var value))
				{
					value = "ST" + propertyInfo.Name;
				}
				return parent.TranslateIntoCanonicalFunction(value, call, call.Expression);
			}
		}

		private sealed class GenericICollectionTranslator : PropertyTranslator
		{
			private readonly Type _elementType;

			private GenericICollectionTranslator(Type elementType)
				: base(Enumerable.Empty<PropertyInfo>())
			{
				_elementType = elementType;
			}

			internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
			{
				return TranslateCount(parent, _elementType, call.Expression);
			}

			internal static bool TryGetPropertyTranslator(PropertyInfo propertyInfo, out PropertyTranslator propertyTranslator)
			{
				if (propertyInfo.Name == "Count" && propertyInfo.PropertyType.Equals(typeof(int)))
				{
					foreach (KeyValuePair<Type, Type> implementedICollection in GetImplementedICollections(propertyInfo.DeclaringType))
					{
						Type key = implementedICollection.Key;
						Type value = implementedICollection.Value;
						if (propertyInfo.IsImplementationOf(key))
						{
							propertyTranslator = new GenericICollectionTranslator(value);
							return true;
						}
					}
				}
				propertyTranslator = null;
				return false;
			}

			private static bool IsICollection(Type candidateType, out Type elementType)
			{
				if (candidateType.IsGenericType() && candidateType.GetGenericTypeDefinition().Equals(typeof(ICollection<>)))
				{
					elementType = candidateType.GetGenericArguments()[0];
					return true;
				}
				elementType = null;
				return false;
			}

			private static IEnumerable<KeyValuePair<Type, Type>> GetImplementedICollections(Type type)
			{
				if (IsICollection(type, out var elementType))
				{
					yield return new KeyValuePair<Type, Type>(type, elementType);
					yield break;
				}
				Type[] interfaces = type.GetInterfaces();
				foreach (Type type2 in interfaces)
				{
					if (IsICollection(type2, out elementType))
					{
						yield return new KeyValuePair<Type, Type>(type2, elementType);
					}
				}
			}
		}

		internal abstract class PropertyTranslator
		{
			private readonly IEnumerable<PropertyInfo> _properties;

			internal IEnumerable<PropertyInfo> Properties => _properties;

			protected PropertyTranslator(params PropertyInfo[] properties)
			{
				_properties = properties;
			}

			protected PropertyTranslator(IEnumerable<PropertyInfo> properties)
			{
				_properties = properties;
			}

			internal abstract DbExpression Translate(ExpressionConverter parent, MemberExpression call);

			public override string ToString()
			{
				return GetType().Name;
			}
		}

		internal sealed class DefaultCanonicalFunctionPropertyTranslator : PropertyTranslator
		{
			internal DefaultCanonicalFunctionPropertyTranslator()
				: base(GetProperties())
			{
			}

			private static IEnumerable<PropertyInfo> GetProperties()
			{
				return new PropertyInfo[17]
				{
					typeof(string).GetDeclaredProperty("Length"),
					typeof(DateTime).GetDeclaredProperty("Year"),
					typeof(DateTime).GetDeclaredProperty("Month"),
					typeof(DateTime).GetDeclaredProperty("Day"),
					typeof(DateTime).GetDeclaredProperty("Hour"),
					typeof(DateTime).GetDeclaredProperty("Minute"),
					typeof(DateTime).GetDeclaredProperty("Second"),
					typeof(DateTime).GetDeclaredProperty("Millisecond"),
					typeof(DateTimeOffset).GetDeclaredProperty("Year"),
					typeof(DateTimeOffset).GetDeclaredProperty("Month"),
					typeof(DateTimeOffset).GetDeclaredProperty("Day"),
					typeof(DateTimeOffset).GetDeclaredProperty("Hour"),
					typeof(DateTimeOffset).GetDeclaredProperty("Minute"),
					typeof(DateTimeOffset).GetDeclaredProperty("Second"),
					typeof(DateTimeOffset).GetDeclaredProperty("Millisecond"),
					typeof(DateTimeOffset).GetDeclaredProperty("LocalDateTime"),
					typeof(DateTimeOffset).GetDeclaredProperty("UtcDateTime")
				};
			}

			internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
			{
				return parent.TranslateIntoCanonicalFunction(call.Member.Name, call, call.Expression);
			}
		}

		internal sealed class RenameCanonicalFunctionPropertyTranslator : PropertyTranslator
		{
			private static readonly Dictionary<PropertyInfo, string> _propertyRenameMap = new Dictionary<PropertyInfo, string>(2);

			internal RenameCanonicalFunctionPropertyTranslator()
				: base(GetProperties())
			{
			}

			private static IEnumerable<PropertyInfo> GetProperties()
			{
				return new PropertyInfo[7]
				{
					GetProperty(typeof(DateTime), "Now", "CurrentDateTime"),
					GetProperty(typeof(DateTime), "UtcNow", "CurrentUtcDateTime"),
					GetProperty(typeof(DateTimeOffset), "Now", "CurrentDateTimeOffset"),
					GetProperty(typeof(TimeSpan), "Hours", "Hour"),
					GetProperty(typeof(TimeSpan), "Minutes", "Minute"),
					GetProperty(typeof(TimeSpan), "Seconds", "Second"),
					GetProperty(typeof(TimeSpan), "Milliseconds", "Millisecond")
				};
			}

			private static PropertyInfo GetProperty(Type declaringType, string propertyName, string canonicalFunctionName)
			{
				PropertyInfo declaredProperty = declaringType.GetDeclaredProperty(propertyName);
				_propertyRenameMap[declaredProperty] = canonicalFunctionName;
				return declaredProperty;
			}

			internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
			{
				PropertyInfo key = (PropertyInfo)call.Member;
				string functionName = _propertyRenameMap[key];
				if (call.Expression == null)
				{
					return parent.TranslateIntoCanonicalFunction(functionName, call);
				}
				return parent.TranslateIntoCanonicalFunction(functionName, call, call.Expression);
			}
		}

		internal sealed class VBDateAndTimeNowTranslator : PropertyTranslator
		{
			private const string s_dateAndTimeTypeFullName = "Microsoft.VisualBasic.DateAndTime";

			internal VBDateAndTimeNowTranslator(Assembly vbAssembly)
				: base(GetProperty(vbAssembly))
			{
			}

			private static PropertyInfo GetProperty(Assembly vbAssembly)
			{
				return vbAssembly.GetType("Microsoft.VisualBasic.DateAndTime").GetDeclaredProperty("Now");
			}

			internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
			{
				return parent.TranslateIntoCanonicalFunction("CurrentDateTime", call);
			}
		}

		internal sealed class EntityCollectionCountTranslator : PropertyTranslator
		{
			internal EntityCollectionCountTranslator()
				: base(GetProperty())
			{
			}

			private static PropertyInfo GetProperty()
			{
				return typeof(EntityCollection<>).GetDeclaredProperty("Count");
			}

			internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
			{
				return TranslateCount(parent, call.Member.DeclaringType.GetGenericArguments()[0], call.Expression);
			}
		}

		internal sealed class NullableHasValueTranslator : PropertyTranslator
		{
			internal NullableHasValueTranslator()
				: base(GetProperty())
			{
			}

			private static PropertyInfo GetProperty()
			{
				return typeof(Nullable<>).GetDeclaredProperty("HasValue");
			}

			internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
			{
				return CreateIsNullExpression(parent.TranslateExpression(call.Expression), call.Expression.Type).Not();
			}
		}

		internal sealed class NullableValueTranslator : PropertyTranslator
		{
			internal NullableValueTranslator()
				: base(GetProperty())
			{
			}

			private static PropertyInfo GetProperty()
			{
				return typeof(Nullable<>).GetDeclaredProperty("Value");
			}

			internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
			{
				return parent.TranslateExpression(call.Expression);
			}
		}

		private static readonly Dictionary<PropertyInfo, PropertyTranslator> _propertyTranslators;

		private static bool _vbPropertiesInitialized;

		private static readonly object _vbInitializerLock;

		internal MemberAccessTranslator()
			: base(new ExpressionType[1] { ExpressionType.MemberAccess })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, MemberExpression linq)
		{
			string name;
			Type type;
			MemberInfo memberInfo = TypeSystem.PropertyOrField(linq.Member, out name, out type);
			if (linq.Expression != null)
			{
				if (ExpressionType.Constant == linq.Expression.NodeType && ((ConstantExpression)linq.Expression).Type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).FirstOrDefault() != null)
				{
					Delegate @delegate = Expression.Lambda(linq).Compile();
					return parent.TranslateExpression(Expression.Constant(@delegate.DynamicInvoke()));
				}
				DbExpression dbExpression = parent.TranslateExpression(linq.Expression);
				if (TryResolveAsProperty(parent, memberInfo, dbExpression.ResultType, dbExpression, out var propertyExpression))
				{
					return propertyExpression;
				}
			}
			if (memberInfo.MemberType == MemberTypes.Property && TryGetTranslator((PropertyInfo)memberInfo, out var propertyTranslator))
			{
				return propertyTranslator.Translate(parent, linq);
			}
			throw new NotSupportedException(Strings.ELinq_UnrecognizedMember(linq.Member.Name));
		}

		static MemberAccessTranslator()
		{
			_vbInitializerLock = new object();
			_propertyTranslators = new Dictionary<PropertyInfo, PropertyTranslator>();
			foreach (PropertyTranslator propertyTranslator in GetPropertyTranslators())
			{
				foreach (PropertyInfo property in propertyTranslator.Properties)
				{
					_propertyTranslators.Add(property, propertyTranslator);
				}
			}
		}

		private static bool TryGetTranslator(PropertyInfo propertyInfo, out PropertyTranslator propertyTranslator)
		{
			PropertyInfo propertyInfo2 = propertyInfo;
			if (propertyInfo.DeclaringType.IsGenericType())
			{
				try
				{
					propertyInfo = propertyInfo.DeclaringType.GetGenericTypeDefinition().GetDeclaredProperty(propertyInfo.Name);
				}
				catch (AmbiguousMatchException)
				{
					propertyTranslator = null;
					return false;
				}
				if (propertyInfo == null)
				{
					propertyTranslator = null;
					return false;
				}
			}
			if (_propertyTranslators.TryGetValue(propertyInfo, out var value))
			{
				propertyTranslator = value;
				return true;
			}
			if ("Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" == propertyInfo.DeclaringType.Assembly().FullName)
			{
				lock (_vbInitializerLock)
				{
					if (!_vbPropertiesInitialized)
					{
						InitializeVBProperties(propertyInfo.DeclaringType.Assembly());
						_vbPropertiesInitialized = true;
					}
					if (_propertyTranslators.TryGetValue(propertyInfo, out value))
					{
						propertyTranslator = value;
						return true;
					}
					propertyTranslator = null;
					return false;
				}
			}
			if (GenericICollectionTranslator.TryGetPropertyTranslator(propertyInfo2, out propertyTranslator))
			{
				return true;
			}
			propertyTranslator = null;
			return false;
		}

		private static bool TryResolveAsProperty(ExpressionConverter parent, MemberInfo clrMember, TypeUsage definingType, DbExpression instance, out DbExpression propertyExpression)
		{
			RowType rowType = definingType.EdmType as RowType;
			string name = clrMember.Name;
			if (rowType != null)
			{
				if (rowType.Members.TryGetValue(name, ignoreCase: false, out var _))
				{
					propertyExpression = instance.Property(name);
					return true;
				}
				propertyExpression = null;
				return false;
			}
			if (definingType.EdmType is StructuralType type)
			{
				EdmMember outMember = null;
				if (parent._perspective.TryGetMember(type, name, ignoreCase: false, out outMember) && outMember != null)
				{
					if (outMember.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty)
					{
						NavigationProperty navProp = (NavigationProperty)outMember;
						propertyExpression = TranslateNavigationProperty(parent, clrMember, instance, navProp);
						return true;
					}
					propertyExpression = instance.Property(name);
					return true;
				}
			}
			if (name == "Key" && DbExpressionKind.Property == instance.ExpressionKind)
			{
				DbPropertyExpression dbPropertyExpression = (DbPropertyExpression)instance;
				if (dbPropertyExpression.Property.Name == "Group" && InitializerMetadata.TryGetInitializerMetadata(dbPropertyExpression.Instance.ResultType, out var initializerMetadata) && initializerMetadata.Kind == InitializerMetadataKind.Grouping)
				{
					propertyExpression = dbPropertyExpression.Instance.Property("Key");
					return true;
				}
			}
			propertyExpression = null;
			return false;
		}

		private static DbExpression TranslateNavigationProperty(ExpressionConverter parent, MemberInfo clrMember, DbExpression instance, NavigationProperty navProp)
		{
			DbExpression dbExpression = instance.Property(navProp);
			if (BuiltInTypeKind.CollectionType == dbExpression.ResultType.EdmType.BuiltInTypeKind)
			{
				Type propertyType = ((PropertyInfo)clrMember).PropertyType;
				if (propertyType.IsGenericType() && propertyType.GetGenericTypeDefinition() == typeof(EntityCollection<>))
				{
					dbExpression = CreateNewRowExpression(new List<KeyValuePair<string, DbExpression>>(2)
					{
						new KeyValuePair<string, DbExpression>("Owner", instance),
						new KeyValuePair<string, DbExpression>("Elements", dbExpression)
					}, InitializerMetadata.CreateEntityCollectionInitializer(parent.EdmItemCollection, propertyType, navProp));
				}
			}
			return dbExpression;
		}

		private static DbExpression TranslateCount(ExpressionConverter parent, Type sequenceElementType, Expression sequence)
		{
			ReflectionUtil.TryLookupMethod(SequenceMethod.Count, out var method);
			method = method.MakeGenericMethod(sequenceElementType);
			Expression linq = Expression.Call(method, sequence);
			return parent.TranslateExpression(linq);
		}

		private static void InitializeVBProperties(Assembly vbAssembly)
		{
			foreach (PropertyTranslator visualBasicPropertyTranslator in GetVisualBasicPropertyTranslators(vbAssembly))
			{
				foreach (PropertyInfo property in visualBasicPropertyTranslator.Properties)
				{
					_propertyTranslators.Add(property, visualBasicPropertyTranslator);
				}
			}
		}

		private static IEnumerable<PropertyTranslator> GetVisualBasicPropertyTranslators(Assembly vbAssembly)
		{
			return new PropertyTranslator[1]
			{
				new VBDateAndTimeNowTranslator(vbAssembly)
			};
		}

		private static IEnumerable<PropertyTranslator> GetPropertyTranslators()
		{
			return new PropertyTranslator[6]
			{
				new DefaultCanonicalFunctionPropertyTranslator(),
				new RenameCanonicalFunctionPropertyTranslator(),
				new EntityCollectionCountTranslator(),
				new NullableHasValueTranslator(),
				new NullableValueTranslator(),
				new SpatialPropertyTranslator()
			};
		}

		internal static bool CanFuncletizePropertyInfo(PropertyInfo propertyInfo)
		{
			if (!GenericICollectionTranslator.TryGetPropertyTranslator(propertyInfo, out var propertyTranslator))
			{
				return !TryGetTranslator(propertyInfo, out propertyTranslator);
			}
			return true;
		}
	}

	internal static class StringTranslatorUtil
	{
		internal static IEnumerable<Expression> GetConcatArgs(Expression linq)
		{
			if (linq.IsStringAddExpression())
			{
				foreach (Expression concatArg in GetConcatArgs((BinaryExpression)linq))
				{
					yield return concatArg;
				}
			}
			else
			{
				yield return linq;
			}
		}

		internal static IEnumerable<Expression> GetConcatArgs(BinaryExpression linq)
		{
			foreach (Expression concatArg in GetConcatArgs(linq.Left))
			{
				yield return concatArg;
			}
			foreach (Expression concatArg2 in GetConcatArgs(linq.Right))
			{
				yield return concatArg2;
			}
		}

		internal static DbExpression ConcatArgs(ExpressionConverter parent, BinaryExpression linq)
		{
			return ConcatArgs(parent, linq, GetConcatArgs(linq).ToArray());
		}

		internal static DbExpression ConcatArgs(ExpressionConverter parent, Expression linq, Expression[] linqArgs)
		{
			DbExpression[] array = (from arg in linqArgs
				where !arg.IsNullConstant()
				select ConvertToString(parent, arg)).ToArray();
			if (array.Length == 0)
			{
				return DbExpressionBuilder.Constant(string.Empty);
			}
			DbExpression dbExpression = array.First();
			foreach (DbExpression item in array.Skip(1))
			{
				dbExpression = parent.CreateCanonicalFunction("Concat", linq, dbExpression, item);
			}
			return dbExpression;
		}

		internal static DbExpression StripNull(Expression sourceExpression, DbExpression inputExpression, DbExpression outputExpression, bool useDatabaseNullSemantics)
		{
			if (sourceExpression.IsNullConstant())
			{
				return DbExpressionBuilder.Constant(string.Empty);
			}
			if (sourceExpression.NodeType == ExpressionType.Constant)
			{
				return outputExpression;
			}
			if (useDatabaseNullSemantics)
			{
				return outputExpression;
			}
			return DbExpressionBuilder.Case(new DbIsNullExpression[1] { inputExpression.IsNull() }, new DbConstantExpression[1] { DbExpressionBuilder.Constant(string.Empty) }, outputExpression);
		}

		internal static DbExpression ConvertToString(ExpressionConverter parent, Expression linqExpression)
		{
			if (linqExpression.Type == typeof(object))
			{
				linqExpression = ((linqExpression is ConstantExpression constantExpression) ? Expression.Constant(constantExpression.Value) : linqExpression.RemoveConvert());
			}
			DbExpression expression = parent.TranslateExpression(linqExpression);
			Type nonNullableType = TypeSystem.GetNonNullableType(linqExpression.Type);
			bool useDatabaseNullSemantics = !parent._funcletizer.RootContext.ContextOptions.UseCSharpNullComparisonBehavior;
			if (nonNullableType.IsEnum)
			{
				if (Attribute.IsDefined(nonNullableType, typeof(FlagsAttribute)))
				{
					throw new NotSupportedException(Strings.Elinq_ToStringNotSupportedForEnumsWithFlags);
				}
				if (linqExpression.IsNullConstant())
				{
					return DbExpressionBuilder.Constant(string.Empty);
				}
				if (linqExpression.NodeType == ExpressionType.Constant)
				{
					object value = ((ConstantExpression)linqExpression).Value;
					return DbExpressionBuilder.Constant(Enum.GetName(nonNullableType, value) ?? value.ToString());
				}
				Type integralType = nonNullableType.GetEnumUnderlyingType();
				TypeUsage type = parent.GetValueLayerType(integralType);
				IEnumerable<DbExpression> whenExpressions = (from object v in nonNullableType.GetEnumValues()
					select System.Convert.ChangeType(v, integralType, CultureInfo.InvariantCulture) into v
					select DbExpressionBuilder.Constant(v)).Select((Func<DbConstantExpression, DbExpression>)((DbConstantExpression c) => expression.CastTo(type).Equal(c))).Concat(new DbIsNullExpression[1] { expression.CastTo(type).IsNull() });
				IEnumerable<DbConstantExpression> thenExpressions = (from s in nonNullableType.GetEnumNames()
					select DbExpressionBuilder.Constant(s)).Concat(new DbConstantExpression[1] { DbExpressionBuilder.Constant(string.Empty) });
				UnaryExpression linq = Expression.Convert(linqExpression, integralType);
				DbCastExpression elseExpression = parent.TranslateExpression(linq).CastTo(parent.GetValueLayerType(typeof(string)));
				return DbExpressionBuilder.Case(whenExpressions, thenExpressions, elseExpression);
			}
			if (TypeSemantics.IsPrimitiveType(expression.ResultType, PrimitiveTypeKind.String))
			{
				return StripNull(linqExpression, expression, expression, useDatabaseNullSemantics);
			}
			if (TypeSemantics.IsPrimitiveType(expression.ResultType, PrimitiveTypeKind.Guid))
			{
				return StripNull(linqExpression, expression, expression.CastTo(parent.GetValueLayerType(typeof(string))).ToLower(), useDatabaseNullSemantics);
			}
			if (TypeSemantics.IsPrimitiveType(expression.ResultType, PrimitiveTypeKind.Boolean))
			{
				if (linqExpression.IsNullConstant())
				{
					return DbExpressionBuilder.Constant(string.Empty);
				}
				if (linqExpression.NodeType == ExpressionType.Constant)
				{
					return DbExpressionBuilder.Constant(((ConstantExpression)linqExpression).Value.ToString());
				}
				DbComparisonExpression dbComparisonExpression = expression.Equal(DbExpressionBuilder.True);
				DbComparisonExpression dbComparisonExpression2 = expression.Equal(DbExpressionBuilder.False);
				DbConstantExpression dbConstantExpression = DbExpressionBuilder.Constant(true.ToString());
				DbConstantExpression dbConstantExpression2 = DbExpressionBuilder.Constant(false.ToString());
				return DbExpressionBuilder.Case(new DbComparisonExpression[2] { dbComparisonExpression, dbComparisonExpression2 }, new DbConstantExpression[2] { dbConstantExpression, dbConstantExpression2 }, DbExpressionBuilder.Constant(string.Empty));
			}
			if (!SupportsCastToString(expression.ResultType))
			{
				throw new NotSupportedException(Strings.Elinq_ToStringNotSupportedForType(expression.ResultType.EdmType.Name));
			}
			return StripNull(linqExpression, expression, expression.CastTo(parent.GetValueLayerType(typeof(string))), useDatabaseNullSemantics);
		}

		internal static bool SupportsCastToString(TypeUsage typeUsage)
		{
			if (!TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.String) && !TypeSemantics.IsNumericType(typeUsage) && !TypeSemantics.IsBooleanType(typeUsage) && !TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.DateTime) && !TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.DateTimeOffset) && !TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.Time))
			{
				return TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.Guid);
			}
			return true;
		}
	}

	internal abstract class Translator
	{
		private readonly ExpressionType[] _nodeTypes;

		internal IEnumerable<ExpressionType> NodeTypes => _nodeTypes;

		protected Translator(params ExpressionType[] nodeTypes)
		{
			_nodeTypes = nodeTypes;
		}

		internal abstract DbExpression Translate(ExpressionConverter parent, Expression linq);

		public override string ToString()
		{
			return GetType().Name;
		}
	}

	internal abstract class TypedTranslator<T_Linq> : Translator where T_Linq : Expression
	{
		protected TypedTranslator(params ExpressionType[] nodeTypes)
			: base(nodeTypes)
		{
		}

		internal override DbExpression Translate(ExpressionConverter parent, Expression linq)
		{
			return TypedTranslate(parent, (T_Linq)linq);
		}

		protected abstract DbExpression TypedTranslate(ExpressionConverter parent, T_Linq linq);
	}

	private sealed class ConstantTranslator : TypedTranslator<ConstantExpression>
	{
		internal ConstantTranslator()
			: base(new ExpressionType[1] { ExpressionType.Constant })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, ConstantExpression linq)
		{
			if (linq == parent._funcletizer.RootContextExpression)
			{
				throw new InvalidOperationException(Strings.ELinq_UnsupportedUseOfContextParameter(parent._funcletizer.RootContextParameter.Name));
			}
			ObjectQuery objectQuery = (linq.Value as IQueryable).TryGetObjectQuery();
			if (objectQuery != null)
			{
				return parent.TranslateInlineQueryOfT(objectQuery);
			}
			if (linq.Value is IEnumerable enumerable)
			{
				Type elementType = TypeSystem.GetElementType(linq.Type);
				if (elementType != null && elementType != linq.Type)
				{
					List<Expression> list = new List<Expression>();
					foreach (object item in enumerable)
					{
						list.Add(Expression.Constant(item, elementType));
					}
					parent._recompileRequired = () => true;
					return parent.TranslateExpression(Expression.NewArrayInit(elementType, list));
				}
			}
			bool flag = linq.Value == null;
			bool flag2 = false;
			Type type = linq.Type;
			if (type == typeof(Enum))
			{
				type = linq.Value.GetType();
			}
			if (parent.TryGetValueLayerType(type, out var type2) && (Helper.IsScalarType(type2.EdmType) || (flag && Helper.IsEntityType(type2.EdmType))))
			{
				flag2 = true;
			}
			if (!flag2)
			{
				if (flag)
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedNullConstant(DescribeClrType(linq.Type)));
				}
				throw new NotSupportedException(Strings.ELinq_UnsupportedConstant(DescribeClrType(linq.Type)));
			}
			if (flag)
			{
				return type2.Null();
			}
			object value = linq.Value;
			if (Helper.IsPrimitiveType(type2.EdmType))
			{
				Type nonNullableType = TypeSystem.GetNonNullableType(type);
				if (nonNullableType.IsEnum())
				{
					value = System.Convert.ChangeType(linq.Value, nonNullableType.GetEnumUnderlyingType(), CultureInfo.InvariantCulture);
				}
			}
			return type2.Constant(value);
		}
	}

	private sealed class ParameterTranslator : TypedTranslator<ParameterExpression>
	{
		internal ParameterTranslator()
			: base(new ExpressionType[1] { ExpressionType.Parameter })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, ParameterExpression linq)
		{
			throw new InvalidOperationException(Strings.ELinq_UnboundParameterExpression(linq.Name));
		}
	}

	private sealed class NewTranslator : TypedTranslator<NewExpression>
	{
		private List<Tuple<Type, Type>> _castableTypes = new List<Tuple<Type, Type>>
		{
			new Tuple<Type, Type>(typeof(Guid), typeof(string))
		};

		internal NewTranslator()
			: base(new ExpressionType[1] { ExpressionType.New })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, NewExpression linq)
		{
			int num = ((linq.Members != null) ? linq.Members.Count : 0);
			if (linq.Arguments.Count == 1 && _castableTypes.Any((Tuple<Type, Type> cast) => cast.Item1 == linq.Constructor.DeclaringType && cast.Item2 == linq.Arguments[0].Type))
			{
				return parent.CreateCastExpression(parent.TranslateExpression(linq.Arguments[0]), linq.Constructor.DeclaringType, linq.Arguments[0].Type);
			}
			if (null == linq.Constructor || linq.Arguments.Count != num)
			{
				throw new NotSupportedException(Strings.ELinq_UnsupportedConstructor);
			}
			parent.CheckInitializerType(linq.Type);
			List<KeyValuePair<string, DbExpression>> list = new List<KeyValuePair<string, DbExpression>>(num + 1);
			HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
			for (int i = 0; i < num; i++)
			{
				TypeSystem.PropertyOrField(linq.Members[i], out var name, out var _);
				DbExpression value = parent.TranslateExpression(linq.Arguments[i]);
				hashSet.Add(name);
				list.Add(new KeyValuePair<string, DbExpression>(name, value));
			}
			InitializerMetadata initializerMetadata;
			if (num == 0)
			{
				list.Add(DbExpressionBuilder.True.As("Key"));
				initializerMetadata = InitializerMetadata.CreateEmptyProjectionInitializer(parent.EdmItemCollection, linq);
			}
			else
			{
				initializerMetadata = InitializerMetadata.CreateProjectionInitializer(parent.EdmItemCollection, linq);
			}
			parent.ValidateInitializerMetadata(initializerMetadata);
			return CreateNewRowExpression(list, initializerMetadata);
		}
	}

	private sealed class NewArrayInitTranslator : TypedTranslator<NewArrayExpression>
	{
		internal NewArrayInitTranslator()
			: base(new ExpressionType[1] { ExpressionType.NewArrayInit })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, NewArrayExpression linq)
		{
			if (linq.Expressions.Count > 0)
			{
				return DbExpressionBuilder.NewCollection(linq.Expressions.Select((Expression e) => parent.TranslateExpression(e)));
			}
			TypeUsage collectionType;
			if (typeof(byte[]) == linq.Type)
			{
				if (parent.TryGetValueLayerType(typeof(byte), out var type))
				{
					collectionType = TypeHelpers.CreateCollectionTypeUsage(type);
					return collectionType.NewEmptyCollection();
				}
			}
			else if (parent.TryGetValueLayerType(linq.Type, out collectionType))
			{
				return collectionType.NewEmptyCollection();
			}
			throw new NotSupportedException(Strings.ELinq_UnsupportedType(DescribeClrType(linq.Type)));
		}
	}

	private sealed class ListInitTranslator : TypedTranslator<ListInitExpression>
	{
		internal ListInitTranslator()
			: base(new ExpressionType[1] { ExpressionType.ListInit })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, ListInitExpression linq)
		{
			if (linq.NewExpression.Constructor != null && linq.NewExpression.Constructor.GetParameters().Length != 0)
			{
				throw new NotSupportedException(Strings.ELinq_UnsupportedConstructor);
			}
			if (linq.Initializers.Any((ElementInit i) => i.Arguments.Count != 1))
			{
				throw new NotSupportedException(Strings.ELinq_UnsupportedInitializers);
			}
			return DbExpressionBuilder.NewCollection(linq.Initializers.Select((ElementInit i) => parent.TranslateExpression(i.Arguments[0])));
		}
	}

	private sealed class MemberInitTranslator : TypedTranslator<MemberInitExpression>
	{
		internal MemberInitTranslator()
			: base(new ExpressionType[1] { ExpressionType.MemberInit })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, MemberInitExpression linq)
		{
			if (null == linq.NewExpression.Constructor || linq.NewExpression.Constructor.GetParameters().Length != 0)
			{
				throw new NotSupportedException(Strings.ELinq_UnsupportedConstructor);
			}
			parent.CheckInitializerType(linq.Type);
			List<KeyValuePair<string, DbExpression>> list = new List<KeyValuePair<string, DbExpression>>(linq.Bindings.Count + 1);
			MemberInfo[] array = new MemberInfo[linq.Bindings.Count];
			HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
			for (int i = 0; i < linq.Bindings.Count; i++)
			{
				if (!(linq.Bindings[i] is MemberAssignment memberAssignment))
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedBinding);
				}
				string name;
				Type type;
				MemberInfo memberInfo = TypeSystem.PropertyOrField(memberAssignment.Member, out name, out type);
				DbExpression value = parent.TranslateExpression(memberAssignment.Expression);
				hashSet.Add(name);
				array[i] = memberInfo;
				list.Add(new KeyValuePair<string, DbExpression>(name, value));
			}
			InitializerMetadata initializerMetadata;
			if (list.Count == 0)
			{
				list.Add(DbExpressionBuilder.Constant(true).As("Key"));
				initializerMetadata = InitializerMetadata.CreateEmptyProjectionInitializer(parent.EdmItemCollection, linq.NewExpression);
			}
			else
			{
				initializerMetadata = InitializerMetadata.CreateProjectionInitializer(parent.EdmItemCollection, linq);
			}
			parent.ValidateInitializerMetadata(initializerMetadata);
			return CreateNewRowExpression(list, initializerMetadata);
		}
	}

	private sealed class ConditionalTranslator : TypedTranslator<ConditionalExpression>
	{
		internal ConditionalTranslator()
			: base(new ExpressionType[1] { ExpressionType.Conditional })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, ConditionalExpression linq)
		{
			DbExpression item = parent.TranslateExpression(linq.Test);
			DbExpression dbExpression;
			DbExpression dbExpression2;
			if (!linq.IfTrue.IsNullConstant())
			{
				dbExpression = parent.TranslateExpression(linq.IfTrue);
				dbExpression2 = ((!linq.IfFalse.IsNullConstant()) ? parent.TranslateExpression(linq.IfFalse) : dbExpression.ResultType.Null());
			}
			else
			{
				if (linq.IfFalse.IsNullConstant())
				{
					throw new NotSupportedException(Strings.ELinq_UnsupportedNullConstant(DescribeClrType(linq.Type)));
				}
				dbExpression2 = parent.TranslateExpression(linq.IfFalse);
				dbExpression = dbExpression2.ResultType.Null();
			}
			return DbExpressionBuilder.Case(new List<DbExpression> { item }, new List<DbExpression> { dbExpression }, dbExpression2);
		}
	}

	private sealed class NotSupportedTranslator : Translator
	{
		internal NotSupportedTranslator(params ExpressionType[] nodeTypes)
			: base(nodeTypes)
		{
		}

		internal override DbExpression Translate(ExpressionConverter parent, Expression linq)
		{
			throw new NotSupportedException(Strings.ELinq_UnsupportedExpressionType(linq.NodeType));
		}
	}

	private sealed class ExtensionTranslator : Translator
	{
		internal ExtensionTranslator()
			: base((ExpressionType)(-1))
		{
		}

		internal override DbExpression Translate(ExpressionConverter parent, Expression linq)
		{
			if (!(linq is QueryParameterExpression queryParameterExpression))
			{
				throw new NotSupportedException(Strings.ELinq_UnsupportedExpressionType(linq.NodeType));
			}
			parent.AddParameter(queryParameterExpression);
			return queryParameterExpression.ParameterReference;
		}
	}

	private abstract class BinaryTranslator : TypedTranslator<BinaryExpression>
	{
		protected BinaryTranslator(params ExpressionType[] nodeTypes)
			: base(nodeTypes)
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, BinaryExpression linq)
		{
			return TranslateBinary(parent, parent.TranslateExpression(linq.Left), parent.TranslateExpression(linq.Right), linq);
		}

		protected abstract DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq);
	}

	private sealed class CoalesceTranslator : BinaryTranslator
	{
		internal CoalesceTranslator()
			: base(ExpressionType.Coalesce)
		{
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			DbExpression item = CreateIsNullExpression(left, linq.Left.Type);
			return DbExpressionBuilder.Case(new List<DbExpression>(1) { item }, new List<DbExpression>(1) { right }, left);
		}
	}

	private sealed class AndAlsoTranslator : BinaryTranslator
	{
		internal AndAlsoTranslator()
			: base(ExpressionType.AndAlso)
		{
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			return left.And(right);
		}
	}

	private sealed class OrElseTranslator : BinaryTranslator
	{
		internal OrElseTranslator()
			: base(ExpressionType.OrElse)
		{
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			return left.Or(right);
		}
	}

	private sealed class LessThanTranslator : BinaryTranslator
	{
		internal LessThanTranslator()
			: base(ExpressionType.LessThan)
		{
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			return left.LessThan(right);
		}
	}

	private sealed class LessThanOrEqualsTranslator : BinaryTranslator
	{
		internal LessThanOrEqualsTranslator()
			: base(ExpressionType.LessThanOrEqual)
		{
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			return left.LessThanOrEqual(right);
		}
	}

	private sealed class GreaterThanTranslator : BinaryTranslator
	{
		internal GreaterThanTranslator()
			: base(ExpressionType.GreaterThan)
		{
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			return left.GreaterThan(right);
		}
	}

	private sealed class GreaterThanOrEqualsTranslator : BinaryTranslator
	{
		internal GreaterThanOrEqualsTranslator()
			: base(ExpressionType.GreaterThanOrEqual)
		{
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			return left.GreaterThanOrEqual(right);
		}
	}

	private sealed class EqualsTranslator : TypedTranslator<BinaryExpression>
	{
		internal EqualsTranslator()
			: base(new ExpressionType[1] { ExpressionType.Equal })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, BinaryExpression linq)
		{
			Expression left = linq.Left;
			Expression right = linq.Right;
			bool flag = left.IsNullConstant();
			bool flag2 = right.IsNullConstant();
			if (flag && flag2)
			{
				return DbExpressionBuilder.True;
			}
			if (flag)
			{
				return CreateIsNullExpression(parent, right);
			}
			if (flag2)
			{
				return CreateIsNullExpression(parent, left);
			}
			DbExpression left2 = parent.TranslateExpression(left);
			DbExpression right2 = parent.TranslateExpression(right);
			EqualsPattern pattern = EqualsPattern.Store;
			if (parent._funcletizer.RootContext.ContextOptions.UseCSharpNullComparisonBehavior)
			{
				pattern = EqualsPattern.PositiveNullEqualityComposable;
			}
			return parent.CreateEqualsExpression(left2, right2, pattern, left.Type, right.Type);
		}

		private static DbExpression CreateIsNullExpression(ExpressionConverter parent, Expression input)
		{
			input = input.RemoveConvert();
			return ExpressionConverter.CreateIsNullExpression(parent.TranslateExpression(input), input.Type);
		}
	}

	private sealed class NotEqualsTranslator : TypedTranslator<BinaryExpression>
	{
		internal NotEqualsTranslator()
			: base(new ExpressionType[1] { ExpressionType.NotEqual })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, BinaryExpression linq)
		{
			Expression linq2 = Expression.Not(Expression.Equal(linq.Left, linq.Right));
			return parent.TranslateExpression(linq2);
		}
	}

	private sealed class IsTranslator : TypedTranslator<TypeBinaryExpression>
	{
		internal IsTranslator()
			: base(new ExpressionType[1] { ExpressionType.TypeIs })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, TypeBinaryExpression linq)
		{
			DbExpression argument = parent.TranslateExpression(linq.Expression);
			TypeUsage isOrAsTargetType = parent.GetIsOrAsTargetType(ExpressionType.TypeIs, linq.TypeOperand, linq.Expression.Type);
			return argument.IsOf(isOrAsTargetType);
		}
	}

	private sealed class AddTranslator : BinaryTranslator
	{
		internal AddTranslator()
			: base(ExpressionType.Add, ExpressionType.AddChecked)
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, BinaryExpression linq)
		{
			if (linq.IsStringAddExpression())
			{
				return StringTranslatorUtil.ConcatArgs(parent, linq);
			}
			return TranslateBinary(parent, parent.TranslateExpression(linq.Left), parent.TranslateExpression(linq.Right), linq);
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			return left.Plus(right);
		}
	}

	private sealed class DivideTranslator : BinaryTranslator
	{
		internal DivideTranslator()
			: base(ExpressionType.Divide)
		{
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			return left.Divide(right);
		}
	}

	private sealed class ModuloTranslator : BinaryTranslator
	{
		internal ModuloTranslator()
			: base(ExpressionType.Modulo)
		{
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			return left.Modulo(right);
		}
	}

	private sealed class MultiplyTranslator : BinaryTranslator
	{
		internal MultiplyTranslator()
			: base(ExpressionType.Multiply, ExpressionType.MultiplyChecked)
		{
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			return left.Multiply(right);
		}
	}

	private sealed class PowerTranslator : BinaryTranslator
	{
		internal PowerTranslator()
			: base(ExpressionType.Power)
		{
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			return left.Power(right);
		}
	}

	private sealed class SubtractTranslator : BinaryTranslator
	{
		internal SubtractTranslator()
			: base(ExpressionType.Subtract, ExpressionType.SubtractChecked)
		{
		}

		protected override DbExpression TranslateBinary(ExpressionConverter parent, DbExpression left, DbExpression right, BinaryExpression linq)
		{
			return left.Minus(right);
		}
	}

	private sealed class NegateTranslator : UnaryTranslator
	{
		internal NegateTranslator()
			: base(ExpressionType.Negate, ExpressionType.NegateChecked)
		{
		}

		protected override DbExpression TranslateUnary(ExpressionConverter parent, UnaryExpression unary, DbExpression operand)
		{
			return operand.UnaryMinus();
		}
	}

	private sealed class UnaryPlusTranslator : UnaryTranslator
	{
		internal UnaryPlusTranslator()
			: base(ExpressionType.UnaryPlus)
		{
		}

		protected override DbExpression TranslateUnary(ExpressionConverter parent, UnaryExpression unary, DbExpression operand)
		{
			return operand;
		}
	}

	private abstract class BitwiseBinaryTranslator : TypedTranslator<BinaryExpression>
	{
		private readonly string _canonicalFunctionName;

		protected BitwiseBinaryTranslator(ExpressionType nodeType, string canonicalFunctionName)
			: base(new ExpressionType[1] { nodeType })
		{
			_canonicalFunctionName = canonicalFunctionName;
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, BinaryExpression linq)
		{
			DbExpression dbExpression = parent.TranslateExpression(linq.Left);
			DbExpression dbExpression2 = parent.TranslateExpression(linq.Right);
			if (TypeSemantics.IsBooleanType(dbExpression.ResultType))
			{
				return TranslateIntoLogicExpression(parent, linq, dbExpression, dbExpression2);
			}
			return parent.CreateCanonicalFunction(_canonicalFunctionName, linq, dbExpression, dbExpression2);
		}

		protected abstract DbExpression TranslateIntoLogicExpression(ExpressionConverter parent, BinaryExpression linq, DbExpression left, DbExpression right);
	}

	private sealed class AndTranslator : BitwiseBinaryTranslator
	{
		internal AndTranslator()
			: base(ExpressionType.And, "BitwiseAnd")
		{
		}

		protected override DbExpression TranslateIntoLogicExpression(ExpressionConverter parent, BinaryExpression linq, DbExpression left, DbExpression right)
		{
			return left.And(right);
		}
	}

	private sealed class OrTranslator : BitwiseBinaryTranslator
	{
		internal OrTranslator()
			: base(ExpressionType.Or, "BitwiseOr")
		{
		}

		protected override DbExpression TranslateIntoLogicExpression(ExpressionConverter parent, BinaryExpression linq, DbExpression left, DbExpression right)
		{
			return left.Or(right);
		}
	}

	private sealed class ExclusiveOrTranslator : BitwiseBinaryTranslator
	{
		internal ExclusiveOrTranslator()
			: base(ExpressionType.ExclusiveOr, "BitwiseXor")
		{
		}

		protected override DbExpression TranslateIntoLogicExpression(ExpressionConverter parent, BinaryExpression linq, DbExpression left, DbExpression right)
		{
			DbAndExpression left2 = left.And(right.Not());
			DbExpression right2 = left.Not().And(right);
			return left2.Or(right2);
		}
	}

	private sealed class NotTranslator : TypedTranslator<UnaryExpression>
	{
		internal NotTranslator()
			: base(new ExpressionType[1] { ExpressionType.Not })
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, UnaryExpression linq)
		{
			DbExpression dbExpression = parent.TranslateExpression(linq.Operand);
			if (TypeSemantics.IsBooleanType(dbExpression.ResultType))
			{
				return dbExpression.Not();
			}
			return parent.CreateCanonicalFunction("BitwiseNot", linq, dbExpression);
		}
	}

	private abstract class UnaryTranslator : TypedTranslator<UnaryExpression>
	{
		protected UnaryTranslator(params ExpressionType[] nodeTypes)
			: base(nodeTypes)
		{
		}

		protected override DbExpression TypedTranslate(ExpressionConverter parent, UnaryExpression linq)
		{
			return TranslateUnary(parent, linq, parent.TranslateExpression(linq.Operand));
		}

		protected abstract DbExpression TranslateUnary(ExpressionConverter parent, UnaryExpression unary, DbExpression operand);
	}

	private sealed class QuoteTranslator : UnaryTranslator
	{
		internal QuoteTranslator()
			: base(ExpressionType.Quote)
		{
		}

		protected override DbExpression TranslateUnary(ExpressionConverter parent, UnaryExpression unary, DbExpression operand)
		{
			return operand;
		}
	}

	private sealed class ConvertTranslator : UnaryTranslator
	{
		internal ConvertTranslator()
			: base(ExpressionType.Convert, ExpressionType.ConvertChecked)
		{
		}

		protected override DbExpression TranslateUnary(ExpressionConverter parent, UnaryExpression unary, DbExpression operand)
		{
			Type type = unary.Type;
			Type type2 = unary.Operand.Type;
			return parent.CreateCastExpression(operand, type, type2);
		}
	}

	private sealed class AsTranslator : UnaryTranslator
	{
		internal AsTranslator()
			: base(ExpressionType.TypeAs)
		{
		}

		protected override DbExpression TranslateUnary(ExpressionConverter parent, UnaryExpression unary, DbExpression operand)
		{
			TypeUsage isOrAsTargetType = parent.GetIsOrAsTargetType(ExpressionType.TypeAs, unary.Type, unary.Operand.Type);
			return operand.TreatAs(isOrAsTargetType);
		}
	}

	private readonly Funcletizer _funcletizer;

	private readonly Perspective _perspective;

	private readonly Expression _expression;

	private readonly BindingContext _bindingContext;

	private Func<bool> _recompileRequired;

	private List<Tuple<ObjectParameter, QueryParameterExpression>> _parameters;

	private Dictionary<DbExpression, Span> _spanMappings;

	private MergeOption? _mergeOption;

	private Dictionary<Type, InitializerMetadata> _initializers;

	private Span _span;

	private HashSet<ObjectQuery> _inlineEntitySqlQueries;

	private int _ignoreInclude;

	private readonly AliasGenerator _aliasGenerator = new AliasGenerator("LQ", 0);

	private readonly OrderByLifter _orderByLifter;

	private const string s_visualBasicAssemblyFullName = "Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	private static readonly Dictionary<ExpressionType, Translator> _translators = InitializeTranslators();

	internal const string KeyColumnName = "Key";

	internal const string GroupColumnName = "Group";

	internal const string EntityCollectionOwnerColumnName = "Owner";

	internal const string EntityCollectionElementsColumnName = "Elements";

	internal const string EdmNamespaceName = "Edm";

	private const string Concat = "Concat";

	private const string IndexOf = "IndexOf";

	private const string Length = "Length";

	private const string Right = "Right";

	private const string Substring = "Substring";

	private const string ToUpper = "ToUpper";

	private const string ToLower = "ToLower";

	private const string Trim = "Trim";

	private const string LTrim = "LTrim";

	private const string RTrim = "RTrim";

	private const string Reverse = "Reverse";

	private const string BitwiseAnd = "BitwiseAnd";

	private const string BitwiseOr = "BitwiseOr";

	private const string BitwiseNot = "BitwiseNot";

	private const string BitwiseXor = "BitwiseXor";

	private const string CurrentUtcDateTime = "CurrentUtcDateTime";

	private const string CurrentDateTimeOffset = "CurrentDateTimeOffset";

	private const string CurrentDateTime = "CurrentDateTime";

	private const string Year = "Year";

	private const string Month = "Month";

	private const string Day = "Day";

	private const string Hour = "Hour";

	private const string Minute = "Minute";

	private const string Second = "Second";

	private const string Millisecond = "Millisecond";

	private const string Like = "Like";

	private const string AsUnicode = "AsUnicode";

	private const string AsNonUnicode = "AsNonUnicode";

	private EdmItemCollection EdmItemCollection => (EdmItemCollection)_funcletizer.RootContext.MetadataWorkspace.GetItemCollection(DataSpace.CSpace, required: true);

	internal DbProviderManifest ProviderManifest => ((StoreItemCollection)_funcletizer.RootContext.MetadataWorkspace.GetItemCollection(DataSpace.SSpace)).ProviderManifest;

	internal MergeOption? PropagatedMergeOption => _mergeOption;

	internal Span PropagatedSpan => _span;

	internal Func<bool> RecompileRequired => _recompileRequired;

	internal int IgnoreInclude
	{
		get
		{
			return _ignoreInclude;
		}
		set
		{
			_ignoreInclude = value;
		}
	}

	internal AliasGenerator AliasGenerator => _aliasGenerator;

	internal ExpressionConverter(Funcletizer funcletizer, Expression expression)
	{
		_funcletizer = funcletizer;
		expression = funcletizer.Funcletize(expression, out _recompileRequired);
		LinqExpressionNormalizer linqExpressionNormalizer = new LinqExpressionNormalizer();
		_expression = linqExpressionNormalizer.Visit(expression);
		_perspective = funcletizer.RootContext.Perspective;
		_bindingContext = new BindingContext();
		_ignoreInclude = 0;
		_orderByLifter = new OrderByLifter(_aliasGenerator);
	}

	private static Dictionary<ExpressionType, Translator> InitializeTranslators()
	{
		Dictionary<ExpressionType, Translator> dictionary = new Dictionary<ExpressionType, Translator>();
		foreach (Translator translator in GetTranslators())
		{
			foreach (ExpressionType nodeType in translator.NodeTypes)
			{
				dictionary.Add(nodeType, translator);
			}
		}
		return dictionary;
	}

	private static IEnumerable<Translator> GetTranslators()
	{
		yield return new AndAlsoTranslator();
		yield return new OrElseTranslator();
		yield return new LessThanTranslator();
		yield return new LessThanOrEqualsTranslator();
		yield return new GreaterThanTranslator();
		yield return new GreaterThanOrEqualsTranslator();
		yield return new EqualsTranslator();
		yield return new NotEqualsTranslator();
		yield return new ConvertTranslator();
		yield return new ConstantTranslator();
		yield return new NotTranslator();
		yield return new MemberAccessTranslator();
		yield return new ParameterTranslator();
		yield return new MemberInitTranslator();
		yield return new NewTranslator();
		yield return new AddTranslator();
		yield return new ConditionalTranslator();
		yield return new DivideTranslator();
		yield return new ModuloTranslator();
		yield return new SubtractTranslator();
		yield return new MultiplyTranslator();
		yield return new PowerTranslator();
		yield return new NegateTranslator();
		yield return new UnaryPlusTranslator();
		yield return new MethodCallTranslator();
		yield return new CoalesceTranslator();
		yield return new AsTranslator();
		yield return new IsTranslator();
		yield return new QuoteTranslator();
		yield return new AndTranslator();
		yield return new OrTranslator();
		yield return new ExclusiveOrTranslator();
		yield return new ExtensionTranslator();
		yield return new NewArrayInitTranslator();
		yield return new ListInitTranslator();
		yield return new NotSupportedTranslator(ExpressionType.LeftShift, ExpressionType.RightShift, ExpressionType.ArrayLength, ExpressionType.ArrayIndex, ExpressionType.Invoke, ExpressionType.Lambda, ExpressionType.NewArrayBounds);
	}

	internal IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> GetParameters()
	{
		if (_parameters != null)
		{
			return _parameters;
		}
		return null;
	}

	internal DbExpression Convert()
	{
		DbExpression dbExpression = TranslateExpression(_expression);
		if (!TryGetSpan(dbExpression, out _span))
		{
			_span = null;
		}
		return dbExpression;
	}

	internal static bool CanFuncletizePropertyInfo(PropertyInfo propertyInfo)
	{
		return MemberAccessTranslator.CanFuncletizePropertyInfo(propertyInfo);
	}

	internal bool CanIncludeSpanInfo()
	{
		return _ignoreInclude == 0;
	}

	private void NotifyMergeOption(MergeOption mergeOption)
	{
		if (!_mergeOption.HasValue)
		{
			_mergeOption = mergeOption;
		}
	}

	internal void ValidateInitializerMetadata(InitializerMetadata metadata)
	{
		if (_initializers != null && _initializers.TryGetValue(metadata.ClrType, out var value))
		{
			if (!metadata.Equals(value))
			{
				throw new NotSupportedException(Strings.ELinq_UnsupportedHeterogeneousInitializers(DescribeClrType(metadata.ClrType)));
			}
			return;
		}
		if (_initializers == null)
		{
			_initializers = new Dictionary<Type, InitializerMetadata>();
		}
		_initializers.Add(metadata.ClrType, metadata);
	}

	private void AddParameter(QueryParameterExpression queryParameter)
	{
		if (_parameters == null)
		{
			_parameters = new List<Tuple<ObjectParameter, QueryParameterExpression>>();
		}
		if (!_parameters.Select((Tuple<ObjectParameter, QueryParameterExpression> p) => p.Item2).Contains(queryParameter))
		{
			ObjectParameter item = new ObjectParameter(queryParameter.ParameterReference.ParameterName, queryParameter.Type);
			_parameters.Add(new Tuple<ObjectParameter, QueryParameterExpression>(item, queryParameter));
		}
	}

	private bool IsQueryRoot(Expression Expression)
	{
		return _expression == Expression;
	}

	private DbExpression AddSpanMapping(DbExpression expression, Span span)
	{
		if (span != null && CanIncludeSpanInfo())
		{
			if (_spanMappings == null)
			{
				_spanMappings = new Dictionary<DbExpression, Span>();
			}
			Span value = null;
			if (_spanMappings.TryGetValue(expression, out value))
			{
				foreach (Span.SpanPath span2 in span.SpanList)
				{
					value.AddSpanPath(span2);
				}
				_spanMappings[expression] = value;
			}
			else
			{
				_spanMappings[expression] = span;
			}
		}
		return expression;
	}

	private bool TryGetSpan(DbExpression expression, out Span span)
	{
		if (_spanMappings != null)
		{
			return _spanMappings.TryGetValue(expression, out span);
		}
		span = null;
		return false;
	}

	private void ApplySpanMapping(DbExpression from, DbExpression to)
	{
		if (TryGetSpan(from, out var span))
		{
			AddSpanMapping(to, span);
		}
	}

	private void UnifySpanMappings(DbExpression left, DbExpression right, DbExpression to)
	{
		Span span = null;
		Span span2 = null;
		bool num = TryGetSpan(left, out span);
		bool flag = TryGetSpan(right, out span2);
		if (num || flag)
		{
			AddSpanMapping(to, Span.CopyUnion(span, span2));
		}
	}

	private DbDistinctExpression Distinct(DbExpression argument)
	{
		DbDistinctExpression dbDistinctExpression = argument.Distinct();
		ApplySpanMapping(argument, dbDistinctExpression);
		return dbDistinctExpression;
	}

	private DbExceptExpression Except(DbExpression left, DbExpression right)
	{
		DbExceptExpression dbExceptExpression = left.Except(right);
		ApplySpanMapping(left, dbExceptExpression);
		return dbExceptExpression;
	}

	private DbExpression Filter(DbExpressionBinding input, DbExpression predicate)
	{
		DbExpression dbExpression = _orderByLifter.Filter(input, predicate);
		ApplySpanMapping(input.Expression, dbExpression);
		return dbExpression;
	}

	private DbIntersectExpression Intersect(DbExpression left, DbExpression right)
	{
		DbIntersectExpression dbIntersectExpression = left.Intersect(right);
		UnifySpanMappings(left, right, dbIntersectExpression);
		return dbIntersectExpression;
	}

	private DbExpression Limit(DbExpression argument, DbExpression limit)
	{
		DbExpression dbExpression = _orderByLifter.Limit(argument, limit);
		ApplySpanMapping(argument, dbExpression);
		return dbExpression;
	}

	private DbExpression OfType(DbExpression argument, TypeUsage ofType)
	{
		DbExpression dbExpression = _orderByLifter.OfType(argument, ofType);
		ApplySpanMapping(argument, dbExpression);
		return dbExpression;
	}

	private DbExpression Project(DbExpressionBinding input, DbExpression projection)
	{
		DbExpression dbExpression = _orderByLifter.Project(input, projection);
		if (projection.ExpressionKind == DbExpressionKind.VariableReference && ((DbVariableReferenceExpression)projection).VariableName.Equals(input.VariableName, StringComparison.Ordinal))
		{
			ApplySpanMapping(input.Expression, dbExpression);
		}
		return dbExpression;
	}

	private DbSortExpression Sort(DbExpressionBinding input, IList<DbSortClause> keys)
	{
		DbSortExpression dbSortExpression = input.Sort(keys);
		ApplySpanMapping(input.Expression, dbSortExpression);
		return dbSortExpression;
	}

	private DbExpression Skip(DbExpressionBinding input, DbExpression skipCount)
	{
		DbExpression dbExpression = _orderByLifter.Skip(input, skipCount);
		ApplySpanMapping(input.Expression, dbExpression);
		return dbExpression;
	}

	private DbUnionAllExpression UnionAll(DbExpression left, DbExpression right)
	{
		DbUnionAllExpression dbUnionAllExpression = left.UnionAll(right);
		UnifySpanMappings(left, right, dbUnionAllExpression);
		return dbUnionAllExpression;
	}

	private TypeUsage GetCastTargetType(TypeUsage fromType, Type toClrType, Type fromClrType, bool preserveCastForDateTime)
	{
		if (fromClrType != null && fromClrType.IsGenericType() && toClrType.IsGenericType() && (fromClrType.GetGenericTypeDefinition() == typeof(ObjectQuery<>) || fromClrType.GetGenericTypeDefinition() == typeof(IQueryable<>) || fromClrType.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>)) && (toClrType.GetGenericTypeDefinition() == typeof(ObjectQuery<>) || toClrType.GetGenericTypeDefinition() == typeof(IQueryable<>) || toClrType.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>)) && fromClrType.GetGenericArguments()[0] == toClrType.GetGenericArguments()[0])
		{
			return null;
		}
		if (fromClrType != null && TypeSystem.GetNonNullableType(fromClrType).IsEnum && toClrType == typeof(Enum))
		{
			return null;
		}
		if (TryGetValueLayerType(toClrType, out var type) && CanOmitCast(fromType, type, preserveCastForDateTime))
		{
			return null;
		}
		return ValidateAndAdjustCastTypes(type, fromType, toClrType, fromClrType);
	}

	private static TypeUsage ValidateAndAdjustCastTypes(TypeUsage toType, TypeUsage fromType, Type toClrType, Type fromClrType)
	{
		if (toType == null || !TypeSemantics.IsScalarType(toType) || !TypeSemantics.IsScalarType(fromType))
		{
			throw new NotSupportedException(Strings.ELinq_UnsupportedCast(DescribeClrType(fromClrType), DescribeClrType(toClrType)));
		}
		PrimitiveTypeKind primitiveTypeKind = Helper.AsPrimitive(fromType.EdmType).PrimitiveTypeKind;
		if (Helper.AsPrimitive(toType.EdmType).PrimitiveTypeKind == PrimitiveTypeKind.Decimal)
		{
			if (primitiveTypeKind != PrimitiveTypeKind.Byte && (uint)(primitiveTypeKind - 8) > 3u)
			{
				throw new NotSupportedException(Strings.ELinq_UnsupportedCastToDecimal);
			}
			toType = TypeUsage.CreateDecimalTypeUsage((PrimitiveType)toType.EdmType, 19, 0);
		}
		return toType;
	}

	private static bool CanOmitCast(TypeUsage fromType, TypeUsage toType, bool preserveCastForDateTime)
	{
		bool flag = TypeSemantics.IsPrimitiveType(fromType);
		if (flag && preserveCastForDateTime && ((PrimitiveType)fromType.EdmType).PrimitiveTypeKind == PrimitiveTypeKind.DateTime)
		{
			return false;
		}
		if (TypeUsageEquals(fromType, toType))
		{
			return true;
		}
		if (flag)
		{
			return fromType.EdmType.EdmEquals(toType.EdmType);
		}
		return TypeSemantics.IsSubTypeOf(fromType, toType);
	}

	private TypeUsage GetIsOrAsTargetType(ExpressionType operationType, Type toClrType, Type fromClrType)
	{
		if (!TryGetValueLayerType(toClrType, out var type) || (!TypeSemantics.IsEntityType(type) && !TypeSemantics.IsComplexType(type)))
		{
			throw new NotSupportedException(Strings.ELinq_UnsupportedIsOrAs(operationType, DescribeClrType(fromClrType), DescribeClrType(toClrType)));
		}
		return type;
	}

	private DbExpression TranslateInlineQueryOfT(ObjectQuery inlineQuery)
	{
		if (_funcletizer.RootContext != inlineQuery.QueryState.ObjectContext)
		{
			throw new NotSupportedException(Strings.ELinq_UnsupportedDifferentContexts);
		}
		if (_inlineEntitySqlQueries == null)
		{
			_inlineEntitySqlQueries = new HashSet<ObjectQuery>();
		}
		bool flag = _inlineEntitySqlQueries.Add(inlineQuery);
		EntitySqlQueryState entitySqlQueryState = (EntitySqlQueryState)inlineQuery.QueryState;
		DbExpression dbExpression = null;
		ObjectParameterCollection parameters = inlineQuery.QueryState.Parameters;
		if (!_funcletizer.IsCompiledQuery || parameters == null || parameters.Count == 0)
		{
			if (flag && parameters != null)
			{
				if (_parameters == null)
				{
					_parameters = new List<Tuple<ObjectParameter, QueryParameterExpression>>();
				}
				foreach (ObjectParameter parameter in inlineQuery.QueryState.Parameters)
				{
					_parameters.Add(new Tuple<ObjectParameter, QueryParameterExpression>(parameter.ShallowCopy(), null));
				}
			}
			return entitySqlQueryState.Parse();
		}
		dbExpression = entitySqlQueryState.Parse();
		return ParameterReferenceRemover.RemoveParameterReferences(dbExpression, parameters);
	}

	private DbExpression CreateCastExpression(DbExpression source, Type toClrType, Type fromClrType)
	{
		DbExpression dbExpression = NormalizeSetSource(source);
		if (source != dbExpression && GetCastTargetType(dbExpression.ResultType, toClrType, fromClrType, preserveCastForDateTime: true) == null)
		{
			return source;
		}
		TypeUsage castTargetType = GetCastTargetType(source.ResultType, toClrType, fromClrType, preserveCastForDateTime: true);
		if (castTargetType == null)
		{
			return source;
		}
		return source.CastTo(castTargetType);
	}

	private DbExpression TranslateLambda(LambdaExpression lambda, DbExpression input, out DbExpressionBinding binding)
	{
		input = NormalizeSetSource(input);
		binding = input.BindAs(_aliasGenerator.Next());
		return TranslateLambda(lambda, binding.Variable);
	}

	private DbExpression TranslateLambda(LambdaExpression lambda, DbExpression input, string bindingName, out DbExpressionBinding binding)
	{
		input = NormalizeSetSource(input);
		binding = input.BindAs(bindingName);
		return TranslateLambda(lambda, binding.Variable);
	}

	private DbExpression TranslateLambda(LambdaExpression lambda, DbExpression input, out DbGroupExpressionBinding binding)
	{
		input = NormalizeSetSource(input);
		string text = _aliasGenerator.Next();
		binding = input.GroupBindAs(text, string.Format(CultureInfo.InvariantCulture, "Group{0}", new object[1] { text }));
		return TranslateLambda(lambda, binding.Variable);
	}

	private DbExpression TranslateLambda(LambdaExpression lambda, DbExpression input)
	{
		Binding binding = new Binding(lambda.Parameters[0], input);
		_bindingContext.PushBindingScope(binding);
		_ignoreInclude++;
		DbExpression result = TranslateExpression(lambda.Body);
		_ignoreInclude--;
		_bindingContext.PopBindingScope();
		return result;
	}

	private DbExpression NormalizeSetSource(DbExpression input)
	{
		if (input.ExpressionKind == DbExpressionKind.Project && !TryGetSpan(input, out var _))
		{
			DbProjectExpression dbProjectExpression = (DbProjectExpression)input;
			if (dbProjectExpression.Projection == dbProjectExpression.Input.Variable)
			{
				input = dbProjectExpression.Input.Expression;
			}
		}
		if (InitializerMetadata.TryGetInitializerMetadata(input.ResultType, out var initializerMetadata))
		{
			if (initializerMetadata.Kind == InitializerMetadataKind.Grouping)
			{
				input = input.Property("Group");
			}
			else if (initializerMetadata.Kind == InitializerMetadataKind.EntityCollection)
			{
				input = input.Property("Elements");
			}
		}
		return input;
	}

	private LambdaExpression GetLambdaExpression(MethodCallExpression callExpression, int argumentOrdinal)
	{
		Expression argument = callExpression.Arguments[argumentOrdinal];
		return (LambdaExpression)GetLambdaExpression(argument);
	}

	private Expression GetLambdaExpression(Expression argument)
	{
		if (ExpressionType.Lambda == argument.NodeType)
		{
			return argument;
		}
		if (ExpressionType.Quote == argument.NodeType)
		{
			return GetLambdaExpression(((UnaryExpression)argument).Operand);
		}
		if (ExpressionType.Call == argument.NodeType)
		{
			if (typeof(Expression).IsAssignableFrom(argument.Type))
			{
				Func<Expression> func = Expression.Lambda<Func<Expression>>(argument, new ParameterExpression[0]).Compile();
				return GetLambdaExpression(func());
			}
		}
		else if (ExpressionType.Invoke == argument.NodeType && typeof(Expression).IsAssignableFrom(argument.Type))
		{
			Func<Expression> func2 = Expression.Lambda<Func<Expression>>(argument, new ParameterExpression[0]).Compile();
			return GetLambdaExpression(func2());
		}
		throw new InvalidOperationException(Strings.ADP_InternalProviderError(1025));
	}

	private DbExpression TranslateSet(Expression linq)
	{
		return NormalizeSetSource(TranslateExpression(linq));
	}

	private DbExpression TranslateExpression(Expression linq)
	{
		if (!_bindingContext.TryGetBoundExpression(linq, out var cqtExpression))
		{
			if (_translators.TryGetValue(linq.NodeType, out var value))
			{
				return value.Translate(this, linq);
			}
			throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UnknownLinqNodeType, -1, linq.NodeType.ToString());
		}
		return cqtExpression;
	}

	private DbExpression AlignTypes(DbExpression cqt, Type toClrType)
	{
		Type fromClrType = null;
		TypeUsage castTargetType = GetCastTargetType(cqt.ResultType, toClrType, fromClrType, preserveCastForDateTime: false);
		if (castTargetType != null)
		{
			return cqt.CastTo(castTargetType);
		}
		return cqt;
	}

	private void CheckInitializerType(Type type)
	{
		if (_funcletizer.RootContext.Perspective.TryGetType(type, out var outTypeUsage))
		{
			BuiltInTypeKind builtInTypeKind = outTypeUsage.EdmType.BuiltInTypeKind;
			if (BuiltInTypeKind.EntityType == builtInTypeKind || BuiltInTypeKind.ComplexType == builtInTypeKind)
			{
				throw new NotSupportedException(Strings.ELinq_UnsupportedNominalType(outTypeUsage.EdmType.FullName));
			}
		}
		if (TypeSystem.IsSequenceType(type))
		{
			throw new NotSupportedException(Strings.ELinq_UnsupportedEnumerableType(DescribeClrType(type)));
		}
	}

	private static bool TypeUsageEquals(TypeUsage left, TypeUsage right)
	{
		if (left.EdmType.EdmEquals(right.EdmType))
		{
			return true;
		}
		if (BuiltInTypeKind.CollectionType == left.EdmType.BuiltInTypeKind && BuiltInTypeKind.CollectionType == right.EdmType.BuiltInTypeKind)
		{
			return TypeUsageEquals(((CollectionType)left.EdmType).TypeUsage, ((CollectionType)right.EdmType).TypeUsage);
		}
		if (BuiltInTypeKind.PrimitiveType == left.EdmType.BuiltInTypeKind && BuiltInTypeKind.PrimitiveType == right.EdmType.BuiltInTypeKind)
		{
			return ((PrimitiveType)left.EdmType).ClrEquivalentType.Equals(((PrimitiveType)right.EdmType).ClrEquivalentType);
		}
		return false;
	}

	private TypeUsage GetValueLayerType(Type linqType)
	{
		if (!TryGetValueLayerType(linqType, out var type))
		{
			throw new NotSupportedException(Strings.ELinq_UnsupportedType(linqType));
		}
		return type;
	}

	private bool TryGetValueLayerType(Type linqType, out TypeUsage type)
	{
		Type type2 = TypeSystem.GetNonNullableType(linqType);
		if (type2.IsEnum() && EdmItemCollection.EdmVersion < 3.0)
		{
			type2 = type2.GetEnumUnderlyingType();
		}
		if (ClrProviderManifest.TryGetPrimitiveTypeKind(type2, out var resolvedPrimitiveTypeKind))
		{
			type = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(resolvedPrimitiveTypeKind);
			return true;
		}
		Type elementType = TypeSystem.GetElementType(type2);
		if (elementType != type2 && TryGetValueLayerType(elementType, out var type3))
		{
			type = TypeHelpers.CreateCollectionTypeUsage(type3);
			return true;
		}
		_perspective.MetadataWorkspace.ImplicitLoadAssemblyForType(linqType, null);
		if (!_perspective.TryGetTypeByName(type2.FullNameWithNesting(), ignoreCase: false, out type) && type2.IsEnum() && ClrProviderManifest.TryGetPrimitiveTypeKind(type2.GetEnumUnderlyingType(), out resolvedPrimitiveTypeKind))
		{
			type = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(resolvedPrimitiveTypeKind);
		}
		return type != null;
	}

	private static void VerifyTypeSupportedForComparison(Type clrType, TypeUsage edmType, Stack<EdmMember> memberPath, bool isNullComparison)
	{
		switch (edmType.EdmType.BuiltInTypeKind)
		{
		case BuiltInTypeKind.EntityType:
		case BuiltInTypeKind.EnumType:
		case BuiltInTypeKind.PrimitiveType:
		case BuiltInTypeKind.RefType:
			return;
		case BuiltInTypeKind.RowType:
		{
			if (!InitializerMetadata.TryGetInitializerMetadata(edmType, out var initializerMetadata) || initializerMetadata.Kind == InitializerMetadataKind.ProjectionInitializer || initializerMetadata.Kind == InitializerMetadataKind.ProjectionNew)
			{
				if (!isNullComparison)
				{
					VerifyRowTypeSupportedForComparison(clrType, (RowType)edmType.EdmType, memberPath, isNullComparison);
				}
				return;
			}
			break;
		}
		}
		if (memberPath == null)
		{
			throw new NotSupportedException(Strings.ELinq_UnsupportedComparison(DescribeClrType(clrType)));
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (EdmMember item in memberPath)
		{
			stringBuilder.Append(Strings.ELinq_UnsupportedRowMemberComparison(item.Name));
		}
		stringBuilder.Append(Strings.ELinq_UnsupportedRowTypeComparison(DescribeClrType(clrType)));
		throw new NotSupportedException(Strings.ELinq_UnsupportedRowComparison(stringBuilder.ToString()));
	}

	private static void VerifyRowTypeSupportedForComparison(Type clrType, RowType rowType, Stack<EdmMember> memberPath, bool isNullComparison)
	{
		foreach (EdmProperty property in rowType.Properties)
		{
			if (memberPath == null)
			{
				memberPath = new Stack<EdmMember>();
			}
			memberPath.Push(property);
			VerifyTypeSupportedForComparison(clrType, property.TypeUsage, memberPath, isNullComparison);
			memberPath.Pop();
		}
	}

	internal static string DescribeClrType(Type clrType)
	{
		if (IsCSharpGeneratedClass(clrType.Name, "DisplayClass") || IsVBGeneratedClass(clrType.Name, "Closure"))
		{
			return Strings.ELinq_ClosureType;
		}
		if (IsCSharpGeneratedClass(clrType.Name, "AnonymousType") || IsVBGeneratedClass(clrType.Name, "AnonymousType"))
		{
			return Strings.ELinq_AnonymousType;
		}
		return clrType.FullName;
	}

	private static bool IsCSharpGeneratedClass(string typeName, string pattern)
	{
		if (typeName.Contains("<>") && typeName.Contains("__"))
		{
			return typeName.Contains(pattern);
		}
		return false;
	}

	private static bool IsVBGeneratedClass(string typeName, string pattern)
	{
		if (typeName.Contains("_") && typeName.Contains("$"))
		{
			return typeName.Contains(pattern);
		}
		return false;
	}

	private static DbExpression CreateIsNullExpression(DbExpression operand, Type operandClrType)
	{
		VerifyTypeSupportedForComparison(operandClrType, operand.ResultType, null, isNullComparison: true);
		return operand.IsNull();
	}

	private DbExpression CreateEqualsExpression(DbExpression left, DbExpression right, EqualsPattern pattern, Type leftClrType, Type rightClrType)
	{
		VerifyTypeSupportedForComparison(leftClrType, left.ResultType, null, isNullComparison: false);
		VerifyTypeSupportedForComparison(rightClrType, right.ResultType, null, isNullComparison: false);
		TypeUsage resultType = left.ResultType;
		TypeUsage resultType2 = right.ResultType;
		if (resultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.RefType && resultType2.EdmType.BuiltInTypeKind == BuiltInTypeKind.RefType && !TypeSemantics.TryGetCommonType(resultType, resultType2, out var _))
		{
			RefType obj = left.ResultType.EdmType as RefType;
			throw new NotSupportedException(Strings.ELinq_UnsupportedRefComparison(p1: (right.ResultType.EdmType as RefType).ElementType.FullName, p0: obj.ElementType.FullName));
		}
		return RecursivelyRewriteEqualsExpression(left, right, pattern);
	}

	private DbExpression RecursivelyRewriteEqualsExpression(DbExpression left, DbExpression right, EqualsPattern pattern)
	{
		RowType rowType = left.ResultType.EdmType as RowType;
		RowType rowType2 = right.ResultType.EdmType as RowType;
		if (rowType != null || rowType2 != null)
		{
			if (rowType != null && rowType2 != null)
			{
				DbExpression dbExpression = null;
				{
					foreach (EdmProperty property in rowType.Properties)
					{
						DbPropertyExpression left2 = left.Property(property);
						DbPropertyExpression right2 = right.Property(property);
						DbExpression dbExpression2 = RecursivelyRewriteEqualsExpression(left2, right2, pattern);
						dbExpression = ((dbExpression != null) ? dbExpression.And(dbExpression2) : dbExpression2);
					}
					return dbExpression;
				}
			}
			return DbExpressionBuilder.False;
		}
		if (!_funcletizer.RootContext.ContextOptions.UseCSharpNullComparisonBehavior)
		{
			return ImplementEquality(left, right, pattern);
		}
		return ImplementEquality(left, right, EqualsPattern.Store);
	}

	private DbExpression ImplementEquality(DbExpression left, DbExpression right, EqualsPattern pattern)
	{
		return left.ExpressionKind switch
		{
			DbExpressionKind.Constant => right.ExpressionKind switch
			{
				DbExpressionKind.Constant => left.Equal(right), 
				DbExpressionKind.Null => DbExpressionBuilder.False, 
				_ => ImplementEqualityConstantAndUnknown((DbConstantExpression)left, right, pattern), 
			}, 
			DbExpressionKind.Null => right.ExpressionKind switch
			{
				DbExpressionKind.Constant => DbExpressionBuilder.False, 
				DbExpressionKind.Null => DbExpressionBuilder.True, 
				_ => right.IsNull(), 
			}, 
			_ => right.ExpressionKind switch
			{
				DbExpressionKind.Constant => ImplementEqualityConstantAndUnknown((DbConstantExpression)right, left, pattern), 
				DbExpressionKind.Null => left.IsNull(), 
				_ => ImplementEqualityUnknownArguments(left, right, pattern), 
			}, 
		};
	}

	private DbExpression ImplementEqualityConstantAndUnknown(DbConstantExpression constant, DbExpression unknown, EqualsPattern pattern)
	{
		switch (pattern)
		{
		case EqualsPattern.Store:
		case EqualsPattern.PositiveNullEqualityNonComposable:
			return constant.Equal(unknown);
		case EqualsPattern.PositiveNullEqualityComposable:
			if (!_funcletizer.RootContext.ContextOptions.UseCSharpNullComparisonBehavior)
			{
				return constant.Equal(unknown);
			}
			return constant.Equal(unknown).And(unknown.IsNull().Not());
		default:
			return null;
		}
	}

	private DbExpression ImplementEqualityUnknownArguments(DbExpression left, DbExpression right, EqualsPattern pattern)
	{
		switch (pattern)
		{
		case EqualsPattern.Store:
			return left.Equal(right);
		case EqualsPattern.PositiveNullEqualityNonComposable:
			return left.Equal(right).Or(left.IsNull().And(right.IsNull()));
		case EqualsPattern.PositiveNullEqualityComposable:
		{
			DbComparisonExpression left2 = left.Equal(right);
			DbAndExpression right2 = left.IsNull().And(right.IsNull());
			if (!_funcletizer.RootContext.ContextOptions.UseCSharpNullComparisonBehavior)
			{
				return left2.Or(right2);
			}
			DbOrExpression argument = left.IsNull().Or(right.IsNull());
			return left2.And(argument.Not()).Or(right2);
		}
		default:
			return null;
		}
	}

	private DbExpression TranslateLike(MethodCallExpression call)
	{
		char escapeCharacter;
		bool num = ProviderManifest.SupportsEscapingLikeArgument(out escapeCharacter);
		Expression linq = call.Arguments[0];
		Expression linq2 = call.Arguments[1];
		Expression expression = ((call.Arguments.Count > 2) ? call.Arguments[2] : null);
		if (!num && expression != null)
		{
			throw new ProviderIncompatibleException(Strings.ProviderDoesNotSupportEscapingLikeArgument);
		}
		DbExpression pattern = TranslateExpression(linq2);
		DbExpression escape = ((expression != null) ? TranslateExpression(expression) : null);
		DbExpression argument = TranslateExpression(linq);
		if (expression == null)
		{
			return argument.Like(pattern);
		}
		return argument.Like(pattern, escape);
	}

	private DbExpression TranslateFunctionIntoLike(MethodCallExpression call, bool insertPercentAtStart, bool insertPercentAtEnd, Func<ExpressionConverter, MethodCallExpression, DbExpression, DbExpression, DbExpression> defaultTranslator)
	{
		char escapeCharacter;
		bool flag = ProviderManifest.SupportsEscapingLikeArgument(out escapeCharacter);
		bool flag2 = false;
		bool flag3 = true;
		Expression expression = call.Arguments[0];
		Expression @object = call.Object;
		QueryParameterExpression queryParameterExpression = expression as QueryParameterExpression;
		if (flag && queryParameterExpression != null)
		{
			flag2 = true;
			MethodInfo? method = typeof(ExpressionConverter).GetMethod("PreparePattern", BindingFlags.Static | BindingFlags.NonPublic);
			ParameterExpression parameterExpression = Expression.Parameter(typeof(string), "input");
			Expression<Func<string, Tuple<string, bool>>> method2 = Expression.Lambda<Func<string, Tuple<string, bool>>>(Expression.Call(method, parameterExpression, Expression.Constant(insertPercentAtStart), Expression.Constant(insertPercentAtEnd), Expression.Constant(ProviderManifest)), new ParameterExpression[1] { parameterExpression });
			expression = queryParameterExpression.EscapeParameterForLike(method2);
		}
		DbExpression dbExpression = TranslateExpression(expression);
		DbExpression dbExpression2 = TranslateExpression(@object);
		if (flag && dbExpression.ExpressionKind == DbExpressionKind.Constant)
		{
			flag2 = true;
			DbConstantExpression obj = (DbConstantExpression)dbExpression;
			Tuple<string, bool> tuple = PreparePattern((string)obj.Value, insertPercentAtStart, insertPercentAtEnd, ProviderManifest);
			string item = tuple.Item1;
			flag3 = tuple.Item2;
			dbExpression = obj.ResultType.Constant(item);
		}
		if (flag2)
		{
			if (flag3)
			{
				DbConstantExpression escape = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.String).Constant(new string(new char[1] { escapeCharacter }));
				return dbExpression2.Like(dbExpression, escape);
			}
			return dbExpression2.Like(dbExpression);
		}
		return defaultTranslator(this, call, dbExpression, dbExpression2);
	}

	private static Tuple<string, bool> PreparePattern(string patternValue, bool insertPercentAtStart, bool insertPercentAtEnd, DbProviderManifest providerManifest)
	{
		if (patternValue == null)
		{
			return new Tuple<string, bool>(null, item2: false);
		}
		string text = providerManifest.EscapeLikeArgument(patternValue);
		if (text == null)
		{
			throw new ProviderIncompatibleException(Strings.ProviderEscapeLikeArgumentReturnedNull);
		}
		bool item = patternValue != text;
		StringBuilder stringBuilder = new StringBuilder();
		if (insertPercentAtStart)
		{
			stringBuilder.Append("%");
		}
		stringBuilder.Append(text);
		if (insertPercentAtEnd)
		{
			stringBuilder.Append("%");
		}
		return new Tuple<string, bool>(stringBuilder.ToString(), item);
	}

	private DbFunctionExpression TranslateIntoCanonicalFunction(string functionName, Expression Expression, params Expression[] linqArguments)
	{
		DbExpression[] array = new DbExpression[linqArguments.Length];
		for (int i = 0; i < linqArguments.Length; i++)
		{
			array[i] = TranslateExpression(linqArguments[i]);
		}
		return CreateCanonicalFunction(functionName, Expression, array);
	}

	private DbFunctionExpression CreateCanonicalFunction(string functionName, Expression Expression, params DbExpression[] translatedArguments)
	{
		List<TypeUsage> list = new List<TypeUsage>(translatedArguments.Length);
		foreach (DbExpression dbExpression in translatedArguments)
		{
			list.Add(dbExpression.ResultType);
		}
		return FindCanonicalFunction(functionName, list, isGroupAggregateFunction: false, Expression).Invoke(translatedArguments);
	}

	private EdmFunction FindCanonicalFunction(string functionName, IList<TypeUsage> argumentTypes, bool isGroupAggregateFunction, Expression Expression)
	{
		return FindFunction("Edm", functionName, argumentTypes, isGroupAggregateFunction, Expression);
	}

	private EdmFunction FindFunction(string namespaceName, string functionName, IList<TypeUsage> argumentTypes, bool isGroupAggregateFunction, Expression Expression)
	{
		if (!_perspective.TryGetFunctionByName(namespaceName, functionName, ignoreCase: false, out var functionOverloads))
		{
			ThrowUnresolvableFunction(Expression);
		}
		bool isAmbiguous;
		EdmFunction edmFunction = FunctionOverloadResolver.ResolveFunctionOverloads(functionOverloads, argumentTypes, isGroupAggregateFunction, out isAmbiguous);
		if (isAmbiguous || edmFunction == null)
		{
			ThrowUnresolvableFunctionOverload(Expression, isAmbiguous);
		}
		return edmFunction;
	}

	private static void ThrowUnresolvableFunction(Expression Expression)
	{
		if (Expression.NodeType == ExpressionType.Call)
		{
			MethodInfo method = ((MethodCallExpression)Expression).Method;
			throw new NotSupportedException(Strings.ELinq_UnresolvableFunctionForMethod(method, method.DeclaringType));
		}
		if (Expression.NodeType == ExpressionType.MemberAccess)
		{
			string name;
			Type type;
			MemberInfo memberInfo = TypeSystem.PropertyOrField(((MemberExpression)Expression).Member, out name, out type);
			throw new NotSupportedException(Strings.ELinq_UnresolvableFunctionForMember(memberInfo, memberInfo.DeclaringType));
		}
		throw new NotSupportedException(Strings.ELinq_UnresolvableFunctionForExpression(Expression.NodeType));
	}

	private static void ThrowUnresolvableFunctionOverload(Expression Expression, bool isAmbiguous)
	{
		if (Expression.NodeType == ExpressionType.Call)
		{
			MethodInfo method = ((MethodCallExpression)Expression).Method;
			if (isAmbiguous)
			{
				throw new NotSupportedException(Strings.ELinq_UnresolvableFunctionForMethodAmbiguousMatch(method, method.DeclaringType));
			}
			throw new NotSupportedException(Strings.ELinq_UnresolvableFunctionForMethodNotFound(method, method.DeclaringType));
		}
		if (Expression.NodeType == ExpressionType.MemberAccess)
		{
			string name;
			Type type;
			MemberInfo memberInfo = TypeSystem.PropertyOrField(((MemberExpression)Expression).Member, out name, out type);
			throw new NotSupportedException(Strings.ELinq_UnresolvableStoreFunctionForMember(memberInfo, memberInfo.DeclaringType));
		}
		throw new NotSupportedException(Strings.ELinq_UnresolvableStoreFunctionForExpression(Expression.NodeType));
	}

	private static DbNewInstanceExpression CreateNewRowExpression(List<KeyValuePair<string, DbExpression>> columns, InitializerMetadata initializerMetadata)
	{
		List<DbExpression> list = new List<DbExpression>(columns.Count);
		List<EdmProperty> list2 = new List<EdmProperty>(columns.Count);
		for (int i = 0; i < columns.Count; i++)
		{
			KeyValuePair<string, DbExpression> keyValuePair = columns[i];
			list.Add(keyValuePair.Value);
			list2.Add(new EdmProperty(keyValuePair.Key, keyValuePair.Value.ResultType));
		}
		return TypeUsage.Create(new RowType(list2, initializerMetadata)).New(list);
	}
}
