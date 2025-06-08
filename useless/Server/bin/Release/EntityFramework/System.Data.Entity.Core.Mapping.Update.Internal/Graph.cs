using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class Graph<TVertex>
{
	private readonly Dictionary<TVertex, HashSet<TVertex>> m_successorMap;

	private readonly Dictionary<TVertex, int> m_predecessorCounts;

	private readonly HashSet<TVertex> m_vertices;

	private readonly IEqualityComparer<TVertex> m_comparer;

	internal IEnumerable<TVertex> Vertices => m_vertices;

	internal IEnumerable<KeyValuePair<TVertex, TVertex>> Edges
	{
		get
		{
			foreach (KeyValuePair<TVertex, HashSet<TVertex>> successors in m_successorMap)
			{
				foreach (TVertex item in successors.Value)
				{
					yield return new KeyValuePair<TVertex, TVertex>(successors.Key, item);
				}
			}
		}
	}

	internal Graph(IEqualityComparer<TVertex> comparer)
	{
		m_comparer = comparer;
		m_successorMap = new Dictionary<TVertex, HashSet<TVertex>>(comparer);
		m_predecessorCounts = new Dictionary<TVertex, int>(comparer);
		m_vertices = new HashSet<TVertex>(comparer);
	}

	internal void AddVertex(TVertex vertex)
	{
		m_vertices.Add(vertex);
	}

	internal void AddEdge(TVertex from, TVertex to)
	{
		if (m_vertices.Contains(from) && m_vertices.Contains(to))
		{
			if (!m_successorMap.TryGetValue(from, out var value))
			{
				value = new HashSet<TVertex>(m_comparer);
				m_successorMap.Add(from, value);
			}
			if (value.Add(to))
			{
				int value2 = ((!m_predecessorCounts.TryGetValue(to, out value2)) ? 1 : (value2 + 1));
				m_predecessorCounts[to] = value2;
			}
		}
	}

	internal bool TryTopologicalSort(out IEnumerable<TVertex> orderedVertices, out IEnumerable<TVertex> remainder)
	{
		SortedSet<TVertex> sortedSet = new SortedSet<TVertex>(Comparer<TVertex>.Default);
		foreach (TVertex vertex in m_vertices)
		{
			if (!m_predecessorCounts.TryGetValue(vertex, out var value) || value == 0)
			{
				sortedSet.Add(vertex);
			}
		}
		TVertex[] array = new TVertex[m_vertices.Count];
		int count = 0;
		while (0 < sortedSet.Count)
		{
			TVertex min = sortedSet.Min;
			sortedSet.Remove(min);
			if (m_successorMap.TryGetValue(min, out var value2))
			{
				foreach (TVertex item in value2)
				{
					int num = m_predecessorCounts[item] - 1;
					m_predecessorCounts[item] = num;
					if (num == 0)
					{
						sortedSet.Add(item);
					}
				}
				m_successorMap.Remove(min);
			}
			array[count++] = min;
			m_vertices.Remove(min);
		}
		if (m_vertices.Count == 0)
		{
			orderedVertices = array;
			remainder = Enumerable.Empty<TVertex>();
			return true;
		}
		orderedVertices = array.Take(count);
		remainder = m_vertices;
		return false;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<TVertex, HashSet<TVertex>> item in m_successorMap)
		{
			bool flag = true;
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}] --> ", new object[1] { item.Key });
			foreach (TVertex item2 in item.Value)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}]", new object[1] { item2 });
			}
			stringBuilder.Append("; ");
		}
		return stringBuilder.ToString();
	}
}
