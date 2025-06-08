using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[DebuggerDisplay("Count = {Count}")]
[ComVisible(true)]
public readonly struct RidList : IEnumerable<uint>, IEnumerable
{
	public struct Enumerator : IEnumerator<uint>, IDisposable, IEnumerator
	{
		private readonly uint startRid;

		private readonly uint length;

		private readonly IList<uint> rids;

		private uint index;

		private uint current;

		public uint Current => current;

		object IEnumerator.Current => current;

		internal Enumerator(in RidList list)
		{
			startRid = list.startRid;
			length = list.length;
			rids = list.rids;
			index = 0u;
			current = 0u;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (rids == null && index < length)
			{
				current = startRid + index;
				index++;
				return true;
			}
			return MoveNextOther();
		}

		private bool MoveNextOther()
		{
			if (index >= length)
			{
				current = 0u;
				return false;
			}
			if (rids != null)
			{
				current = rids[(int)index];
			}
			else
			{
				current = startRid + index;
			}
			index++;
			return true;
		}

		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private readonly uint startRid;

	private readonly uint length;

	private readonly IList<uint> rids;

	public static readonly RidList Empty = Create(0u, 0u);

	public uint this[int index]
	{
		get
		{
			if (rids != null)
			{
				if ((uint)index >= (uint)rids.Count)
				{
					return 0u;
				}
				return rids[index];
			}
			if ((uint)index >= length)
			{
				return 0u;
			}
			return startRid + (uint)index;
		}
	}

	public int Count => (int)length;

	public static RidList Create(uint startRid, uint length)
	{
		return new RidList(startRid, length);
	}

	public static RidList Create(IList<uint> rids)
	{
		return new RidList(rids);
	}

	private RidList(uint startRid, uint length)
	{
		this.startRid = startRid;
		this.length = length;
		rids = null;
	}

	private RidList(IList<uint> rids)
	{
		this.rids = rids ?? throw new ArgumentNullException("rids");
		startRid = 0u;
		length = (uint)rids.Count;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(in this);
	}

	IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
