using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class ClassLayoutUser : ClassLayout
{
	public ClassLayoutUser()
	{
	}

	public ClassLayoutUser(ushort packingSize, uint classSize)
	{
		base.packingSize = packingSize;
		base.classSize = classSize;
	}
}
