using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class Solver
{
	private class VertexValueComparer : IEqualityComparer<Vertex>
	{
		internal static readonly VertexValueComparer Instance = new VertexValueComparer();

		private VertexValueComparer()
		{
		}

		public bool Equals(Vertex x, Vertex y)
		{
			if (x.IsSink())
			{
				return x.Equals(y);
			}
			if (x.Variable != y.Variable || x.Children.Length != y.Children.Length)
			{
				return false;
			}
			for (int i = 0; i < x.Children.Length; i++)
			{
				if (!x.Children[i].Equals(y.Children[i]))
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(Vertex vertex)
		{
			if (vertex.IsSink())
			{
				return vertex.GetHashCode();
			}
			return (vertex.Children[0].GetHashCode() << 5) + 1 + vertex.Children[1].GetHashCode();
		}
	}

	private readonly Dictionary<Triple<Vertex, Vertex, Vertex>, Vertex> _computedIfThenElseValues = new Dictionary<Triple<Vertex, Vertex, Vertex>, Vertex>();

	private readonly Dictionary<Vertex, Vertex> _knownVertices = new Dictionary<Vertex, Vertex>(VertexValueComparer.Instance);

	private int _variableCount;

	internal static readonly Vertex[] BooleanVariableChildren = new Vertex[2]
	{
		Vertex.One,
		Vertex.Zero
	};

	internal int CreateVariable()
	{
		return ++_variableCount;
	}

	internal Vertex Not(Vertex vertex)
	{
		return IfThenElse(vertex, Vertex.Zero, Vertex.One);
	}

	internal Vertex And(IEnumerable<Vertex> children)
	{
		return children.OrderByDescending((Vertex child) => child.Variable).Aggregate(Vertex.One, (Vertex left, Vertex right) => IfThenElse(left, right, Vertex.Zero));
	}

	internal Vertex And(Vertex left, Vertex right)
	{
		return IfThenElse(left, right, Vertex.Zero);
	}

	internal Vertex Or(IEnumerable<Vertex> children)
	{
		return children.OrderByDescending((Vertex child) => child.Variable).Aggregate(Vertex.Zero, (Vertex left, Vertex right) => IfThenElse(left, Vertex.One, right));
	}

	internal Vertex CreateLeafVertex(int variable, Vertex[] children)
	{
		return GetUniqueVertex(variable, children);
	}

	private Vertex GetUniqueVertex(int variable, Vertex[] children)
	{
		Vertex vertex = new Vertex(variable, children);
		if (_knownVertices.TryGetValue(vertex, out var value))
		{
			return value;
		}
		_knownVertices.Add(vertex, vertex);
		return vertex;
	}

	private Vertex IfThenElse(Vertex condition, Vertex then, Vertex @else)
	{
		if (condition.IsOne())
		{
			return then;
		}
		if (condition.IsZero())
		{
			return @else;
		}
		if (then.IsOne() && @else.IsZero())
		{
			return condition;
		}
		if (then.Equals(@else))
		{
			return then;
		}
		Triple<Vertex, Vertex, Vertex> key = new Triple<Vertex, Vertex, Vertex>(condition, then, @else);
		if (_computedIfThenElseValues.TryGetValue(key, out var value))
		{
			return value;
		}
		int topVariableDomainCount;
		int variable = DetermineTopVariable(condition, then, @else, out topVariableDomainCount);
		Vertex[] array = new Vertex[topVariableDomainCount];
		bool flag = true;
		for (int i = 0; i < topVariableDomainCount; i++)
		{
			array[i] = IfThenElse(EvaluateFor(condition, variable, i), EvaluateFor(then, variable, i), EvaluateFor(@else, variable, i));
			if (i > 0 && flag && !array[i].Equals(array[0]))
			{
				flag = false;
			}
		}
		if (flag)
		{
			return array[0];
		}
		value = GetUniqueVertex(variable, array);
		_computedIfThenElseValues.Add(key, value);
		return value;
	}

	private static int DetermineTopVariable(Vertex condition, Vertex then, Vertex @else, out int topVariableDomainCount)
	{
		int variable;
		if (condition.Variable < then.Variable)
		{
			variable = condition.Variable;
			topVariableDomainCount = condition.Children.Length;
		}
		else
		{
			variable = then.Variable;
			topVariableDomainCount = then.Children.Length;
		}
		if (@else.Variable < variable)
		{
			variable = @else.Variable;
			topVariableDomainCount = @else.Children.Length;
		}
		return variable;
	}

	private static Vertex EvaluateFor(Vertex vertex, int variable, int variableAssignment)
	{
		if (variable < vertex.Variable)
		{
			return vertex;
		}
		return vertex.Children[variableAssignment];
	}

	[Conditional("DEBUG")]
	private void AssertVerticesValid(IEnumerable<Vertex> vertices)
	{
		foreach (Vertex vertex in vertices)
		{
			_ = vertex;
		}
	}

	[Conditional("DEBUG")]
	private void AssertVertexValid(Vertex vertex)
	{
		vertex.IsSink();
	}
}
