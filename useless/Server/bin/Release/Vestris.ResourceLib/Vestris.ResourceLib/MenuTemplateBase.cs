using System;
using System.IO;

namespace Vestris.ResourceLib;

public abstract class MenuTemplateBase
{
	internal abstract IntPtr Read(IntPtr lpRes);

	internal abstract void Write(BinaryWriter w);
}
