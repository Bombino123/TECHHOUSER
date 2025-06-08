using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class VarVec : IEnumerable<Var>, IEnumerable
{
	internal class VarVecEnumerator : IEnumerator<Var>, IDisposable, IEnumerator
	{
		private int m_position;

		private Command m_command;

		private BitVec m_bitArray;

		private static readonly int[] MultiplyDeBruijnBitPosition = new int[32]
		{
			0, 1, 28, 2, 29, 14, 24, 3, 30, 22,
			20, 15, 25, 17, 4, 8, 31, 27, 13, 23,
			21, 19, 16, 7, 26, 12, 18, 6, 11, 5,
			10, 9
		};

		public Var Current
		{
			get
			{
				if (m_position < 0 || m_position >= m_bitArray.Length)
				{
					return null;
				}
				return m_command.GetVar(m_position);
			}
		}

		object IEnumerator.Current => Current;

		internal VarVecEnumerator(VarVec vec)
		{
			Init(vec);
		}

		internal void Init(VarVec vec)
		{
			m_position = -1;
			m_command = vec.m_command;
			m_bitArray = vec.m_bitVector;
		}

		public bool MoveNext()
		{
			int[] array = m_bitArray.m_array;
			m_position++;
			int length = m_bitArray.Length;
			int arrayLength = BitVec.GetArrayLength(length, 32);
			int num = m_position / 32;
			int num2 = 0;
			int num3 = 0;
			if (num < arrayLength)
			{
				num2 = array[num];
				num3 = -1 << m_position % 32;
				num2 &= num3;
				if (num2 != 0)
				{
					m_position = num * 32 + MultiplyDeBruijnBitPosition[(uint)((long)(num2 & -num2) * 125613361L) >> 27];
					return true;
				}
				for (num++; num < arrayLength; num++)
				{
					num2 = array[num];
					if (num2 != 0)
					{
						m_position = num * 32 + MultiplyDeBruijnBitPosition[(uint)((long)(num2 & -num2) * 125613361L) >> 27];
						return true;
					}
				}
			}
			m_position = length;
			return false;
		}

		public void Reset()
		{
			m_position = -1;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			m_bitArray = null;
			m_command.ReleaseVarVecEnumerator(this);
		}
	}

	private readonly BitVec m_bitVector;

	private readonly Command m_command;

	internal int Count
	{
		get
		{
			int num = 0;
			using IEnumerator<Var> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				num++;
			}
			return num;
		}
	}

	internal bool IsEmpty => First == null;

	internal Var First
	{
		get
		{
			using (IEnumerator<Var> enumerator = GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					return enumerator.Current;
				}
			}
			return null;
		}
	}

	internal void Clear()
	{
		m_bitVector.Length = 0;
	}

	internal void And(VarVec other)
	{
		Align(other);
		m_bitVector.And(other.m_bitVector);
	}

	internal void Or(VarVec other)
	{
		Align(other);
		m_bitVector.Or(other.m_bitVector);
	}

	internal void Minus(VarVec other)
	{
		VarVec varVec = m_command.CreateVarVec(other);
		varVec.m_bitVector.Length = m_bitVector.Length;
		varVec.m_bitVector.Not();
		And(varVec);
		m_command.ReleaseVarVec(varVec);
	}

	internal bool Overlaps(VarVec other)
	{
		VarVec varVec = m_command.CreateVarVec(other);
		varVec.And(this);
		bool result = !varVec.IsEmpty;
		m_command.ReleaseVarVec(varVec);
		return result;
	}

	internal bool Subsumes(VarVec other)
	{
		int[] array = m_bitVector.m_array;
		int[] array2 = other.m_bitVector.m_array;
		if (array2.Length > array.Length)
		{
			for (int i = array.Length; i < array2.Length; i++)
			{
				if (array2[i] != 0)
				{
					return false;
				}
			}
		}
		int num = Math.Min(array2.Length, array.Length);
		for (int j = 0; j < num; j++)
		{
			if ((array[j] & array2[j]) != array2[j])
			{
				return false;
			}
		}
		return true;
	}

	internal void InitFrom(VarVec other)
	{
		Clear();
		m_bitVector.Length = other.m_bitVector.Length;
		m_bitVector.Or(other.m_bitVector);
	}

	internal void InitFrom(IEnumerable<Var> other)
	{
		InitFrom(other, ignoreParameters: false);
	}

	internal void InitFrom(IEnumerable<Var> other, bool ignoreParameters)
	{
		Clear();
		foreach (Var item in other)
		{
			if (!ignoreParameters || item.VarType != 0)
			{
				Set(item);
			}
		}
	}

	public IEnumerator<Var> GetEnumerator()
	{
		return m_command.GetVarVecEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal bool IsSet(Var v)
	{
		Align(v.Id);
		return m_bitVector.Get(v.Id);
	}

	internal void Set(Var v)
	{
		Align(v.Id);
		m_bitVector.Set(v.Id, value: true);
	}

	internal void Clear(Var v)
	{
		Align(v.Id);
		m_bitVector.Set(v.Id, value: false);
	}

	internal VarVec Remap(IDictionary<Var, Var> varMap)
	{
		VarVec varVec = m_command.CreateVarVec();
		using IEnumerator<Var> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Var current = enumerator.Current;
			if (!varMap.TryGetValue(current, out var value))
			{
				value = current;
			}
			varVec.Set(value);
		}
		return varVec;
	}

	internal VarVec(Command command)
	{
		m_bitVector = new BitVec(64);
		m_command = command;
	}

	private void Align(VarVec other)
	{
		if (other.m_bitVector.Length != m_bitVector.Length)
		{
			if (other.m_bitVector.Length > m_bitVector.Length)
			{
				m_bitVector.Length = other.m_bitVector.Length;
			}
			else
			{
				other.m_bitVector.Length = m_bitVector.Length;
			}
		}
	}

	private void Align(int idx)
	{
		if (idx >= m_bitVector.Length)
		{
			m_bitVector.Length = idx + 1;
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = string.Empty;
		using (IEnumerator<Var> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Var current = enumerator.Current;
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", new object[2] { text, current.Id });
				text = ",";
			}
		}
		return stringBuilder.ToString();
	}

	public VarVec Clone()
	{
		VarVec varVec = m_command.CreateVarVec();
		varVec.InitFrom(this);
		return varVec;
	}
}
