using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal class ExpressionPrinter : TreePrinter
{
	private class PrinterVisitor : DbExpressionVisitor<TreeNode>
	{
		private static readonly Dictionary<DbExpressionKind, string> _opMap = InitializeOpMap();

		private int _maxStringLength = 80;

		private bool _infix = true;

		private static Dictionary<DbExpressionKind, string> InitializeOpMap()
		{
			return new Dictionary<DbExpressionKind, string>(12)
			{
				[DbExpressionKind.Divide] = "/",
				[DbExpressionKind.Modulo] = "%",
				[DbExpressionKind.Multiply] = "*",
				[DbExpressionKind.Plus] = "+",
				[DbExpressionKind.Minus] = "-",
				[DbExpressionKind.UnaryMinus] = "-",
				[DbExpressionKind.Equals] = "=",
				[DbExpressionKind.LessThan] = "<",
				[DbExpressionKind.LessThanOrEquals] = "<=",
				[DbExpressionKind.GreaterThan] = ">",
				[DbExpressionKind.GreaterThanOrEquals] = ">=",
				[DbExpressionKind.NotEquals] = "<>"
			};
		}

		internal TreeNode VisitExpression(DbExpression expr)
		{
			return expr.Accept(this);
		}

		internal TreeNode VisitExpression(string name, DbExpression expr)
		{
			return new TreeNode(name, expr.Accept(this));
		}

		internal TreeNode VisitBinding(string propName, DbExpressionBinding binding)
		{
			return VisitWithLabel(propName, binding.VariableName, binding.Expression);
		}

		internal TreeNode VisitFunction(EdmFunction func, IList<DbExpression> args)
		{
			TreeNode treeNode = new TreeNode();
			AppendFullName(treeNode.Text, func);
			AppendParameters(treeNode, func.Parameters.Select((FunctionParameter fp) => new KeyValuePair<string, TypeUsage>(fp.Name, fp.TypeUsage)));
			if (args != null)
			{
				AppendArguments(treeNode, func.Parameters.Select((FunctionParameter fp) => fp.Name).ToArray(), args);
			}
			return treeNode;
		}

		private static TreeNode NodeFromExpression(DbExpression expr)
		{
			return new TreeNode(Enum.GetName(typeof(DbExpressionKind), expr.ExpressionKind));
		}

		private static void AppendParameters(TreeNode node, IEnumerable<KeyValuePair<string, TypeUsage>> paramInfos)
		{
			node.Text.Append("(");
			int num = 0;
			foreach (KeyValuePair<string, TypeUsage> paramInfo in paramInfos)
			{
				if (num > 0)
				{
					node.Text.Append(", ");
				}
				AppendType(node, paramInfo.Value);
				node.Text.Append(" ");
				node.Text.Append(paramInfo.Key);
				num++;
			}
			node.Text.Append(")");
		}

		internal static void AppendTypeSpecifier(TreeNode node, TypeUsage type)
		{
			node.Text.Append(" : ");
			AppendType(node, type);
		}

		internal static void AppendType(TreeNode node, TypeUsage type)
		{
			BuildTypeName(node.Text, type);
		}

		private static void BuildTypeName(StringBuilder text, TypeUsage type)
		{
			RowType rowType = type.EdmType as RowType;
			CollectionType collectionType = type.EdmType as CollectionType;
			RefType refType = type.EdmType as RefType;
			if (TypeSemantics.IsPrimitiveType(type))
			{
				text.Append(type);
			}
			else if (collectionType != null)
			{
				text.Append("Collection{");
				BuildTypeName(text, collectionType.TypeUsage);
				text.Append("}");
			}
			else if (refType != null)
			{
				text.Append("Ref<");
				AppendFullName(text, refType.ElementType);
				text.Append(">");
			}
			else if (rowType != null)
			{
				text.Append("Record[");
				int num = 0;
				foreach (EdmProperty property in rowType.Properties)
				{
					text.Append("'");
					text.Append(property.Name);
					text.Append("'");
					text.Append("=");
					BuildTypeName(text, property.TypeUsage);
					num++;
					if (num < rowType.Properties.Count)
					{
						text.Append(", ");
					}
				}
				text.Append("]");
			}
			else
			{
				if (!string.IsNullOrEmpty(type.EdmType.NamespaceName))
				{
					text.Append(type.EdmType.NamespaceName);
					text.Append(".");
				}
				text.Append(type.EdmType.Name);
			}
		}

		private static void AppendFullName(StringBuilder text, EdmType type)
		{
			if (BuiltInTypeKind.RowType != type.BuiltInTypeKind && !string.IsNullOrEmpty(type.NamespaceName))
			{
				text.Append(type.NamespaceName);
				text.Append(".");
			}
			text.Append(type.Name);
		}

		private List<TreeNode> VisitParams(IList<string> paramInfo, IList<DbExpression> args)
		{
			List<TreeNode> list = new List<TreeNode>();
			for (int i = 0; i < paramInfo.Count; i++)
			{
				TreeNode treeNode = new TreeNode(paramInfo[i]);
				treeNode.Children.Add(VisitExpression(args[i]));
				list.Add(treeNode);
			}
			return list;
		}

		private void AppendArguments(TreeNode node, IList<string> paramNames, IList<DbExpression> args)
		{
			if (paramNames.Count > 0)
			{
				node.Children.Add(new TreeNode("Arguments", VisitParams(paramNames, args)));
			}
		}

		private TreeNode VisitWithLabel(string label, string name, DbExpression def)
		{
			TreeNode treeNode = new TreeNode(label);
			treeNode.Text.Append(" : '");
			treeNode.Text.Append(name);
			treeNode.Text.Append("'");
			treeNode.Children.Add(VisitExpression(def));
			return treeNode;
		}

		private TreeNode VisitBindingList(string propName, IList<DbExpressionBinding> bindings)
		{
			List<TreeNode> list = new List<TreeNode>();
			for (int i = 0; i < bindings.Count; i++)
			{
				list.Add(VisitBinding(StringUtil.FormatIndex(propName, i), bindings[i]));
			}
			return new TreeNode(propName, list);
		}

		private TreeNode VisitGroupBinding(DbGroupExpressionBinding groupBinding)
		{
			TreeNode item = VisitExpression(groupBinding.Expression);
			TreeNode treeNode = new TreeNode();
			treeNode.Children.Add(item);
			treeNode.Text.AppendFormat(CultureInfo.InvariantCulture, "Input : '{0}', '{1}'", new object[2] { groupBinding.VariableName, groupBinding.GroupVariableName });
			return treeNode;
		}

		private TreeNode Visit(string name, params DbExpression[] exprs)
		{
			TreeNode treeNode = new TreeNode(name);
			foreach (DbExpression expr in exprs)
			{
				treeNode.Children.Add(VisitExpression(expr));
			}
			return treeNode;
		}

		private TreeNode VisitInfix(DbExpression left, string name, DbExpression right)
		{
			if (_infix)
			{
				TreeNode treeNode = new TreeNode("");
				treeNode.Children.Add(VisitExpression(left));
				treeNode.Children.Add(new TreeNode(name));
				treeNode.Children.Add(VisitExpression(right));
				return treeNode;
			}
			return Visit(name, left, right);
		}

		private TreeNode VisitUnary(DbUnaryExpression expr)
		{
			return VisitUnary(expr, appendType: false);
		}

		private TreeNode VisitUnary(DbUnaryExpression expr, bool appendType)
		{
			TreeNode treeNode = NodeFromExpression(expr);
			if (appendType)
			{
				AppendTypeSpecifier(treeNode, expr.ResultType);
			}
			treeNode.Children.Add(VisitExpression(expr.Argument));
			return treeNode;
		}

		private TreeNode VisitBinary(DbBinaryExpression expr)
		{
			TreeNode treeNode = NodeFromExpression(expr);
			treeNode.Children.Add(VisitExpression(expr.Left));
			treeNode.Children.Add(VisitExpression(expr.Right));
			return treeNode;
		}

		public override TreeNode Visit(DbExpression e)
		{
			Check.NotNull(e, "e");
			throw new NotSupportedException(Strings.Cqt_General_UnsupportedExpression(e.GetType().FullName));
		}

		public override TreeNode Visit(DbConstantExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = new TreeNode();
			if (e.Value is string text)
			{
				string text2 = text.Replace("\r\n", "\\r\\n");
				int num = text2.Length;
				if (_maxStringLength > 0)
				{
					num = Math.Min(text2.Length, _maxStringLength);
				}
				treeNode.Text.Append("'");
				treeNode.Text.Append(text2, 0, num);
				if (text2.Length > num)
				{
					treeNode.Text.Append("...");
				}
				treeNode.Text.Append("'");
			}
			else
			{
				treeNode.Text.Append(e.Value);
			}
			return treeNode;
		}

		public override TreeNode Visit(DbNullExpression e)
		{
			Check.NotNull(e, "e");
			return new TreeNode("null");
		}

		public override TreeNode Visit(DbVariableReferenceExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = new TreeNode();
			treeNode.Text.AppendFormat("Var({0})", e.VariableName);
			return treeNode;
		}

		public override TreeNode Visit(DbParameterReferenceExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = new TreeNode();
			treeNode.Text.AppendFormat("@{0}", e.ParameterName);
			return treeNode;
		}

		public override TreeNode Visit(DbFunctionExpression e)
		{
			Check.NotNull(e, "e");
			return VisitFunction(e.Function, e.Arguments);
		}

		public override TreeNode Visit(DbLambdaExpression expression)
		{
			Check.NotNull(expression, "expression");
			TreeNode treeNode = new TreeNode();
			treeNode.Text.Append("Lambda");
			AppendParameters(treeNode, expression.Lambda.Variables.Select((DbVariableReferenceExpression v) => new KeyValuePair<string, TypeUsage>(v.VariableName, v.ResultType)));
			AppendArguments(treeNode, expression.Lambda.Variables.Select((DbVariableReferenceExpression v) => v.VariableName).ToArray(), expression.Arguments);
			treeNode.Children.Add(Visit("Body", expression.Lambda.Body));
			return treeNode;
		}

		public override TreeNode Visit(DbPropertyExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = null;
			if (e.Instance != null)
			{
				treeNode = VisitExpression(e.Instance);
				if (e.Instance.ExpressionKind == DbExpressionKind.VariableReference || (e.Instance.ExpressionKind == DbExpressionKind.Property && treeNode.Children.Count == 0))
				{
					treeNode.Text.Append(".");
					treeNode.Text.Append(e.Property.Name);
					return treeNode;
				}
			}
			TreeNode treeNode2 = new TreeNode(".");
			if (e.Property is EdmProperty edmProperty && !(edmProperty.DeclaringType is RowType))
			{
				AppendFullName(treeNode2.Text, edmProperty.DeclaringType);
				treeNode2.Text.Append(".");
			}
			treeNode2.Text.Append(e.Property.Name);
			if (treeNode != null)
			{
				treeNode2.Children.Add(new TreeNode("Instance", treeNode));
			}
			return treeNode2;
		}

		public override TreeNode Visit(DbComparisonExpression e)
		{
			Check.NotNull(e, "e");
			return VisitInfix(e.Left, _opMap[e.ExpressionKind], e.Right);
		}

		public override TreeNode Visit(DbLikeExpression e)
		{
			Check.NotNull(e, "e");
			return Visit("Like", e.Argument, e.Pattern, e.Escape);
		}

		public override TreeNode Visit(DbLimitExpression e)
		{
			Check.NotNull(e, "e");
			return Visit(e.WithTies ? "LimitWithTies" : "Limit", e.Argument, e.Limit);
		}

		public override TreeNode Visit(DbIsNullExpression e)
		{
			Check.NotNull(e, "e");
			return VisitUnary(e);
		}

		public override TreeNode Visit(DbArithmeticExpression e)
		{
			Check.NotNull(e, "e");
			if (DbExpressionKind.UnaryMinus == e.ExpressionKind)
			{
				return Visit(_opMap[e.ExpressionKind], e.Arguments[0]);
			}
			return VisitInfix(e.Arguments[0], _opMap[e.ExpressionKind], e.Arguments[1]);
		}

		public override TreeNode Visit(DbAndExpression e)
		{
			Check.NotNull(e, "e");
			return VisitInfix(e.Left, "And", e.Right);
		}

		public override TreeNode Visit(DbOrExpression e)
		{
			Check.NotNull(e, "e");
			return VisitInfix(e.Left, "Or", e.Right);
		}

		public override TreeNode Visit(DbInExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode;
			if (_infix)
			{
				treeNode = new TreeNode(string.Empty);
				treeNode.Children.Add(VisitExpression(e.Item));
				treeNode.Children.Add(new TreeNode("In"));
			}
			else
			{
				treeNode = new TreeNode("In");
				treeNode.Children.Add(VisitExpression(e.Item));
			}
			foreach (DbExpression item in e.List)
			{
				treeNode.Children.Add(VisitExpression(item));
			}
			return treeNode;
		}

		public override TreeNode Visit(DbNotExpression e)
		{
			Check.NotNull(e, "e");
			return VisitUnary(e);
		}

		public override TreeNode Visit(DbDistinctExpression e)
		{
			Check.NotNull(e, "e");
			return VisitUnary(e);
		}

		public override TreeNode Visit(DbElementExpression e)
		{
			Check.NotNull(e, "e");
			return VisitUnary(e, appendType: true);
		}

		public override TreeNode Visit(DbIsEmptyExpression e)
		{
			Check.NotNull(e, "e");
			return VisitUnary(e);
		}

		public override TreeNode Visit(DbUnionAllExpression e)
		{
			Check.NotNull(e, "e");
			return VisitBinary(e);
		}

		public override TreeNode Visit(DbIntersectExpression e)
		{
			Check.NotNull(e, "e");
			return VisitBinary(e);
		}

		public override TreeNode Visit(DbExceptExpression e)
		{
			Check.NotNull(e, "e");
			return VisitBinary(e);
		}

		private TreeNode VisitCastOrTreat(string op, DbUnaryExpression e)
		{
			TreeNode treeNode = null;
			TreeNode treeNode2 = VisitExpression(e.Argument);
			if (treeNode2.Children.Count == 0)
			{
				treeNode2.Text.Insert(0, op);
				treeNode2.Text.Insert(op.Length, '(');
				treeNode2.Text.Append(" As ");
				AppendType(treeNode2, e.ResultType);
				treeNode2.Text.Append(")");
				treeNode = treeNode2;
			}
			else
			{
				treeNode = new TreeNode(op);
				AppendTypeSpecifier(treeNode, e.ResultType);
				treeNode.Children.Add(treeNode2);
			}
			return treeNode;
		}

		public override TreeNode Visit(DbTreatExpression e)
		{
			Check.NotNull(e, "e");
			return VisitCastOrTreat("Treat", e);
		}

		public override TreeNode Visit(DbCastExpression e)
		{
			Check.NotNull(e, "e");
			return VisitCastOrTreat("Cast", e);
		}

		public override TreeNode Visit(DbIsOfExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = new TreeNode();
			if (DbExpressionKind.IsOfOnly == e.ExpressionKind)
			{
				treeNode.Text.Append("IsOfOnly");
			}
			else
			{
				treeNode.Text.Append("IsOf");
			}
			AppendTypeSpecifier(treeNode, e.OfType);
			treeNode.Children.Add(VisitExpression(e.Argument));
			return treeNode;
		}

		public override TreeNode Visit(DbOfTypeExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = new TreeNode((e.ExpressionKind == DbExpressionKind.OfTypeOnly) ? "OfTypeOnly" : "OfType");
			AppendTypeSpecifier(treeNode, e.OfType);
			treeNode.Children.Add(VisitExpression(e.Argument));
			return treeNode;
		}

		public override TreeNode Visit(DbCaseExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = new TreeNode("Case");
			for (int i = 0; i < e.When.Count; i++)
			{
				treeNode.Children.Add(Visit("When", e.When[i]));
				treeNode.Children.Add(Visit("Then", e.Then[i]));
			}
			treeNode.Children.Add(Visit("Else", e.Else));
			return treeNode;
		}

		public override TreeNode Visit(DbNewInstanceExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = NodeFromExpression(e);
			AppendTypeSpecifier(treeNode, e.ResultType);
			if (BuiltInTypeKind.CollectionType == e.ResultType.EdmType.BuiltInTypeKind)
			{
				foreach (DbExpression argument in e.Arguments)
				{
					treeNode.Children.Add(VisitExpression(argument));
				}
			}
			else
			{
				string label = ((BuiltInTypeKind.RowType == e.ResultType.EdmType.BuiltInTypeKind) ? "Column" : "Property");
				IList<EdmProperty> properties = TypeHelpers.GetProperties(e.ResultType);
				for (int i = 0; i < properties.Count; i++)
				{
					treeNode.Children.Add(VisitWithLabel(label, properties[i].Name, e.Arguments[i]));
				}
				if (BuiltInTypeKind.EntityType == e.ResultType.EdmType.BuiltInTypeKind && e.HasRelatedEntityReferences)
				{
					TreeNode treeNode2 = new TreeNode("RelatedEntityReferences");
					foreach (DbRelatedEntityRef relatedEntityReference in e.RelatedEntityReferences)
					{
						TreeNode treeNode3 = CreateNavigationNode(relatedEntityReference.SourceEnd, relatedEntityReference.TargetEnd);
						treeNode3.Children.Add(CreateRelationshipNode((RelationshipType)relatedEntityReference.SourceEnd.DeclaringType));
						treeNode3.Children.Add(VisitExpression(relatedEntityReference.TargetEntityReference));
						treeNode2.Children.Add(treeNode3);
					}
					treeNode.Children.Add(treeNode2);
				}
			}
			return treeNode;
		}

		public override TreeNode Visit(DbRefExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = new TreeNode("Ref");
			treeNode.Text.Append("<");
			AppendFullName(treeNode.Text, TypeHelpers.GetEdmType<RefType>(e.ResultType).ElementType);
			treeNode.Text.Append(">");
			TreeNode treeNode2 = new TreeNode("EntitySet : ");
			treeNode2.Text.Append(e.EntitySet.EntityContainer.Name);
			treeNode2.Text.Append(".");
			treeNode2.Text.Append(e.EntitySet.Name);
			treeNode.Children.Add(treeNode2);
			treeNode.Children.Add(Visit("Keys", e.Argument));
			return treeNode;
		}

		private static TreeNode CreateRelationshipNode(RelationshipType relType)
		{
			TreeNode treeNode = new TreeNode("Relationship");
			treeNode.Text.Append(" : ");
			AppendFullName(treeNode.Text, relType);
			return treeNode;
		}

		private static TreeNode CreateNavigationNode(RelationshipEndMember fromEnd, RelationshipEndMember toEnd)
		{
			TreeNode treeNode = new TreeNode();
			treeNode.Text.Append("Navigation : ");
			treeNode.Text.Append(fromEnd.Name);
			treeNode.Text.Append(" -> ");
			treeNode.Text.Append(toEnd.Name);
			return treeNode;
		}

		public override TreeNode Visit(DbRelationshipNavigationExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = NodeFromExpression(e);
			treeNode.Children.Add(CreateRelationshipNode(e.Relationship));
			treeNode.Children.Add(CreateNavigationNode(e.NavigateFrom, e.NavigateTo));
			treeNode.Children.Add(Visit("Source", e.NavigationSource));
			return treeNode;
		}

		public override TreeNode Visit(DbDerefExpression e)
		{
			Check.NotNull(e, "e");
			return VisitUnary(e);
		}

		public override TreeNode Visit(DbRefKeyExpression e)
		{
			Check.NotNull(e, "e");
			return VisitUnary(e, appendType: true);
		}

		public override TreeNode Visit(DbEntityRefExpression e)
		{
			Check.NotNull(e, "e");
			return VisitUnary(e, appendType: true);
		}

		public override TreeNode Visit(DbScanExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = NodeFromExpression(e);
			treeNode.Text.Append(" : ");
			treeNode.Text.Append(e.Target.EntityContainer.Name);
			treeNode.Text.Append(".");
			treeNode.Text.Append(e.Target.Name);
			return treeNode;
		}

		public override TreeNode Visit(DbFilterExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = NodeFromExpression(e);
			treeNode.Children.Add(VisitBinding("Input", e.Input));
			treeNode.Children.Add(Visit("Predicate", e.Predicate));
			return treeNode;
		}

		public override TreeNode Visit(DbProjectExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = NodeFromExpression(e);
			treeNode.Children.Add(VisitBinding("Input", e.Input));
			treeNode.Children.Add(Visit("Projection", e.Projection));
			return treeNode;
		}

		public override TreeNode Visit(DbCrossJoinExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = NodeFromExpression(e);
			treeNode.Children.Add(VisitBindingList("Inputs", e.Inputs));
			return treeNode;
		}

		public override TreeNode Visit(DbJoinExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = NodeFromExpression(e);
			treeNode.Children.Add(VisitBinding("Left", e.Left));
			treeNode.Children.Add(VisitBinding("Right", e.Right));
			treeNode.Children.Add(Visit("JoinCondition", e.JoinCondition));
			return treeNode;
		}

		public override TreeNode Visit(DbApplyExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = NodeFromExpression(e);
			treeNode.Children.Add(VisitBinding("Input", e.Input));
			treeNode.Children.Add(VisitBinding("Apply", e.Apply));
			return treeNode;
		}

		public override TreeNode Visit(DbGroupByExpression e)
		{
			Check.NotNull(e, "e");
			List<TreeNode> list = new List<TreeNode>();
			List<TreeNode> list2 = new List<TreeNode>();
			RowType edmType = TypeHelpers.GetEdmType<RowType>(TypeHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage);
			int num = 0;
			for (int i = 0; i < e.Keys.Count; i++)
			{
				list.Add(VisitWithLabel("Key", edmType.Properties[i].Name, e.Keys[num]));
				num++;
			}
			int num2 = 0;
			for (int j = e.Keys.Count; j < edmType.Properties.Count; j++)
			{
				TreeNode treeNode = new TreeNode("Aggregate : '");
				treeNode.Text.Append(edmType.Properties[j].Name);
				treeNode.Text.Append("'");
				if (e.Aggregates[num2] is DbFunctionAggregate dbFunctionAggregate)
				{
					TreeNode treeNode2 = VisitFunction(dbFunctionAggregate.Function, dbFunctionAggregate.Arguments);
					if (dbFunctionAggregate.Distinct)
					{
						treeNode2 = new TreeNode("Distinct", treeNode2);
					}
					treeNode.Children.Add(treeNode2);
				}
				else
				{
					DbGroupAggregate dbGroupAggregate = e.Aggregates[num2] as DbGroupAggregate;
					treeNode.Children.Add(Visit("GroupAggregate", dbGroupAggregate.Arguments[0]));
				}
				list2.Add(treeNode);
				num2++;
			}
			TreeNode treeNode3 = NodeFromExpression(e);
			treeNode3.Children.Add(VisitGroupBinding(e.Input));
			if (list.Count > 0)
			{
				treeNode3.Children.Add(new TreeNode("Keys", list));
			}
			if (list2.Count > 0)
			{
				treeNode3.Children.Add(new TreeNode("Aggregates", list2));
			}
			return treeNode3;
		}

		private TreeNode VisitSortOrder(IList<DbSortClause> sortOrder)
		{
			TreeNode treeNode = new TreeNode("SortOrder");
			foreach (DbSortClause item in sortOrder)
			{
				TreeNode treeNode2 = Visit(item.Ascending ? "Asc" : "Desc", item.Expression);
				if (!string.IsNullOrEmpty(item.Collation))
				{
					treeNode2.Text.Append(" : ");
					treeNode2.Text.Append(item.Collation);
				}
				treeNode.Children.Add(treeNode2);
			}
			return treeNode;
		}

		public override TreeNode Visit(DbSkipExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = NodeFromExpression(e);
			treeNode.Children.Add(VisitBinding("Input", e.Input));
			treeNode.Children.Add(VisitSortOrder(e.SortOrder));
			treeNode.Children.Add(Visit("Count", e.Count));
			return treeNode;
		}

		public override TreeNode Visit(DbSortExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = NodeFromExpression(e);
			treeNode.Children.Add(VisitBinding("Input", e.Input));
			treeNode.Children.Add(VisitSortOrder(e.SortOrder));
			return treeNode;
		}

		public override TreeNode Visit(DbQuantifierExpression e)
		{
			Check.NotNull(e, "e");
			TreeNode treeNode = NodeFromExpression(e);
			treeNode.Children.Add(VisitBinding("Input", e.Input));
			treeNode.Children.Add(Visit("Predicate", e.Predicate));
			return treeNode;
		}
	}

	private readonly PrinterVisitor _visitor = new PrinterVisitor();

	internal string Print(DbDeleteCommandTree tree)
	{
		TreeNode treeNode = ((tree.Target == null) ? new TreeNode("Target") : _visitor.VisitBinding("Target", tree.Target));
		TreeNode treeNode2 = ((tree.Predicate == null) ? new TreeNode("Predicate") : _visitor.VisitExpression("Predicate", tree.Predicate));
		return Print(new TreeNode("DbDeleteCommandTree", CreateParametersNode(tree), treeNode, treeNode2));
	}

	internal string Print(DbFunctionCommandTree tree)
	{
		TreeNode treeNode = new TreeNode("EdmFunction");
		if (tree.EdmFunction != null)
		{
			treeNode.Children.Add(_visitor.VisitFunction(tree.EdmFunction, null));
		}
		TreeNode treeNode2 = new TreeNode("ResultType");
		if (tree.ResultType != null)
		{
			PrinterVisitor.AppendTypeSpecifier(treeNode2, tree.ResultType);
		}
		return Print(new TreeNode("DbFunctionCommandTree", CreateParametersNode(tree), treeNode, treeNode2));
	}

	internal string Print(DbInsertCommandTree tree)
	{
		TreeNode treeNode = null;
		treeNode = ((tree.Target == null) ? new TreeNode("Target") : _visitor.VisitBinding("Target", tree.Target));
		TreeNode treeNode2 = new TreeNode("SetClauses");
		foreach (DbModificationClause setClause in tree.SetClauses)
		{
			if (setClause != null)
			{
				treeNode2.Children.Add(setClause.Print(_visitor));
			}
		}
		TreeNode treeNode3 = null;
		treeNode3 = ((tree.Returning == null) ? new TreeNode("Returning") : new TreeNode("Returning", _visitor.VisitExpression(tree.Returning)));
		return Print(new TreeNode("DbInsertCommandTree", CreateParametersNode(tree), treeNode, treeNode2, treeNode3));
	}

	internal string Print(DbUpdateCommandTree tree)
	{
		TreeNode treeNode = null;
		treeNode = ((tree.Target == null) ? new TreeNode("Target") : _visitor.VisitBinding("Target", tree.Target));
		TreeNode treeNode2 = new TreeNode("SetClauses");
		foreach (DbModificationClause setClause in tree.SetClauses)
		{
			if (setClause != null)
			{
				treeNode2.Children.Add(setClause.Print(_visitor));
			}
		}
		TreeNode treeNode3 = ((tree.Predicate == null) ? new TreeNode("Predicate") : new TreeNode("Predicate", _visitor.VisitExpression(tree.Predicate)));
		TreeNode treeNode4 = ((tree.Returning == null) ? new TreeNode("Returning") : new TreeNode("Returning", _visitor.VisitExpression(tree.Returning)));
		return Print(new TreeNode("DbUpdateCommandTree", CreateParametersNode(tree), treeNode, treeNode2, treeNode3, treeNode4));
	}

	internal string Print(DbQueryCommandTree tree)
	{
		TreeNode treeNode = new TreeNode("Query");
		if (tree.Query != null)
		{
			PrinterVisitor.AppendTypeSpecifier(treeNode, tree.Query.ResultType);
			treeNode.Children.Add(_visitor.VisitExpression(tree.Query));
		}
		return Print(new TreeNode("DbQueryCommandTree", CreateParametersNode(tree), treeNode));
	}

	private static TreeNode CreateParametersNode(DbCommandTree tree)
	{
		TreeNode treeNode = new TreeNode("Parameters");
		foreach (KeyValuePair<string, TypeUsage> parameter in tree.Parameters)
		{
			TreeNode treeNode2 = new TreeNode(parameter.Key);
			PrinterVisitor.AppendTypeSpecifier(treeNode2, parameter.Value);
			treeNode.Children.Add(treeNode2);
		}
		return treeNode;
	}
}
