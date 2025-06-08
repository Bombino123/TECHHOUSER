using dnlib.DotNet.MD;

namespace dnlib.DotNet;

public abstract class Constant : IMDTokenProvider
{
	protected uint rid;

	protected ElementType type;

	protected object value;

	public MDToken MDToken => new MDToken(Table.Constant, rid);

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

	public ElementType Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	public object Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}
}
