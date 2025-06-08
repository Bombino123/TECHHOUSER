using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal class ColumnMapKeyBuilder : ColumnMapVisitor<int>
{
	private readonly StringBuilder _builder = new StringBuilder();

	private readonly SpanIndex _spanIndex;

	private ColumnMapKeyBuilder(SpanIndex spanIndex)
	{
		_spanIndex = spanIndex;
	}

	internal static string GetColumnMapKey(ColumnMap columnMap, SpanIndex spanIndex)
	{
		ColumnMapKeyBuilder columnMapKeyBuilder = new ColumnMapKeyBuilder(spanIndex);
		columnMap.Accept(columnMapKeyBuilder, 0);
		return columnMapKeyBuilder._builder.ToString();
	}

	internal void Append(string value)
	{
		_builder.Append(value);
	}

	internal void Append(string prefix, Type type)
	{
		Append(prefix, type.AssemblyQualifiedName);
	}

	internal void Append(string prefix, TypeUsage type)
	{
		if (type != null)
		{
			if (InitializerMetadata.TryGetInitializerMetadata(type, out var initializerMetadata))
			{
				initializerMetadata.AppendColumnMapKey(this);
			}
			Append(prefix, type.EdmType);
		}
	}

	internal void Append(string prefix, EdmType type)
	{
		if (type == null)
		{
			return;
		}
		Append(prefix, type.NamespaceName);
		Append(".", type.Name);
		if (type.BuiltInTypeKind != BuiltInTypeKind.RowType || _spanIndex == null)
		{
			return;
		}
		Append("<<");
		Dictionary<int, AssociationEndMember> spanMap = _spanIndex.GetSpanMap((RowType)type);
		if (spanMap != null)
		{
			string value = string.Empty;
			foreach (KeyValuePair<int, AssociationEndMember> item in spanMap)
			{
				Append(value);
				AppendValue("C", item.Key);
				Append(":", item.Value.DeclaringType);
				Append(".", item.Value.Name);
				value = ",";
			}
		}
		Append(">>");
	}

	private void Append(string prefix, string value)
	{
		Append(prefix);
		Append("'");
		Append(value);
		Append("'");
	}

	private void Append(string prefix, ColumnMap columnMap)
	{
		Append(prefix);
		Append("[");
		columnMap?.Accept(this, 0);
		Append("]");
	}

	private void Append(string prefix, IEnumerable<ColumnMap> elements)
	{
		Append(prefix);
		Append("{");
		if (elements != null)
		{
			string prefix2 = string.Empty;
			foreach (ColumnMap element in elements)
			{
				Append(prefix2, element);
				prefix2 = ",";
			}
		}
		Append("}");
	}

	private void Append(string prefix, EntityIdentity entityIdentity)
	{
		Append(prefix);
		Append("[");
		Append(",K", entityIdentity.Keys);
		if (entityIdentity is SimpleEntityIdentity simpleEntityIdentity)
		{
			Append(",", simpleEntityIdentity.EntitySet);
		}
		else
		{
			DiscriminatedEntityIdentity discriminatedEntityIdentity = (DiscriminatedEntityIdentity)entityIdentity;
			Append("CM", discriminatedEntityIdentity.EntitySetColumnMap);
			EntitySet[] entitySetMap = discriminatedEntityIdentity.EntitySetMap;
			foreach (EntitySet entitySet in entitySetMap)
			{
				Append(",E", entitySet);
			}
		}
		Append("]");
	}

	private void Append(string prefix, EntitySet entitySet)
	{
		if (entitySet != null)
		{
			Append(prefix, entitySet.EntityContainer.Name);
			Append(".", entitySet.Name);
		}
	}

	private void AppendValue(string prefix, object value)
	{
		Append(prefix, string.Format(CultureInfo.InvariantCulture, "{0}", new object[1] { value }));
	}

	internal override void Visit(ComplexTypeColumnMap columnMap, int dummy)
	{
		Append("C-", columnMap.Type);
		Append(",N", columnMap.NullSentinel);
		Append(",P", columnMap.Properties);
	}

	internal override void Visit(DiscriminatedCollectionColumnMap columnMap, int dummy)
	{
		Append("DC-D", columnMap.Discriminator);
		AppendValue(",DV", columnMap.DiscriminatorValue);
		Append(",FK", columnMap.ForeignKeys);
		Append(",K", columnMap.Keys);
		Append(",E", columnMap.Element);
	}

	internal override void Visit(EntityColumnMap columnMap, int dummy)
	{
		Append("E-", columnMap.Type);
		Append(",N", columnMap.NullSentinel);
		Append(",P", columnMap.Properties);
		Append(",I", columnMap.EntityIdentity);
	}

	internal override void Visit(SimplePolymorphicColumnMap columnMap, int dummy)
	{
		Append("SP-", columnMap.Type);
		Append(",D", columnMap.TypeDiscriminator);
		Append(",N", columnMap.NullSentinel);
		Append(",P", columnMap.Properties);
		foreach (KeyValuePair<object, TypedColumnMap> typeChoice in columnMap.TypeChoices)
		{
			AppendValue(",K", typeChoice.Key);
			Append(":", typeChoice.Value);
		}
	}

	internal override void Visit(RecordColumnMap columnMap, int dummy)
	{
		Append("R-", columnMap.Type);
		Append(",N", columnMap.NullSentinel);
		Append(",P", columnMap.Properties);
	}

	internal override void Visit(RefColumnMap columnMap, int dummy)
	{
		Append("Ref-", columnMap.EntityIdentity);
		TypeHelpers.TryGetRefEntityType(columnMap.Type, out var referencedEntityType);
		Append(",T", referencedEntityType);
	}

	internal override void Visit(ScalarColumnMap columnMap, int dummy)
	{
		string value = string.Format(CultureInfo.InvariantCulture, "S({0}-{1}:{2})", new object[3]
		{
			columnMap.CommandId,
			columnMap.ColumnPos,
			columnMap.Type.Identity
		});
		Append(value);
	}

	internal override void Visit(SimpleCollectionColumnMap columnMap, int dummy)
	{
		Append("DC-FK", columnMap.ForeignKeys);
		Append(",K", columnMap.Keys);
		Append(",E", columnMap.Element);
	}

	internal override void Visit(VarRefColumnMap columnMap, int dummy)
	{
	}

	internal override void Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, int dummy)
	{
		Append(string.Format(CultureInfo.InvariantCulture, "MD-{0}", new object[1] { Guid.NewGuid() }));
	}
}
