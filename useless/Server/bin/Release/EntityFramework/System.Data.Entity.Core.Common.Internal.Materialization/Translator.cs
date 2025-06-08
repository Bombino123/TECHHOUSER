using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common.QueryCache;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal class Translator
{
	internal class TranslatorVisitor : ColumnMapVisitorWithResults<TranslatorResult, TranslatorArg>
	{
		private readonly MetadataWorkspace _workspace;

		private readonly SpanIndex _spanIndex;

		private readonly MergeOption _mergeOption;

		private readonly bool _streaming;

		private readonly bool IsValueLayer;

		private CoordinatorScratchpad _currentCoordinatorScratchpad;

		private readonly Dictionary<EdmType, ObjectTypeMapping> _objectTypeMappings = new Dictionary<EdmType, ObjectTypeMapping>();

		private bool _inNullableType;

		public static readonly MethodInfo Translator_MultipleDiscriminatorPolymorphicColumnMapHelper = typeof(TranslatorVisitor).GetOnlyDeclaredMethod("MultipleDiscriminatorPolymorphicColumnMapHelper");

		public static readonly MethodInfo Translator_TypedCreateInlineDelegate = typeof(TranslatorVisitor).GetOnlyDeclaredMethod("TypedCreateInlineDelegate");

		public CoordinatorScratchpad RootCoordinatorScratchpad { get; private set; }

		public int StateSlotCount { get; private set; }

		public Dictionary<int, Type> ColumnTypes { get; private set; }

		public Set<int> NullableColumns { get; private set; }

		public TranslatorVisitor(MetadataWorkspace workspace, SpanIndex spanIndex, MergeOption mergeOption, bool streaming, bool valueLayer)
		{
			_workspace = workspace;
			_spanIndex = spanIndex;
			_mergeOption = mergeOption;
			_streaming = streaming;
			ColumnTypes = new Dictionary<int, Type>();
			NullableColumns = new Set<int>();
			IsValueLayer = valueLayer;
		}

		private static TranslatorResult AcceptWithMappedType(TranslatorVisitor translatorVisitor, ColumnMap columnMap)
		{
			Type requestedType = translatorVisitor.DetermineClrType(columnMap.Type);
			return columnMap.Accept(translatorVisitor, new TranslatorArg(requestedType));
		}

		internal override TranslatorResult Visit(ComplexTypeColumnMap columnMap, TranslatorArg arg)
		{
			Expression expression = null;
			Expression expression2 = null;
			bool inNullableType = _inNullableType;
			if (columnMap.NullSentinel != null)
			{
				expression2 = CodeGenEmitter.Emit_Reader_IsDBNull(columnMap.NullSentinel);
				_inNullableType = true;
				int columnPos = ((ScalarColumnMap)columnMap.NullSentinel).ColumnPos;
				if (!_streaming && !NullableColumns.Contains(columnPos))
				{
					NullableColumns.Add(columnPos);
				}
			}
			if (IsValueLayer)
			{
				expression = BuildExpressionToGetRecordState(columnMap, null, null, expression2);
			}
			else
			{
				ComplexType complexType = (ComplexType)columnMap.Type.EdmType;
				ConstructorInfo constructorForType = DelegateFactory.GetConstructorForType(DetermineClrType(complexType));
				expression = Expression.MemberInit(bindings: CreatePropertyBindings(columnMap, complexType.Properties), newExpression: Expression.New(constructorForType));
				if (expression2 != null)
				{
					expression = Expression.Condition(expression2, CodeGenEmitter.Emit_NullConstant(expression.Type), expression);
				}
			}
			_inNullableType = inNullableType;
			return new TranslatorResult(expression, arg.RequestedType);
		}

		internal override TranslatorResult Visit(EntityColumnMap columnMap, TranslatorArg arg)
		{
			EntityIdentity entityIdentity = columnMap.EntityIdentity;
			Expression entitySetReader = null;
			Expression expression = Emit_EntityKey_ctor(this, entityIdentity, columnMap.Type.EdmType, isForColumnValue: false, out entitySetReader);
			Expression returnedExpression;
			if (IsValueLayer)
			{
				Expression nullCheckExpression = Expression.Not(CodeGenEmitter.Emit_EntityKey_HasValue(entityIdentity.Keys));
				returnedExpression = BuildExpressionToGetRecordState(columnMap, expression, entitySetReader, nullCheckExpression);
			}
			else
			{
				Expression expression2 = null;
				EntityType entityType = (EntityType)columnMap.Type.EdmType;
				ClrEntityType clrEntityType = (ClrEntityType)LookupObjectMapping(entityType).ClrType;
				Type clrType = clrEntityType.ClrType;
				List<MemberBinding> propertyBindings = CreatePropertyBindings(columnMap, entityType.Properties);
				EntityProxyTypeInfo proxyType = EntityProxyFactory.GetProxyType(clrEntityType, _workspace);
				Expression expression3 = Emit_ConstructEntity(clrEntityType, propertyBindings, expression, entitySetReader, arg, null);
				if (proxyType == null)
				{
					expression2 = expression3;
				}
				else
				{
					Expression ifTrue = Emit_ConstructEntity(clrEntityType, propertyBindings, expression, entitySetReader, arg, proxyType);
					expression2 = Expression.Condition(CodeGenEmitter.Shaper_ProxyCreationEnabled, ifTrue, expression3);
				}
				if (MergeOption.NoTracking != _mergeOption)
				{
					Type c = ((proxyType == null) ? clrType : proxyType.ProxyType);
					if (typeof(IEntityWithKey).IsAssignableFrom(c) && _mergeOption != 0)
					{
						expression2 = Expression.Call(CodeGenEmitter.Shaper_Parameter, CodeGenEmitter.Shaper_HandleIEntityWithKey.MakeGenericMethod(clrType), expression2, entitySetReader);
					}
					else if (_mergeOption == MergeOption.AppendOnly)
					{
						LambdaExpression arg2 = CreateInlineDelegate(expression2);
						expression2 = Expression.Call(CodeGenEmitter.Shaper_Parameter, CodeGenEmitter.Shaper_HandleEntityAppendOnly.MakeGenericMethod(clrType), arg2, expression, entitySetReader);
					}
					else
					{
						expression2 = Expression.Call(CodeGenEmitter.Shaper_Parameter, CodeGenEmitter.Shaper_HandleEntity.MakeGenericMethod(clrType), expression2, expression, entitySetReader);
					}
				}
				else
				{
					expression2 = Expression.Call(CodeGenEmitter.Shaper_Parameter, CodeGenEmitter.Shaper_HandleEntityNoTracking.MakeGenericMethod(clrType), expression2);
				}
				returnedExpression = Expression.Condition(CodeGenEmitter.Emit_EntityKey_HasValue(entityIdentity.Keys), expression2, CodeGenEmitter.Emit_WrappedNullConstant());
			}
			int columnPos = ((ScalarColumnMap)entityIdentity.Keys[0]).ColumnPos;
			if (!_streaming && !NullableColumns.Contains(columnPos))
			{
				NullableColumns.Add(columnPos);
			}
			return new TranslatorResult(returnedExpression, arg.RequestedType);
		}

		private Expression Emit_ConstructEntity(EntityType oSpaceType, IEnumerable<MemberBinding> propertyBindings, Expression entityKeyReader, Expression entitySetReader, TranslatorArg arg, EntityProxyTypeInfo proxyTypeInfo)
		{
			bool flag = proxyTypeInfo != null;
			Type clrType = oSpaceType.ClrType;
			Type actualType;
			Expression input;
			if (flag)
			{
				input = Expression.MemberInit(Expression.New(proxyTypeInfo.ProxyType), propertyBindings);
				actualType = proxyTypeInfo.ProxyType;
			}
			else
			{
				input = Expression.MemberInit(Expression.New(DelegateFactory.GetConstructorForType(clrType)), propertyBindings);
				actualType = clrType;
			}
			input = CodeGenEmitter.Emit_EnsureTypeAndWrap(input, entityKeyReader, entitySetReader, arg.RequestedType, clrType, actualType, (_mergeOption == MergeOption.NoTracking) ? MergeOption.NoTracking : MergeOption.AppendOnly, flag);
			if (flag)
			{
				input = Expression.Call(Expression.Constant(proxyTypeInfo), CodeGenEmitter.EntityProxyTypeInfo_SetEntityWrapper, input);
				if (proxyTypeInfo.InitializeEntityCollections != null)
				{
					input = Expression.Call(proxyTypeInfo.InitializeEntityCollections, input);
				}
			}
			return input;
		}

		private List<MemberBinding> CreatePropertyBindings(StructuredColumnMap columnMap, ReadOnlyMetadataCollection<EdmProperty> properties)
		{
			List<MemberBinding> list = new List<MemberBinding>(columnMap.Properties.Length);
			ObjectTypeMapping objectTypeMapping = LookupObjectMapping(columnMap.Type.EdmType);
			for (int i = 0; i < columnMap.Properties.Length; i++)
			{
				PropertyInfo propertyInfo = DelegateFactory.ValidateSetterProperty(objectTypeMapping.GetPropertyMap(properties[i].Name).ClrProperty.PropertyInfo);
				MethodInfo methodInfo = propertyInfo.Setter();
				Type propertyType = propertyInfo.PropertyType;
				Expression expression = columnMap.Properties[i].Accept(this, new TranslatorArg(propertyType)).Expression;
				if (columnMap.Properties[i] is ScalarColumnMap scalarColumnMap)
				{
					string propertyName = methodInfo.Name.Substring(4);
					Expression expressionWithErrorHandling = CodeGenEmitter.Emit_Shaper_GetPropertyValueWithErrorHandling(propertyType, scalarColumnMap.ColumnPos, propertyName, methodInfo.DeclaringType.Name, scalarColumnMap.Type);
					_currentCoordinatorScratchpad.AddExpressionWithErrorHandling(expression, expressionWithErrorHandling);
				}
				list.Add(Expression.Bind(propertyInfo, expression));
			}
			return list;
		}

		internal override TranslatorResult Visit(SimplePolymorphicColumnMap columnMap, TranslatorArg arg)
		{
			Expression expression = AcceptWithMappedType(this, columnMap.TypeDiscriminator).Expression;
			Expression expression2 = ((!IsValueLayer) ? CodeGenEmitter.Emit_WrappedNullConstant() : CodeGenEmitter.Emit_EnsureType(BuildExpressionToGetRecordState(columnMap, null, null, Expression.Constant(true)), arg.RequestedType));
			foreach (KeyValuePair<object, TypedColumnMap> typeChoice in columnMap.TypeChoices)
			{
				if (!DetermineClrType(typeChoice.Value.Type).IsAbstract())
				{
					Expression expression3 = Expression.Constant(typeChoice.Key, expression.Type);
					Expression test = ((!(expression.Type == typeof(string))) ? CodeGenEmitter.Emit_Equal(expression3, expression) : Expression.Call(Expression.Constant(TrailingSpaceStringComparer.Instance), CodeGenEmitter.IEqualityComparerOfString_Equals, expression3, expression));
					bool inNullableType = _inNullableType;
					_inNullableType = true;
					expression2 = Expression.Condition(test, typeChoice.Value.Accept(this, arg).Expression, expression2);
					_inNullableType = inNullableType;
				}
			}
			return new TranslatorResult(expression2, arg.RequestedType);
		}

		internal override TranslatorResult Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, TranslatorArg arg)
		{
			return new TranslatorResult((Expression)Translator_MultipleDiscriminatorPolymorphicColumnMapHelper.MakeGenericMethod(arg.RequestedType).Invoke(this, new object[1] { columnMap }), arg.RequestedType);
		}

		private Expression MultipleDiscriminatorPolymorphicColumnMapHelper<TElement>(MultipleDiscriminatorPolymorphicColumnMap columnMap)
		{
			Expression[] array = new Expression[columnMap.TypeDiscriminators.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = columnMap.TypeDiscriminators[i].Accept(this, new TranslatorArg(typeof(object))).Expression;
			}
			Expression arg = Expression.NewArrayInit(typeof(object), array);
			List<Expression> list = new List<Expression>();
			Type typeFromHandle = typeof(KeyValuePair<EntityType, Func<Shaper, TElement>>);
			ConstructorInfo declaredConstructor = typeFromHandle.GetDeclaredConstructor(typeof(EntityType), typeof(Func<Shaper, TElement>));
			foreach (KeyValuePair<EntityType, TypedColumnMap> typeChoice in columnMap.TypeChoices)
			{
				Expression body = CodeGenEmitter.Emit_EnsureType(AcceptWithMappedType(this, typeChoice.Value).UnwrappedExpression, typeof(TElement));
				LambdaExpression lambdaExpression = CreateInlineDelegate(body);
				Expression item = Expression.New(declaredConstructor, Expression.Constant(typeChoice.Key), lambdaExpression);
				list.Add(item);
			}
			MethodInfo method = CodeGenEmitter.Shaper_Discriminate.MakeGenericMethod(typeof(TElement));
			return Expression.Call(CodeGenEmitter.Shaper_Parameter, method, arg, Expression.Constant(columnMap.Discriminate), Expression.NewArrayInit(typeFromHandle, list));
		}

		internal override TranslatorResult Visit(RecordColumnMap columnMap, TranslatorArg arg)
		{
			Expression expression = null;
			Expression expression2 = null;
			bool inNullableType = _inNullableType;
			if (columnMap.NullSentinel != null)
			{
				expression2 = CodeGenEmitter.Emit_Reader_IsDBNull(columnMap.NullSentinel);
				_inNullableType = true;
				int columnPos = ((ScalarColumnMap)columnMap.NullSentinel).ColumnPos;
				if (!_streaming && !NullableColumns.Contains(columnPos))
				{
					NullableColumns.Add(columnPos);
				}
			}
			if (IsValueLayer)
			{
				expression = BuildExpressionToGetRecordState(columnMap, null, null, expression2);
			}
			else
			{
				Expression ifTrue;
				if (InitializerMetadata.TryGetInitializerMetadata(columnMap.Type, out var initializerMetadata))
				{
					expression = HandleLinqRecord(columnMap, initializerMetadata);
					ifTrue = CodeGenEmitter.Emit_NullConstant(expression.Type);
				}
				else
				{
					RowType spanRowType = (RowType)columnMap.Type.EdmType;
					if (_spanIndex != null && _spanIndex.HasSpanMap(spanRowType))
					{
						expression = HandleSpandexRecord(columnMap, arg, spanRowType);
						ifTrue = CodeGenEmitter.Emit_WrappedNullConstant();
					}
					else
					{
						expression = HandleRegularRecord(columnMap, arg, spanRowType);
						ifTrue = CodeGenEmitter.Emit_NullConstant(expression.Type);
					}
				}
				if (expression2 != null)
				{
					expression = Expression.Condition(expression2, ifTrue, expression);
				}
			}
			_inNullableType = inNullableType;
			return new TranslatorResult(expression, arg.RequestedType);
		}

		private Expression BuildExpressionToGetRecordState(StructuredColumnMap columnMap, Expression entityKeyReader, Expression entitySetReader, Expression nullCheckExpression)
		{
			RecordStateScratchpad recordStateScratchpad = _currentCoordinatorScratchpad.CreateRecordStateScratchpad();
			int num2 = (recordStateScratchpad.StateSlotNumber = AllocateStateSlot());
			int num3 = columnMap.Properties.Length;
			int num4 = ((entityKeyReader != null) ? (num3 + 1) : num3);
			recordStateScratchpad.ColumnCount = num3;
			EntityType type = null;
			if (TypeHelpers.TryGetEdmType<EntityType>(columnMap.Type, out type))
			{
				recordStateScratchpad.DataRecordInfo = new EntityRecordInfo(type, EntityKey.EntityNotValidKey, null);
			}
			else
			{
				TypeUsage modelTypeUsage = Helper.GetModelTypeUsage(columnMap.Type);
				recordStateScratchpad.DataRecordInfo = new DataRecordInfo(modelTypeUsage);
			}
			Expression[] array = new Expression[num4];
			string[] array2 = new string[recordStateScratchpad.ColumnCount];
			TypeUsage[] array3 = new TypeUsage[recordStateScratchpad.ColumnCount];
			for (int i = 0; i < num3; i++)
			{
				Expression expression = columnMap.Properties[i].Accept(this, new TranslatorArg(typeof(object))).Expression;
				array[i] = Expression.Call(CodeGenEmitter.Shaper_Parameter, CodeGenEmitter.Shaper_SetColumnValue, Expression.Constant(num2), Expression.Constant(i), Expression.Coalesce(expression, CodeGenEmitter.DBNull_Value));
				array2[i] = columnMap.Properties[i].Name;
				array3[i] = columnMap.Properties[i].Type;
			}
			if (entityKeyReader != null)
			{
				array[num4 - 1] = Expression.Call(CodeGenEmitter.Shaper_Parameter, CodeGenEmitter.Shaper_SetEntityRecordInfo, Expression.Constant(num2), entityKeyReader, entitySetReader);
			}
			recordStateScratchpad.GatherData = CodeGenEmitter.Emit_BitwiseOr(array);
			recordStateScratchpad.PropertyNames = array2;
			recordStateScratchpad.TypeUsages = array3;
			Expression expression2 = Expression.Call(CodeGenEmitter.Emit_Shaper_GetState(num2, typeof(RecordState)), CodeGenEmitter.RecordState_GatherData, CodeGenEmitter.Shaper_Parameter);
			if (nullCheckExpression != null)
			{
				Expression ifTrue = Expression.Call(CodeGenEmitter.Emit_Shaper_GetState(num2, typeof(RecordState)), CodeGenEmitter.RecordState_SetNullRecord);
				expression2 = Expression.Condition(nullCheckExpression, ifTrue, expression2);
			}
			return expression2;
		}

		private Expression HandleLinqRecord(RecordColumnMap columnMap, InitializerMetadata initializerMetadata)
		{
			List<TranslatorResult> list = new List<TranslatorResult>(columnMap.Properties.Length);
			foreach (KeyValuePair<ColumnMap, Type> item2 in columnMap.Properties.Zip(initializerMetadata.GetChildTypes()))
			{
				ColumnMap key = item2.Key;
				Type type = item2.Value;
				if (null == type)
				{
					type = DetermineClrType(key.Type);
				}
				TranslatorResult item = key.Accept(this, new TranslatorArg(type));
				list.Add(item);
			}
			return initializerMetadata.Emit(list);
		}

		private Expression HandleRegularRecord(RecordColumnMap columnMap, TranslatorArg arg, RowType spanRowType)
		{
			Expression[] array = new Expression[columnMap.Properties.Length];
			for (int i = 0; i < array.Length; i++)
			{
				Expression unwrappedExpression = AcceptWithMappedType(this, columnMap.Properties[i]).UnwrappedExpression;
				array[i] = Expression.Coalesce(CodeGenEmitter.Emit_EnsureType(unwrappedExpression, typeof(object)), CodeGenEmitter.DBNull_Value);
			}
			Expression expression = Expression.NewArrayInit(typeof(object), array);
			TypeUsage typeUsage = columnMap.Type;
			if (_spanIndex != null)
			{
				typeUsage = _spanIndex.GetSpannedRowType(spanRowType) ?? typeUsage;
			}
			Expression expression2 = Expression.Constant(typeUsage, typeof(TypeUsage));
			return CodeGenEmitter.Emit_EnsureType(Expression.New(CodeGenEmitter.MaterializedDataRecord_ctor, CodeGenEmitter.Shaper_Workspace, expression2, expression), arg.RequestedType);
		}

		private Expression HandleSpandexRecord(RecordColumnMap columnMap, TranslatorArg arg, RowType spanRowType)
		{
			Dictionary<int, AssociationEndMember> spanMap = _spanIndex.GetSpanMap(spanRowType);
			Expression expression = columnMap.Properties[0].Accept(this, arg).Expression;
			for (int i = 1; i < columnMap.Properties.Length; i++)
			{
				AssociationEndMember value = spanMap[i];
				TranslatorResult translatorResult = AcceptWithMappedType(this, columnMap.Properties[i]);
				Expression expression2 = translatorResult.Expression;
				if (translatorResult is CollectionTranslatorResult { ExpressionToGetCoordinator: var expressionToGetCoordinator })
				{
					Type type = expression2.Type.GetGenericArguments()[0];
					MethodInfo method = CodeGenEmitter.Shaper_HandleFullSpanCollection.MakeGenericMethod(type);
					expression = Expression.Call(CodeGenEmitter.Shaper_Parameter, method, expression, expressionToGetCoordinator, Expression.Constant(value));
				}
				else if (typeof(EntityKey) == expression2.Type)
				{
					MethodInfo shaper_HandleRelationshipSpan = CodeGenEmitter.Shaper_HandleRelationshipSpan;
					expression = Expression.Call(CodeGenEmitter.Shaper_Parameter, shaper_HandleRelationshipSpan, expression, expression2, Expression.Constant(value));
				}
				else
				{
					MethodInfo shaper_HandleFullSpanElement = CodeGenEmitter.Shaper_HandleFullSpanElement;
					expression = Expression.Call(CodeGenEmitter.Shaper_Parameter, shaper_HandleFullSpanElement, expression, expression2, Expression.Constant(value));
				}
			}
			return expression;
		}

		internal override TranslatorResult Visit(SimpleCollectionColumnMap columnMap, TranslatorArg arg)
		{
			return ProcessCollectionColumnMap(columnMap, arg);
		}

		internal override TranslatorResult Visit(DiscriminatedCollectionColumnMap columnMap, TranslatorArg arg)
		{
			return ProcessCollectionColumnMap(columnMap, arg, columnMap.Discriminator, columnMap.DiscriminatorValue);
		}

		private TranslatorResult ProcessCollectionColumnMap(CollectionColumnMap columnMap, TranslatorArg arg)
		{
			return ProcessCollectionColumnMap(columnMap, arg, null, null);
		}

		private TranslatorResult ProcessCollectionColumnMap(CollectionColumnMap columnMap, TranslatorArg arg, ColumnMap discriminatorColumnMap, object discriminatorValue)
		{
			Type type = DetermineElementType(arg.RequestedType, columnMap);
			CoordinatorScratchpad coordinatorScratchpad = new CoordinatorScratchpad(type);
			EnterCoordinatorTranslateScope(coordinatorScratchpad);
			ColumnMap columnMap2 = columnMap.Element;
			if (IsValueLayer)
			{
				StructuredColumnMap structuredColumnMap = columnMap2 as StructuredColumnMap;
				if (structuredColumnMap == null)
				{
					ColumnMap[] properties = new ColumnMap[1] { columnMap.Element };
					columnMap2 = new RecordColumnMap(columnMap.Element.Type, columnMap.Element.Name, properties, null);
				}
			}
			bool inNullableType = _inNullableType;
			if (discriminatorColumnMap != null)
			{
				_inNullableType = true;
			}
			Expression unconvertedExpression = columnMap2.Accept(this, new TranslatorArg(type)).UnconvertedExpression;
			Expression[] array;
			if (columnMap.Keys != null)
			{
				array = new Expression[columnMap.Keys.Length];
				for (int i = 0; i < array.Length; i++)
				{
					Expression expression = AcceptWithMappedType(this, columnMap.Keys[i]).Expression;
					array[i] = expression;
				}
			}
			else
			{
				array = new Expression[0];
			}
			Expression discriminator = null;
			if (discriminatorColumnMap != null)
			{
				discriminator = AcceptWithMappedType(this, discriminatorColumnMap).Expression;
				_inNullableType = inNullableType;
			}
			Expression expression2 = BuildExpressionToGetCoordinator(type, unconvertedExpression, array, discriminator, discriminatorValue, coordinatorScratchpad);
			MethodInfo genericElementsMethod = GetGenericElementsMethod(type);
			Expression expression3;
			if (IsValueLayer)
			{
				expression3 = expression2;
			}
			else
			{
				expression3 = Expression.Call(expression2, genericElementsMethod);
				coordinatorScratchpad.Element = CodeGenEmitter.Emit_EnsureType(coordinatorScratchpad.Element, type);
				Type type2 = arg.RequestedType.TryGetElementType(typeof(ICollection<>));
				if (type2 != null)
				{
					Type type3 = EntityUtil.DetermineCollectionType(arg.RequestedType);
					if (type3 == null)
					{
						throw new InvalidOperationException(Strings.ObjectQuery_UnableToMaterializeArbitaryProjectionType(arg.RequestedType));
					}
					Type type4 = typeof(List<>).MakeGenericType(type2);
					if (type3 != type4)
					{
						coordinatorScratchpad.InitializeCollection = CodeGenEmitter.Emit_EnsureType(DelegateFactory.GetNewExpressionForCollectionType(type3), typeof(ICollection<>).MakeGenericType(type2));
					}
					expression3 = CodeGenEmitter.Emit_EnsureType(expression3, arg.RequestedType);
				}
				else if (!arg.RequestedType.IsAssignableFrom(expression3.Type))
				{
					Type type5 = typeof(CompensatingCollection<>).MakeGenericType(type);
					expression3 = CodeGenEmitter.Emit_EnsureType(Expression.New(type5.GetConstructors()[0], expression3), type5);
				}
			}
			ExitCoordinatorTranslateScope();
			return new CollectionTranslatorResult(expression3, arg.RequestedType, expression2);
		}

		public static MethodInfo GetGenericElementsMethod(Type elementType)
		{
			return typeof(Coordinator<>).MakeGenericType(elementType).GetOnlyDeclaredMethod("GetElements");
		}

		private Type DetermineElementType(Type collectionType, CollectionColumnMap columnMap)
		{
			Type type = null;
			if (IsValueLayer)
			{
				type = typeof(RecordState);
			}
			else
			{
				type = TypeSystem.GetElementType(collectionType);
				if (type == collectionType)
				{
					TypeUsage typeUsage = ((CollectionType)columnMap.Type.EdmType).TypeUsage;
					type = DetermineClrType(typeUsage);
				}
			}
			return type;
		}

		private void EnterCoordinatorTranslateScope(CoordinatorScratchpad coordinatorScratchpad)
		{
			if (RootCoordinatorScratchpad == null)
			{
				coordinatorScratchpad.Depth = 0;
				RootCoordinatorScratchpad = coordinatorScratchpad;
				_currentCoordinatorScratchpad = coordinatorScratchpad;
			}
			else
			{
				coordinatorScratchpad.Depth = _currentCoordinatorScratchpad.Depth + 1;
				_currentCoordinatorScratchpad.AddNestedCoordinator(coordinatorScratchpad);
				_currentCoordinatorScratchpad = coordinatorScratchpad;
			}
		}

		private void ExitCoordinatorTranslateScope()
		{
			_currentCoordinatorScratchpad = _currentCoordinatorScratchpad.Parent;
		}

		private Expression BuildExpressionToGetCoordinator(Type elementType, Expression element, Expression[] keyReaders, Expression discriminator, object discriminatorValue, CoordinatorScratchpad coordinatorScratchpad)
		{
			int stateSlotNumber = (coordinatorScratchpad.StateSlotNumber = AllocateStateSlot());
			coordinatorScratchpad.Element = element;
			List<Expression> list = new List<Expression>(keyReaders.Length);
			List<Expression> list2 = new List<Expression>(keyReaders.Length);
			foreach (Expression expression in keyReaders)
			{
				int stateSlotNumber2 = AllocateStateSlot();
				list.Add(CodeGenEmitter.Emit_Shaper_SetState(stateSlotNumber2, expression));
				list2.Add(CodeGenEmitter.Emit_Equal(CodeGenEmitter.Emit_Shaper_GetState(stateSlotNumber2, expression.Type), expression));
			}
			coordinatorScratchpad.SetKeys = CodeGenEmitter.Emit_BitwiseOr(list);
			coordinatorScratchpad.CheckKeys = CodeGenEmitter.Emit_AndAlso(list2);
			if (discriminator != null)
			{
				coordinatorScratchpad.HasData = CodeGenEmitter.Emit_Equal(Expression.Constant(discriminatorValue, discriminator.Type), discriminator);
			}
			return CodeGenEmitter.Emit_Shaper_GetState(stateSlotNumber, typeof(Coordinator<>).MakeGenericType(elementType));
		}

		internal override TranslatorResult Visit(RefColumnMap columnMap, TranslatorArg arg)
		{
			EntityIdentity entityIdentity = columnMap.EntityIdentity;
			Expression entitySetReader;
			ConditionalExpression returnedExpression = Expression.Condition(CodeGenEmitter.Emit_EntityKey_HasValue(entityIdentity.Keys), Emit_EntityKey_ctor(this, entityIdentity, ((RefType)columnMap.Type.EdmType).ElementType, isForColumnValue: true, out entitySetReader), Expression.Constant(null, typeof(EntityKey)));
			int columnPos = ((ScalarColumnMap)entityIdentity.Keys[0]).ColumnPos;
			if (!_streaming && !NullableColumns.Contains(columnPos))
			{
				NullableColumns.Add(columnPos);
			}
			return new TranslatorResult(returnedExpression, arg.RequestedType);
		}

		internal override TranslatorResult Visit(ScalarColumnMap columnMap, TranslatorArg arg)
		{
			Type requestedType = arg.RequestedType;
			TypeUsage type = columnMap.Type;
			int columnPos = columnMap.ColumnPos;
			Type type2 = null;
			Expression expression;
			if (Helper.IsSpatialType(type, out var _))
			{
				expression = CodeGenEmitter.Emit_Conditional_NotDBNull(Helper.IsGeographicType((PrimitiveType)type.EdmType) ? CodeGenEmitter.Emit_EnsureType(CodeGenEmitter.Emit_Shaper_GetGeographyColumnValue(columnPos), requestedType) : CodeGenEmitter.Emit_EnsureType(CodeGenEmitter.Emit_Shaper_GetGeometryColumnValue(columnPos), requestedType), columnPos, requestedType);
				if (!_streaming && !NullableColumns.Contains(columnPos))
				{
					NullableColumns.Add(columnPos);
				}
			}
			else if (Helper.IsHierarchyIdType(type))
			{
				expression = CodeGenEmitter.Emit_Conditional_NotDBNull(CodeGenEmitter.Emit_EnsureType(CodeGenEmitter.Emit_Shaper_GetHierarchyIdColumnValue(columnPos), requestedType), columnPos, requestedType);
			}
			else
			{
				bool isNullable;
				MethodInfo readerMethod = CodeGenEmitter.GetReaderMethod(requestedType, out isNullable);
				expression = Expression.Call(CodeGenEmitter.Shaper_Reader, readerMethod, Expression.Constant(columnPos));
				type2 = TypeSystem.GetNonNullableType(requestedType);
				if (type2.IsEnum() && type2 != requestedType)
				{
					expression = Expression.Convert(expression, type2);
				}
				else if (requestedType == typeof(object) && !IsValueLayer && TypeSemantics.IsEnumerationType(type))
				{
					expression = Expression.Condition(CodeGenEmitter.Emit_Reader_IsDBNull(columnPos), expression, Expression.Convert(Expression.Convert(expression, TypeSystem.GetNonNullableType(DetermineClrType(type.EdmType))), typeof(object)));
					if (!_streaming && !NullableColumns.Contains(columnPos))
					{
						NullableColumns.Add(columnPos);
					}
				}
				expression = CodeGenEmitter.Emit_EnsureType(expression, requestedType);
				if (isNullable)
				{
					expression = CodeGenEmitter.Emit_Conditional_NotDBNull(expression, columnPos, requestedType);
					if (!_streaming && !NullableColumns.Contains(columnPos))
					{
						NullableColumns.Add(columnPos);
					}
				}
			}
			if (!_streaming)
			{
				Type type3 = type2 ?? requestedType;
				type3 = (type3.IsEnum() ? type3.GetEnumUnderlyingType() : type3);
				if (ColumnTypes.TryGetValue(columnPos, out var value))
				{
					if (value == typeof(object) && type3 != typeof(object))
					{
						ColumnTypes[columnPos] = type3;
					}
				}
				else
				{
					ColumnTypes.Add(columnPos, type3);
					if (_inNullableType && !NullableColumns.Contains(columnPos))
					{
						NullableColumns.Add(columnPos);
					}
				}
			}
			Expression expressionWithErrorHandling = CodeGenEmitter.Emit_Shaper_GetColumnValueWithErrorHandling(arg.RequestedType, columnPos, type);
			_currentCoordinatorScratchpad.AddExpressionWithErrorHandling(expression, expressionWithErrorHandling);
			return new TranslatorResult(expression, requestedType);
		}

		internal override TranslatorResult Visit(VarRefColumnMap columnMap, TranslatorArg arg)
		{
			throw new InvalidOperationException(string.Empty);
		}

		private int AllocateStateSlot()
		{
			return StateSlotCount++;
		}

		private Type DetermineClrType(TypeUsage typeUsage)
		{
			return DetermineClrType(typeUsage.EdmType);
		}

		private Type DetermineClrType(EdmType edmType)
		{
			Type type = null;
			edmType = ResolveSpanType(edmType);
			switch (edmType.BuiltInTypeKind)
			{
			case BuiltInTypeKind.ComplexType:
			case BuiltInTypeKind.EntityType:
				type = ((!IsValueLayer) ? LookupObjectMapping(edmType).ClrType.ClrType : typeof(RecordState));
				break;
			case BuiltInTypeKind.RefType:
				type = typeof(EntityKey);
				break;
			case BuiltInTypeKind.CollectionType:
			{
				if (IsValueLayer)
				{
					type = typeof(Coordinator<RecordState>);
					break;
				}
				EdmType edmType2 = ((CollectionType)edmType).TypeUsage.EdmType;
				type = DetermineClrType(edmType2);
				type = typeof(IEnumerable<>).MakeGenericType(type);
				break;
			}
			case BuiltInTypeKind.EnumType:
				if (IsValueLayer)
				{
					type = DetermineClrType(((EnumType)edmType).UnderlyingType);
					break;
				}
				type = LookupObjectMapping(edmType).ClrType.ClrType;
				type = typeof(Nullable<>).MakeGenericType(type);
				break;
			case BuiltInTypeKind.PrimitiveType:
				type = ((PrimitiveType)edmType).ClrEquivalentType;
				if (type.IsValueType())
				{
					type = typeof(Nullable<>).MakeGenericType(type);
				}
				break;
			case BuiltInTypeKind.RowType:
			{
				if (IsValueLayer)
				{
					type = typeof(RecordState);
					break;
				}
				InitializerMetadata initializerMetadata = ((RowType)edmType).InitializerMetadata;
				type = ((initializerMetadata == null) ? typeof(DbDataRecord) : initializerMetadata.ClrType);
				break;
			}
			}
			return type;
		}

		private static ConstructorInfo GetConstructor(Type type)
		{
			if (!type.IsAbstract())
			{
				return DelegateFactory.GetConstructorForType(type);
			}
			return null;
		}

		private ObjectTypeMapping LookupObjectMapping(EdmType edmType)
		{
			EdmType edmType2 = ResolveSpanType(edmType);
			if (edmType2 == null)
			{
				edmType2 = edmType;
			}
			if (!_objectTypeMappings.TryGetValue(edmType2, out var value))
			{
				value = Util.GetObjectMapping(edmType2, _workspace);
				_objectTypeMappings.Add(edmType2, value);
			}
			return value;
		}

		private EdmType ResolveSpanType(EdmType edmType)
		{
			EdmType edmType2 = edmType;
			switch (edmType2.BuiltInTypeKind)
			{
			case BuiltInTypeKind.CollectionType:
				edmType2 = ResolveSpanType(((CollectionType)edmType2).TypeUsage.EdmType);
				if (edmType2 != null)
				{
					edmType2 = new CollectionType(edmType2);
				}
				break;
			case BuiltInTypeKind.RowType:
			{
				RowType rowType = (RowType)edmType2;
				if (_spanIndex != null && _spanIndex.HasSpanMap(rowType))
				{
					edmType2 = rowType.Members[0].TypeUsage.EdmType;
				}
				break;
			}
			}
			return edmType2;
		}

		private LambdaExpression CreateInlineDelegate(Expression body)
		{
			Type type = body.Type;
			return (LambdaExpression)Translator_TypedCreateInlineDelegate.MakeGenericMethod(type).Invoke(this, new object[1] { body });
		}

		private Expression<Func<Shaper, T>> TypedCreateInlineDelegate<T>(Expression body)
		{
			Expression<Func<Shaper, T>> expression = Expression.Lambda<Func<Shaper, T>>(body, new ParameterExpression[1] { CodeGenEmitter.Shaper_Parameter });
			_currentCoordinatorScratchpad.AddInlineDelegate(expression);
			return expression;
		}

		private Expression Emit_EntityKey_ctor(TranslatorVisitor translatorVisitor, EntityIdentity entityIdentity, EdmType type, bool isForColumnValue, out Expression entitySetReader)
		{
			Expression expression = null;
			List<Expression> list = new List<Expression>(entityIdentity.Keys.Length);
			if (IsValueLayer)
			{
				for (int i = 0; i < entityIdentity.Keys.Length; i++)
				{
					Expression expression2 = entityIdentity.Keys[i].Accept(translatorVisitor, new TranslatorArg(typeof(object))).Expression;
					list.Add(expression2);
				}
			}
			else
			{
				ObjectTypeMapping objectTypeMapping = LookupObjectMapping(type);
				for (int j = 0; j < entityIdentity.Keys.Length; j++)
				{
					Type propertyType = DelegateFactory.ValidateSetterProperty(objectTypeMapping.GetPropertyMap(entityIdentity.Keys[j].Name).ClrProperty.PropertyInfo).PropertyType;
					Expression expression3 = entityIdentity.Keys[j].Accept(translatorVisitor, new TranslatorArg(propertyType)).Expression;
					list.Add(CodeGenEmitter.Emit_EnsureType(expression3, typeof(object)));
				}
			}
			if (entityIdentity is SimpleEntityIdentity simpleEntityIdentity)
			{
				if (simpleEntityIdentity.EntitySet == null)
				{
					entitySetReader = Expression.Constant(null, typeof(EntitySet));
					return Expression.Constant(null, typeof(EntityKey));
				}
				entitySetReader = Expression.Constant(simpleEntityIdentity.EntitySet, typeof(EntitySet));
			}
			else
			{
				DiscriminatedEntityIdentity obj = (DiscriminatedEntityIdentity)entityIdentity;
				Expression expression4 = obj.EntitySetColumnMap.Accept(translatorVisitor, new TranslatorArg(typeof(int?))).Expression;
				EntitySet[] entitySetMap = obj.EntitySetMap;
				entitySetReader = Expression.Constant(null, typeof(EntitySet));
				for (int k = 0; k < entitySetMap.Length; k++)
				{
					entitySetReader = Expression.Condition(Expression.Equal(expression4, Expression.Constant(k, typeof(int?))), Expression.Constant(entitySetMap[k], typeof(EntitySet)), entitySetReader);
				}
				int stateSlotNumber = translatorVisitor.AllocateStateSlot();
				expression = CodeGenEmitter.Emit_Shaper_SetStatePassthrough(stateSlotNumber, entitySetReader);
				entitySetReader = CodeGenEmitter.Emit_Shaper_GetState(stateSlotNumber, typeof(EntitySet));
			}
			Expression expression5 = ((1 != entityIdentity.Keys.Length) ? Expression.New(CodeGenEmitter.EntityKey_ctor_CompositeKey, entitySetReader, Expression.NewArrayInit(typeof(object), list)) : Expression.New(CodeGenEmitter.EntityKey_ctor_SingleKey, entitySetReader, list[0]));
			if (expression != null)
			{
				expression5 = Expression.Condition(ifTrue: (!translatorVisitor.IsValueLayer || isForColumnValue) ? Expression.Constant(null, typeof(EntityKey)) : Expression.Constant(EntityKey.NoEntitySetKey, typeof(EntityKey)), test: Expression.Equal(expression, Expression.Constant(null, typeof(EntitySet))), ifFalse: expression5);
			}
			return expression5;
		}
	}

	public static readonly MethodInfo GenericTranslateColumnMap = typeof(Translator).GetDeclaredMethod("TranslateColumnMap", typeof(ColumnMap), typeof(MetadataWorkspace), typeof(SpanIndex), typeof(MergeOption), typeof(bool), typeof(bool));

	internal virtual ShaperFactory<T> TranslateColumnMap<T>(ColumnMap columnMap, MetadataWorkspace workspace, SpanIndex spanIndex, MergeOption mergeOption, bool streaming, bool valueLayer)
	{
		ShaperFactoryQueryCacheKey<T> shaperFactoryQueryCacheKey = new ShaperFactoryQueryCacheKey<T>(ColumnMapKeyBuilder.GetColumnMapKey(columnMap, spanIndex), mergeOption, streaming, valueLayer);
		QueryCacheManager queryCacheManager = workspace.GetQueryCacheManager();
		if (queryCacheManager.TryCacheLookup<ShaperFactoryQueryCacheKey<T>, ShaperFactory<T>>(shaperFactoryQueryCacheKey, out var value))
		{
			return value;
		}
		TranslatorVisitor translatorVisitor = new TranslatorVisitor(workspace, spanIndex, mergeOption, streaming, valueLayer);
		columnMap.Accept(translatorVisitor, new TranslatorArg(typeof(IEnumerable<>).MakeGenericType(typeof(T))));
		CoordinatorFactory<T> rootCoordinatorFactory = (CoordinatorFactory<T>)translatorVisitor.RootCoordinatorScratchpad.Compile();
		Type[] array = null;
		bool[] array2 = null;
		if (!streaming)
		{
			int num = Math.Max(translatorVisitor.ColumnTypes.Any() ? translatorVisitor.ColumnTypes.Keys.Max() : 0, translatorVisitor.NullableColumns.Any() ? translatorVisitor.NullableColumns.Max() : 0);
			array = new Type[num + 1];
			foreach (KeyValuePair<int, Type> columnType in translatorVisitor.ColumnTypes)
			{
				array[columnType.Key] = columnType.Value;
			}
			array2 = new bool[num + 1];
			foreach (int nullableColumn in translatorVisitor.NullableColumns)
			{
				array2[nullableColumn] = true;
			}
		}
		value = new ShaperFactory<T>(translatorVisitor.StateSlotCount, rootCoordinatorFactory, array, array2, mergeOption);
		QueryCacheEntry outQueryCacheEntry = new QueryCacheEntry(shaperFactoryQueryCacheKey, value);
		if (queryCacheManager.TryLookupAndAdd(outQueryCacheEntry, out outQueryCacheEntry))
		{
			value = (ShaperFactory<T>)outQueryCacheEntry.GetTarget();
		}
		return value;
	}

	internal static ShaperFactory TranslateColumnMap(Translator translator, Type elementType, ColumnMap columnMap, MetadataWorkspace workspace, SpanIndex spanIndex, MergeOption mergeOption, bool streaming, bool valueLayer)
	{
		return (ShaperFactory)GenericTranslateColumnMap.MakeGenericMethod(elementType).Invoke(translator, new object[6] { columnMap, workspace, spanIndex, mergeOption, streaming, valueLayer });
	}
}
