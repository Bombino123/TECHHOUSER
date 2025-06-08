using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Text;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class UndirectedGraph<TVertex> : InternalBase
{
	private class ComponentNum
	{
		internal int componentNum;

		internal ComponentNum(int compNum)
		{
			componentNum = compNum;
		}

		public override string ToString()
		{
			return StringUtil.FormatInvariant("{0}", componentNum);
		}
	}

	private readonly Graph<TVertex> m_graph;

	private readonly IEqualityComparer<TVertex> m_comparer;

	internal IEnumerable<TVertex> Vertices => m_graph.Vertices;

	internal IEnumerable<KeyValuePair<TVertex, TVertex>> Edges => m_graph.Edges;

	internal UndirectedGraph(IEqualityComparer<TVertex> comparer)
	{
		m_graph = new Graph<TVertex>(comparer);
		m_comparer = comparer;
	}

	internal void AddVertex(TVertex vertex)
	{
		m_graph.AddVertex(vertex);
	}

	internal void AddEdge(TVertex first, TVertex second)
	{
		m_graph.AddEdge(first, second);
		m_graph.AddEdge(second, first);
	}

	internal KeyToListMap<int, TVertex> GenerateConnectedComponents()
	{
		int num = 0;
		Dictionary<TVertex, ComponentNum> dictionary = new Dictionary<TVertex, ComponentNum>(m_comparer);
		foreach (TVertex vertex in Vertices)
		{
			dictionary.Add(vertex, new ComponentNum(num));
			num++;
		}
		foreach (KeyValuePair<TVertex, TVertex> edge in Edges)
		{
			if (dictionary[edge.Key].componentNum == dictionary[edge.Value].componentNum)
			{
				continue;
			}
			int componentNum = dictionary[edge.Value].componentNum;
			int componentNum2 = dictionary[edge.Key].componentNum;
			dictionary[edge.Value].componentNum = componentNum2;
			foreach (TVertex key in dictionary.Keys)
			{
				if (dictionary[key].componentNum == componentNum)
				{
					dictionary[key].componentNum = componentNum2;
				}
			}
		}
		KeyToListMap<int, TVertex> keyToListMap = new KeyToListMap<int, TVertex>(EqualityComparer<int>.Default);
		foreach (TVertex vertex2 in Vertices)
		{
			int componentNum3 = dictionary[vertex2].componentNum;
			keyToListMap.Add(componentNum3, vertex2);
		}
		return keyToListMap;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append(m_graph);
	}
}
