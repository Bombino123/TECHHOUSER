using System.Collections.Generic;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class KeyVec
{
	private readonly VarVec m_keys;

	private bool m_noKeys;

	internal VarVec KeyVars => m_keys;

	internal bool NoKeys
	{
		get
		{
			return m_noKeys;
		}
		set
		{
			m_noKeys = value;
		}
	}

	internal KeyVec(Command itree)
	{
		m_keys = itree.CreateVarVec();
		m_noKeys = true;
	}

	internal void InitFrom(KeyVec keyset)
	{
		m_keys.InitFrom(keyset.m_keys);
		m_noKeys = keyset.m_noKeys;
	}

	internal void InitFrom(IEnumerable<Var> varSet)
	{
		InitFrom(varSet, ignoreParameters: false);
	}

	internal void InitFrom(IEnumerable<Var> varSet, bool ignoreParameters)
	{
		m_keys.InitFrom(varSet, ignoreParameters);
		m_noKeys = false;
	}

	internal void InitFrom(KeyVec left, KeyVec right)
	{
		if (left.m_noKeys || right.m_noKeys)
		{
			m_noKeys = true;
			return;
		}
		m_noKeys = false;
		m_keys.InitFrom(left.m_keys);
		m_keys.Or(right.m_keys);
	}

	internal void InitFrom(List<KeyVec> keyVecList)
	{
		m_noKeys = false;
		m_keys.Clear();
		foreach (KeyVec keyVec in keyVecList)
		{
			if (keyVec.m_noKeys)
			{
				m_noKeys = true;
				break;
			}
			m_keys.Or(keyVec.m_keys);
		}
	}

	internal void Clear()
	{
		m_noKeys = true;
		m_keys.Clear();
	}
}
