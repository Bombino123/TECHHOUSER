using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class MultipleDiscriminatorPolymorphicColumnMap : TypedColumnMap
{
	private readonly SimpleColumnMap[] m_typeDiscriminators;

	private readonly Dictionary<EntityType, TypedColumnMap> m_typeChoices;

	private readonly Func<object[], EntityType> m_discriminate;

	internal SimpleColumnMap[] TypeDiscriminators => m_typeDiscriminators;

	internal Dictionary<EntityType, TypedColumnMap> TypeChoices => m_typeChoices;

	internal Func<object[], EntityType> Discriminate => m_discriminate;

	internal MultipleDiscriminatorPolymorphicColumnMap(TypeUsage type, string name, ColumnMap[] baseTypeColumns, SimpleColumnMap[] typeDiscriminators, Dictionary<EntityType, TypedColumnMap> typeChoices, Func<object[], EntityType> discriminate)
		: base(type, name, baseTypeColumns)
	{
		m_typeDiscriminators = typeDiscriminators;
		m_typeChoices = typeChoices;
		m_discriminate = discriminate;
	}

	[DebuggerNonUserCode]
	internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
	{
		visitor.Visit(this, arg);
	}

	[DebuggerNonUserCode]
	internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
	{
		return visitor.Visit(this, arg);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = string.Empty;
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "P{{TypeId=<{0}>, ", new object[1] { StringUtil.ToCommaSeparatedString(TypeDiscriminators) });
		foreach (KeyValuePair<EntityType, TypedColumnMap> typeChoice in TypeChoices)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}(<{1}>,{2})", new object[3] { text, typeChoice.Key, typeChoice.Value });
			text = ",";
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}
}
