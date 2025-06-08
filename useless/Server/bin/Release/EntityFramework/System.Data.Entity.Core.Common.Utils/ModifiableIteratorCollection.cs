using System.Collections.Generic;
using System.Text;

namespace System.Data.Entity.Core.Common.Utils;

internal class ModifiableIteratorCollection<TElement> : InternalBase
{
	private readonly List<TElement> m_elements;

	private int m_currentIteratorIndex;

	internal bool IsEmpty => m_elements.Count == 0;

	internal ModifiableIteratorCollection(IEnumerable<TElement> elements)
	{
		m_elements = new List<TElement>(elements);
		m_currentIteratorIndex = -1;
	}

	internal TElement RemoveOneElement()
	{
		return Remove(m_elements.Count - 1);
	}

	internal void ResetIterator()
	{
		m_currentIteratorIndex = -1;
	}

	internal void RemoveCurrentOfIterator()
	{
		Remove(m_currentIteratorIndex);
		m_currentIteratorIndex--;
	}

	internal IEnumerable<TElement> Elements()
	{
		for (m_currentIteratorIndex = 0; m_currentIteratorIndex < m_elements.Count; m_currentIteratorIndex++)
		{
			yield return m_elements[m_currentIteratorIndex];
		}
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		StringUtil.ToCommaSeparatedString(builder, m_elements);
	}

	private TElement Remove(int index)
	{
		TElement result = m_elements[index];
		int index2 = m_elements.Count - 1;
		m_elements[index] = m_elements[index2];
		m_elements.RemoveAt(index2);
		return result;
	}
}
