using System.Collections.Generic;
using System.Linq;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class DomainConstraintConversionContext<T_Variable, T_Element> : ConversionContext<DomainConstraint<T_Variable, T_Element>>
{
	private readonly Dictionary<DomainVariable<T_Variable, T_Element>, int> _domainVariableToRobddVariableMap = new Dictionary<DomainVariable<T_Variable, T_Element>, int>();

	private Dictionary<int, DomainVariable<T_Variable, T_Element>> _inverseMap;

	internal override Vertex TranslateTermToVertex(TermExpr<DomainConstraint<T_Variable, T_Element>> term)
	{
		Set<T_Element> range = term.Identifier.Range;
		DomainVariable<T_Variable, T_Element> variable = term.Identifier.Variable;
		Set<T_Element> domain = variable.Domain;
		if (range.All((T_Element element) => !domain.Contains(element)))
		{
			return Vertex.Zero;
		}
		if (domain.All((T_Element element) => range.Contains(element)))
		{
			return Vertex.One;
		}
		Vertex[] children = domain.Select((T_Element element) => (!range.Contains(element)) ? Vertex.Zero : Vertex.One).ToArray();
		if (!_domainVariableToRobddVariableMap.TryGetValue(variable, out var value))
		{
			value = Solver.CreateVariable();
			_domainVariableToRobddVariableMap[variable] = value;
		}
		return Solver.CreateLeafVertex(value, children);
	}

	internal override IEnumerable<LiteralVertexPair<DomainConstraint<T_Variable, T_Element>>> GetSuccessors(Vertex vertex)
	{
		InitializeInverseMap();
		DomainVariable<T_Variable, T_Element> domainVariable = _inverseMap[vertex.Variable];
		T_Element[] array = domainVariable.Domain.ToArray();
		Dictionary<Vertex, Set<T_Element>> dictionary = new Dictionary<Vertex, Set<T_Element>>();
		for (int i = 0; i < vertex.Children.Length; i++)
		{
			Vertex key = vertex.Children[i];
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = new Set<T_Element>(domainVariable.Domain.Comparer);
				dictionary.Add(key, value);
			}
			value.Add(array[i]);
		}
		foreach (KeyValuePair<Vertex, Set<T_Element>> item in dictionary)
		{
			Vertex key2 = item.Key;
			Set<T_Element> value2 = item.Value;
			Literal<DomainConstraint<T_Variable, T_Element>> literal = new Literal<DomainConstraint<T_Variable, T_Element>>(new TermExpr<DomainConstraint<T_Variable, T_Element>>(new DomainConstraint<T_Variable, T_Element>(domainVariable, value2.MakeReadOnly())), isTermPositive: true);
			yield return new LiteralVertexPair<DomainConstraint<T_Variable, T_Element>>(key2, literal);
		}
	}

	private void InitializeInverseMap()
	{
		if (_inverseMap == null)
		{
			_inverseMap = _domainVariableToRobddVariableMap.ToDictionary((KeyValuePair<DomainVariable<T_Variable, T_Element>, int> kvp) => kvp.Value, (KeyValuePair<DomainVariable<T_Variable, T_Element>, int> kvp) => kvp.Key);
		}
	}
}
