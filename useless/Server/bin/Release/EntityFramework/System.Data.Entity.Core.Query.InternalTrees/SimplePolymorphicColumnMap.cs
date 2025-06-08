using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class SimplePolymorphicColumnMap : TypedColumnMap
{
	private readonly SimpleColumnMap m_typeDiscriminator;

	private readonly Dictionary<object, TypedColumnMap> m_typedColumnMap;

	internal SimpleColumnMap TypeDiscriminator => m_typeDiscriminator;

	internal Dictionary<object, TypedColumnMap> TypeChoices => m_typedColumnMap;

	internal SimplePolymorphicColumnMap(TypeUsage type, string name, ColumnMap[] baseTypeColumns, SimpleColumnMap typeDiscriminator, Dictionary<object, TypedColumnMap> typeChoices)
		: base(type, name, baseTypeColumns)
	{
		m_typedColumnMap = typeChoices;
		m_typeDiscriminator = typeDiscriminator;
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
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "P{{TypeId={0}, ", new object[1] { TypeDiscriminator });
		foreach (KeyValuePair<object, TypedColumnMap> typeChoice in TypeChoices)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}({1},{2})", new object[3] { text, typeChoice.Key, typeChoice.Value });
			text = ",";
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}
}
