using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public abstract class MenuTemplateBase
{
	internal abstract IntPtr Read(IntPtr lpRes);

	internal abstract void Write(BinaryWriter w);
}
