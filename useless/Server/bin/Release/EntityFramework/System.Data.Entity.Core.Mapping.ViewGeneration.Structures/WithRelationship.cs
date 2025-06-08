using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal sealed class WithRelationship : InternalBase
{
	private readonly AssociationSet m_associationSet;

	private readonly RelationshipEndMember m_fromEnd;

	private readonly EntityType m_fromEndEntityType;

	private readonly RelationshipEndMember m_toEnd;

	private readonly EntityType m_toEndEntityType;

	private readonly EntitySet m_toEndEntitySet;

	private readonly IEnumerable<MemberPath> m_toEndEntityKeyMemberPaths;

	internal EntityType FromEndEntityType => m_fromEndEntityType;

	internal WithRelationship(AssociationSet associationSet, AssociationEndMember fromEnd, EntityType fromEndEntityType, AssociationEndMember toEnd, EntityType toEndEntityType, IEnumerable<MemberPath> toEndEntityKeyMemberPaths)
	{
		m_associationSet = associationSet;
		m_fromEnd = fromEnd;
		m_fromEndEntityType = fromEndEntityType;
		m_toEnd = toEnd;
		m_toEndEntityType = toEndEntityType;
		m_toEndEntitySet = MetadataHelper.GetEntitySetAtEnd(associationSet, toEnd);
		m_toEndEntityKeyMemberPaths = toEndEntityKeyMemberPaths;
	}

	internal StringBuilder AsEsql(StringBuilder builder, string blockAlias, int indentLevel)
	{
		StringUtil.IndentNewLine(builder, indentLevel + 1);
		builder.Append("RELATIONSHIP(");
		List<string> list = new List<string>();
		builder.Append("CREATEREF(");
		CqlWriter.AppendEscapedQualifiedName(builder, m_toEndEntitySet.EntityContainer.Name, m_toEndEntitySet.Name);
		builder.Append(", ROW(");
		foreach (MemberPath toEndEntityKeyMemberPath in m_toEndEntityKeyMemberPaths)
		{
			string qualifiedName = CqlWriter.GetQualifiedName(blockAlias, toEndEntityKeyMemberPath.CqlFieldAlias);
			list.Add(qualifiedName);
		}
		StringUtil.ToSeparatedString(builder, list, ", ", null);
		builder.Append(')');
		builder.Append(",");
		CqlWriter.AppendEscapedTypeName(builder, m_toEndEntityType);
		builder.Append(')');
		builder.Append(',');
		CqlWriter.AppendEscapedTypeName(builder, m_associationSet.ElementType);
		builder.Append(',');
		CqlWriter.AppendEscapedName(builder, m_fromEnd.Name);
		builder.Append(',');
		CqlWriter.AppendEscapedName(builder, m_toEnd.Name);
		builder.Append(')');
		builder.Append(' ');
		return builder;
	}

	internal DbRelatedEntityRef AsCqt(DbExpression row)
	{
		return DbExpressionBuilder.CreateRelatedEntityRef(m_fromEnd, m_toEnd, m_toEndEntitySet.CreateRef(m_toEndEntityType, m_toEndEntityKeyMemberPaths.Select((MemberPath keyMember) => row.Property(keyMember.CqlFieldAlias))));
	}

	internal override void ToCompactString(StringBuilder builder)
	{
	}
}
