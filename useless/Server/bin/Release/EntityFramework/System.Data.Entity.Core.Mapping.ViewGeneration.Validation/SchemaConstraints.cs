using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal class SchemaConstraints<TKeyConstraint> : InternalBase where TKeyConstraint : InternalBase
{
	private readonly List<TKeyConstraint> m_keyConstraints;

	internal IEnumerable<TKeyConstraint> KeyConstraints => m_keyConstraints;

	internal SchemaConstraints()
	{
		m_keyConstraints = new List<TKeyConstraint>();
	}

	internal void Add(TKeyConstraint constraint)
	{
		m_keyConstraints.Add(constraint);
	}

	private static void ConstraintsToBuilder<Constraint>(IEnumerable<Constraint> constraints, StringBuilder builder) where Constraint : InternalBase
	{
		foreach (Constraint constraint in constraints)
		{
			constraint.ToCompactString(builder);
			builder.Append(Environment.NewLine);
		}
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		ConstraintsToBuilder(m_keyConstraints, builder);
	}
}
