using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public class InfoQueueFilter
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public InfoQueueFilterDescription.__Native AllowList;

		public InfoQueueFilterDescription.__Native DenyList;
	}

	public InfoQueueFilterDescription AllowList;

	public InfoQueueFilterDescription DenyList;

	internal void __MarshalFree(ref __Native @ref)
	{
		AllowList.__MarshalFree(ref @ref.AllowList);
		DenyList.__MarshalFree(ref @ref.DenyList);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		AllowList.__MarshalFrom(ref @ref.AllowList);
		DenyList.__MarshalFrom(ref @ref.DenyList);
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		AllowList.__MarshalTo(ref @ref.AllowList);
		DenyList.__MarshalTo(ref @ref.DenyList);
	}
}
