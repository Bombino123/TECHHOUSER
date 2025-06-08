using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal sealed class TypeConstant : Constant
{
	private readonly EdmType m_edmType;

	internal EdmType EdmType => m_edmType;

	internal TypeConstant(EdmType type)
	{
		m_edmType = type;
	}

	internal override bool IsNull()
	{
		return false;
	}

	internal override bool IsNotNull()
	{
		return false;
	}

	internal override bool IsUndefined()
	{
		return false;
	}

	internal override bool HasNotNull()
	{
		return false;
	}

	protected override bool IsEqualTo(Constant right)
	{
		if (!(right is TypeConstant typeConstant))
		{
			return false;
		}
		return m_edmType == typeConstant.m_edmType;
	}

	public override int GetHashCode()
	{
		if (m_edmType == null)
		{
			return 0;
		}
		return m_edmType.GetHashCode();
	}

	internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias)
	{
		AsCql(delegate(EntitySet refScopeEntitySet, IList<MemberPath> keyMemberOutputPaths)
		{
			EntityType type = (EntityType)((RefType)outputMember.EdmType).ElementType;
			builder.Append("CreateRef(");
			CqlWriter.AppendEscapedQualifiedName(builder, refScopeEntitySet.EntityContainer.Name, refScopeEntitySet.Name);
			builder.Append(", row(");
			for (int j = 0; j < keyMemberOutputPaths.Count; j++)
			{
				if (j > 0)
				{
					builder.Append(", ");
				}
				string qualifiedName2 = CqlWriter.GetQualifiedName(blockAlias, keyMemberOutputPaths[j].CqlFieldAlias);
				builder.Append(qualifiedName2);
			}
			builder.Append("), ");
			CqlWriter.AppendEscapedTypeName(builder, type);
			builder.Append(')');
		}, delegate(IList<MemberPath> membersOutputPaths)
		{
			CqlWriter.AppendEscapedTypeName(builder, m_edmType);
			builder.Append('(');
			for (int i = 0; i < membersOutputPaths.Count; i++)
			{
				if (i > 0)
				{
					builder.Append(", ");
				}
				string qualifiedName = CqlWriter.GetQualifiedName(blockAlias, membersOutputPaths[i].CqlFieldAlias);
				builder.Append(qualifiedName);
			}
			builder.Append(')');
		}, outputMember);
		return builder;
	}

	internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
	{
		DbExpression cqt = null;
		AsCql(delegate(EntitySet refScopeEntitySet, IList<MemberPath> keyMemberOutputPaths)
		{
			EntityType entityType = (EntityType)((RefType)outputMember.EdmType).ElementType;
			cqt = refScopeEntitySet.CreateRef(entityType, keyMemberOutputPaths.Select((MemberPath km) => row.Property(km.CqlFieldAlias)));
		}, delegate(IList<MemberPath> membersOutputPaths)
		{
			cqt = TypeUsage.Create(m_edmType).New(membersOutputPaths.Select((MemberPath m) => row.Property(m.CqlFieldAlias)));
		}, outputMember);
		return cqt;
	}

	private void AsCql(Action<EntitySet, IList<MemberPath>> createRef, Action<IList<MemberPath>> createType, MemberPath outputMember)
	{
		EntitySet scopeOfRelationEnd = outputMember.GetScopeOfRelationEnd();
		if (scopeOfRelationEnd != null)
		{
			List<MemberPath> arg = new List<MemberPath>(scopeOfRelationEnd.ElementType.KeyMembers.Select((EdmMember km) => new MemberPath(outputMember, km)));
			createRef(scopeOfRelationEnd, arg);
			return;
		}
		List<MemberPath> list = new List<MemberPath>();
		foreach (EdmMember allStructuralMember in Helper.GetAllStructuralMembers(m_edmType))
		{
			list.Add(new MemberPath(outputMember, allStructuralMember));
		}
		createType(list);
	}

	internal override string ToUserString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		ToCompactString(stringBuilder);
		return stringBuilder.ToString();
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append(m_edmType.Name);
	}
}
