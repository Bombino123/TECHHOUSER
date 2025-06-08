using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal abstract class IdentifierService<T_Identifier>
{
	private class GenericIdentifierService : IdentifierService<T_Identifier>
	{
		internal override Literal<T_Identifier> NegateLiteral(Literal<T_Identifier> literal)
		{
			return new Literal<T_Identifier>(literal.Term, !literal.IsTermPositive);
		}

		internal override ConversionContext<T_Identifier> CreateConversionContext()
		{
			return new GenericConversionContext<T_Identifier>();
		}

		internal override BoolExpr<T_Identifier> LocalSimplify(BoolExpr<T_Identifier> expression)
		{
			return expression.Accept(Simplifier<T_Identifier>.Instance);
		}
	}

	private class DomainConstraintIdentifierService<T_Variable, T_Element> : IdentifierService<DomainConstraint<T_Variable, T_Element>>
	{
		internal override Literal<DomainConstraint<T_Variable, T_Element>> NegateLiteral(Literal<DomainConstraint<T_Variable, T_Element>> literal)
		{
			return new Literal<DomainConstraint<T_Variable, T_Element>>(new TermExpr<DomainConstraint<T_Variable, T_Element>>(literal.Term.Identifier.InvertDomainConstraint()), literal.IsTermPositive);
		}

		internal override ConversionContext<DomainConstraint<T_Variable, T_Element>> CreateConversionContext()
		{
			return new DomainConstraintConversionContext<T_Variable, T_Element>();
		}

		internal override BoolExpr<DomainConstraint<T_Variable, T_Element>> LocalSimplify(BoolExpr<DomainConstraint<T_Variable, T_Element>> expression)
		{
			expression = NegationPusher.EliminateNot(expression);
			return expression.Accept(Simplifier<DomainConstraint<T_Variable, T_Element>>.Instance);
		}
	}

	internal static readonly IdentifierService<T_Identifier> Instance = GetIdentifierService();

	private static IdentifierService<T_Identifier> GetIdentifierService()
	{
		Type typeFromHandle = typeof(T_Identifier);
		if (typeFromHandle.IsGenericType() && typeFromHandle.GetGenericTypeDefinition() == typeof(DomainConstraint<, >))
		{
			Type[] genericArguments = typeFromHandle.GetGenericArguments();
			Type type = genericArguments[0];
			Type type2 = genericArguments[1];
			return (IdentifierService<T_Identifier>)Activator.CreateInstance(typeof(DomainConstraintIdentifierService<, >).MakeGenericType(typeFromHandle, type, type2));
		}
		return new GenericIdentifierService();
	}

	private IdentifierService()
	{
	}

	internal abstract Literal<T_Identifier> NegateLiteral(Literal<T_Identifier> literal);

	internal abstract ConversionContext<T_Identifier> CreateConversionContext();

	internal abstract BoolExpr<T_Identifier> LocalSimplify(BoolExpr<T_Identifier> expression);
}
