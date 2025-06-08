using System.Linq;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class Converter<T_Identifier>
{
	private readonly Vertex _vertex;

	private readonly ConversionContext<T_Identifier> _context;

	private DnfSentence<T_Identifier> _dnf;

	private CnfSentence<T_Identifier> _cnf;

	internal Vertex Vertex => _vertex;

	internal DnfSentence<T_Identifier> Dnf
	{
		get
		{
			InitializeNormalForms();
			return _dnf;
		}
	}

	internal CnfSentence<T_Identifier> Cnf
	{
		get
		{
			InitializeNormalForms();
			return _cnf;
		}
	}

	internal Converter(BoolExpr<T_Identifier> expr, ConversionContext<T_Identifier> context)
	{
		_context = context ?? IdentifierService<T_Identifier>.Instance.CreateConversionContext();
		_vertex = ToDecisionDiagramConverter<T_Identifier>.TranslateToRobdd(expr, _context);
	}

	private void InitializeNormalForms()
	{
		if (_cnf == null)
		{
			if (_vertex.IsOne())
			{
				_cnf = new CnfSentence<T_Identifier>(Set<CnfClause<T_Identifier>>.Empty);
				DnfClause<T_Identifier> element = new DnfClause<T_Identifier>(Set<Literal<T_Identifier>>.Empty);
				Set<DnfClause<T_Identifier>> set = new Set<DnfClause<T_Identifier>>();
				set.Add(element);
				_dnf = new DnfSentence<T_Identifier>(set.MakeReadOnly());
			}
			else if (_vertex.IsZero())
			{
				CnfClause<T_Identifier> element2 = new CnfClause<T_Identifier>(Set<Literal<T_Identifier>>.Empty);
				Set<CnfClause<T_Identifier>> set2 = new Set<CnfClause<T_Identifier>>();
				set2.Add(element2);
				_cnf = new CnfSentence<T_Identifier>(set2.MakeReadOnly());
				_dnf = new DnfSentence<T_Identifier>(Set<DnfClause<T_Identifier>>.Empty);
			}
			else
			{
				Set<DnfClause<T_Identifier>> set3 = new Set<DnfClause<T_Identifier>>();
				Set<CnfClause<T_Identifier>> set4 = new Set<CnfClause<T_Identifier>>();
				Set<Literal<T_Identifier>> path = new Set<Literal<T_Identifier>>();
				FindAllPaths(_vertex, set4, set3, path);
				_cnf = new CnfSentence<T_Identifier>(set4.MakeReadOnly());
				_dnf = new DnfSentence<T_Identifier>(set3.MakeReadOnly());
			}
		}
	}

	private void FindAllPaths(Vertex vertex, Set<CnfClause<T_Identifier>> cnfClauses, Set<DnfClause<T_Identifier>> dnfClauses, Set<Literal<T_Identifier>> path)
	{
		if (vertex.IsOne())
		{
			DnfClause<T_Identifier> element = new DnfClause<T_Identifier>(path);
			dnfClauses.Add(element);
			return;
		}
		if (vertex.IsZero())
		{
			CnfClause<T_Identifier> element2 = new CnfClause<T_Identifier>(new Set<Literal<T_Identifier>>(path.Select((Literal<T_Identifier> l) => l.MakeNegated())));
			cnfClauses.Add(element2);
			return;
		}
		foreach (LiteralVertexPair<T_Identifier> successor in _context.GetSuccessors(vertex))
		{
			path.Add(successor.Literal);
			FindAllPaths(successor.Vertex, cnfClauses, dnfClauses, path);
			path.Remove(successor.Literal);
		}
	}
}
