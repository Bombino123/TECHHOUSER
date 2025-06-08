using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.CommandTrees;

internal sealed class DbRelatedEntityRef
{
	private readonly RelationshipEndMember _sourceEnd;

	private readonly RelationshipEndMember _targetEnd;

	private readonly DbExpression _targetEntityRef;

	internal RelationshipEndMember SourceEnd => _sourceEnd;

	internal RelationshipEndMember TargetEnd => _targetEnd;

	internal DbExpression TargetEntityReference => _targetEntityRef;

	internal DbRelatedEntityRef(RelationshipEndMember sourceEnd, RelationshipEndMember targetEnd, DbExpression targetEntityRef)
	{
		if (sourceEnd.DeclaringType != targetEnd.DeclaringType)
		{
			throw new ArgumentException(Strings.Cqt_RelatedEntityRef_TargetEndFromDifferentRelationship, "targetEnd");
		}
		if (sourceEnd == targetEnd)
		{
			throw new ArgumentException(Strings.Cqt_RelatedEntityRef_TargetEndSameAsSourceEnd, "targetEnd");
		}
		if (targetEnd.RelationshipMultiplicity != RelationshipMultiplicity.One && targetEnd.RelationshipMultiplicity != 0)
		{
			throw new ArgumentException(Strings.Cqt_RelatedEntityRef_TargetEndMustBeAtMostOne, "targetEnd");
		}
		if (!TypeSemantics.IsReferenceType(targetEntityRef.ResultType))
		{
			throw new ArgumentException(Strings.Cqt_RelatedEntityRef_TargetEntityNotRef, "targetEntityRef");
		}
		EntityTypeBase elementType = TypeHelpers.GetEdmType<RefType>(targetEnd.TypeUsage).ElementType;
		EntityTypeBase elementType2 = TypeHelpers.GetEdmType<RefType>(targetEntityRef.ResultType).ElementType;
		if (!elementType.EdmEquals(elementType2) && !TypeSemantics.IsSubTypeOf(elementType2, elementType))
		{
			throw new ArgumentException(Strings.Cqt_RelatedEntityRef_TargetEntityNotCompatible, "targetEntityRef");
		}
		_targetEntityRef = targetEntityRef;
		_targetEnd = targetEnd;
		_sourceEnd = sourceEnd;
	}
}
