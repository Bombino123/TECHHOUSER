using System.Runtime.InteropServices;
using dnlib.DotNet.MD;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class ClassLayout : IMDTokenProvider
{
	protected uint rid;

	protected ushort packingSize;

	protected uint classSize;

	public MDToken MDToken => new MDToken(Table.ClassLayout, rid);

	public uint Rid
	{
		get
		{
			return rid;
		}
		set
		{
			rid = value;
		}
	}

	public ushort PackingSize
	{
		get
		{
			return packingSize;
		}
		set
		{
			packingSize = value;
		}
	}

	public uint ClassSize
	{
		get
		{
			return classSize;
		}
		set
		{
			classSize = value;
		}
	}
}
