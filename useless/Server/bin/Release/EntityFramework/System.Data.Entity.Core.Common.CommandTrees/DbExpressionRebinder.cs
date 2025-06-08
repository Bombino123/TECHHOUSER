using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.EntitySql;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees;

public class DbExpressionRebinder : DefaultExpressionVisitor
{
	private readonly MetadataWorkspace _metadata;

	private readonly Perspective _perspective;

	internal DbExpressionRebinder()
	{
	}

	protected DbExpressionRebinder(MetadataWorkspace targetWorkspace)
	{
		_metadata = targetWorkspace;
		_perspective = new ModelPerspective(targetWorkspace);
	}

	protected override EntitySetBase VisitEntitySet(EntitySetBase entitySet)
	{
		if (_metadata.TryGetEntityContainer(entitySet.EntityContainer.Name, entitySet.EntityContainer.DataSpace, out var entityContainer))
		{
			EntitySetBase item = null;
			if (entityContainer.BaseEntitySets.TryGetValue(entitySet.Name, ignoreCase: false, out item) && item != null && entitySet.BuiltInTypeKind == item.BuiltInTypeKind)
			{
				return item;
			}
			throw new ArgumentException(Strings.Cqt_Copier_EntitySetNotFound(entitySet.EntityContainer.Name, entitySet.Name));
		}
		throw new ArgumentException(Strings.Cqt_Copier_EntityContainerNotFound(entitySet.EntityContainer.Name));
	}

	protected override EdmFunction VisitFunction(EdmFunction functionMetadata)
	{
		List<TypeUsage> list = new List<TypeUsage>(functionMetadata.Parameters.Count);
		foreach (FunctionParameter parameter in functionMetadata.Parameters)
		{
			TypeUsage item = VisitTypeUsage(parameter.TypeUsage);
			list.Add(item);
		}
		IList<EdmFunction> functionOverloads;
		if (DataSpace.SSpace == functionMetadata.DataSpace)
		{
			EdmFunction function = null;
			if (_metadata.TryGetFunction(functionMetadata.Name, functionMetadata.NamespaceName, list.ToArray(), ignoreCase: false, functionMetadata.DataSpace, out function) && function != null)
			{
				return function;
			}
		}
		else if (_perspective.TryGetFunctionByName(functionMetadata.NamespaceName, functionMetadata.Name, ignoreCase: false, out functionOverloads))
		{
			bool isAmbiguous;
			EdmFunction edmFunction = FunctionOverloadResolver.ResolveFunctionOverloads(functionOverloads, list, isGroupAggregateFunction: false, out isAmbiguous);
			if (!isAmbiguous && edmFunction != null)
			{
				return edmFunction;
			}
		}
		throw new ArgumentException(Strings.Cqt_Copier_FunctionNotFound(TypeHelpers.GetFullName(functionMetadata.NamespaceName, functionMetadata.Name)));
	}

	protected override EdmType VisitType(EdmType type)
	{
		EdmType type2 = type;
		if (BuiltInTypeKind.RefType == type.BuiltInTypeKind)
		{
			RefType refType = (RefType)type;
			EntityType entityType = (EntityType)VisitType(refType.ElementType);
			if (refType.ElementType != entityType)
			{
				type2 = new RefType(entityType);
			}
		}
		else if (BuiltInTypeKind.CollectionType == type.BuiltInTypeKind)
		{
			CollectionType collectionType = (CollectionType)type;
			TypeUsage typeUsage = VisitTypeUsage(collectionType.TypeUsage);
			if (collectionType.TypeUsage != typeUsage)
			{
				type2 = new CollectionType(typeUsage);
			}
		}
		else if (BuiltInTypeKind.RowType == type.BuiltInTypeKind)
		{
			RowType rowType = (RowType)type;
			List<KeyValuePair<string, TypeUsage>> list = null;
			for (int i = 0; i < rowType.Properties.Count; i++)
			{
				EdmProperty edmProperty = rowType.Properties[i];
				TypeUsage typeUsage2 = VisitTypeUsage(edmProperty.TypeUsage);
				if (edmProperty.TypeUsage == typeUsage2)
				{
					continue;
				}
				if (list == null)
				{
					list = new List<KeyValuePair<string, TypeUsage>>(rowType.Properties.Select((EdmProperty prop) => new KeyValuePair<string, TypeUsage>(prop.Name, prop.TypeUsage)));
				}
				list[i] = new KeyValuePair<string, TypeUsage>(edmProperty.Name, typeUsage2);
			}
			if (list != null)
			{
				type2 = new RowType(list.Select((KeyValuePair<string, TypeUsage> propInfo) => new EdmProperty(propInfo.Key, propInfo.Value)), rowType.InitializerMetadata);
			}
		}
		else if (!_metadata.TryGetType(type.Name, type.NamespaceName, type.DataSpace, out type2) || type2 == null)
		{
			throw new ArgumentException(Strings.Cqt_Copier_TypeNotFound(TypeHelpers.GetFullName(type.NamespaceName, type.Name)));
		}
		return type2;
	}

	protected override TypeUsage VisitTypeUsage(TypeUsage type)
	{
		EdmType edmType = VisitType(type.EdmType);
		if (edmType == type.EdmType)
		{
			return type;
		}
		Facet[] array = new Facet[type.Facets.Count];
		int num = 0;
		foreach (Facet facet in type.Facets)
		{
			array[num] = facet;
			num++;
		}
		return TypeUsage.Create(edmType, array);
	}

	private static bool TryGetMember<TMember>(DbExpression instance, string memberName, out TMember member) where TMember : EdmMember
	{
		member = null;
		if (instance.ResultType.EdmType is StructuralType structuralType)
		{
			EdmMember item = null;
			if (structuralType.Members.TryGetValue(memberName, ignoreCase: false, out item))
			{
				member = item as TMember;
			}
		}
		return member != null;
	}

	public override DbExpression Visit(DbPropertyExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression result = expression;
		DbExpression dbExpression = VisitExpression(expression.Instance);
		if (expression.Instance != dbExpression)
		{
			if (Helper.IsRelationshipEndMember(expression.Property))
			{
				if (!TryGetMember<RelationshipEndMember>(dbExpression, expression.Property.Name, out var member))
				{
					EdmType edmType = dbExpression.ResultType.EdmType;
					throw new ArgumentException(Strings.Cqt_Copier_EndNotFound(expression.Property.Name, TypeHelpers.GetFullName(edmType.NamespaceName, edmType.Name)));
				}
				result = dbExpression.Property(member);
			}
			else if (Helper.IsNavigationProperty(expression.Property))
			{
				if (!TryGetMember<NavigationProperty>(dbExpression, expression.Property.Name, out var member2))
				{
					EdmType edmType2 = dbExpression.ResultType.EdmType;
					throw new ArgumentException(Strings.Cqt_Copier_NavPropertyNotFound(expression.Property.Name, TypeHelpers.GetFullName(edmType2.NamespaceName, edmType2.Name)));
				}
				result = dbExpression.Property(member2);
			}
			else
			{
				if (!TryGetMember<EdmProperty>(dbExpression, expression.Property.Name, out var member3))
				{
					EdmType edmType3 = dbExpression.ResultType.EdmType;
					throw new ArgumentException(Strings.Cqt_Copier_PropertyNotFound(expression.Property.Name, TypeHelpers.GetFullName(edmType3.NamespaceName, edmType3.Name)));
				}
				result = dbExpression.Property(member3);
			}
		}
		return result;
	}
}
