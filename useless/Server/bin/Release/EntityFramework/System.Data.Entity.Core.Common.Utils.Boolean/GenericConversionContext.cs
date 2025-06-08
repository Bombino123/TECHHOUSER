using System.Collections.Generic;
using System.Linq;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class GenericConversionContext<T_Identifier> : ConversionContext<T_Identifier>
{
	private readonly Dictionary<TermExpr<T_Identifier>, int> _variableMap = new Dictionary<TermExpr<T_Identifier>, int>();

	private Dictionary<int, TermExpr<T_Identifier>> _inverseVariableMap;

	internal override Vertex TranslateTermToVertex(TermExpr<T_Identifier> term)
	{
		if (!_variableMap.TryGetValue(term, out var value))
		{
			value = Solver.CreateVariable();
			_variableMap.Add(term, value);
		}
		return Solver.CreateLeafVertex(value, Solver.BooleanVariableChildren);
	}

	internal override IEnumerable<LiteralVertexPair<T_Identifier>> GetSuccessors(Vertex vertex)
	{
		LiteralVertexPair<T_Identifier>[] array = new LiteralVertexPair<T_Identifier>[2];
		Vertex vertex2 = vertex.Children[0];
		Vertex vertex3 = vertex.Children[1];
		InitializeInverseVariableMap();
		Literal<T_Identifier> literal = new Literal<T_Identifier>(_inverseVariableMap[vertex.Variable], isTermPositive: true);
		array[0] = new LiteralVertexPair<T_Identifier>(vertex2, literal);
		literal = literal.MakeNegated();
		array[1] = new LiteralVertexPair<T_Identifier>(vertex3, literal);
		return array;
	}

	private void InitializeInverseVariableMap()
	{
		if (_inverseVariableMap == null)
		{
			_inverseVariableMap = _variableMap.ToDictionary((KeyValuePair<TermExpr<T_Identifier>, int> kvp) => kvp.Value, (KeyValuePair<TermExpr<T_Identifier>, int> kvp) => kvp.Key);
		}
	}
}
