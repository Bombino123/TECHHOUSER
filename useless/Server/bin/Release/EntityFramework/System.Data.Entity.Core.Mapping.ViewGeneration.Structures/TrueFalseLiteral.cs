using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Common.Utils.Boolean;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal abstract class TrueFalseLiteral : BoolLiteral
{
	internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> GetDomainBoolExpression(MemberDomainMap domainMap)
	{
		IEnumerable<Constant> elements = new Constant[1]
		{
			new ScalarConstant(true)
		};
		Set<Constant> domain = new Set<Constant>(new Constant[2]
		{
			new ScalarConstant(true),
			new ScalarConstant(false)
		}, Constant.EqualityComparer).MakeReadOnly();
		Set<Constant> range = new Set<Constant>(elements, Constant.EqualityComparer).MakeReadOnly();
		return BoolLiteral.MakeTermExpression(this, domain, range);
	}

	internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> FixRange(Set<Constant> range, MemberDomainMap memberDomainMap)
	{
		ScalarConstant obj = (ScalarConstant)range.First();
		BoolExpr<DomainConstraint<BoolLiteral, Constant>> boolExpr = GetDomainBoolExpression(memberDomainMap);
		if (!(bool)obj.Value)
		{
			boolExpr = new NotExpr<DomainConstraint<BoolLiteral, Constant>>(boolExpr);
		}
		return boolExpr;
	}
}
