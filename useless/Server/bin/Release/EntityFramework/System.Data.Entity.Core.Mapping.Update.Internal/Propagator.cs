using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Resources;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class Propagator : UpdateExpressionVisitor<ChangeNode>
{
	private class Evaluator : UpdateExpressionVisitor<PropagatorResult>
	{
		private readonly PropagatorResult m_row;

		private static readonly string _visitorName = typeof(Evaluator).FullName;

		protected override string VisitorName => _visitorName;

		private Evaluator(PropagatorResult row)
		{
			m_row = row;
		}

		internal static IEnumerable<PropagatorResult> Filter(DbExpression predicate, IEnumerable<PropagatorResult> rows)
		{
			foreach (PropagatorResult row in rows)
			{
				if (EvaluatePredicate(predicate, row))
				{
					yield return row;
				}
			}
		}

		internal static bool EvaluatePredicate(DbExpression predicate, PropagatorResult row)
		{
			Evaluator visitor = new Evaluator(row);
			return ConvertResultToBool(predicate.Accept(visitor)).GetValueOrDefault();
		}

		internal static PropagatorResult Evaluate(DbExpression node, PropagatorResult row)
		{
			DbExpressionVisitor<PropagatorResult> visitor = new Evaluator(row);
			return node.Accept(visitor);
		}

		private static bool? ConvertResultToBool(PropagatorResult result)
		{
			if (result.IsNull)
			{
				return null;
			}
			return (bool)result.GetSimpleValue();
		}

		private static PropagatorResult ConvertBoolToResult(bool? booleanValue, params PropagatorResult[] inputs)
		{
			return PropagatorResult.CreateSimpleValue(value: (!booleanValue.HasValue) ? null : ((object)booleanValue.Value), flags: PropagateUnknownAndPreserveFlags(null, inputs));
		}

		public override PropagatorResult Visit(DbIsOfExpression predicate)
		{
			Check.NotNull(predicate, "predicate");
			if (DbExpressionKind.IsOfOnly != predicate.ExpressionKind)
			{
				throw ConstructNotSupportedException(predicate);
			}
			PropagatorResult propagatorResult = Visit(predicate.Argument);
			bool value = !propagatorResult.IsNull && propagatorResult.StructuralType.EdmEquals(predicate.OfType.EdmType);
			return ConvertBoolToResult(value, propagatorResult);
		}

		public override PropagatorResult Visit(DbComparisonExpression predicate)
		{
			Check.NotNull(predicate, "predicate");
			if (DbExpressionKind.Equals == predicate.ExpressionKind)
			{
				PropagatorResult propagatorResult = Visit(predicate.Left);
				PropagatorResult propagatorResult2 = Visit(predicate.Right);
				bool? booleanValue;
				if (propagatorResult.IsNull || propagatorResult2.IsNull)
				{
					booleanValue = null;
				}
				else
				{
					object simpleValue = propagatorResult.GetSimpleValue();
					object simpleValue2 = propagatorResult2.GetSimpleValue();
					booleanValue = ByValueEqualityComparer.Default.Equals(simpleValue, simpleValue2);
				}
				return ConvertBoolToResult(booleanValue, propagatorResult, propagatorResult2);
			}
			throw ConstructNotSupportedException(predicate);
		}

		public override PropagatorResult Visit(DbAndExpression predicate)
		{
			Check.NotNull(predicate, "predicate");
			PropagatorResult propagatorResult = Visit(predicate.Left);
			PropagatorResult propagatorResult2 = Visit(predicate.Right);
			bool? left = ConvertResultToBool(propagatorResult);
			bool? right = ConvertResultToBool(propagatorResult2);
			if ((left.HasValue && !left.Value && PreservedAndKnown(propagatorResult)) || (right.HasValue && !right.Value && PreservedAndKnown(propagatorResult2)))
			{
				return CreatePerservedAndKnownResult(false);
			}
			return ConvertBoolToResult(left.And(right), propagatorResult, propagatorResult2);
		}

		public override PropagatorResult Visit(DbOrExpression predicate)
		{
			Check.NotNull(predicate, "predicate");
			PropagatorResult propagatorResult = Visit(predicate.Left);
			PropagatorResult propagatorResult2 = Visit(predicate.Right);
			bool? left = ConvertResultToBool(propagatorResult);
			bool? right = ConvertResultToBool(propagatorResult2);
			if ((left.HasValue && left.Value && PreservedAndKnown(propagatorResult)) || (right.HasValue && right.Value && PreservedAndKnown(propagatorResult2)))
			{
				return CreatePerservedAndKnownResult(true);
			}
			return ConvertBoolToResult(left.Or(right), propagatorResult, propagatorResult2);
		}

		private static PropagatorResult CreatePerservedAndKnownResult(object value)
		{
			return PropagatorResult.CreateSimpleValue(PropagatorFlags.Preserve, value);
		}

		private static bool PreservedAndKnown(PropagatorResult result)
		{
			return PropagatorFlags.Preserve == (result.PropagatorFlags & (PropagatorFlags.Preserve | PropagatorFlags.Unknown));
		}

		public override PropagatorResult Visit(DbNotExpression predicate)
		{
			Check.NotNull(predicate, "predicate");
			PropagatorResult propagatorResult = Visit(predicate.Argument);
			return ConvertBoolToResult(ConvertResultToBool(propagatorResult).Not(), propagatorResult);
		}

		public override PropagatorResult Visit(DbCaseExpression node)
		{
			Check.NotNull(node, "node");
			int num = -1;
			int num2 = 0;
			List<PropagatorResult> list = new List<PropagatorResult>();
			foreach (DbExpression item in node.When)
			{
				PropagatorResult propagatorResult = Visit(item);
				list.Add(propagatorResult);
				if (ConvertResultToBool(propagatorResult).GetValueOrDefault())
				{
					num = num2;
					break;
				}
				num2++;
			}
			PropagatorResult propagatorResult2 = ((-1 != num) ? Visit(node.Then[num]) : Visit(node.Else));
			list.Add(propagatorResult2);
			PropagatorFlags flags = PropagateUnknownAndPreserveFlags(propagatorResult2, list);
			return propagatorResult2.ReplicateResultWithNewFlags(flags);
		}

		public override PropagatorResult Visit(DbVariableReferenceExpression node)
		{
			Check.NotNull(node, "node");
			return m_row;
		}

		public override PropagatorResult Visit(DbPropertyExpression node)
		{
			Check.NotNull(node, "node");
			PropagatorResult propagatorResult = Visit(node.Instance);
			if (propagatorResult.IsNull)
			{
				return PropagatorResult.CreateSimpleValue(propagatorResult.PropagatorFlags, null);
			}
			return propagatorResult.GetMemberValue(node.Property);
		}

		public override PropagatorResult Visit(DbConstantExpression node)
		{
			Check.NotNull(node, "node");
			return PropagatorResult.CreateSimpleValue(PropagatorFlags.Preserve, node.Value);
		}

		public override PropagatorResult Visit(DbRefKeyExpression node)
		{
			Check.NotNull(node, "node");
			return Visit(node.Argument);
		}

		public override PropagatorResult Visit(DbNullExpression node)
		{
			Check.NotNull(node, "node");
			return PropagatorResult.CreateSimpleValue(PropagatorFlags.Preserve, null);
		}

		public override PropagatorResult Visit(DbTreatExpression node)
		{
			Check.NotNull(node, "node");
			PropagatorResult propagatorResult = Visit(node.Argument);
			if (MetadataHelper.IsSuperTypeOf(node.ResultType.EdmType, propagatorResult.StructuralType))
			{
				return propagatorResult;
			}
			return PropagatorResult.CreateSimpleValue(propagatorResult.PropagatorFlags, null);
		}

		public override PropagatorResult Visit(DbCastExpression node)
		{
			Check.NotNull(node, "node");
			PropagatorResult propagatorResult = Visit(node.Argument);
			TypeUsage resultType = node.ResultType;
			if (!propagatorResult.IsSimple || BuiltInTypeKind.PrimitiveType != resultType.EdmType.BuiltInTypeKind)
			{
				throw new NotSupportedException(Strings.Update_UnsupportedCastArgument(resultType.EdmType.Name));
			}
			object value;
			if (propagatorResult.IsNull)
			{
				value = null;
			}
			else
			{
				try
				{
					value = Cast(propagatorResult.GetSimpleValue(), ((PrimitiveType)resultType.EdmType).ClrEquivalentType);
				}
				catch
				{
					throw;
				}
			}
			return propagatorResult.ReplicateResultWithNewValue(value);
		}

		private static object Cast(object value, Type clrPrimitiveType)
		{
			IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
			if (value == null || value == DBNull.Value || value.GetType() == clrPrimitiveType)
			{
				return value;
			}
			if (value is DateTime && clrPrimitiveType == typeof(DateTimeOffset))
			{
				return new DateTimeOffset(((DateTime)value).Ticks, TimeSpan.Zero);
			}
			return Convert.ChangeType(value, clrPrimitiveType, invariantCulture);
		}

		public override PropagatorResult Visit(DbIsNullExpression node)
		{
			Check.NotNull(node, "node");
			PropagatorResult propagatorResult = Visit(node.Argument);
			return ConvertBoolToResult(propagatorResult.IsNull, propagatorResult);
		}

		private static PropagatorFlags PropagateUnknownAndPreserveFlags(PropagatorResult result, IEnumerable<PropagatorResult> inputs)
		{
			bool flag = false;
			bool flag2 = true;
			bool flag3 = true;
			foreach (PropagatorResult input in inputs)
			{
				flag3 = false;
				PropagatorFlags propagatorFlags = input.PropagatorFlags;
				if ((PropagatorFlags.Unknown & propagatorFlags) != 0)
				{
					flag = true;
				}
				if ((PropagatorFlags.Preserve & propagatorFlags) == 0)
				{
					flag2 = false;
				}
			}
			if (flag3)
			{
				flag2 = false;
			}
			if (result != null)
			{
				PropagatorFlags propagatorFlags2 = result.PropagatorFlags;
				if (flag)
				{
					propagatorFlags2 |= PropagatorFlags.Unknown;
				}
				if (!flag2)
				{
					propagatorFlags2 &= ~PropagatorFlags.Preserve;
				}
				return propagatorFlags2;
			}
			PropagatorFlags propagatorFlags3 = PropagatorFlags.NoFlags;
			if (flag)
			{
				propagatorFlags3 |= PropagatorFlags.Unknown;
			}
			if (flag2)
			{
				propagatorFlags3 |= PropagatorFlags.Preserve;
			}
			return propagatorFlags3;
		}
	}

	internal class ExtentPlaceholderCreator
	{
		private static readonly Dictionary<PrimitiveTypeKind, object> _typeDefaultMap = InitializeTypeDefaultMap();

		private static readonly Lazy<Dictionary<PrimitiveTypeKind, object>> _spatialTypeDefaultMap = new Lazy<Dictionary<PrimitiveTypeKind, object>>(InitializeSpatialTypeDefaultMap);

		private static Dictionary<PrimitiveTypeKind, object> InitializeTypeDefaultMap()
		{
			return new Dictionary<PrimitiveTypeKind, object>(EqualityComparer<PrimitiveTypeKind>.Default)
			{
				[PrimitiveTypeKind.Binary] = new byte[0],
				[PrimitiveTypeKind.Boolean] = false,
				[PrimitiveTypeKind.Byte] = (byte)0,
				[PrimitiveTypeKind.DateTime] = default(DateTime),
				[PrimitiveTypeKind.Time] = default(TimeSpan),
				[PrimitiveTypeKind.DateTimeOffset] = default(DateTimeOffset),
				[PrimitiveTypeKind.Decimal] = 0m,
				[PrimitiveTypeKind.Double] = 0.0,
				[PrimitiveTypeKind.Guid] = default(Guid),
				[PrimitiveTypeKind.Int16] = (short)0,
				[PrimitiveTypeKind.Int32] = 0,
				[PrimitiveTypeKind.Int64] = 0L,
				[PrimitiveTypeKind.Single] = 0f,
				[PrimitiveTypeKind.SByte] = (sbyte)0,
				[PrimitiveTypeKind.String] = string.Empty,
				[PrimitiveTypeKind.HierarchyId] = HierarchyId.GetRoot()
			};
		}

		private static Dictionary<PrimitiveTypeKind, object> InitializeSpatialTypeDefaultMap()
		{
			return new Dictionary<PrimitiveTypeKind, object>(EqualityComparer<PrimitiveTypeKind>.Default)
			{
				[PrimitiveTypeKind.Geometry] = DbGeometry.FromText("POINT EMPTY"),
				[PrimitiveTypeKind.GeometryPoint] = DbGeometry.FromText("POINT EMPTY"),
				[PrimitiveTypeKind.GeometryLineString] = DbGeometry.FromText("LINESTRING EMPTY"),
				[PrimitiveTypeKind.GeometryPolygon] = DbGeometry.FromText("POLYGON EMPTY"),
				[PrimitiveTypeKind.GeometryMultiPoint] = DbGeometry.FromText("MULTIPOINT EMPTY"),
				[PrimitiveTypeKind.GeometryMultiLineString] = DbGeometry.FromText("MULTILINESTRING EMPTY"),
				[PrimitiveTypeKind.GeometryMultiPolygon] = DbGeometry.FromText("MULTIPOLYGON EMPTY"),
				[PrimitiveTypeKind.GeometryCollection] = DbGeometry.FromText("GEOMETRYCOLLECTION EMPTY"),
				[PrimitiveTypeKind.Geography] = DbGeography.FromText("POINT EMPTY"),
				[PrimitiveTypeKind.GeographyPoint] = DbGeography.FromText("POINT EMPTY"),
				[PrimitiveTypeKind.GeographyLineString] = DbGeography.FromText("LINESTRING EMPTY"),
				[PrimitiveTypeKind.GeographyPolygon] = DbGeography.FromText("POLYGON EMPTY"),
				[PrimitiveTypeKind.GeographyMultiPoint] = DbGeography.FromText("MULTIPOINT EMPTY"),
				[PrimitiveTypeKind.GeographyMultiLineString] = DbGeography.FromText("MULTILINESTRING EMPTY"),
				[PrimitiveTypeKind.GeographyMultiPolygon] = DbGeography.FromText("MULTIPOLYGON EMPTY"),
				[PrimitiveTypeKind.GeographyCollection] = DbGeography.FromText("GEOMETRYCOLLECTION EMPTY")
			};
		}

		private static bool TryGetDefaultValue(PrimitiveType primitiveType, out object defaultValue)
		{
			PrimitiveTypeKind primitiveTypeKind = primitiveType.PrimitiveTypeKind;
			if (!Helper.IsSpatialType(primitiveType))
			{
				return _typeDefaultMap.TryGetValue(primitiveTypeKind, out defaultValue);
			}
			return _spatialTypeDefaultMap.Value.TryGetValue(primitiveTypeKind, out defaultValue);
		}

		internal static PropagatorResult CreatePlaceholder(EntitySetBase extent)
		{
			ExtentPlaceholderCreator extentPlaceholderCreator = new ExtentPlaceholderCreator();
			if (extent is AssociationSet associationSet)
			{
				return extentPlaceholderCreator.CreateAssociationSetPlaceholder(associationSet);
			}
			if (extent is EntitySet entitySet)
			{
				return extentPlaceholderCreator.CreateEntitySetPlaceholder(entitySet);
			}
			throw new NotSupportedException(Strings.Update_UnsupportedExtentType(extent.Name, extent.GetType().Name));
		}

		private PropagatorResult CreateEntitySetPlaceholder(EntitySet entitySet)
		{
			ReadOnlyMetadataCollection<EdmProperty> properties = entitySet.ElementType.Properties;
			PropagatorResult[] array = new PropagatorResult[properties.Count];
			for (int i = 0; i < properties.Count; i++)
			{
				PropagatorResult propagatorResult = CreateMemberPlaceholder(properties[i]);
				array[i] = propagatorResult;
			}
			return PropagatorResult.CreateStructuralValue(array, entitySet.ElementType, isModified: false);
		}

		private PropagatorResult CreateAssociationSetPlaceholder(AssociationSet associationSet)
		{
			ReadOnlyMetadataCollection<AssociationEndMember> associationEndMembers = associationSet.ElementType.AssociationEndMembers;
			PropagatorResult[] array = new PropagatorResult[associationEndMembers.Count];
			for (int i = 0; i < associationEndMembers.Count; i++)
			{
				EntityType entityType = (EntityType)((RefType)associationEndMembers[i].TypeUsage.EdmType).ElementType;
				PropagatorResult[] array2 = new PropagatorResult[entityType.KeyMembers.Count];
				for (int j = 0; j < entityType.KeyMembers.Count; j++)
				{
					EdmMember member = entityType.KeyMembers[j];
					PropagatorResult propagatorResult = CreateMemberPlaceholder(member);
					array2[j] = propagatorResult;
				}
				RowType keyRowType = entityType.GetKeyRowType();
				PropagatorResult propagatorResult2 = PropagatorResult.CreateStructuralValue(array2, keyRowType, isModified: false);
				array[i] = propagatorResult2;
			}
			return PropagatorResult.CreateStructuralValue(array, associationSet.ElementType, isModified: false);
		}

		private PropagatorResult CreateMemberPlaceholder(EdmMember member)
		{
			return Visit(member);
		}

		internal PropagatorResult Visit(EdmMember node)
		{
			TypeUsage modelTypeUsage = Helper.GetModelTypeUsage(node);
			if (Helper.IsScalarType(modelTypeUsage.EdmType))
			{
				GetPropagatorResultForPrimitiveType(Helper.AsPrimitive(modelTypeUsage.EdmType), out var result);
				return result;
			}
			StructuralType structuralType = (StructuralType)modelTypeUsage.EdmType;
			IBaseList<EdmMember> allStructuralMembers = TypeHelpers.GetAllStructuralMembers(structuralType);
			PropagatorResult[] array = new PropagatorResult[allStructuralMembers.Count];
			for (int i = 0; i < allStructuralMembers.Count; i++)
			{
				array[i] = Visit(allStructuralMembers[i]);
			}
			return PropagatorResult.CreateStructuralValue(array, structuralType, isModified: false);
		}

		internal static void GetPropagatorResultForPrimitiveType(PrimitiveType primitiveType, out PropagatorResult result)
		{
			if (!TryGetDefaultValue(primitiveType, out var defaultValue))
			{
				defaultValue = (byte)0;
			}
			result = PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, defaultValue);
		}
	}

	private class JoinPropagator
	{
		[Flags]
		private enum Ops : uint
		{
			Nothing = 0u,
			LeftInsert = 1u,
			LeftDelete = 2u,
			RightInsert = 4u,
			RightDelete = 8u,
			LeftUnknown = 0x20u,
			RightNullModified = 0x80u,
			RightNullPreserve = 0x100u,
			RightUnknown = 0x200u,
			LeftUpdate = 3u,
			RightUpdate = 0xCu,
			Unsupported = 0x1000u,
			LeftInsertJoinRightInsert = 5u,
			LeftDeleteJoinRightDelete = 0xAu,
			LeftInsertNullModifiedExtended = 0x81u,
			LeftInsertNullPreserveExtended = 0x101u,
			LeftInsertUnknownExtended = 0x201u,
			LeftDeleteNullModifiedExtended = 0x82u,
			LeftDeleteNullPreserveExtended = 0x102u,
			LeftDeleteUnknownExtended = 0x202u,
			LeftUnknownNullModifiedExtended = 0xA0u,
			LeftUnknownNullPreserveExtended = 0x120u,
			RightInsertUnknownExtended = 0x24u,
			RightDeleteUnknownExtended = 0x28u
		}

		private class JoinConditionVisitor : UpdateExpressionVisitor<object>
		{
			private readonly List<DbExpression> m_leftKeySelectors;

			private readonly List<DbExpression> m_rightKeySelectors;

			private static readonly string _visitorName = typeof(JoinConditionVisitor).FullName;

			protected override string VisitorName => _visitorName;

			private JoinConditionVisitor()
			{
				m_leftKeySelectors = new List<DbExpression>();
				m_rightKeySelectors = new List<DbExpression>();
			}

			internal static void GetKeySelectors(DbExpression joinCondition, out ReadOnlyCollection<DbExpression> leftKeySelectors, out ReadOnlyCollection<DbExpression> rightKeySelectors)
			{
				JoinConditionVisitor joinConditionVisitor = new JoinConditionVisitor();
				joinCondition.Accept(joinConditionVisitor);
				leftKeySelectors = new ReadOnlyCollection<DbExpression>(joinConditionVisitor.m_leftKeySelectors);
				rightKeySelectors = new ReadOnlyCollection<DbExpression>(joinConditionVisitor.m_rightKeySelectors);
			}

			public override object Visit(DbAndExpression node)
			{
				Check.NotNull(node, "node");
				Visit(node.Left);
				Visit(node.Right);
				return null;
			}

			public override object Visit(DbComparisonExpression node)
			{
				Check.NotNull(node, "node");
				if (DbExpressionKind.Equals == node.ExpressionKind)
				{
					m_leftKeySelectors.Add(node.Left);
					m_rightKeySelectors.Add(node.Right);
					return null;
				}
				throw ConstructNotSupportedException(node);
			}
		}

		private enum PopulateMode
		{
			NullModified,
			NullPreserve,
			Unknown
		}

		private static class PlaceholderPopulator
		{
			internal static PropagatorResult Populate(PropagatorResult placeholder, CompositeKey key, CompositeKey placeholderKey, PopulateMode mode)
			{
				bool isNull = mode == PopulateMode.NullModified || mode == PopulateMode.NullPreserve;
				bool num = mode == PopulateMode.NullPreserve || mode == PopulateMode.Unknown;
				PropagatorFlags flags = PropagatorFlags.NoFlags;
				if (!isNull)
				{
					flags |= PropagatorFlags.Unknown;
				}
				if (num)
				{
					flags |= PropagatorFlags.Preserve;
				}
				return placeholder.Replace(delegate(PropagatorResult node)
				{
					int num2 = -1;
					for (int i = 0; i < placeholderKey.KeyComponents.Length; i++)
					{
						if (placeholderKey.KeyComponents[i] == node)
						{
							num2 = i;
							break;
						}
					}
					if (num2 != -1)
					{
						return key.KeyComponents[num2];
					}
					object value = (isNull ? null : node.GetSimpleValue());
					return PropagatorResult.CreateSimpleValue(flags, value);
				});
			}
		}

		private static readonly Dictionary<Ops, Ops> _innerJoinInsertRules;

		private static readonly Dictionary<Ops, Ops> _innerJoinDeleteRules;

		private static readonly Dictionary<Ops, Ops> _leftOuterJoinInsertRules;

		private static readonly Dictionary<Ops, Ops> _leftOuterJoinDeleteRules;

		private readonly DbJoinExpression m_joinExpression;

		private readonly Propagator m_parent;

		private readonly Dictionary<Ops, Ops> m_insertRules;

		private readonly Dictionary<Ops, Ops> m_deleteRules;

		private readonly ReadOnlyCollection<DbExpression> m_leftKeySelectors;

		private readonly ReadOnlyCollection<DbExpression> m_rightKeySelectors;

		private readonly ChangeNode m_left;

		private readonly ChangeNode m_right;

		private readonly CompositeKey m_leftPlaceholderKey;

		private readonly CompositeKey m_rightPlaceholderKey;

		internal JoinPropagator(ChangeNode left, ChangeNode right, DbJoinExpression node, Propagator parent)
		{
			m_left = left;
			m_right = right;
			m_joinExpression = node;
			m_parent = parent;
			if (DbExpressionKind.InnerJoin == m_joinExpression.ExpressionKind)
			{
				m_insertRules = _innerJoinInsertRules;
				m_deleteRules = _innerJoinDeleteRules;
			}
			else
			{
				m_insertRules = _leftOuterJoinInsertRules;
				m_deleteRules = _leftOuterJoinDeleteRules;
			}
			JoinConditionVisitor.GetKeySelectors(node.JoinCondition, out m_leftKeySelectors, out m_rightKeySelectors);
			m_leftPlaceholderKey = ExtractKey(m_left.Placeholder, m_leftKeySelectors);
			m_rightPlaceholderKey = ExtractKey(m_right.Placeholder, m_rightKeySelectors);
		}

		static JoinPropagator()
		{
			_innerJoinInsertRules = new Dictionary<Ops, Ops>(EqualityComparer<Ops>.Default);
			_innerJoinDeleteRules = new Dictionary<Ops, Ops>(EqualityComparer<Ops>.Default);
			_leftOuterJoinInsertRules = new Dictionary<Ops, Ops>(EqualityComparer<Ops>.Default);
			_leftOuterJoinDeleteRules = new Dictionary<Ops, Ops>(EqualityComparer<Ops>.Default);
			InitializeRule(Ops.LeftUpdate | Ops.RightUpdate, Ops.LeftInsertJoinRightInsert, Ops.LeftDeleteJoinRightDelete, Ops.LeftInsertJoinRightInsert, Ops.LeftDeleteJoinRightDelete);
			InitializeRule(Ops.LeftDeleteJoinRightDelete, Ops.Nothing, Ops.LeftDeleteJoinRightDelete, Ops.Nothing, Ops.LeftDeleteJoinRightDelete);
			InitializeRule(Ops.LeftInsertJoinRightInsert, Ops.LeftInsertJoinRightInsert, Ops.Nothing, Ops.LeftInsertJoinRightInsert, Ops.Nothing);
			InitializeRule(Ops.LeftUpdate, Ops.LeftInsertUnknownExtended, Ops.LeftDeleteUnknownExtended, Ops.LeftInsertUnknownExtended, Ops.LeftDeleteUnknownExtended);
			InitializeRule(Ops.RightUpdate, Ops.RightInsertUnknownExtended, Ops.RightDeleteUnknownExtended, Ops.RightInsertUnknownExtended, Ops.RightDeleteUnknownExtended);
			InitializeRule(Ops.LeftUpdate | Ops.RightDelete, Ops.Unsupported, Ops.Unsupported, Ops.LeftInsertNullModifiedExtended, Ops.LeftDeleteJoinRightDelete);
			InitializeRule(Ops.LeftUpdate | Ops.RightInsert, Ops.Unsupported, Ops.Unsupported, Ops.LeftInsertJoinRightInsert, Ops.LeftDeleteNullModifiedExtended);
			InitializeRule(Ops.LeftDelete, Ops.Unsupported, Ops.Unsupported, Ops.Nothing, Ops.LeftDeleteNullPreserveExtended);
			InitializeRule(Ops.LeftInsert, Ops.Unsupported, Ops.Unsupported, Ops.LeftInsertNullModifiedExtended, Ops.Nothing);
			InitializeRule(Ops.RightDelete, Ops.Unsupported, Ops.Unsupported, Ops.LeftUnknownNullModifiedExtended, Ops.RightDeleteUnknownExtended);
			InitializeRule(Ops.RightInsert, Ops.Unsupported, Ops.Unsupported, Ops.RightInsertUnknownExtended, Ops.LeftUnknownNullModifiedExtended);
			InitializeRule(Ops.RightUpdate | Ops.LeftDelete, Ops.Unsupported, Ops.Unsupported, Ops.Unsupported, Ops.Unsupported);
			InitializeRule(Ops.LeftDelete | Ops.RightInsert, Ops.Unsupported, Ops.Unsupported, Ops.Unsupported, Ops.Unsupported);
			InitializeRule(Ops.RightUpdate | Ops.LeftInsert, Ops.Unsupported, Ops.Unsupported, Ops.Unsupported, Ops.Unsupported);
			InitializeRule(Ops.LeftInsert | Ops.RightDelete, Ops.Unsupported, Ops.Unsupported, Ops.Unsupported, Ops.Unsupported);
		}

		private static void InitializeRule(Ops input, Ops joinInsert, Ops joinDelete, Ops lojInsert, Ops lojDelete)
		{
			_innerJoinInsertRules.Add(input, joinInsert);
			_innerJoinDeleteRules.Add(input, joinDelete);
			_leftOuterJoinInsertRules.Add(input, lojInsert);
			_leftOuterJoinDeleteRules.Add(input, lojDelete);
		}

		internal ChangeNode Propagate()
		{
			ChangeNode changeNode = BuildChangeNode(m_joinExpression);
			Dictionary<CompositeKey, Tuple<CompositeKey, PropagatorResult>> dictionary = ProcessKeys(m_left.Deleted, m_leftKeySelectors);
			Dictionary<CompositeKey, Tuple<CompositeKey, PropagatorResult>> dictionary2 = ProcessKeys(m_left.Inserted, m_leftKeySelectors);
			Dictionary<CompositeKey, Tuple<CompositeKey, PropagatorResult>> dictionary3 = ProcessKeys(m_right.Deleted, m_rightKeySelectors);
			Dictionary<CompositeKey, Tuple<CompositeKey, PropagatorResult>> dictionary4 = ProcessKeys(m_right.Inserted, m_rightKeySelectors);
			foreach (CompositeKey item in dictionary.Keys.Concat(dictionary2.Keys).Concat(dictionary3.Keys).Concat(dictionary4.Keys)
				.Distinct(m_parent.UpdateTranslator.KeyComparer))
			{
				Propagate(item, changeNode, dictionary, dictionary2, dictionary3, dictionary4);
			}
			changeNode.Placeholder = CreateResultTuple(Tuple.Create<CompositeKey, PropagatorResult>(null, m_left.Placeholder), Tuple.Create<CompositeKey, PropagatorResult>(null, m_right.Placeholder), changeNode);
			return changeNode;
		}

		private void Propagate(CompositeKey key, ChangeNode result, Dictionary<CompositeKey, Tuple<CompositeKey, PropagatorResult>> leftDeletes, Dictionary<CompositeKey, Tuple<CompositeKey, PropagatorResult>> leftInserts, Dictionary<CompositeKey, Tuple<CompositeKey, PropagatorResult>> rightDeletes, Dictionary<CompositeKey, Tuple<CompositeKey, PropagatorResult>> rightInserts)
		{
			Tuple<CompositeKey, PropagatorResult> value = null;
			Tuple<CompositeKey, PropagatorResult> value2 = null;
			Tuple<CompositeKey, PropagatorResult> value3 = null;
			Tuple<CompositeKey, PropagatorResult> value4 = null;
			Ops ops = Ops.Nothing;
			if (leftInserts.TryGetValue(key, out value))
			{
				ops |= Ops.LeftInsert;
			}
			if (leftDeletes.TryGetValue(key, out value2))
			{
				ops |= Ops.LeftDelete;
			}
			if (rightInserts.TryGetValue(key, out value3))
			{
				ops |= Ops.RightInsert;
			}
			if (rightDeletes.TryGetValue(key, out value4))
			{
				ops |= Ops.RightDelete;
			}
			Ops ops2 = m_insertRules[ops];
			Ops ops3 = m_deleteRules[ops];
			if (Ops.Unsupported == ops2 || Ops.Unsupported == ops3)
			{
				List<IEntityStateEntry> stateEntries = new List<IEntityStateEntry>();
				Action<Tuple<CompositeKey, PropagatorResult>> obj = delegate(Tuple<CompositeKey, PropagatorResult> r)
				{
					if (r != null)
					{
						stateEntries.AddRange(SourceInterpreter.GetAllStateEntries(r.Item2, m_parent.m_updateTranslator, m_parent.m_table));
					}
				};
				obj(value);
				obj(value2);
				obj(value3);
				obj(value4);
				throw new UpdateException(Strings.Update_InvalidChanges, null, stateEntries.Cast<ObjectStateEntry>().Distinct());
			}
			if ((Ops.LeftUnknown & ops2) != 0)
			{
				value = Tuple.Create(key, LeftPlaceholder(key, PopulateMode.Unknown));
			}
			if ((Ops.LeftUnknown & ops3) != 0)
			{
				value2 = Tuple.Create(key, LeftPlaceholder(key, PopulateMode.Unknown));
			}
			if ((Ops.RightNullModified & ops2) != 0)
			{
				value3 = Tuple.Create(key, RightPlaceholder(key, PopulateMode.NullModified));
			}
			else if ((Ops.RightNullPreserve & ops2) != 0)
			{
				value3 = Tuple.Create(key, RightPlaceholder(key, PopulateMode.NullPreserve));
			}
			else if ((Ops.RightUnknown & ops2) != 0)
			{
				value3 = Tuple.Create(key, RightPlaceholder(key, PopulateMode.Unknown));
			}
			if ((Ops.RightNullModified & ops3) != 0)
			{
				value4 = Tuple.Create(key, RightPlaceholder(key, PopulateMode.NullModified));
			}
			else if ((Ops.RightNullPreserve & ops3) != 0)
			{
				value4 = Tuple.Create(key, RightPlaceholder(key, PopulateMode.NullPreserve));
			}
			else if ((Ops.RightUnknown & ops3) != 0)
			{
				value4 = Tuple.Create(key, RightPlaceholder(key, PopulateMode.Unknown));
			}
			if (value != null && value3 != null)
			{
				result.Inserted.Add(CreateResultTuple(value, value3, result));
			}
			if (value2 != null && value4 != null)
			{
				result.Deleted.Add(CreateResultTuple(value2, value4, result));
			}
		}

		private PropagatorResult CreateResultTuple(Tuple<CompositeKey, PropagatorResult> left, Tuple<CompositeKey, PropagatorResult> right, ChangeNode result)
		{
			CompositeKey item = left.Item1;
			CompositeKey item2 = right.Item1;
			Dictionary<PropagatorResult, PropagatorResult> map = null;
			if (item != null && item2 != null && item != item2)
			{
				CompositeKey compositeKey = item.Merge(m_parent.m_updateTranslator.KeyManager, item2);
				map = new Dictionary<PropagatorResult, PropagatorResult>();
				for (int i = 0; i < item.KeyComponents.Length; i++)
				{
					map[item.KeyComponents[i]] = compositeKey.KeyComponents[i];
					map[item2.KeyComponents[i]] = compositeKey.KeyComponents[i];
				}
			}
			PropagatorResult propagatorResult = PropagatorResult.CreateStructuralValue(new PropagatorResult[2] { left.Item2, right.Item2 }, (StructuralType)result.ElementType.EdmType, isModified: false);
			if (map != null)
			{
				propagatorResult = propagatorResult.Replace((PropagatorResult original) => (!map.TryGetValue(original, out var replacement)) ? original : replacement);
			}
			return propagatorResult;
		}

		private PropagatorResult LeftPlaceholder(CompositeKey key, PopulateMode mode)
		{
			return PlaceholderPopulator.Populate(m_left.Placeholder, key, m_leftPlaceholderKey, mode);
		}

		private PropagatorResult RightPlaceholder(CompositeKey key, PopulateMode mode)
		{
			return PlaceholderPopulator.Populate(m_right.Placeholder, key, m_rightPlaceholderKey, mode);
		}

		private Dictionary<CompositeKey, Tuple<CompositeKey, PropagatorResult>> ProcessKeys(IEnumerable<PropagatorResult> instances, ReadOnlyCollection<DbExpression> keySelectors)
		{
			Dictionary<CompositeKey, Tuple<CompositeKey, PropagatorResult>> dictionary = new Dictionary<CompositeKey, Tuple<CompositeKey, PropagatorResult>>(m_parent.UpdateTranslator.KeyComparer);
			foreach (PropagatorResult instance in instances)
			{
				CompositeKey compositeKey = ExtractKey(instance, keySelectors);
				dictionary[compositeKey] = Tuple.Create(compositeKey, instance);
			}
			return dictionary;
		}

		private static CompositeKey ExtractKey(PropagatorResult change, ReadOnlyCollection<DbExpression> keySelectors)
		{
			PropagatorResult[] array = new PropagatorResult[keySelectors.Count];
			for (int i = 0; i < keySelectors.Count; i++)
			{
				PropagatorResult propagatorResult = Evaluator.Evaluate(keySelectors[i], change);
				array[i] = propagatorResult;
			}
			return new CompositeKey(array);
		}
	}

	private readonly UpdateTranslator m_updateTranslator;

	private readonly EntitySet m_table;

	private static readonly string _visitorName = typeof(Propagator).FullName;

	internal UpdateTranslator UpdateTranslator => m_updateTranslator;

	protected override string VisitorName => _visitorName;

	private Propagator(UpdateTranslator parent, EntitySet table)
	{
		m_updateTranslator = parent;
		m_table = table;
	}

	internal static ChangeNode Propagate(UpdateTranslator parent, EntitySet table, DbQueryCommandTree umView)
	{
		DbExpressionVisitor<ChangeNode> visitor = new Propagator(parent, table);
		return umView.Query.Accept(visitor);
	}

	private static ChangeNode BuildChangeNode(DbExpression node)
	{
		return new ChangeNode(MetadataHelper.GetElementType(node.ResultType));
	}

	public override ChangeNode Visit(DbCrossJoinExpression node)
	{
		Check.NotNull(node, "node");
		throw new NotSupportedException(Strings.Update_UnsupportedJoinType(node.ExpressionKind));
	}

	public override ChangeNode Visit(DbJoinExpression node)
	{
		Check.NotNull(node, "node");
		if (DbExpressionKind.InnerJoin != node.ExpressionKind && DbExpressionKind.LeftOuterJoin != node.ExpressionKind)
		{
			throw new NotSupportedException(Strings.Update_UnsupportedJoinType(node.ExpressionKind));
		}
		DbExpression expression = node.Left.Expression;
		DbExpression expression2 = node.Right.Expression;
		ChangeNode left = Visit(expression);
		ChangeNode right = Visit(expression2);
		return new JoinPropagator(left, right, node, this).Propagate();
	}

	public override ChangeNode Visit(DbUnionAllExpression node)
	{
		Check.NotNull(node, "node");
		ChangeNode changeNode = BuildChangeNode(node);
		ChangeNode changeNode2 = Visit(node.Left);
		ChangeNode changeNode3 = Visit(node.Right);
		changeNode.Inserted.AddRange(changeNode2.Inserted);
		changeNode.Inserted.AddRange(changeNode3.Inserted);
		changeNode.Deleted.AddRange(changeNode2.Deleted);
		changeNode.Deleted.AddRange(changeNode3.Deleted);
		changeNode.Placeholder = changeNode2.Placeholder;
		return changeNode;
	}

	public override ChangeNode Visit(DbProjectExpression node)
	{
		Check.NotNull(node, "node");
		ChangeNode changeNode = BuildChangeNode(node);
		ChangeNode changeNode2 = Visit(node.Input.Expression);
		foreach (PropagatorResult item in changeNode2.Inserted)
		{
			changeNode.Inserted.Add(Project(node, item, changeNode.ElementType));
		}
		foreach (PropagatorResult item2 in changeNode2.Deleted)
		{
			changeNode.Deleted.Add(Project(node, item2, changeNode.ElementType));
		}
		changeNode.Placeholder = Project(node, changeNode2.Placeholder, changeNode.ElementType);
		return changeNode;
	}

	private static PropagatorResult Project(DbProjectExpression node, PropagatorResult row, TypeUsage resultType)
	{
		if (!(node.Projection is DbNewInstanceExpression dbNewInstanceExpression))
		{
			throw new NotSupportedException(Strings.Update_UnsupportedProjection(node.Projection.ExpressionKind));
		}
		PropagatorResult[] array = new PropagatorResult[dbNewInstanceExpression.Arguments.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Evaluator.Evaluate(dbNewInstanceExpression.Arguments[i], row);
		}
		return PropagatorResult.CreateStructuralValue(array, (StructuralType)resultType.EdmType, isModified: false);
	}

	public override ChangeNode Visit(DbFilterExpression node)
	{
		Check.NotNull(node, "node");
		ChangeNode changeNode = BuildChangeNode(node);
		ChangeNode changeNode2 = Visit(node.Input.Expression);
		changeNode.Inserted.AddRange(Evaluator.Filter(node.Predicate, changeNode2.Inserted));
		changeNode.Deleted.AddRange(Evaluator.Filter(node.Predicate, changeNode2.Deleted));
		changeNode.Placeholder = changeNode2.Placeholder;
		return changeNode;
	}

	public override ChangeNode Visit(DbScanExpression node)
	{
		Check.NotNull(node, "node");
		EntitySetBase target = node.Target;
		ChangeNode extentModifications = UpdateTranslator.GetExtentModifications(target);
		if (extentModifications.Placeholder == null)
		{
			extentModifications.Placeholder = ExtentPlaceholderCreator.CreatePlaceholder(target);
		}
		return extentModifications;
	}
}
